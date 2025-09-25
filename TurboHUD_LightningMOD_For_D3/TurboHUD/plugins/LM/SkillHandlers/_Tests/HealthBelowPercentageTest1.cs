namespace Turbo.Plugins.LM
{
    using System;

    public class HealthBelowPercentageTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> PercentageFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var limit = PercentageFunc(context);
            return context.Skill.Player.Defense.HealthPct < limit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class HealthBelowPercentageTestFluent
    {
        public static HealthBelowPercentageTest1 IfHealthPercentageIsBelow(this AbstractSkillTest1 parent, Func<TestContext1, int> percentageFunc)
        {
            var test = new HealthBelowPercentageTest1()
            {
                PercentageFunc = percentageFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}