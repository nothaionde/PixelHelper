using System.Linq;

namespace Turbo.Plugins.LightningMod
{
    public class WitchDoctorGargantuanPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorGargantuanPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_Gargantuan;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var exists = Hud.Game.Actors.Any(actor =>
                        actor.SnoActor.Sno == ActorSnoEnum._wd_gargantuan_slam ||
                        actor.SnoActor.Sno == ActorSnoEnum._wd_gargantuan_cleave ||
                        actor.SnoActor.Sno == ActorSnoEnum._wd_gargantuan_poison ||
                        actor.SnoActor.Sno == ActorSnoEnum._wd_gargantuan_cooldown);
                    return exists;
                }).ThenNoCastElseContinue()
                .IfEliteOrBossNearbyCursor(ctx => 12).ThenCastElseContinue()
                ;
        }
    }
}