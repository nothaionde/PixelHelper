namespace Turbo.Plugins.LM
{
    using System;

    public class SecondaryResourceRequirementTest1 : AbstractSkillTest1
    {
        public int SpareResource { get; set; }
        public Func<TestContext1, int> BaseResourceCalculatorFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
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
        public static SecondaryResourceRequirementTest1 IfSecondaryResourceIsEnough(this AbstractSkillTest1 parent, int spareResource, Func<TestContext1, int> baseResourceCalculatorDelegate)
        {
            var test = new SecondaryResourceRequirementTest1()
            {
                SpareResource = spareResource,
                BaseResourceCalculatorFunc = baseResourceCalculatorDelegate,
            };

            parent.NextTest = test;
            return test;
        }
    }
}