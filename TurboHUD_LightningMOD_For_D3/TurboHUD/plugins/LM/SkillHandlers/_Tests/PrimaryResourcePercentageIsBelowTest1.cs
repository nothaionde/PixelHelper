namespace Turbo.Plugins.LM
{
    public class PrimaryResourcePercentageIsBelowTest1 : AbstractSkillTest1
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Skill.Player.Stats.ResourcePctPri >= PercentageLimit ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class PrimaryResourcePercentageIsBelowTestFluent
    {
        public static PrimaryResourcePercentageIsBelowTest1 IfPrimaryResourcePercentageIsBelow(this AbstractSkillTest1 parent, int percentageLimit)
        {
            var test = new PrimaryResourcePercentageIsBelowTest1()
            {
                PercentageLimit = percentageLimit,
            };

            parent.NextTest = test;
            return test;
        }
    }
}