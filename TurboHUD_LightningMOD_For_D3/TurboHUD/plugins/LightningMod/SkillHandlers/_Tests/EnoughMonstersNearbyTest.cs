namespace Turbo.Plugins.LightningMod
{
    using System;

    public class EnoughMonstersNearbyTest : AbstractSkillTest
    {
        public Func<TestContext, int> RangeFunc { get; set; }
        public Func<TestContext, int> MonsterCountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (RangeFunc == null || MonsterCountFunc == null) return SkillTestResult.Continue;

            var range = RangeFunc(context);
            var limit = MonsterCountFunc(context);

            var density = context.Skill.Player.Density.GetDensity(range);
            return density >= limit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class IsEnoughMonstersNearbyTestFluent
    {
        public static EnoughMonstersNearbyTest IfEnoughMonstersNearby(this AbstractSkillTest parent, Func<TestContext, int> rangeFunc, Func<TestContext, int> monsterCountFunc)
        {
            var test = new EnoughMonstersNearbyTest()
            {
                RangeFunc = rangeFunc,
                MonsterCountFunc = monsterCountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}