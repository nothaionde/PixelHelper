namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class NearbyPartyMemberIsInDangerTest : AbstractSkillTest
    {
        public int Range { get; set; }
        public int LimitWhenPotionAvailable { get; set; }
        public int LimitWhenPotionOnCoolDown { get; set; }
        public int LimitWhenCantMove { get; set; }
        public bool TriggerOnInstantDeath { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            return context.Hud.Game.Players.Any(player =>
                !player.IsMe
                && player.HasValidActor
                && !player.IsDead
                && player.Defense.HealthPct >= 1
                && player.NormalizedXyDistanceToMe <= Range
                    && ((TriggerOnInstantDeath && player.AvoidablesInRange.Any(x => x.AvoidableDefinition.InstantDeath))
                     || (!player.Powers.HealthPotionSkill.IsOnCooldown && player.Defense.HealthPct < LimitWhenPotionAvailable)
                     || (player.Powers.HealthPotionSkill.IsOnCooldown && player.Defense.HealthPct < LimitWhenPotionOnCoolDown)
                     || (player.Powers.CantMove && player.Defense.HealthPct < LimitWhenCantMove)))
                ? ResultOnSuccess
                : ResultOnFail;
        }
    }

    public static class PartyMemberIsInDangerTestFluent
    {
        public static NearbyPartyMemberIsInDangerTest IfNearbyPartyMemberIsInDanger(this AbstractSkillTest parent, int range, int limitWhenPotionAvailable, int limitWhenPotionOnCooldown, int limitWhenCantMove, bool triggerOnInstantDeath)
        {
            var test = new NearbyPartyMemberIsInDangerTest()
            {
                Range = range,
                LimitWhenPotionAvailable = limitWhenPotionAvailable,
                LimitWhenPotionOnCoolDown = limitWhenPotionOnCooldown,
                LimitWhenCantMove = limitWhenCantMove,
                TriggerOnInstantDeath = triggerOnInstantDeath,
            };

            parent.NextTest = test;
            return test;
        }
    }
}