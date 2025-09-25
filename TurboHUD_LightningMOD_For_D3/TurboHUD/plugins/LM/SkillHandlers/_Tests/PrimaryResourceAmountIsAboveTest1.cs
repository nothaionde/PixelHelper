namespace Turbo.Plugins.LM
{
    using System;

    public class PrimaryResourceAmountIsAboveTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> AmountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var amount = AmountFunc(context);
            return context.Skill.Player.Stats.ResourceCurPri >= amount ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class PrimaryResourceAmountIsAboveTestFluent
    {
        public static PrimaryResourceAmountIsAboveTest1 IfPrimaryResourceAmountIsAbove(this AbstractSkillTest1 parent, Func<TestContext1, int> amountFunc)
        {
            var test = new PrimaryResourceAmountIsAboveTest1()
            {
                AmountFunc = amountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}