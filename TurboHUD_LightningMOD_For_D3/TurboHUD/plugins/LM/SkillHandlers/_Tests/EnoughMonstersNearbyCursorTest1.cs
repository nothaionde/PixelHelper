namespace Turbo.Plugins.LM
{
    using System;
    using System.Linq;

    public class EnoughMonstersNearbyCursorTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, int> RangeFunc { get; set; }
        public Func<TestContext1, int> MonsterCountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (RangeFunc == null || MonsterCountFunc == null) return SkillTestResult.Continue;

            var range = RangeFunc(context);
            var limit = MonsterCountFunc(context);
            IWorldCoordinate cursor = context.Hud.Window.CreateScreenCoordinate(context.Hud.Window.CursorX, context.Hud.Window.CursorY).ToWorldCoordinate();
            var density = context.Hud.Game.Monsters.Count(m => (m.FloorCoordinate.XYZDistanceTo(cursor) - m.RadiusBottom) <= range && m.IsAlive && !m.Invulnerable && !m.Invisible);
            return density >= limit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class IsEnoughMonstersNearbyCursorTestFluent
    {
        public static EnoughMonstersNearbyCursorTest1 IfEnoughMonstersNearbyCursor(this AbstractSkillTest1 parent, Func<TestContext1, int> rangeFunc, Func<TestContext1, int> monsterCountFunc)
        {
            var test = new EnoughMonstersNearbyCursorTest1()
            {
                RangeFunc = rangeFunc,
                MonsterCountFunc = monsterCountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}