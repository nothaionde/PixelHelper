namespace Turbo.Plugins.LightningMod
{
    using System;

    public class EliteIsNearbyTest : AbstractSkillTest
    {
        public Func<TestContext, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
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
        public static EliteIsNearbyTest IfEliteIsNearby(this AbstractSkillTest parent, Func<TestContext, int> rangeFunc)
        {
            var test = new EliteIsNearbyTest()
            {
                RangeFunc = rangeFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}