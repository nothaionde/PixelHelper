namespace Turbo.Plugins.LM
{
    using System;

    public class PrimaryResourceAmountIsBelowTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurPri >= amount ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class PrimaryResourceAmountIsBelowTestFluent
    {
        public static PrimaryResourceAmountIsBelowTest1 IfPrimaryResourceAmountIsBelow(this AbstractSkillTest1 parent, Func<TestContext1, int> amountFunc)
        {
            var test = new PrimaryResourceAmountIsBelowTest1()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}