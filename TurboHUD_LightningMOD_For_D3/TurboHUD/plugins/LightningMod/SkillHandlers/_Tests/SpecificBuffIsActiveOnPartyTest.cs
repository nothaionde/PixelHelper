using System.Linq;

namespace Turbo.Plugins.LightningMod
{
    public class SpecificBuffIsActiveOnPartyTest : AbstractSkillTest
    {
        public ISnoPower SnoPower { get; set; }
        public int IconIndex { get; set; }
        public HeroClass HeroClass { get; set; }
        public double Range { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (context.Hud.Game.NumberOfPlayersInGame <= 1)
                return ResultOnFail;
            var otherPlayersAround = context.Hud.Game.Players.Where(
                x => !x.IsMe
                && x.HasValidActor
                && (HeroClass == HeroClass.All || x.HeroClassDefinition.HeroClass == HeroClass)
                && x.CentralXyDistanceToMe <= Range);

            foreach (var player in otherPlayersAround)
            {
                if (player.Powers.BuffIsActive(SnoPower.Sno, IconIndex))
                {
                    return ResultOnSuccess;
                }
                else
                {
                    return ResultOnFail;
                }
            }

            return ResultOnFail;
        }
    }

    public static class SpecificBuffIsActiveOnPartyTestFluent
    {
        public static SpecificBuffIsActiveOnPartyTest IfSpecificBuffIsActiveOnParty(this AbstractSkillTest parent, ISnoPower snoPower, int iconIndex, HeroClass heroClass, double range)
        {
            var test = new SpecificBuffIsActiveOnPartyTest()
            {
                SnoPower = snoPower,
                IconIndex = iconIndex,
                HeroClass = heroClass,
                Range = range,
            };

            parent.NextTest = test;
            return test;
        }
    }
}