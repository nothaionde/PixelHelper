namespace Turbo.Plugins.LM
{
    public class ForceCastTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            return SkillTestResult.Cast;
        }
    }

    public static class ForceCastTestFluent
    {
        public static ForceCastTest1 ForceCast(this AbstractSkillTest1 parent)
        {
            var test = new ForceCastTest1();

            parent.NextTest = test;
            return test;
        }
    }
}