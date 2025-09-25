using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class WizardBlizzardPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardBlizzardPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_Blizzard;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.WinterFlurry).ThenContinueElseNoCast()//寒流法器
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    var HydraCount = ctx.Hud.Game.Actors.Where(act => act.SummonerAcdDynamicId == ctx.Skill.Player.SummonerAcdDynamicId && 
                    (
                    act.SnoActor.Sno == ActorSnoEnum._wizard_hydra_defaultfire_pool ||
                    act.SnoActor.Sno == ActorSnoEnum._wizard_hydra_runearcane_pool ||
                    act.SnoActor.Sno == ActorSnoEnum._wizard_hydra_runefrost_pool ||
                    act.SnoActor.Sno == ActorSnoEnum._wizard_hydra_runelightning_pool
                    ) && act.FloorCoordinate.XYZDistanceTo(cursor) <= (ctx.Skill.Rune == 1 ? 30 : 12)).Count();//预计暴风雪覆盖范围内多头蛇数量
                    return HydraCount >= 1;
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    var Monsters = ctx.Hud.Game.AliveMonsters.Where(m => 
                    //(m.IsElite || m.SnoMonster.Priority == MonsterPriority.goblin) && 
                    m.FloorCoordinate.XYZDistanceTo(cursor) <= (ctx.Skill.Rune == 1 ? 15 : 6) && !m.Invulnerable && !m.Invisible && !m.Illusion &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_3_Visual_Effect_None, 30680) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_3_Visual_Effect_C, 30680) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_3_Visual_Effect_E, 30680) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_3_Visual_Effect_D, 30680) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_3_Visual_Effect_B, 30680) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_3_Visual_Effect_A, 30680) != 1
                    );
                    return Monsters.Count() > 0;//暴风雪覆盖范围区域内的任意怪未受暴风雪打击时施放
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    bool isInBlizzard = ctx.Hud.Game.Actors.Any(x => (
                    x.SnoActor.Sno == ActorSnoEnum._wizard_blizzard ||
                    x.SnoActor.Sno == ActorSnoEnum._wizard_blizzard_addfreeze ||
                    x.SnoActor.Sno == ActorSnoEnum._wizard_blizzard_addtime ||
                    x.SnoActor.Sno == ActorSnoEnum._wizard_blizzard_addsize ||
                    x.SnoActor.Sno == ActorSnoEnum._wizard_blizzard_reducecost
                    ) && (x.FloorCoordinate.XYZDistanceTo(cursor) < (ctx.Skill.Rune == 1 ? 15 : 6)));//判断鼠标是否处于暴风雪区域内
                    return isInBlizzard;
                }
                ).ThenContinueElseCast()
                .IfCanCastSkill(1500, 1500, 10000).ThenCastElseContinue()
                ;
        }
    }
}