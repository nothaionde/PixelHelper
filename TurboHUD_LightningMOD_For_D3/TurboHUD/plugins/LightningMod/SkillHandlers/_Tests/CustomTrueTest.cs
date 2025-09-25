namespace Turbo.Plugins.LightningMod
{
    using System;

    public class CustomTrueTest : AbstractSkillTest
    {
        public Func<TestContext, bool> TestFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (TestFunc == null) return SkillTestResult.Continue;

            var result = TestFunc.Invoke(context);
            return result ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CustomTrueTestFluent
    {
        public static CustomTrueTest IfTrue(this AbstractSkillTest parent, Func<TestContext, bool> testFunc)
        {
            var test = new CustomTrueTest()
            {
                TestFunc = testFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}