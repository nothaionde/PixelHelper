namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterRainofVengeancePlugin : AbstractSkillHandler, ISkillHandler
	{
        public DemonHunterRainofVengeancePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_RainOfVengeance;

            CreateCastRule()

               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               ;

        }
    }
}