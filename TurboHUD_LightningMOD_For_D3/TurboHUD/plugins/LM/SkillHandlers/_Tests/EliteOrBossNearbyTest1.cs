namespace Turbo.Plugins.LM
{
    using System;

    public class EliteOrBossNearbyTest1 : AbstractSkillTest1
    {
		public bool IncludeMinion { get; set; }
        public Func<TestContext1, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
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
        public static EliteOrBossNearbyTest1 IfEliteOrBossIsNearby(this AbstractSkillTest1 parent, Func<TestContext1, int> rangeFunc, bool includeMinion = false)
        {
            var test = new EliteOrBossNearbyTest1()
            {
                RangeFunc = rangeFunc,
                IncludeMinion = includeMinion,
            };

            parent.NextTest = test;
            return test;
        }
    }
}