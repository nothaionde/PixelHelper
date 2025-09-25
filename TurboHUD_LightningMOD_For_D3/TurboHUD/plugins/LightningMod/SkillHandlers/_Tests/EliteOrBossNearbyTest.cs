namespace Turbo.Plugins.LightningMod
{
    using System;

    public class EliteOrBossNearbyTest : AbstractSkillTest
    {
		public bool IncludeMinion { get; set; }
        public Func<TestContext, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var range = RangeFunc(context);
            return context.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(range, IncludeMinion)
                || (context.Hud.Game.ActorQuery.NearestGoblin != null && context.Hud.Game.ActorQuery.NearestGoblin.NormalizedXyDistanceToMe <= range)
                ? ResultOnSuccess
                : ResultOnFail;
        }
    }

    public static class IsEliteOrBossNearbyTestFluent
    {
        public static EliteOrBossNearbyTest IfEliteOrBossIsNearby(this AbstractSkillTest parent, Func<TestContext, int> rangeFunc, bool includeMinion = false)
        {
            var test = new EliteOrBossNearbyTest()
            {
                RangeFunc = rangeFunc,
                IncludeMinion = includeMinion,
            };

            parent.NextTest = test;
            return test;
        }
    }
}