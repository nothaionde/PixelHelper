namespace Turbo.Plugins.LightningMod
{
    public class SpecificSkillOnCooldownTest : AbstractSkillTest
    {
        public ISnoPower SnoPower { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            var usedSkill = context.Skill.Player.Powers.GetUsedSkill(SnoPower);
            return usedSkill?.IsOnCooldown == true ? ResultOnSuccess : ResultOnFail;
        }
    }

    public static class SpecificSkillOnCooldownTestFluent
    {
        public static SpecificSkillOnCooldownTest IfSpecificSkillOnCooldown(this AbstractSkillTest parent, ISnoPower snoPower)
        {
            var test = new SpecificSkillOnCooldownTest()
            {
                SnoPower = snoPower,
            };

            parent.NextTest = test;
            return test;
        }
    }
}