namespace Turbo.Plugins.LM
{
    using System;

    public class SecondaryResourceAmountIsAboveTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurSec >= amount ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SecondaryResourceAmountIsAboveTestFluent
    {
        public static SecondaryResourceAmountIsAboveTest1 IfSecondaryResourceAmountIsAbove(this AbstractSkillTest1 parent, Func<TestContext1, int> amountFunc)
        {
            var test = new SecondaryResourceAmountIsAboveTest1()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}