namespace Turbo.Plugins.LM
{
    using System;

    public class EnoughMonstersNearbyTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> RangeFunc { get; set; }
        public Func<TestContext1, int> MonsterCountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
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
        public static EnoughMonstersNearbyTest1 IfEnoughMonstersNearby(this AbstractSkillTest1 parent, Func<TestContext1, int> rangeFunc, Func<TestContext1, int> monsterCountFunc)
        {
            var test = new EnoughMonstersNearbyTest1()
            {
                RangeFunc = rangeFunc,
                MonsterCountFunc = monsterCountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}