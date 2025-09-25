namespace Turbo.Plugins.LM
{
    using System;

    public class CustomFalseTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, bool> TestFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (TestFunc == null) return SkillTestResult.Continue;

            var result = TestFunc.Invoke(context);
            return result ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class CustomFalseTestFluent
    {
        public static CustomFalseTest1 IfFalse(this AbstractSkillTest1 parent, Func<TestContext1, bool> testFunc)
        {
            var test = new CustomFalseTest1()
            {
                TestFunc = testFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}