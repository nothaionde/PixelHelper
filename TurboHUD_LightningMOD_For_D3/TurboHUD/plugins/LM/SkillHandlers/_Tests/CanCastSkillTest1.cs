using System;

namespace Turbo.Plugins.LM
{
    public class CanCastSkillTest1 : AbstractSkillTest1
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int ChangeAfter { get; set; }
        public Func<TestContext1, double> Multiplier { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var multiplier = Multiplier != null ? Multiplier(context) : 1.0d;
            var min = Convert.ToInt32(Min * multiplier);
            var max = Convert.ToInt32(Max * multiplier);
            var changeAfter = Convert.ToInt32(ChangeAfter * multiplier);

            var limit = min != max ? AbstractSkillHandler1.ChangeRnd(context.Hud, "SkillCastDelayTest_" + context.Skill.SnoPower.Code, min, max, changeAfter) : min;
            if (limit < 0)
                limit = 0;
            return context.Skill.LastUsed.TimerTest(limit) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class CanCastSkillTestFluent
    {
        public static CanCastSkillTest1 IfCanCastSkill(this AbstractSkillTest1 parent, int min, int max, int changeAfter, Func<TestContext1, double> multiplier = null)
        {
            var test = new CanCastSkillTest1()
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