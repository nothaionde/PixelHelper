namespace Turbo.Plugins.LightningMod
{
    public class AttackingTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.Attacking ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class AttackingTestFluent
    {
        public static AttackingTest IfAttacking(this AbstractSkillTest parent)
        {
            var test = new AttackingTest();

            parent.NextTest = test;
            return test;
        }
    }
}