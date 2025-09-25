namespace Turbo.Plugins.LightningMod
{
    public class CanCastBuffTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            var limit = AbstractSkillHandler.ChangeRnd(context.Hud, "CanCastBuff", 400, 800, 300);
            if (limit < 0)
                limit = 0;
            return context.LastBuffCasted.TimerTest(limit) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CanCastBuffTestFluent
    {
        public static CanCastBuffTest IfCanCastBuff(this AbstractSkillTest parent)
        {
            var test = new CanCastBuffTest();

            parent.NextTest = test;
            return test;
        }
    }
}