using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecDevourPlugin : AbstractSkillHandler, ISkillHandler
    {
        public NecDevourPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_Devour;
            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Rune == 3;
                }).ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Hud.Game.AliveMonsters.Count() == 0 && ctx.Hud.Game.Me.Defense.HealthPct == 100 && ctx.Hud.Game.Me.Stats.ResourcePctEssence == 100;
                }).ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Hud.Game.Actors.Any(actor => actor.SnoActor.Sno == ActorSnoEnum._p6_necro_corpse_flesh && actor.CentralXyDistanceToMe <= 60) || ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno);
                }).ThenCastElseContinue()
                ;
        }
    }
}