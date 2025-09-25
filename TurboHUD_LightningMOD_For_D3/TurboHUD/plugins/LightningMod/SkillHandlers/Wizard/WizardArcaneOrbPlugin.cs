namespace Turbo.Plugins.LightningMod
{
    public class WizardArcaneOrbPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SpareResource { get; set; }

        public WizardArcaneOrbPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_ArcaneOrb;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    return !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Wizard_ArcaneOrb.Sno, 5) && ctx.Skill.Player.Powers.UsedWizardPowers.ArcaneOrb.Rune == 2;
                }).ThenCastElseContinue()
                ;
        }
    }
}