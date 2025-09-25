using System.Linq;

namespace Turbo.Plugins.LightningMod
{
    public class SpecificBuffIsAboutToExpireOnPartyTest : AbstractSkillTest
    {
        public ISnoPower SnoPower { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int IconIndex { get; set; }
        public HeroClass HeroClass { get; set; }
        public double Range { get; set; }

        internal override SkillTestResult Test(TestContext context)
        {
            if (context.Hud.Game.NumberOfPlayersInGame <= 1)
                return ResultOnFail;

            var limit = AbstractSkillHandler.ChangeRnd(context.Hud, "BuffIsAboutToExpire_" + context.Skill.SnoPower.Code, Min, Max, Max);
            if (limit < 0)
                limit = 0;

            var otherPlayersAround = context.Hud.Game.Players.Where(
                x => !x.IsMe
                && !x.IsDead 
                && !x.Powers.BuffIsActive(224639)//灵魂状态
                && x.HasValidActor
                && (HeroClass == HeroClass.All || x.HeroClassDefinition.HeroClass == HeroClass)
                && x.CentralXyDistanceToMe <= Range);

            foreach (var player in otherPlayersAround)
            {
                var buff = player.Powers.GetBuff(SnoPower.Sno);
                if (buff?.Active == true)
                {
                    var remaining = buff.TimeLeftSeconds[IconIndex];
                    if (remaining <= (limit / 1000.0f))
                    {
                        return ResultOnSuccess;
                    }
                }
                else
                {
                    return ResultOnSuccess;
                }
            }

            return ResultOnFail;
        }
    }

    public static class SpecificBuffIsAboutToExpireOnPartyTestFluent
    {
        public static SpecificBuffIsAboutToExpireOnPartyTest IfSpecificBuffIsAboutToExpireOnParty(this AbstractSkillTest parent, ISnoPower snoPower, int iconIndex, int min, int max, HeroClass heroClass, double range)
        {
            var test = new SpecificBuffIsAboutToExpireOnPartyTest()
            {
                SnoPower = snoPower,
                Min = min,
                Max = max,
                IconIndex = iconIndex,
                HeroClass = heroClass,
                Range = range,
            };

            parent.NextTest = test;
            return test;
        }
    }
}