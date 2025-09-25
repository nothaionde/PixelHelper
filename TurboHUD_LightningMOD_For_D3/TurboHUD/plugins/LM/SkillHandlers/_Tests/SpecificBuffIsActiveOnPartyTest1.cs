using System.Linq;

namespace Turbo.Plugins.LM
{
    public class SpecificBuffIsActiveOnPartyTest1 : AbstractSkillTest1
    {
        public ISnoPower SnoPower { get; set; }
        public int IconIndex { get; set; }
        public HeroClass HeroClass { get; set; }
        public double Range { get; set; }

        internal override SkillTestResult Test(TestContext1 context)
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
        public static SpecificBuffIsActiveOnPartyTest1 IfSpecificBuffIsActiveOnParty(this AbstractSkillTest1 parent, ISnoPower snoPower, int iconIndex, HeroClass heroClass, double range)
        {
            var test = new SpecificBuffIsActiveOnPartyTest1()
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