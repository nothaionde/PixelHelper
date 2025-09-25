namespace Turbo.Plugins.LM
{
    using System;
    using System.Linq;
    using Turbo.Plugins.glq;

    public class EnoughMonstersInSectorTest1 : AbstractSkillTest1
    {
        public Func<TestContext1, float> SkillWidthFunc { get; set; }
        public Func<TestContext1, float> SkillRangeFunc { get; set; }
        public Func<TestContext1, float> CursorZoffsetFunc { get; set; }
        
        public Func<TestContext1, int> MonsterCountFunc { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            if (SkillWidthFunc == null || SkillRangeFunc == null || MonsterCountFunc == null) return SkillTestResult.Continue;

            var width = SkillWidthFunc(context);
            var range = SkillRangeFunc(context);
            var cursorzoffset = CursorZoffsetFunc(context);
            var limit = MonsterCountFunc(context);
            var density = context.Hud.Game.Monsters.Count(m => m.IsAlive && !m.Invulnerable && !m.Invisible && PublicClassPlugin.isMobInSkillRange(context.Hud, m, width, range, cursorzoffset));
            return density >= limit ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class IsEnoughMonstersInSectorTestFluent
    {
        public static EnoughMonstersInSectorTest1 IfEnoughMonstersInSector(this AbstractSkillTest1 parent, Func<TestContext1, float> skillWithFunc, Func<TestContext1, float> skillRangeFunc, Func<TestContext1, float> cursorZoffsetFunc, Func<TestContext1, int> monsterCountFunc)
        {
            var test = new EnoughMonstersInSectorTest1()
            {
                SkillWidthFunc = skillWithFunc,
                SkillRangeFunc = skillRangeFunc,
                MonsterCountFunc = monsterCountFunc,
                CursorZoffsetFunc = cursorZoffsetFunc,
            };

            parent.NextTest = test;
            return test;
        }
    }
}