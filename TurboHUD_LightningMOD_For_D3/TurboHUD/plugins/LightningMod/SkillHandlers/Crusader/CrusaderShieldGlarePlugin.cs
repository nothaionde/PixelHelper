using Turbo.Plugins.glq;
using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderShieldGlarePlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderShieldGlarePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_ShieldGlare;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    bool enemiesAround = ctx.Hud.Game.AliveMonsters.Any(m => (m.IsElite || m.SnoMonster.Priority == MonsterPriority.goblin || m.Rarity == ActorRarity.Boss) && m.Rarity != ActorRarity.RareMinion && !m.AffixSnoList.Any(a => a.Affix == MonsterAffix.Juggernaut) && !m.Blind && !m.Illusion && !m.Invisible && !m.Invulnerable && m.NormalizedXyDistanceToMe <= 30 && 
                    (ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.TheFinalWitness.Sno) ? true : //最后的见证者为30码内
                    PublicClassPlugin.isMobInSkillRange(ctx.Hud, m, 40, 30, 0)));//否则为鼠标扇形范围内
                    return 
                    ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno)//元素戒指
                    && PublicClassPlugin.IsElementReady(ctx.Hud, 0.5, ctx.Skill.Player, Hud.GetPlugin<PublicClassPlugin>().CoeIndex)//爆发前0.5秒
                    && enemiesAround
                    ;
                }).ThenCastElseContinue()
                ;
        }
    }
}