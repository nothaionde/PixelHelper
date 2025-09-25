namespace Turbo.Plugins.LM
{
    public class CastingIdentifyTest1 : AbstractSkillTest1
    {
        internal override SkillTestResult Test(TestContext1 context)
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
        public static CastingIdentifyTest1 IfCastingIdentify(this AbstractSkillTest1 parent)
        {
            var test = new CastingIdentifyTest1();

            parent.NextTest = test;
            return test;
        }
    }
}