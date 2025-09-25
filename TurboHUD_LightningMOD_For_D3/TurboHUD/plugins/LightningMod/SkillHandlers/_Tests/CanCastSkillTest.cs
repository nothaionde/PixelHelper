using System;

namespace Turbo.Plugins.LightningMod
{
    public class CanCastSkillTest : AbstractSkillTest
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int ChangeAfter { get; set; }
        public Func<TestContext, double> Multiplier { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var multiplier = Multiplier != null ? Multiplier(context) : 1.0d;
            var min = Convert.ToInt32(Min * multiplier);
            var max = Convert.ToInt32(Max * multiplier);
            var changeAfter = Convert.ToInt32(ChangeAfter * multiplier);

            var limit = min != max ? AbstractSkillHandler.ChangeRnd(context.Hud, "SkillCastDelayTest_" + context.Skill.SnoPower.Code, min, max, changeAfter) : min;
            if (limit < 0)
                limit = 0;
            return context.Skill.LastUsed.TimerTest(limit) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CanCastSkillTestFluent
    {
        public static CanCastSkillTest IfCanCastSkill(this AbstractSkillTest parent, int min, int max, int changeAfter, Func<TestContext, double> multiplier = null)
        {
            var test = new CanCastSkillTest()
            {
                Min = min,
                Max = max,
                ChangeAfter = changeAfter,
                Multiplier = multiplier,
            };

            parent.NextTest = test;
            return test;
        }
    }
}