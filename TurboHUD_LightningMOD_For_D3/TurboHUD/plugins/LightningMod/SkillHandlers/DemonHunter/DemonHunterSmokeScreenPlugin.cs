namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    using System;
    public class DemonHunterSmokeScreenPlugin : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterSmokeScreenPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_SmokeScreen;

            CreateCastRule()//普通规则100码内有怪危险时施放
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Hud.Avoidance.CurrentValue).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.AvoidablesInRange.Any(x => x.AvoidableDefinition.InstantDeath)).ThenCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfHealthWarning(60, 80).ThenCastElseContinue();

            CreateCastRule()//自动保持残影戒指BUFF
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.UsedLegendaryPowers.ElusiveRing?.Active == true).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.ElusiveRing, 1, 100, 150).ThenCastElseContinue();//残影戒指Buff

            CreateCastRule()//独门烟雾+蓄势待发+80码遇10+怪或精英或BOSS施放
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 3 && (ctx.Skill.Player.Stats.ResourceCostReduction > 0.8 ? true : ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null)).ThenContinueElseNoCast()//独门烟雾+蓄势待发
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 80, ctx => 10).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => 80).ThenCastElseContinue()
               ;

            CreateCastRule()//独门烟雾+蓄势待发+80码遇怪每2.8~3秒施放一次
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 3 && (ctx.Skill.Player.Stats.ResourceCostReduction > 0.8 ? true : ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null)).ThenContinueElseNoCast()//独门烟雾+蓄势待发
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenContinueElseNoCast()
                .IfCanCastSkill(2800, 3000, 5000).ThenCastElseContinue()//每2.8~3秒施放一次
               ;

            CreateCastRule()//飘忽不定+蓄势待发+减耗高于40%+80码遇怪+移动施放
                 .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                 .IfTrue(ctx =>
                 {
                     float ResourceCostReduction = ctx.Skill.Player.Stats.ResourceCostReduction;
                     return ctx.Skill.Rune == 4 && ResourceCostReduction >= 0.4 && (ResourceCostReduction > 0.8 ? true : ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null);
                     }
                 ).ThenContinueElseNoCast()//飘忽不定+蓄势待发+减耗高于40%
                 .IfInTown().ThenNoCastElseContinue()
                 .IfCastingIdentify().ThenNoCastElseContinue()
                 .IfCastingPortal().ThenNoCastElseContinue()
                 .IfOnCooldown().ThenNoCastElseContinue()
                 .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                 .IfCanCastSimple().ThenContinueElseNoCast()
                 .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                 .IfRunning(true).ThenCastElseContinue()//移动时施放
                 .IfEliteOrBossIsNearby(ctx => 80).ThenCastElseContinue()//80码内有精英或BOSS时施放
                 ;

            CreateCastRule()//飘忽不定+蓄势待发+80码遇怪+移动施放
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenContinueElseNoCast()//80码内遭遇敌人使用该规则
                .IfTrue(ctx => ctx.Skill.Rune == 4 && (ctx.Skill.Player.Stats.ResourceCostReduction > 0.8 ? true : ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null)).ThenContinueElseNoCast()//飘忽不定+蓄势待发
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfRunning(true).ThenCastElseContinue()//移动时施放
                .IfEliteOrBossIsNearby(ctx => 80).ThenCastElseContinue()//80码内有精英或BOSS时施放
               ;

            CreateCastRule()//飘忽不定+蓄势待发+无敌人+移动时每2.8~3秒施放一次
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenNoCastElseContinue()//80码内遭遇敌人不使用该规则
                .IfTrue(ctx => ctx.Skill.Rune == 4 && (ctx.Skill.Player.Stats.ResourceCostReduction > 0.8 ? true : ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null)).ThenContinueElseNoCast()//飘忽不定+蓄势待发
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfRunning(true).ThenContinueElseNoCast()//不移动不施放
                .IfCanCastSkill(2800, 3000, 5000).ThenCastElseContinue()//每2.8~3秒施放一次
               ;

            CreateCastRule()//飘忽不定+80码遇怪+移动施放
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 0 && ctx.Skill.Player.Powers.UsedLegendaryPowers.Ingeom?.Active == true).ThenContinueElseNoCast()//消失粉末+寅剑
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenCastElseContinue()//80码内有怪时施放
               ;


            CreateCastRule()//尖刺陷阱+娜塔亚2件+80码遇怪或施放尖刺陷阱时施放
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(635131) >= 2 && ctx.Skill.Player.Powers.UsedDemonHunterPowers.SpikeTrap != null).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_female_cast_spiketrap_01 || ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_male_cast_spiketrap_01).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => 80).ThenCastElseContinue()//80码内有精英或BOSS时施放
               ;

        }
    }
}