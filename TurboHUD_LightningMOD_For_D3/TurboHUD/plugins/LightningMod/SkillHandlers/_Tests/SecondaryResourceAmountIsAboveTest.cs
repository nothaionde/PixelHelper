namespace Turbo.Plugins.LightningMod
{
    using System;

    public class SecondaryResourceAmountIsAboveTest : AbstractSkillTest
    {
        public Func<TestContext, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurSec >= amount ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SecondaryResourceAmountIsAboveTestFluent
    {
        public static SecondaryResourceAmountIsAboveTest IfSecondaryResourceAmountIsAbove(this AbstractSkillTest parent, Func<TestContext, int> amountFunc)
        {
            var test = new SecondaryResourceAmountIsAboveTest()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}