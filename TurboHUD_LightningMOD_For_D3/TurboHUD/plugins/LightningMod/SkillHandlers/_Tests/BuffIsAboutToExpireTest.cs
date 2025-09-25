namespace Turbo.Plugins.LightningMod
{
    public class BuffIsAboutToExpireTest : AbstractSkillTest
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int IconIndex { get; set; }
        public bool ExcludeZero { get; set; }
        internal override SkillTestResult Test(TestContext context)
        {
            var limit = AbstractSkillHandler.ChangeRnd(context.Hud, "BuffIsAboutToExpire_" + context.Skill.SnoPower.Code, Min, Max, Max);
            if (limit < 0)
                limit = 0;
            var remaining = IconIndex == -1 ? context.Skill.RemainingBuffTime() : context.Skill.RemainingBuffTime(IconIndex);
            return remaining <= (limit / 1000.0f) && ((ExcludeZero && remaining > 0) || !ExcludeZero) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class BuffIsAboutToExpireTestFluent
    {
        public static BuffIsAboutToExpireTest IfBuffIsAboutToExpire(this AbstractSkillTest parent, int min, int max, int iconIndex = -1, bool excludeZero = false)
        {
            var test = new BuffIsAboutToExpireTest()
            {
                Min = min,
                Max = max,
                IconIndex = iconIndex,
                ExcludeZero = excludeZero,
            };

            parent.NextTest = test;
            return test;
        }
    }
}