namespace Turbo.Plugins.LM
{
    public class PrimaryResourcePercentageIsAboveTest1 : AbstractSkillTest1
    {
        public int PercentageLimit { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Skill.Player.Stats.ResourcePctPri >= PercentageLimit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class PrimaryResourcePercentageIsAboveTestFluent
    {
        public static PrimaryResourcePercentageIsAboveTest1 IfPrimaryResourcePercentageIsAbove(this AbstractSkillTest1 parent, int percentageLimit)
        {
            var test = new PrimaryResourcePercentageIsAboveTest1()
            {
                PercentageLimit = percentageLimit,
            };

            parent.NextTest = test;
            return test;
        }
    }
}