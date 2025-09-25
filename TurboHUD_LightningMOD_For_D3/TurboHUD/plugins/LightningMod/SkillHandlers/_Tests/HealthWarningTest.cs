namespace Turbo.Plugins.LightningMod
{
    public class HealthWarningTest : AbstractSkillTest
    {
        public int LimitWhenPotionAvailable { get; set; }
        public int LimitWhenPotionOnCoolDown { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (!context.Skill.Player.Powers.HealthPotionSkill.IsOnCooldown && context.Skill.Player.Defense.HealthPct <= LimitWhenPotionAvailable) return ResultOnSuccess;
            if (context.Skill.Player.Powers.HealthPotionSkill.IsOnCooldown && context.Skill.Player.Defense.HealthPct <= LimitWhenPotionOnCoolDown) return ResultOnSuccess;

            return ResultOnFail;
        }
    }

    public static class HealthWarningTestFluent
    {
        public static HealthWarningTest IfHealthWarning(this AbstractSkillTest parent, int limitWhenPotionAvailable, int limitWhenPotionOnCooldown)
        {
            var test = new HealthWarningTest()
            {
                LimitWhenPotionAvailable = limitWhenPotionAvailable,
                LimitWhenPotionOnCoolDown = limitWhenPotionOnCooldown,
            };

            parent.NextTest = test;
            return test;
        }
    }
}