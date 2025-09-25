namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;

    public class ChannelingSkillTest : AbstractSkillTest
    {
        public List<ISnoPower> Skills { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            foreach (var otherPower in Skills)
            {
                var otherSkill = context.Skill.Player.Powers.GetUsedSkill(otherPower);
                if (otherSkill == null) continue;

                if (context.Hud.Interaction.IsContinuousActionStarted(otherSkill.Key))
                {
                    return ResultOnSuccess;
                }
            }

            return ResultOnFail;
        }
    }

    public static class ChannelingSkillTestFluent
    {
        public static ChannelingSkillTest IfChannelingSkillTest(this AbstractSkillTest parent, List<ISnoPower> skills)
        {
            var test = new ChannelingSkillTest()
            {
                Skills = skills,
            };

            parent.NextTest = test;
            return test;
        }
    }
}