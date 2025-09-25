namespace Turbo.Plugins.LightningMod
{
    public class SecondaryResourcePercentageIsBelowTest : AbstractSkillTest
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return context.Skill.Player.Stats.ResourcePctSec >= PercentageLimit ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class SecondaryResourcePercentageIsBelowTestFluent
    {
        public static SecondaryResourcePercentageIsBelowTest IfSecondaryResourcePercentageIsBelow(this AbstractSkillTest parent, int percentageLimit)
        {
            var test = new SecondaryResourcePercentageIsBelowTest()
            {
                PercentageLimit = percentageLimit
            };

            parent.NextTest = test;
            return test;
        }
    }
}