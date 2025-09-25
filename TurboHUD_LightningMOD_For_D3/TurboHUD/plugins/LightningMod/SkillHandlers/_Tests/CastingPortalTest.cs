namespace Turbo.Plugins.LightningMod
{
    public class CastingPortalTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CastingTownPortalTestFluent
    {
        public static CastingPortalTest IfCastingPortal(this AbstractSkillTest parent)
        {
            var test = new CastingPortalTest();

            parent.NextTest = test;
            return test;
        }
    }
}