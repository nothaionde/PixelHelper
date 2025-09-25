namespace Turbo.Plugins.LM
{
    public class CanCastSimpleTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            var limit = AbstractSkillHandler1.ChangeRnd(context.Hud, "CanCastSimple", 150, 250, 250);
            if (limit < 0)
                limit = 0;
            return context.LastSimpleCasted.TimerTest(limit) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CanCastSimpleTestFluent
    {
        public static CanCastSimpleTest1 IfCanCastSimple(this AbstractSkillTest1 parent)
        {
            var test = new CanCastSimpleTest1();

            parent.NextTest = test;
            return test;
        }
    }
}