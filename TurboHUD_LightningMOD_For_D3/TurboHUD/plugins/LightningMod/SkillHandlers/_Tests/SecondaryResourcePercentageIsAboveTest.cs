namespace Turbo.Plugins.LightningMod
{
    public class SecondaryResourcePercentageIsAboveTest : AbstractSkillTest
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return context.Skill.Player.Stats.ResourcePctSec >= PercentageLimit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SecondaryResourcePercentageIsAboveTestFluent
    {
        public static SecondaryResourcePercentageIsAboveTest IfSecondaryResourcePercentageIsAbove(this AbstractSkillTest parent, int percentageLimit)
        {
            var test = new SecondaryResourcePercentageIsAboveTest()
            {
                PercentageLimit = percentageLimit
            };

            parent.NextTest = test;
            return test;
        }
    }
}