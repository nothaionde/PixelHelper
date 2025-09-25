namespace Turbo.Plugins.LightningMod
{
    using System;

    public class SecondaryResourceAmountIsBelowTest : AbstractSkillTest
    {
        public Func<TestContext, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurSec >= amount ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class SecondaryResourceAmountIsBelowTestFluent
    {
        public static SecondaryResourceAmountIsBelowTest IfSecondaryResourceAmountIsBelow(this AbstractSkillTest parent, Func<TestContext, int> amountFunc)
        {
            var test = new SecondaryResourceAmountIsBelowTest()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}