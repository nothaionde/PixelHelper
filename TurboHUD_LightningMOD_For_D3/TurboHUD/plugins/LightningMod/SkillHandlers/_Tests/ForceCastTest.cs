namespace Turbo.Plugins.LightningMod
{
    public class ForceCastTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return SkillTestResult.Cast;
        }
    }

    public static class ForceCastTestFluent
    {
        public static ForceCastTest ForceCast(this AbstractSkillTest parent)
        {
            var test = new ForceCastTest();

            parent.NextTest = test;
            return test;
        }
    }
}