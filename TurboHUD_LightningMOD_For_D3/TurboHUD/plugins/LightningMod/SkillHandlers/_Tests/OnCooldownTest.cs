namespace Turbo.Plugins.LightningMod
{
    public class OnCooldownTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Skill.IsOnCooldown ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class OnCooldownTestFluent
    {
        public static OnCooldownTest IfOnCooldown(this AbstractSkillTest parent)
        {
            var test = new OnCooldownTest();

            parent.NextTest = test;
            return test;
        }
    }
}