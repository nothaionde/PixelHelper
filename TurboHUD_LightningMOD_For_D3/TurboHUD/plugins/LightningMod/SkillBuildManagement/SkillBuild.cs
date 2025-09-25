namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;

    public class SkillBuild
    {
        public Dictionary<ActionKey, SkillBuildSkillWithRune> Skills { get; set; }
        public ISnoPower[] Passives { get; }

        public SkillBuild()
        {
            Passives = new ISnoPower[4];
            Skills = new Dictionary<ActionKey, SkillBuildSkillWithRune>();
        }
    }

    public class SkillBuildSkillWithRune
    {
        public ISnoPower SnoPower { get; set; }
        public string RuneLocalizedName { get; set; }

        public SkillBuildSkillWithRune(ISnoPower snoPower, string runeLocalizedName)
        {
            SnoPower = snoPower;
            RuneLocalizedName = runeLocalizedName;
        }
    }
}