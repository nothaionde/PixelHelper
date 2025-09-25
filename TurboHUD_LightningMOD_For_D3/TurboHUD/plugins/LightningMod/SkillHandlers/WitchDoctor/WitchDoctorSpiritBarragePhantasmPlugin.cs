using System.Linq;

namespace Turbo.Plugins.LightningMod
{
    public class WitchDoctorSpiritBarragePhantasmPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorSpiritBarragePhantasmPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_SpiritBarrage;
            Rune = 2;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(200, ctx => 100).ThenContinueElseNoCast()
                .IfTrue(ctx =>hud.Interaction.IsHotKeySet(ActionKey.Move) && hud.Interaction.IsContinuousActionStarted(ActionKey.Move)).ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    var actorCount = Hud.Game.Actors.Count(actor =>
                        actor.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel &&
                        actor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId &&
                        actor.NormalizedXyDistanceToMe <= 60);
                    return actorCount < 3;
                }).ThenCastElseContinue();
        }
    }
}