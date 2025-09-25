namespace Turbo.Plugins.LM
{
    using System.Linq;

    public class LevelingNearbyPartyMemberTest1 : AbstractSkillTest1
    {
        public int Range { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
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
        public static LevelingNearbyPartyMemberTest1 IfLevelingNearbyPartyMember(this AbstractSkillTest1 parent, int range)
        {
            var test = new LevelingNearbyPartyMemberTest1()
            {
                Range = range,
            };

            parent.NextTest = test;
            return test;
        }
    }
}