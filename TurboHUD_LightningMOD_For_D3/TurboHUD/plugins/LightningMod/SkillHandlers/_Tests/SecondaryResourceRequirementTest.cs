namespace Turbo.Plugins.LightningMod
{
    using System;

    public class SecondaryResourceRequirementTest : AbstractSkillTest
    {
        public int SpareResource { get; set; }
        public Func<TestContext, int> BaseResourceCalculatorFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (BaseResourceCalculatorFunc == null) return SkillTestResult.Continue;
            var resourceRequirement = this.BaseResourceCalculatorFunc(context) == 0 ? context.Skill.GetResourceRequirement() : context.Skill.GetResourceRequirement(this.BaseResourceCalculatorFunc(context));
            resourceRequirement += this.SpareResource;
            if (context.Skill.Player.Stats.ResourceCurSec < resourceRequirement)
            {
                return ResultOnFail;
            }
            else return ResultOnSuccess;
        }
    }

    public static class SecondaryResourceRequirementTestFluent
    {
        public static SecondaryResourceRequirementTest IfSecondaryResourceIsEnough(this AbstractSkillTest parent, int spareResource, Func<TestContext, int> baseResourceCalculatorDelegate)
        {
            var test = new SecondaryResourceRequirementTest()
            {
                SpareResource = spareResource,
                BaseResourceCalculatorFunc = baseResourceCalculatorDelegate,
            };

            parent.NextTest = test;
            return test;
        }
    }
}