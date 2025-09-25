namespace Turbo.Plugins.LightningMod
{
    public class SpecificBuffIsActiveTest : AbstractSkillTest
    {
        public ISnoPower SnoPower { get; set; }
        public int IconIndex { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return (IconIndex == -1 ? context.Skill.Player.Powers.BuffIsActive(SnoPower.Sno) : context.Skill.Player.Powers.BuffIsActive(SnoPower.Sno, IconIndex)) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SpecificBuffIsActiveTestFluent
    {
        public static SpecificBuffIsActiveTest IfSpecificBuffIsActive(this AbstractSkillTest parent, ISnoPower snoPower, int iconIndex = -1)
        {
            var test = new SpecificBuffIsActiveTest()
            {
                SnoPower = snoPower,
                IconIndex = iconIndex,
            };

            parent.NextTest = test;
            return test;
        }
    }
}