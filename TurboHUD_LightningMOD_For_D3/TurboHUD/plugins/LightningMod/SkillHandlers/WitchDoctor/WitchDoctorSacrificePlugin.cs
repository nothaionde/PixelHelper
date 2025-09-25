namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.glq;

    public class WitchDoctorSacrificePlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorSacrificePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_Sacrifice;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfCanCastSkill(300, 500, 1000).ThenContinueElseNoCast()
                .IfRunning().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 0).ThenContinueElseNoCast()//Provoke the Pack 激怒兽群
                .IfEliteOrBossNearbyCursor(ctx => 12).ThenContinueElseNoCast()
                .IfTrue(ctx => PublicClassPlugin.GetBuffCount (ctx.Hud, Hud.Sno.SnoPowers.WitchDoctor_Sacrifice.Sno, 0) < 5).ThenCastElseContinue()
                ;

        }
    }
}