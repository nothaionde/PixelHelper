namespace Turbo.Plugins.LM
{
    public class SecondaryResourcePercentageIsBelowTest1 : AbstractSkillTest1
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Skill.Player.Stats.ResourcePctSec >= PercentageLimit ? ResultOnFail : ResultOnSuccess;
        }
    }

    public static class SecondaryResourcePercentageIsBelowTestFluent
    {
        public static SecondaryResourcePercentageIsBelowTest1 IfSecondaryResourcePercentageIsBelow(this AbstractSkillTest1 parent, int percentageLimit)
        {
            var test = new SecondaryResourcePercentageIsBelowTest1()
            {
                PercentageLimit = percentageLimit
            };

            parent.NextTest = test;
            return test;
        }
    }
}