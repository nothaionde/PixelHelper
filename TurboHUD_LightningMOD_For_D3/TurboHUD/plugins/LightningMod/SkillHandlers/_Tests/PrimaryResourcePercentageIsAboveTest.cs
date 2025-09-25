namespace Turbo.Plugins.LightningMod
{
    public class PrimaryResourcePercentageIsAboveTest : AbstractSkillTest
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return context.Skill.Player.Stats.ResourcePctPri >= PercentageLimit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class PrimaryResourcePercentageIsAboveTestFluent
    {
        public static PrimaryResourcePercentageIsAboveTest IfPrimaryResourcePercentageIsAbove(this AbstractSkillTest parent, int percentageLimit)
        {
            var test = new PrimaryResourcePercentageIsAboveTest()
            {
                PercentageLimit = percentageLimit,
            };

            parent.NextTest = test;
            return test;
        }
    }
}