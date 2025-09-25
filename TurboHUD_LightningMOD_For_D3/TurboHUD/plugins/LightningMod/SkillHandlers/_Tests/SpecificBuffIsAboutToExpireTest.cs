namespace Turbo.Plugins.LightningMod
{
    public class SpecificBuffIsAboutToExpireTest : AbstractSkillTest
    {
        public ISnoPower SnoPower { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int IconIndex { get; set; }
        public bool ExcludeZero { get; set; }
        public int MaxStacks { get; set; }
        internal override SkillTestResult Test(TestContext context)
        {
            var limit = AbstractSkillHandler.ChangeRnd(context.Hud, "BuffIsAboutToExpire_" + context.Skill.SnoPower.Code, Min, Max, Max);
            if (limit < 0)
                limit = 0;

            var buff = context.Skill.Player.Powers.GetBuff(SnoPower.Sno);
            var remaining = buff?.Active == true ? buff.TimeLeftSeconds[IconIndex] : 0.0d;
            var stack = buff?.Active == true ? buff.IconCounts[IconIndex] : 0;
            return (remaining <= (limit / 1000.0f) && ((ExcludeZero && remaining > 0) || !ExcludeZero)) || (stack < MaxStacks && !ExcludeZero) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SpecificBuffIsAboutToExpireTestFluent
    {
        public static SpecificBuffIsAboutToExpireTest IfSpecificBuffIsAboutToExpire(this AbstractSkillTest parent, ISnoPower snoPower, int iconIndex, int min, int max, bool excludeZero = false, int maxStacks = 1)
        {
            var test = new SpecificBuffIsAboutToExpireTest()
            {
                SnoPower = snoPower,
                Min = min,
                Max = max,
                IconIndex = iconIndex,
                ExcludeZero = excludeZero,
                MaxStacks = maxStacks,
            };

            parent.NextTest = test;
            return test;
        }
    }
}