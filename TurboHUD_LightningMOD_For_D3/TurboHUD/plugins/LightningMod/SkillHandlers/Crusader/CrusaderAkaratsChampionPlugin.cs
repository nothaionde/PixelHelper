using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Turbo.Plugins.glq;

namespace Turbo.Plugins.LightningMod
{
    public class CrusaderAkaratsChampionPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderAkaratsChampionPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_AkaratsChampion;

            CreateCastRule()//一般规则
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => !isSanGuang()).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting, 0, 500, 2000, true).ThenCastElseContinue()//减耗塔即将结束前施放
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var buff = ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno);
                    var remaining = buff?.Active == true ? buff.TimeLeftSeconds[1] : 0.0d;
                    return remaining <= 1d || ctx.Skill.Rune == 2;//buff剩余小于1s或者符文为集结号令
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var set = Hud.Game.Me.GetSetItemCount(580748);// 阿克汗套
                    return !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) && (ctx.Skill.Player.Stats.CooldownReduction >= 0.75 || (ctx.Skill.Player.Stats.CooldownReduction >= 0.5 && (set >= 4 || ctx.Hud.Game.Me.Powers.BuffIsActive(402459) || ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.AkaratsAwakening.Sno))));//CDR大于75或（50且带了黄道或阿卡拉特顿悟或阿克汗4件）
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    var Crusader_SteedCharge = ctx.Skill.Player.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Crusader_SteedCharge);
                    bool isNoFatal = (!ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Sno, 0) || ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.Crusader_Passive_Indestructible.Sno, 1)) &&//没带铁胆钢心或CD中
                    ctx.Skill.Rune == 3 &&//先知化身
                    (ctx.Skill.Player.HeroIsHardcore || ctx.Hud.Avoidance.CurrentValue || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(20, false) || ctx.Skill.Player.AvoidablesInRange.Any(x => x.AvoidableDefinition.InstantDeath) || ctx.Skill.Player.Powers.CantMove || (Hud.Game.Me.Defense.HealthPct < (Hud.Game.Me.Powers.HealthPotionSkill.IsOnCooldown ? 60 : 30))) &&//专家模式或危险时
                    (!ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) || (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) && cando()))
                    ;
                    
                    return isNoFatal ? true : !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno);
                }).ThenContinueElseNoCast()//优先保命
                .IfTrue(ctx =>
                {
                    var isDLegacyOfDreams = ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.LegacyOfDreamsPrimary.Sno);//梦之遗礼宝石
                    var isAkkhansLeniency = ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.AkkhansLeniency.Sno);//阿克汗的宽容
                    var isJekangbord = ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Jekangbord.Sno);//杰伏坎盾
                    return isDLegacyOfDreams && isAkkhansLeniency && isJekangbord && PublicClassPlugin.IsElementReady(ctx.Hud, 0.1, ctx.Skill.Player, 3);//火元素爆发
                }).ThenCastElseContinue()
                .IfPrimaryResourcePercentageIsBelow(20).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => 40).ThenCastElseContinue()
                ;

            CreateCastRule()//三光规则
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => isSanGuang()).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 30, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.ConventionOfElements.Sno, 4) && (!ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) || (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno) && cando()));
                }).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100, 200).ThenCastElseContinue()
                ;
        }
        private bool cando()
        {
            var Crusader_SteedCharge = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Crusader_SteedCharge);
            return ((!Hud.Interaction.IsHotKeySet(ActionKey.Move) || (Hud.Interaction.IsHotKeySet(ActionKey.Move) && !Hud.Interaction.IsContinuousActionStarted(ActionKey.Move))) && (Crusader_SteedCharge != null && !Hud.Interaction.IsContinuousActionStarted(Crusader_SteedCharge.Key)));//未按下强制移动且未按住骑马
        }

        private bool isSanGuang()
        {
            bool isAegisofValor = Hud.Game.Me.GetSetItemCount(192736) >= 6;//勇气6件套
            bool isFateoftheFell = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FateOfTheFell.Sno, 0);//妖邪必败
            bool isConventionOfElements = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 0);//元素戒指
            return isAegisofValor && isFateoftheFell && isConventionOfElements;
        }
    }
}