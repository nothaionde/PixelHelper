namespace Turbo.Plugins.LM
{
    using System;

    public class PrimaryResourceRequirementTest1 : AbstractSkillTest1
    {
        public int SpareResource { get; set; }
        public Func<TestContext1, int> BaseResourceCalculatorFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (BaseResourceCalculatorFunc == null) return SkillTestResult.Continue;
            var resourceRequirement = this.BaseResourceCalculatorFunc(context) == 0 ? context.Skill.GetResourceRequirement() : context.Skill.GetResourceRequirement(this.BaseResourceCalculatorFunc(context));
            resourceRequirement += this.SpareResource;

            if (context.Skill.Player.Stats.ResourceCurPri < resourceRequirement)
            {
                return ResultOnFail;
            }
            else return ResultOnSuccess;
        }
    }

    public static class PrimaryResourceRequirementTestFluent
    {
        public static PrimaryResourceRequirementTest1 IfPrimaryResourceIsEnough(this AbstractSkillTest1 parent, int spareResource, Func<TestContext1, int> baseResourceCalculatorFunc)
        {
            var test = new PrimaryResourceRequirementTest1()
            {
                SpareResource = spareResource,
                BaseResourceCalculatorFunc = baseResourceCalculatorFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}