using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class MonkMysticAllyPlugin : AbstractSkillHandler, ISkillHandler
	{
        public MonkMysticAllyPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_MysticAlly;
               
            CreateCastRule()
                .IfCanCastSkill(150, 200, 500).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => (ctx.Skill.Rune == 3 || (ctx.Skill.Rune == 1 && ctx.Skill.Player.GetSetItemCount(742942) >= 6)) && ctx.Skill.Player.Stats.ResourceCurPri < ctx.Skill.Player.Stats.ResourceMaxPri - 200).ThenCastElseContinue()//风相幻身3或水幻身时殷娜6件套且内力低于最大资源-200
                .IfTrue(ctx => ctx.Skill.Rune == 4 && ctx.Skill.Player.Defense.HealthPct < 30).ThenCastElseContinue()//坚毅幻身4
                ;
            CreateCastRule()
                .IfCanCastSkill(100, 200, 500).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfTrue(ctx => {//水相幻身1和土相幻身
                    bool isBossOrEliteNearby = ctx.Hud.Game.AliveMonsters.Any(x => (x.Rarity == ActorRarity.Boss || x.Rarity == ActorRarity.Champion || x.Rarity == ActorRarity.Rare || x.Rarity == ActorRarity.Unique) && x.NormalizedXyDistanceToMe < 20 && !x.Illusion && !x.Invulnerable && !x.Invisible);
                    return (ctx.Skill.Rune == 1 || ctx.Skill.Rune == 2) && ctx.Skill.Player.GetSetItemCount(742942) >= 6 && ((ctx.Skill.Player.Density.GetDensity(20) > 1 || isBossOrEliteNearby) && (getCurrentMysticAlly() - getCurrentStone()) >= getMaxMysticAlly());//殷娜6件时施放
                }).ThenCastElseContinue()
                .IfTrue(ctx => {//火相幻身0
                    bool isCOE = ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno);
                    bool isLesserGods = ctx.Hud.Game.Me.Powers.BuffIsActive(485725); //蒙尘者绑腕
                    bool isBossOrEliteNearby = ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(20, false) || ctx.Hud.Game.ActorQuery.NearestGoblin?.NormalizedXyDistanceToMe < 20 || ctx.Hud.Game.ActorQuery.NearestKeywarden?.NormalizedXyDistanceToMe < 20;
                    bool isLesserGodsDebuff = ctx.Hud.Game.AliveMonsters.Any(x => (isBossOrEliteNearby ? (x.Rarity == ActorRarity.Boss || x.Rarity == ActorRarity.Champion || x.Rarity == ActorRarity.Rare || x.Rarity == ActorRarity.Unique) : true) && x.NormalizedXyDistanceToMe < 20 && !x.Illusion && !x.Invulnerable && !x.Invisible && x.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 485725) == 1);
                    return ctx.Skill.Rune == 0 && isLesserGods && isLesserGodsDebuff && (isCOE ? ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno, 3) : true) && getCurrentMysticAlly() >= getMaxMysticAlly();
                }).ThenCastElseContinue()
                ;
        }
        private int getCurrentStone()
        {
            return Hud.Game.Actors.Where(x => x.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && (x.SnoActor.Sno == ActorSnoEnum._monk_female_mystically_crimson || x.SnoActor.Sno == ActorSnoEnum._x1_projectile_mystically_runec_boulder //土幻身石头
           )
            ).Count();
        }
        private int getCurrentMysticAlly()
        {
            /*bool isanymysticallymini = Hud.Game.Actors.Any(x => x.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && (x.SnoActor.Sno == ActorSnoEnum._x1_monk_female_mysticallymini_crimson) //火幻身小人
           );
            if (isanymysticallymini && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno,0) == true)//减耗塔且有小幻身时
            {
                return 0;
            }*/

            int mysticallymini = Hud.Game.Actors.Count(x => x.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && (x.SnoActor.Sno == ActorSnoEnum._x1_monk_female_mysticallymini_crimson) //火幻身小人
           );
            int threshold = 5;//爆炸多少个小火人后再次触发
            if (mysticallymini > 0 && mysticallymini < ((getMaxMysticAlly() * 2 - threshold) < 1 ? 1: (getMaxMysticAlly() * 2 - threshold)) && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno, 0) == true)//减耗塔且有小幻身时
            {
                return (getMaxMysticAlly() * 2);
            }
            if (mysticallymini >= ((getMaxMysticAlly() * 2 - threshold) < 1 ? 1 : (getMaxMysticAlly() * 2 - threshold)) && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno, 0) == true) return 0;

            return  Hud.Game.Actors.Where(x => x.SummonerAcdDynamicId == Hud.Game.Me.SummonerId && (x.SnoActor.Sno == ActorSnoEnum._monk_female_mystically_crimson || x.SnoActor.Sno == ActorSnoEnum._monk_male_mystically_crimson || //火幻身
           x.SnoActor.Sno == ActorSnoEnum._monk_female_mystically_indigo || x.SnoActor.Sno == ActorSnoEnum._monk_male_mystically_indigo ||//水幻身
           x.SnoActor.Sno == ActorSnoEnum._monk_female_mystically_obsidian || x.SnoActor.Sno == ActorSnoEnum._monk_male_mystically_obsidian//土幻身
           )
           ).Count();
        }
        private int getMaxMysticAlly()
        {
            if (Hud.Game.Me.GetSetItemCount(742942) >= 6) return 10;
            if (Hud.Game.Me.Powers.BuffIsActive(409811) || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheCrudestBoots.Sno)) return 2;//新老粗糙鞋
            return 1;
        }
    }
}