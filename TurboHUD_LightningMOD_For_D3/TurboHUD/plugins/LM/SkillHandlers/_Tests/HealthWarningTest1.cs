namespace Turbo.Plugins.LM
{
    public class HealthWarningTest1 : AbstractSkillTest1
    {
        public int LimitWhenPotionAvailable { get; set; }
        public int LimitWhenPotionOnCoolDown { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (!context.Skill.Player.Powers.HealthPotionSkill.IsOnCooldown && context.Skill.Player.Defense.HealthPct <= LimitWhenPotionAvailable) return ResultOnSuccess;
            if (context.Skill.Player.Powers.HealthPotionSkill.IsOnCooldown && context.Skill.Player.Defense.HealthPct <= LimitWhenPotionOnCoolDown) return ResultOnSuccess;

            return ResultOnFail;
        }
    }

    public static class HealthWarningTestFluent
    {
        public static HealthWarningTest1 IfHealthWarning(this AbstractSkillTest1 parent, int limitWhenPotionAvailable, int limitWhenPotionOnCooldown)
        {
            var test = new HealthWarningTest1()
            {
                LimitWhenPotionAvailable = limitWhenPotionAvailable,
                LimitWhenPotionOnCoolDown = limitWhenPotionOnCooldown,
            };

            parent.NextTest = test;
            return test;
        }
    }
}