namespace Turbo.Plugins.LM
{
    public class SpecificSkillOnCooldownTest1 : AbstractSkillTest1
    {
        public ISnoPower SnoPower { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
        {
            var usedSkill = context.Skill.Player.Powers.GetUsedSkill(SnoPower);
            return usedSkill?.IsOnCooldown == true ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SpecificSkillOnCooldownTestFluent
    {
        public static SpecificSkillOnCooldownTest1 IfSpecificSkillOnCooldown(this AbstractSkillTest1 parent, ISnoPower snoPower)
        {
            var test = new SpecificSkillOnCooldownTest1()
            {
                SnoPower = snoPower,
            };

            parent.NextTest = test;
            return test;
        }
    }
}