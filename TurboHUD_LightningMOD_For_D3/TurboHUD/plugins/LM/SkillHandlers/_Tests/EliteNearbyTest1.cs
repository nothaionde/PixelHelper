namespace Turbo.Plugins.LM
{
    using System;

    public class EliteIsNearbyTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var range = RangeFunc(context);
            return (context.Hud.Game.ActorQuery.NearestEliteButNoMinion != null && context.Hud.Game.ActorQuery.NearestEliteButNoMinion.NormalizedXyDistanceToMe <= range)
                || (context.Hud.Game.ActorQuery.NearestGoblin != null && context.Hud.Game.ActorQuery.NearestGoblin.NormalizedXyDistanceToMe <= range)
                ? ResultOnSuccess
                : ResultOnFail;
        }
    }

    public static class EliteIsNearbyTestFluent
    {
        public static EliteIsNearbyTest1 IfEliteIsNearby(this AbstractSkillTest1 parent, Func<TestContext1, int> rangeFunc)
        {
            var test = new EliteIsNearbyTest1()
            {
                RangeFunc = rangeFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}