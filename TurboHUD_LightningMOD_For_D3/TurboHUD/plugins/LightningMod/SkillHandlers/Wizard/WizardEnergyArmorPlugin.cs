namespace Turbo.Plugins.LightningMod
{
    public class WizardEnergyArmorPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardEnergyArmorPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_EnergyArmor;

            CreateCastRule()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(55000, 65000).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.Powers.UsedWizardPowers.Archon != null && ctx.Skill.RemainingBuffTime() < 120;
                }).ThenCastElseContinue();
        }
    }
}