namespace Turbo.Plugins.LightningMod
{
    public class CanCastSimpleTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            var limit = AbstractSkillHandler.ChangeRnd(context.Hud, "CanCastSimple", 150, 250, 250);
            if (limit < 0)
                limit = 0;
            return context.LastSimpleCasted.TimerTest(limit) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CanCastSimpleTestFluent
    {
        public static CanCastSimpleTest IfCanCastSimple(this AbstractSkillTest parent)
        {
            var test = new CanCastSimpleTest();

            parent.NextTest = test;
            return test;
        }
    }
}