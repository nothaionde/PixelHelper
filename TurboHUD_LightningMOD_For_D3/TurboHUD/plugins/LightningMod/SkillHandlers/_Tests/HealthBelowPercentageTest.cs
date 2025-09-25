namespace Turbo.Plugins.LightningMod
{
    using System;

    public class HealthBelowPercentageTest : AbstractSkillTest
    {
        public Func<TestContext, int> PercentageFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var limit = PercentageFunc(context);
            return context.Skill.Player.Defense.HealthPct < limit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class HealthBelowPercentageTestFluent
    {
        public static HealthBelowPercentageTest IfHealthPercentageIsBelow(this AbstractSkillTest parent, Func<TestContext, int> percentageFunc)
        {
            var test = new HealthBelowPercentageTest()
            {
                PercentageFunc = percentageFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}