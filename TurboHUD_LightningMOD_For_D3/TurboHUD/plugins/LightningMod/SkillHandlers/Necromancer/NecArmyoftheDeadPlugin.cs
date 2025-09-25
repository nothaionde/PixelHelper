using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecArmyoftheDeadPlugin : AbstractSkillHandler, ISkillHandler
    {
        public NecArmyoftheDeadPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_ArmyOfTheDead;

            CreateCastRule()//不装备元素戒指的规则
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.FuneraryPick).ThenNoCastElseContinue()//墓葬镐时不生效
                .IfTrue(ctx =>
                {
                    int range = 14;
                    bool IncludeMinion = false;
                    IWorldCoordinate cursor = ctx.Hud.Window.CreateScreenCoordinate(ctx.Hud.Window.CursorX, ctx.Hud.Window.CursorY).ToWorldCoordinate();
                    bool Result = ctx.Hud.Game.AliveMonsters.Any(m => ((m.IsElite && (IncludeMinion ? true : m.Rarity != ActorRarity.RareMinion)) || m.SnoMonster.Priority == MonsterPriority.goblin) && m.FloorCoordinate.XYDistanceTo(cursor) <= range && !m.Invisible && !m.Illusion);
                    return Result;
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    var MonsterRiftProgression = 0d;
                    IWorldCoordinate cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    var monsters = ctx.Hud.Game.AliveMonsters.Where( m => m.FloorCoordinate.XYDistanceTo(cur) <= ((ctx.Skill.Rune == 3 && glq.PublicClassPlugin.IsElementReady(hud, 1, ctx.Skill.Player)) ? 30 : 20)) ;
                    foreach (var monster in monsters)
                    {
                        MonsterRiftProgression += monster.SnoMonster.RiftProgression * 100.0d / Hud.Game.MaxQuestProgress;
                    }
                    return MonsterRiftProgression >= 1.3d;
                }
                ).ThenCastElseContinue()
                //拉斯玛套装build
                .IfTrue(ctx =>
                {
                    var Revives = ctx.Hud.Game.Actors.Where(m =>
                    (m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 462239) == 1 ||
                    m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_A, 462239) == 1 ||
                    m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_B, 462239) == 1 ||
                    m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_C, 462239) == 1 ||
                    m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_D, 462239) == 1 ||
                    m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_E, 462239) == 1) &&
                    m.SummonerAcdDynamicId == ctx.Skill.Player.SummonerId);//复生怪物
                    int sets = ctx.Skill.Player.GetSetItemCount(740279);//拉斯玛套装
                    return sets >= 2 && Revives.Count() < 10;
                }).ThenContinueElseNoCast()
                .IfEnoughMonstersNearbyCursor(ctx => 15, ctx => 4).ThenCastElseContinue()
                .IfEliteOrBossNearbyCursor(ctx => 15, false).ThenCastElseContinue()
                ;


            CreateCastRule()//当死灵鼠标的14码内有任意怪物且释放恐镰或鲜血虹吸时，则释放亡者大军
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearbyCursor(ctx => 14, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return Hud.Game.Me.Animation == AnimSnoEnum._p6_necro_male_hth_cast_grimscythe_leftright ||
                    Hud.Game.Me.Animation == AnimSnoEnum._p6_necro_male_hth_cast_grimscythe_rightleft ||
                    Hud.Game.Me.Animation == AnimSnoEnum._p6_necro_female_hth_cast_grimscythe_leftright ||
                    Hud.Game.Me.Animation == AnimSnoEnum._p6_necro_female_hth_cast_grimscythe_rightleft ||
                    Hud.Game.Me.Animation == AnimSnoEnum._p6_necro_male_hth_cast_bloodsiphon ||
                    Hud.Game.Me.Animation == AnimSnoEnum._p6_necro_female_hth_cast_bloodsiphon
                    ;
                }).ThenCastElseContinue()
                ;


        }

    }
}