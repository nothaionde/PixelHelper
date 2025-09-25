namespace Turbo.Plugins.LM
{
    public class InTownTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Hud.Game.IsInTown ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class InTownTestFluent
    {
        public static InTownTest1 IfInTown(this AbstractSkillTest1 parent)
        {
            var test = new InTownTest1();

            parent.NextTest = test;
            return test;
        }
    }
}