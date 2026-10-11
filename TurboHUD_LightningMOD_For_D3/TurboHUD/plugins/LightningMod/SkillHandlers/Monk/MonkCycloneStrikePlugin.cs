using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class MonkCycloneStrikePlugin : AbstractSkillHandler, ISkillHandler
	{
        public MonkCycloneStrikePlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Attack)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_CycloneStrike;

            CreateCastRule()//保持减伤肩膀buff
               .IfCanCastSkill(100, 200, 1000).ThenContinueElseNoCast()
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
               .IfEnoughMonstersNearby(ctx => ctx.Skill.Rune == 1 ? 34 : 24, ctx => 1).ThenContinueElseNoCast()
               .IfTrue(ctx =>ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.LefebvresSoliloquy.Sno, 0)).ThenContinueElseNoCast()//带劝诫肩膀
               .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Monk_CycloneStrike, 0, 1000, 1500).ThenCastElseContinue()//减伤BUFF
               ;
            //聚力爆破1 疗伤清风2
            CreateCastRule()
                .IfCanCastSkill(100, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 50, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var range = ctx.Skill.Rune == 1 ? 34 : 24;
                    return ctx.Hud.Game.ActorQuery.NearestBoss != null && ctx.Hud.Game.ActorQuery.NearestBoss.NormalizedXyDistanceToMe <= range && ctx.Skill.Player.Density.GetDensity(34) > 1;
                }).ThenNoCastElseContinue()//非单体BOSS不放技能
                .IfTrue(ctx =>
                {
                    return (ctx.Skill.Rune == 2);//疗伤清风自动施放
                }).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfCanCastSkill(1000, 1000, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(100, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 50, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return isLesserGodsOK(ctx.Skill.Rune);
                }).ThenCastElseContinue()//装备蒙尘者绑腕时周围有精英或BOSS没有触发debuff时施放，否则任何怪物没有触发debuff时施放
               .IfTrue(ctx =>
               {
                   return (ctx.Hud.Game.Items.Where(i => i.Location == ItemLocation.LeftHand).FirstOrDefault()?.SnoItem.Sno == Hud.Sno.SnoItems.Unique_CombatStaff_2H_001_x1.Sno//殷娜的审判
                   )
                   ;
               }).ThenCastElseContinue()//装备寅剑时精力大于50贼直接施放
                ;

            CreateCastRule()
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 50, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx => Hud.Game.Me.Powers.GetUsedSkill(ctx.Hud.Sno.SnoPowers.Monk_ExplodingPalm) != null).ThenNoCastElseContinue()//装备爆裂掌不生效
                .IfTrue(ctx =>
                {
                    return ctx.Hud.Game.Items.Where(i => i.Location == ItemLocation.LeftHand).FirstOrDefault()?.SnoItem.Sno == Hud.Sno.SnoItems.Unique_CombatStaff_2H_001_x1.Sno//殷娜的审判
                    && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno)//黄道
                    && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.FlyingDragon.Sno)//翔龙
                    && ctx.Skill.Player.GetSetItemCount(707760) >= 3//船长
                    ;
                }).ThenCastElseContinue()//装备殷娜的审判和黄道和降龙直接施放

                ;

            CreateCastRule()
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
               .IfTrue(ctx =>
               {
                   int season = ctx.Hud.Game.Me.Hero.Season;
                   int bell = ctx.Hud.Game.Actors.Count(a => a.SnoActor.Sno == ActorSnoEnum._p74_monk_bell_waveoflight_runeb);//27赛季火钟
                   return season == 34//仅限制27赛季
                  && bell == 5;
                   ;
               }).ThenCastElseContinue()
                ;

        }
        private bool isLesserGodsOK(int Rune)
        {
            bool isLesserGods = Hud.Game.Me.Powers.BuffIsActive(485725); //蒙尘者绑腕
            var range = Rune == 1 ? 34 : 24;
            bool isBossOrEliteNearby = Hud.Game.ActorQuery.IsEliteOrBossCloserThan(range, false) || Hud.Game.ActorQuery.NearestGoblin?.NormalizedXyDistanceToMe < range || Hud.Game.ActorQuery.NearestKeywarden?.NormalizedXyDistanceToMe < range;
            bool noLesserGodsDebuff = Hud.Game.AliveMonsters.Any(x => (isBossOrEliteNearby ? (x.Rarity == ActorRarity.Boss || x.Rarity == ActorRarity.Champion || x.Rarity == ActorRarity.Rare || x.Rarity == ActorRarity.Unique || ((Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Ingeom.Sno) || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.BaneOfThePowerfulPrimary.Sno)) ? x.Rarity == ActorRarity.RareMinion : true)) : true) && x.NormalizedXyDistanceToMe < range && x.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 485725) != 1);
            return isLesserGods && noLesserGodsDebuff;
        }

    }
}

