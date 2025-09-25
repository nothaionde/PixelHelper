namespace Turbo.Plugins.LM
{
    public class CastingPortalTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CastingTownPortalTestFluent
    {
        public static CastingPortalTest1 IfCastingPortal(this AbstractSkillTest1 parent)
        {
            var test = new CastingPortalTest1();

            parent.NextTest = test;
            return test;
        }
    }
}