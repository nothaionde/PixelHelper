namespace Turbo.Plugins.LightningMod
{
    public class InTownTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.IsInTown ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class InTownTestFluent
    {
        public static InTownTest IfInTown(this AbstractSkillTest parent)
        {
            var test = new InTownTest();

            parent.NextTest = test;
            return test;
        }
    }
}