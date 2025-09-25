namespace Turbo.Plugins.LM
{
    using System;

    public class CustomTrueTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, bool> TestFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (TestFunc == null) return SkillTestResult.Continue;

            var result = TestFunc.Invoke(context);
            return result ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CustomTrueTestFluent
    {
        public static CustomTrueTest1 IfTrue(this AbstractSkillTest1 parent, Func<TestContext1, bool> testFunc)
        {
            var test = new CustomTrueTest1()
            {
                TestFunc = testFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}