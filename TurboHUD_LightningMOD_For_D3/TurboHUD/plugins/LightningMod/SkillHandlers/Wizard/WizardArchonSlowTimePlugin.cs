namespace Turbo.Plugins.LightningMod
{
    public class WizardArchonSlowTimePlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SpareResource { get; set; }

        public WizardArchonSlowTimePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_ArchonSlowTime;
            CreateCastRule()
                .IfCanCastSkill(50,100,200)
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    bool isSlowTime = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Wizard_ArchonSlowTime.Sno);
                    return isSlowTime;
                }
                ).ThenContinueElseCast()
                ;
        }
    }
}