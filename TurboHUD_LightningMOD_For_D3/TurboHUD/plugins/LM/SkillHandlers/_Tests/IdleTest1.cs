namespace Turbo.Plugins.LM
{
    public class IdleTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.Idle ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class IdleTestFluent
    {
        public static IdleTest1 IfIdle(this AbstractSkillTest1 parent)
        {
            var test = new IdleTest1();

            parent.NextTest = test;
            return test;
        }
    }
}