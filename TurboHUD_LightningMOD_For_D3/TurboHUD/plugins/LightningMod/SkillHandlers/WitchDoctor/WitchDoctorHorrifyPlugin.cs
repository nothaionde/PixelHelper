namespace Turbo.Plugins.LightningMod
{

    public class WitchDoctorHorrifyPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorHorrifyPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_Horrify;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => ctx.Skill.Rune == 1 ? 24 : 18, ctx => ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.RechelsRingOfLarceny.Sno) == true ? 1 : 5).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => ctx.Skill.Rune == 1 ? 24 : 18).ThenCastElseContinue()
                .IfTrue(ctx =>
                ctx.Skill.Player.Density.GetDensity(ctx.Skill.Rune == 1 ? 24 : 18) > 0 &&
                ctx.Hud.Game.Me.Powers.BuffIsActive(402459)).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 0 && ctx.Skill.Player.Density.GetDensity(50) > 0).ThenContinueElseNoCast()//∫ß»À“«»›
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.WitchDoctor_Horrify, 2, 100, 500).ThenCastElseContinue()
                ;
        }
    }
}