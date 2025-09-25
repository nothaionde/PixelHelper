namespace Turbo.Plugins.LightningMod
{
    public class WizardStormArmorPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardStormArmorPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_StormArmor;

            CreateCastRule()
                .IfCanCastSkill(100, 200, 500).ThenContinueElseNoCast()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(50, ctx => 25).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(1000, 2000).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.Powers.UsedWizardPowers.Archon != null && ctx.Skill.RemainingBuffTime() < 120;
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    var SpeedBuff = ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Generic_PagesBuffRunSpeed.Sno);//加速塔
                    var isSpeedActive = (SpeedBuff == null || SpeedBuff.Active == false || SpeedBuff.TimeElapsedSeconds[0] > 0.5d) ? false : true;
                    return isSpeedActive;
                }).ThenCastElseContinue()
                ;
        }
    }
}