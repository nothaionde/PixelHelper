namespace Turbo.Plugins.LM
{
    using System;

    public class SecondaryResourceAmountIsBelowTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurSec >= amount ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class SecondaryResourceAmountIsBelowTestFluent
    {
        public static SecondaryResourceAmountIsBelowTest1 IfSecondaryResourceAmountIsBelow(this AbstractSkillTest1 parent, Func<TestContext1, int> amountFunc)
        {
            var test = new SecondaryResourceAmountIsBelowTest1()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}