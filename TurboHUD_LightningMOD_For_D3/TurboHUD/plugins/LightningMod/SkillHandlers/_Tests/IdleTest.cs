namespace Turbo.Plugins.LightningMod
{
    public class IdleTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.Idle ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class IdleTestFluent
    {
        public static IdleTest IfIdle(this AbstractSkillTest parent)
        {
            var test = new IdleTest();

            parent.NextTest = test;
            return test;
        }
    }
}