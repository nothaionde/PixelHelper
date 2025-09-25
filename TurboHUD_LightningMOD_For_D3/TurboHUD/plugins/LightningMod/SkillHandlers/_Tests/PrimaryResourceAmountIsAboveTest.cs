namespace Turbo.Plugins.LightningMod
{
    using System;

    public class PrimaryResourceAmountIsAboveTest : AbstractSkillTest
    {
        public Func<TestContext, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurPri >= amount ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class PrimaryResourceAmountIsAboveTestFluent
    {
        public static PrimaryResourceAmountIsAboveTest IfPrimaryResourceAmountIsAbove(this AbstractSkillTest parent, Func<TestContext, int> amountFunc)
        {
            var test = new PrimaryResourceAmountIsAboveTest()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}