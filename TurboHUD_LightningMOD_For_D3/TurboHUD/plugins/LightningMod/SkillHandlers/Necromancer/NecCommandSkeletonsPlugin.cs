using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecCommandSkeletonsPlugin : AbstractSkillHandler, ISkillHandler
    {
        private bool isCast = false;
        public NecCommandSkeletonsPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = true;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_CommandSkeletons;
            CreateCastRule()
                .IfTrue(ctx =>
                {//毒骷髅不继续往下了
                    return ctx.Skill.Rune == 3;
                }
                ).ThenNoCastElseContinue()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {//选中BOSS时
                    var monster = ctx.Hud.Game.SelectedMonster2;
                    if (monster == null) return false;
                    return monster?.Rarity == ActorRarity.Boss;
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {//不激活的时候激活一次
                    return !ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno);
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    bool cast = false;
                    if (ctx.Hud.Game.Me.Powers.UsedLegendaryPowers.BloodsongMail?.Active == true)//血歌锁甲
                    {
                        if (ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno))//亡者领域
                        {
                            if (!isCast)
                            {
                                isCast = true;
                                cast = true;
                            }
                        }
                        else
                        {
                            isCast = false;
                        }
                    }
                    else
                    {
                        isCast = false;
                    }
                    return cast;
                }).ThenCastElseContinue()
                ;


            CreateCastRule()
                .IfTrue(ctx =>
                {//毒骷髅不继续往下了
                    return ctx.Skill.Rune == 3;
                }
                ).ThenNoCastElseContinue()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.Sno)).ThenNoCastElseContinue()//激活时不继续
                .IfTrue(ctx =>
                {//选中任意怪时
                    var monster = ctx.Hud.Game.SelectedMonster2;
                    if (monster == null)
                    {
                        return false;
                    }
                    else
                    {
                        bool JessethArms = ctx.Skill.Player.Powers.BuffIsActive(476047);//刀盾套
                        if (JessethArms)//刀盾 = 激活一次
                        {
                            return true;
                        }
                    }
                    return false;
                }).ThenCastElseContinue()
                ;


            CreateCastRule()
                .IfTrue(ctx =>
                {//毒骷髅不继续往下了
                    return ctx.Skill.Rune == 3;
                }
                ).ThenNoCastElseContinue()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {//选中任意怪时
                    var monster = ctx.Hud.Game.SelectedMonster2;
                    if (monster == null)
                    {
                        return false;
                    }
                    else
                    {
                        bool ObsidianRingoftheZodiac = ctx.Skill.Player.Powers.BuffIsActive(402459);//黄道
                        if (ctx.Skill.GetResourceRequirement() <= 35 && ObsidianRingoftheZodiac)//消耗低于35+黄道 = 持续施放
                        {
                            return true;
                        }
                    }
                    return false;
                }).ThenCastElseContinue()
                ;

            CreateCastRule()
               .IfTrue(ctx =>
               {//毒骷髅不继续往下了
                    return ctx.Skill.Rune == 3;
               }
               ).ThenNoCastElseContinue()
               .IfCanCastSkill(2000, 2000, 2000).ThenContinueElseNoCast()
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
               //.IfSpecificBuffIsActive(Hud.Sno.SnoPowers.BondsOfCLena).ThenContinueElseNoCast()//克雷纳的束缚
               .IfTrue(ctx =>
               {//选中精英怪或BOSS时
                    var monster = Hud.Game.SelectedMonster2;
                   if (monster == null)
                   {
                       return false;
                   }
                   var CommandSkeletonsTarget = ctx.Hud.Game.Actors.Where(x => x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_None, 453801) == 1 || x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_A, 453801) == 1 || x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_B, 453801) == 1 || x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_C, 453801) == 1 || x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_D, 453801) == 1 || x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_E, 453801) == 1).FirstOrDefault();
                   bool isCommandSkeletonsTargetOnScreen = CommandSkeletonsTarget?.IsOnScreen == true;
                   var ArmyoftheDead = ctx.Hud.Game.Me.Powers.GetUsedSkill(ctx.Hud.Sno.SnoPowers.Necromancer_ArmyOfTheDead);
                   bool AttackAnyMonster = ArmyoftheDead?.IsOnCooldown == true && isCommandSkeletonsTargetOnScreen;
                   return (monster.Rarity == ActorRarity.Boss || monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare || AttackAnyMonster) && !monster.Illusion;
               }).ThenCastElseContinue()
               ;

        }
    }
}