using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterSentryPlugin : AbstractSkillHandler, ISkillHandler
	{
        public DemonHunterSentryPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Sentry;

            CreateCastRule()
                .IfCanCastSkill(150, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if (ctx.Skill.Charges == 0)
                        return false;
                    if (ctx.Skill.Rune != 3)//仅限使用寒冰塔符文
                        return false;
                    IWorldCoordinate cursor = ctx.Hud.Window.CreateScreenCoordinate(ctx.Hud.Window.CursorX, ctx.Hud.Window.CursorY).ToWorldCoordinate();
                    bool BaneOfTheTrapped = ctx.Skill.Player.Powers.UsedLegendaryGems.BaneOfTheTrappedPrimary?.Active == true;
                    int Elites = ctx.Hud.Game.AliveMonsters.Where(m => m.IsElite && m.Rarity != ActorRarity.RareMinion && !m.Slow && !m.Stunned && !m.Frozen && !m.Chilled && !m.Blind && m.FloorCoordinate.XYZDistanceTo(cursor) <= 16 && !m.AffixSnoList.Any(a=> a.Affix == MonsterAffix.Juggernaut)).Count();
                    return BaneOfTheTrapped && Elites > 0;
                }).ThenCastElseContinue()//困者且鼠标16码内的精英或BOSS没有被控时施放寒冰塔
                .IfEliteOrBossIsNearby(ctx => 60, false).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if (ctx.Skill.Charges == 0) return false;
                    bool set = true;//ctx.Skill.Player.GetSetItemCount(254427) >= 6;
                    int Sentrys = ctx.Hud.Game.Actors.Where(a =>
                    a.CentralXyDistanceToMe <= 60 && a.SummonerAcdDynamicId == ctx.Hud.Game.Me.SummonerId
                    &&
                    (
                    a.SnoActor.Sno == ActorSnoEnum._dh_sentry ||
                    a.SnoActor.Sno == ActorSnoEnum._dh_sentry_addsmissiles ||
                    a.SnoActor.Sno == ActorSnoEnum._dh_sentry_addsduration ||
                    a.SnoActor.Sno == ActorSnoEnum._dh_sentry_tether ||
                    a.SnoActor.Sno == ActorSnoEnum._dh_sentry_addsheals ||
                    a.SnoActor.Sno == ActorSnoEnum._dh_sentry_addsshield
                    )
                    ).Count();
                    return set && Sentrys < GetMaxSentrys();
                }).ThenCastElseContinue()//掠夺6件套 且屏幕内箭塔小于5个时施放
                ;
        }

        private int GetMaxSentrys()
        {
            int Sentrys = 2;
            if(Hud.Game.Me.Powers.UsedPassives.Any(s => s.Sno == Hud.Sno.SnoPowers.DemonHunter_Passive_CustomEngineering.Sno))//兵器专家被动
            {
                Sentrys = 3;
            }
            if(Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.BombardiersRucksack.Sno) || Hud.Game.Me.Powers.BuffIsActive(318804))//新老炮手箭袋
            {
                Sentrys = Sentrys + 2;
            }
            return Sentrys;
        }
    }
}