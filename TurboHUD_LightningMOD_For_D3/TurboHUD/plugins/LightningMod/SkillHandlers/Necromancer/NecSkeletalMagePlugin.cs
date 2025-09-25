using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    
    public class NecSkeletalMagePlugin : AbstractSkillHandler, ISkillHandler
    {
        private bool tempwalk1;
        private bool tempwalk2;
        public NecSkeletalMagePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_SkeletalMage;
            CreateCastRule()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Rune == 4 && !ctx.Hud.Game.Me.Powers.BuffIsActive(484311);//不穿拉泽斯的意志
                }
                ).ThenContinueElseNoCast()//弓箭手符文 
                .IfCanCastSkill((int)Hud.Game.CurrentLatency + 50, (int)Hud.Game.CurrentLatency + 100, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfPrimaryResourceAmountIsAbove(ctx => (int)(40 - 40 * Hud.Game.Me.Stats.ResourceCostReduction) + 1).ThenContinueElseNoCast()//确保有保底能量
                .IfBossIsNearby(ctx => 60).ThenContinueElseNoCast()//附近60码内有BOSS
                .IfTrue(ctx =>
                {//施放亡者领域时施放召唤骷髅弓箭手确保亡者领域全程覆盖攻速BUFF
                    var buffSimulacrum = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno);//血魂双分期间
                    var buffSkeletalMage = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_SkeletalMage.Sno);//骷髅法师
                    return buffSkeletalMage?.IconCounts[6] < 2 && buffSimulacrum?.TimeElapsedSeconds[1] < 0.5 && buffSimulacrum?.TimeElapsedSeconds[1] != 0;
                }).ThenCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.AquilaCuirass, 1).ThenContinueElseNoCast()//天鹰激活
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Necromancer_SkeletalMage, 6).ThenNoCastElseContinue()//骷髅法师激活
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Necromancer_SkeletalMage, 3, 2000, 3000).ThenCastElseContinue()//弓箭手BUFF即将过期（2~3秒以内）
                ;

            CreateCastRule()//魂法
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Rune == 1 || ctx.Hud.Game.Me.Powers.BuffIsActive(484311);//拉泽斯的意志
                }
                ).ThenContinueElseNoCast()//精魂灌注符文
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Rune == 3 && ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.NayrsBlackDeath.Sno, 0);//黑镰+毒骷髅不使用
                }
                ).ThenNoCastElseContinue()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 55, ctx => 1).ThenContinueElseNoCast()//至少周围要有怪
                /*.IfTrue(ctx =>//自动保持杨裤，因为效果不理想暂时屏蔽
                {
                    int tempX = Hud.Window.CursorX;
                    int tempY = Hud.Window.CursorY;
                    IScreenCoordinate myCoordinate = ctx.Skill.Player.FloorCoordinate.ToScreenCoordinate();
                    if (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno) && (ctx.Hud.Interaction.IsHotKeySet(ActionKey.Move) && !ctx.Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)))
                    {
                        ctx.Hud.Interaction.MouseMove(myCoordinate.X + (tempwalk2 ?  0 : (tempwalk1 ? 1 : -1)), myCoordinate.Y + (tempwalk1 ? 0  :(tempwalk2 ? 1 : -1)));
                        ctx.Hud.Interaction.DoAction(ActionKey.Move);
                        ctx.Hud.Interaction.MouseMove(tempX, tempY);
                        tempwalk1 = !tempwalk1;
                        tempwalk2 = !tempwalk2;
                    }
                    return true;
                })*/
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfPrimaryResourceAmountIsAbove(ctx => (int)(40 - 40 * Hud.Game.Me.Stats.ResourceCostReduction) + 1).ThenContinueElseNoCast()//确保有保底能量
                .IfTrue(ctx =>
                {
                    int skeletons = 2;
                    if (ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(55, false))//有精英时
                    {
                        skeletons = 2;
                    }
                    else if (ctx.Skill.Player.Density.GetDensity(55) <= 15)//没有精英且小于等于15个怪时
                    {
                        skeletons = 2;
                    }
                    else//没有精英且大于15个怪时
                    {
                        skeletons = 8;
                    }
                    var buff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_SkeletalMage.Sno);
                    return (buff?.IconCounts[6] < skeletons);//小于指定骷髅数施放
                }).ThenCastElseContinue()//优先保证骷髅最低数量再保证骷髅质量
                .IfPrimaryResourcePercentageIsAbove((Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno) || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno) || (Hud.Game.Actors.Where(x => x.SnoActor.Kind == ActorKind.HealthGlobe && x.IsOnScreen).Count() * Hud.Game.Me.Stats.ResourceMaxEssence * GetReapersWrapspercent() + (Hud.Game.Me.Powers.UsedSkills.Any(x => x.SnoPower.Sno == 460757) ? Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._p6_necro_corpse_flesh && x.CentralXyDistanceToMe <= 60).Count() * 10 : 0) + Hud.Game.Me.Stats.ResourceCurEssence >= Hud.Game.Me.Stats.ResourceMaxEssence)) ? 100 : 90).ThenContinueElseNoCast()//判断为高能状态
                .IfTrue(ctx =>
                {
                    var actorsSkeletonMage = Hud.Game.Actors.Where(x => (
                    x.SnoActor.Sno == ActorSnoEnum._p6_necro_skeletonmage_b ||//无符文、亡魂之赐
                    x.SnoActor.Sno == ActorSnoEnum._p6_necro_skeletonmage_c ||//精魂灌注
                    x.SnoActor.Sno == ActorSnoEnum._p6_necro_skeletonmage_d ||//生命供给
                    x.SnoActor.Sno == ActorSnoEnum._p6_necro_skeletonmage_e ||//污染
                    x.SnoActor.Sno == ActorSnoEnum._p6_necro_skeletonmage_f_archer//骷髅弓箭手
                    ) && x.SummonerAcdDynamicId == ctx.Hud.Game.Me.SummonerId //x.GetAttributeValueAsInt(Hud.Sno.Attributes.Pet_Owner, 0xFFFFF) == 0 
                    && (x.GetAttributeValue(Hud.Sno.Attributes.Multiplicative_Damage_Percent_Bonus, 0xFFFFF) >= (Hud.Game.Me.Stats.ResourceMaxEssence * 0.9 - (40 - 40 * Hud.Game.Me.Stats.ResourceCostReduction)) * 3 / 100 + 1));//自己招的高能骷髅，随动最大精魂上限的90%
                    return actorsSkeletonMage.Count() < 10;
                }).ThenCastElseContinue()//10个高能骷髅时往下判断
                .IfTrue(ctx => {
                    var monster = ctx.Hud.Game.SelectedMonster2;
                    return monster != null && monster.Invisible == false && (monster.IsElite == true || (monster.SnoMonster.Priority == MonsterPriority.goblin) || (monster.SnoMonster.Priority == MonsterPriority.boss) || (monster.SnoMonster.Priority == MonsterPriority.keywarden));
                    }
                ).ThenCastElseContinue()//鼠标选中精英、BOSS、哥布林、钥匙怪时施放
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Necromancer_SkeletalMage, 6, 100, 500).ThenCastElseContinue()//有高能骷髅即将消失（0.5秒以内）
                ;
        }

        private float GetReapersWrapspercent()//获取夺魂者裹腕特效
        {
            float percent = 0;
            if (Hud.Game.Me.CubeSnoItem2 != null && Hud.Game.Me.CubeSnoItem2.Sno == Hud.Sno.SnoItems.Unique_Bracer_103_x1.Sno)
            {
                percent = 0.3f;
            }
            else
            {
                var Reapers = Hud.Game.Items.FirstOrDefault(item => item.Location == ItemLocation.Bracers && item.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Bracer_103_x1.Sno);
                percent = Reapers == null ? 0 : (float)Reapers.Perfections.FirstOrDefault(p => p.Attribute.Code == "Item_Power_Passive").Cur;
            }
            return percent;
        }
    }
}