namespace Turbo.Plugins.LM
{
    public class OnCooldownTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Skill.IsOnCooldown ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class OnCooldownTestFluent
    {
        public static OnCooldownTest1 IfOnCooldown(this AbstractSkillTest1 parent)
        {
            var test = new OnCooldownTest1();

            parent.NextTest = test;
            return test;
        }
    }
}