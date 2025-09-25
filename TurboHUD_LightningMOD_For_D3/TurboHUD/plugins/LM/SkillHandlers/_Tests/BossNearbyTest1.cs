namespace Turbo.Plugins.LM
{
    using System;

    public class BossNearbyTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var range = RangeFunc(context);
            return context.Hud.Game.ActorQuery.NearestBoss != null && context.Hud.Game.ActorQuery.NearestBoss.NormalizedXyDistanceToMe <= range ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class BossNearbyTestFluent
    {
        public static BossNearbyTest1 IfBossIsNearby(this AbstractSkillTest1 parent, Func<TestContext1, int> rangeFunc)
        {
            var test = new BossNearbyTest1()
            {
                RangeFunc = rangeFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}