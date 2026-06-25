namespace Turbo.Plugins.LightningMod
{
    using System;
    using System.Linq;
    using Turbo.Plugins.glq;

    public class EnoughMonstersInSectorTest : AbstractSkillTest
    {
        public Func<TestContext, float> SkillWidthFunc { get; set; }
        public Func<TestContext, float> SkillRangeFunc { get; set; }
        public Func<TestContext, float> CursorZoffsetFunc { get; set; }
        
        public Func<TestContext, int> MonsterCountFunc { get; set; }

        internal override SkillTestResult Test(TestContext context)
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
        public static EnoughMonstersInSectorTest IfEnoughMonstersInSector(this AbstractSkillTest parent, Func<TestContext, float> skillWithFunc, Func<TestContext, float> skillRangeFunc, Func<TestContext, float> cursorZoffsetFunc, Func<TestContext, int> monsterCountFunc)
        {
            var test = new EnoughMonstersInSectorTest()
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