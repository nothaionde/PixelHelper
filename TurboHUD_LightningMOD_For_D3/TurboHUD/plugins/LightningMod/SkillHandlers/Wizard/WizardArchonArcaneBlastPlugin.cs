namespace Turbo.Plugins.LightningMod
{
    public class WizardArchonArcaneBlastPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SpareResource { get; set; }

        public WizardArchonArcaneBlastPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_ArchonArcaneBlast;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 18 , ctx => 1).ThenCastElseContinue()
                ;
        }
    }
}