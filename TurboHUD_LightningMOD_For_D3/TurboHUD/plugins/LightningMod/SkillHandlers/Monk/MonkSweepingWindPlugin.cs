namespace Turbo.Plugins.LightningMod
{
    public class MonkSweepingWindPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int ActivationRange { get; set; }

        public MonkSweepingWindPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
            ActivationRange = 40;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_SweepingWind;

            CreateCastRule()
                .IfCanCastSkill(150, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(15, ctx => 75).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(300, 1000).ThenCastElseContinue();
        }
    }
}