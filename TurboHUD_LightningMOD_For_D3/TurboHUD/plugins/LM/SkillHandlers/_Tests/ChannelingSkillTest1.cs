namespace Turbo.Plugins.LM
{
    using System.Collections.Generic;

    public class ChannelingSkillTest1 : AbstractSkillTest1
    {
        public List<ISnoPower> Skills { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
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
        public static ChannelingSkillTest1 IfChannelingSkillTest(this AbstractSkillTest1 parent, List<ISnoPower> skills)
        {
            var test = new ChannelingSkillTest1()
            {
                Skills = skills,
            };

            parent.NextTest = test;
            return test;
        }
    }
}