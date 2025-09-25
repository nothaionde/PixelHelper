namespace Turbo.Plugins.LightningMod
{
    using System;

    public class CustomFalseTest : AbstractSkillTest
    {
        public Func<TestContext, bool> TestFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (TestFunc == null) return SkillTestResult.Continue;

            var result = TestFunc.Invoke(context);
            return result ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class CustomFalseTestFluent
    {
        public static CustomFalseTest IfFalse(this AbstractSkillTest parent, Func<TestContext, bool> testFunc)
        {
            var test = new CustomFalseTest()
            {
                TestFunc = testFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}