namespace Turbo.Plugins.LightningMod
{
    using System;

    public class PrimaryResourceAmountIsBelowTest : AbstractSkillTest
    {
        public Func<TestContext, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurPri >= amount ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class PrimaryResourceAmountIsBelowTestFluent
    {
        public static PrimaryResourceAmountIsBelowTest IfPrimaryResourceAmountIsBelow(this AbstractSkillTest parent, Func<TestContext, int> amountFunc)
        {
            var test = new PrimaryResourceAmountIsBelowTest()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}