namespace Turbo.Plugins.LightningMod
{
    public class WitchDoctorPiranhasPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorPiranhasPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_Piranhas;
            Rune = 2;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEliteOrBossNearbyCursor(ctx => 12).ThenCastElseContinue()
                .IfAttacking().ThenContinueElseNoCast()
                .IfEnoughMonstersNearbyCursor(ctx => 12, ctx => 3).ThenCastElseContinue()
                ;

        }
    }
}