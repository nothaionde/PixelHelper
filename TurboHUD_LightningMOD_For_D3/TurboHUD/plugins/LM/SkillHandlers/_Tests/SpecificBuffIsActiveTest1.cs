namespace Turbo.Plugins.LM
{
    public class SpecificBuffIsActiveTest1 : AbstractSkillTest1
    {
        public ISnoPower SnoPower { get; set; }
        public int IconIndex { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            return (IconIndex == -1 ? context.Skill.Player.Powers.BuffIsActive(SnoPower.Sno) : context.Skill.Player.Powers.BuffIsActive(SnoPower.Sno, IconIndex)) ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SpecificBuffIsActiveTestFluent
    {
        public static SpecificBuffIsActiveTest1 IfSpecificBuffIsActive(this AbstractSkillTest1 parent, ISnoPower snoPower, int iconIndex = -1)
        {
            var test = new SpecificBuffIsActiveTest1()
            {
                SnoPower = snoPower,
                IconIndex = iconIndex,
            };

            parent.NextTest = test;
            return test;
        }
    }
}