namespace Turbo.Plugins.LM
{
    public class SecondaryResourcePercentageIsAboveTest1 : AbstractSkillTest1
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Skill.Player.Stats.ResourcePctSec >= PercentageLimit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SecondaryResourcePercentageIsAboveTestFluent
    {
        public static SecondaryResourcePercentageIsAboveTest1 IfSecondaryResourcePercentageIsAbove(this AbstractSkillTest1 parent, int percentageLimit)
        {
            var test = new SecondaryResourcePercentageIsAboveTest1()
            {
                PercentageLimit = percentageLimit
            };

            parent.NextTest = test;
            return test;
        }
    }
}