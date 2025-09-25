namespace Turbo.Plugins.LightningMod
{
    using System;

    public class BossNearbyTest : AbstractSkillTest
    {
        public Func<TestContext, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var range = RangeFunc(context);
            return context.Hud.Game.ActorQuery.NearestBoss != null && context.Hud.Game.ActorQuery.NearestBoss.NormalizedXyDistanceToMe <= range ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class BossNearbyTestFluent
    {
        public static BossNearbyTest IfBossIsNearby(this AbstractSkillTest parent, Func<TestContext, int> rangeFunc)
        {
            var test = new BossNearbyTest()
            {
                RangeFunc = rangeFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}