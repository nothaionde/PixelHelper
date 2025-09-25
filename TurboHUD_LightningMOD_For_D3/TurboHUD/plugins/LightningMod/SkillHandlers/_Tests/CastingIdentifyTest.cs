namespace Turbo.Plugins.LightningMod
{
    public class CastingIdentifyTest : AbstractSkillTest
    {
        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.Me.Powers.BuffIsActive(context.Hud.Sno.SnoPowers.Generic_IdentifyAllWithCast.Sno)
                || context.Hud.Game.Me.Powers.BuffIsActive(context.Hud.Sno.SnoPowers.Generic_IdentifyWithCast.Sno)
                || context.Hud.Game.Me.Powers.BuffIsActive(context.Hud.Sno.SnoPowers.Generic_IdentifyWithCastLegendary.Sno)
                ? ResultOnSuccess
                : ResultOnFail;
        }
    }

    public static class CastingIdentifyTestFluent
    {
        public static CastingIdentifyTest IfCastingIdentify(this AbstractSkillTest parent)
        {
            var test = new CastingIdentifyTest();

            parent.NextTest = test;
            return test;
        }
    }
}