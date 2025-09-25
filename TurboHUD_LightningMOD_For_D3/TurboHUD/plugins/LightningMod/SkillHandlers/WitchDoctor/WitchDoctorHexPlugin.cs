namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class WitchDoctorHexPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorHexPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_Hex;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.BaneOfTheStrickenPrimary).ThenNoCastElseContinue()//带受罚不放
                .IfTrue(ctx => Hud.Game.Me.GetSetItemCount(859926) >= 4).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.WitchDoctor_Hex, 5,500,1000).ThenCastElseContinue()
                ;

        }
    }
}