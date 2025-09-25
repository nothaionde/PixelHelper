namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class LevelingNearbyPartyMemberTest : AbstractSkillTest
    {
        public int Range { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return context.Skill.Player.CurrentLevelNormal == context.Skill.Player.CurrentLevelNormalCap
                && context.Hud.Game.Players.Any(player =>
                !player.IsMe
                && player.AcdId != 0
                && !player.Powers.BuffIsActive(224639)//灵魂状态
                && !player.IsDead
                && player.Defense.HealthPct >= 1
                && player.NormalizedXyDistanceToMe <= Range
                && player.CurrentLevelNormal < player.CurrentLevelNormalCap)
                ? ResultOnSuccess
                : ResultOnFail;
        }
    }

    public static class LevelingNearbyPartyMemberTestFluent
    {
        public static LevelingNearbyPartyMemberTest IfLevelingNearbyPartyMember(this AbstractSkillTest parent, int range)
        {
            var test = new LevelingNearbyPartyMemberTest()
            {
                Range = range,
            };

            parent.NextTest = test;
            return test;
        }
    }
}