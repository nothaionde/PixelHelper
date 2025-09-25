namespace Turbo.Plugins.LM
{
    public class AttackingTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.Attacking ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class AttackingTestFluent
    {
        public static AttackingTest1 IfAttacking(this AbstractSkillTest1 parent)
        {
            var test = new AttackingTest1();

            parent.NextTest = test;
            return test;
        }
    }
}