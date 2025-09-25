namespace Turbo.Plugins.LightningMod
{
    using System;
    using System.Linq;

    public class EnoughMonstersNearbyCursorTest : AbstractSkillTest
    {
        public Func<TestContext, int> RangeFunc { get; set; }
        public Func<TestContext, int> MonsterCountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
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
        public static EnoughMonstersNearbyCursorTest IfEnoughMonstersNearbyCursor(this AbstractSkillTest parent, Func<TestContext, int> rangeFunc, Func<TestContext, int> monsterCountFunc)
        {
            var test = new EnoughMonstersNearbyCursorTest()
            {
                RangeFunc = rangeFunc,
                MonsterCountFunc = monsterCountFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}