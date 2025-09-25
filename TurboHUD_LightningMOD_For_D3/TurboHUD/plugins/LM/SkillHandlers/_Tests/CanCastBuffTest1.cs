namespace Turbo.Plugins.LM
{
    public class CanCastBuffTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            var limit = AbstractSkillHandler1.ChangeRnd(context.Hud, "CanCastBuff", 400, 800, 300);
            if (limit < 0)
                limit = 0;
            return context.LastBuffCasted.TimerTest(limit) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CanCastBuffTestFluent
    {
        public static CanCastBuffTest1 IfCanCastBuff(this AbstractSkillTest1 parent)
        {
            var test = new CanCastBuffTest1();

            parent.NextTest = test;
            return test;
        }
    }
}