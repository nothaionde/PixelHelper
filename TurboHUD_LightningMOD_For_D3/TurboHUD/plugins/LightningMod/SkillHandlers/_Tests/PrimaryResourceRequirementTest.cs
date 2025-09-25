using System;

namespace Turbo.Plugins.LightningMod
{
    public class PrimaryResourceRequirementTest : AbstractSkillTest
    {
        public Func<TestContext, int> SpareResourceFunc { get; set; }
        public Func<TestContext, int> BaseResourceCalculatorFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (BaseResourceCalculatorFunc == null)
                return SkillTestResult.Continue;
            var resourceRequirement = BaseResourceCalculatorFunc(context) == 0 ? context.Skill.GetResourceRequirement() : context.Skill.GetResourceRequirement(BaseResourceCalculatorFunc(context));
            resourceRequirement += SpareResourceFunc(context);
            return context.Skill.Player.Stats.ResourceCurPri < resourceRequirement ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class PrimaryResourceRequirementTestFluent
    {
        public static PrimaryResourceRequirementTest IfPrimaryResourceIsEnough(this AbstractSkillTest parent, int spareResource, Func<TestContext, int> baseResourceCalculatorFunc)
        {
            var test = new PrimaryResourceRequirementTest()
            {
                SpareResourceFunc = ctx => spareResource,
                BaseResourceCalculatorFunc = baseResourceCalculatorFunc,
            };

            parent.NextTest = test;
            return test;
        }

        public static PrimaryResourceRequirementTest IfPrimaryResourceIsEnough(this AbstractSkillTest parent, Func<TestContext, int> spareResourceFunc, Func<TestContext, int> baseResourceCalculatorFunc)
        {
            var test = new PrimaryResourceRequirementTest()
            {
                SpareResourceFunc = spareResourceFunc,
                BaseResourceCalculatorFunc = baseResourceCalculatorFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}