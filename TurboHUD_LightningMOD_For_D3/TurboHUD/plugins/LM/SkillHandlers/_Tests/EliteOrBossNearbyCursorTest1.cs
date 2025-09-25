namespace Turbo.Plugins.LM
{
    using System;
    using System.Linq;

    public class EliteOrBossNearbyCursorTest1 : AbstractSkillTest1
    {
        public bool IncludeMinion { get; set; }
        public Func<TestContext1, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (RangeFunc == null) return SkillTestResult.Continue;

            var range = RangeFunc(context);
            IWorldCoordinate cursor = context.Hud.Window.CreateScreenCoordinate(context.Hud.Window.CursorX, context.Hud.Window.CursorY).ToWorldCoordinate();
                  bool Result = context.Hud.Game.AliveMonsters.Any(m =>((m.IsElite && (IncludeMinion ? true : m.Rarity != ActorRarity.RareMinion)) || m.SnoMonster.Priority == MonsterPriority.goblin) && (m.FloorCoordinate.XYZDistanceTo(cursor) - m.RadiusBottom) <= range && !m.Invulnerable && !m.Invisible && !m.Illusion);
            return Result ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class IsEliteOrBossNearbyCursorTestFluent
    {
        public static EliteOrBossNearbyCursorTest1 IfEliteOrBossNearbyCursor(this AbstractSkillTest1 parent, Func<TestContext1, int> rangeFunc, bool includeMinion = false)
        {
            var test = new EliteOrBossNearbyCursorTest1()
            {
                RangeFunc = rangeFunc,
                IncludeMinion = includeMinion,
            };

            parent.NextTest = test;
            return test;
        }
    }
}