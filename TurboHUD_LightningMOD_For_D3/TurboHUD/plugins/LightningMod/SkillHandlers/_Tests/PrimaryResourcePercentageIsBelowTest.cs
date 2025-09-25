namespace Turbo.Plugins.LightningMod
{
    public class PrimaryResourcePercentageIsBelowTest : AbstractSkillTest
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return context.Skill.Player.Stats.ResourcePctPri >= PercentageLimit ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class PrimaryResourcePercentageIsBelowTestFluent
    {
        public static PrimaryResourcePercentageIsBelowTest IfPrimaryResourcePercentageIsBelow(this AbstractSkillTest parent, int percentageLimit)
        {
            var test = new PrimaryResourcePercentageIsBelowTest()
            {
                PercentageLimit = percentageLimit,
            };

            parent.NextTest = test;
            return test;
        }
    }
}