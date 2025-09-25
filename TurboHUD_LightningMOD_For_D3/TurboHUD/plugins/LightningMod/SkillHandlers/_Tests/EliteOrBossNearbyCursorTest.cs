namespace Turbo.Plugins.LightningMod
{
    using System;
    using System.Linq;

    public class EliteOrBossNearbyCursorTest : AbstractSkillTest
    {
        public bool IncludeMinion { get; set; }
        public Func<TestContext, int> RangeFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
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
        public static EliteOrBossNearbyCursorTest IfEliteOrBossNearbyCursor(this AbstractSkillTest parent, Func<TestContext, int> rangeFunc, bool includeMinion = false)
        {
            var test = new EliteOrBossNearbyCursorTest()
            {
                RangeFunc = rangeFunc,
                IncludeMinion = includeMinion,
            };

            parent.NextTest = test;
            return test;
        }
    }
}