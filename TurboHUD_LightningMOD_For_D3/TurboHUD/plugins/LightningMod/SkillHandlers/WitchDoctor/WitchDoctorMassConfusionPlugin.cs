namespace Turbo.Plugins.LightningMod
{
    public class WitchDoctorMassConfusionPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int ActivationRange { get; set; }

        public WitchDoctorMassConfusionPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.PreAttack)
        {
            Enabled = false;
            ActivationRange = 20;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_MassConfusion;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx => 20).ThenCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 20, ctx => 5).ThenCastElseContinue();
        }
    }
}