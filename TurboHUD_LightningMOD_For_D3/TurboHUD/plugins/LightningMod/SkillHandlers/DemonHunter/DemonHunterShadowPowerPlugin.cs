using System;
namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterShadowPowerPlugin : AbstractSkillHandler, ISkillHandler
    {
        public bool ElusiveRingEnabled { get; set; } = true;
        public bool LifeOnHitEnabled { get; set; } = true;
        public DemonHunterShadowPowerPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.PreAttack)
        {
            Enabled = false;
            ElusiveRingEnabled = false;
            LifeOnHitEnabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_ShadowPower;

            // normal 一般情况危险时自动施放
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.DemonHunter_ShadowPower.Sno)).ThenNoCastElseContinue()
                .IfTrue(ctx => Hud.Game.Me.GetSetItemCount(916931) >= 4).ThenCastElseContinue()//暗影4件套
                .IfFalse(ctx =>
                {
                    var lotsOfDiscipline = (ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null) &&
                                            (ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation.Rune != 0) &&
                                            !ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation.IsOnCooldown; // not for Punishment

                    return (lotsOfDiscipline && (ctx.Skill.Player.Defense.HealthPct <= 65)) || LifeInWarning(ctx.Skill.Player);
                }).ThenNoCastElseContinue()
                .IfBuffIsAboutToExpire(30, 100).ThenCastElseContinue();

            //暗影滑行+恐惧6件套+蓄势待发+移动时施放
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.DemonHunter_ShadowPower.Sno)).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 1 && (ctx.Skill.Player.GetSetItemCount(791249) >= 6 || ctx.Skill.Player.GetSetItemCount(254164) >= 6) && ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null).ThenContinueElseNoCast()//暗影滑行+恐惧6件套或邪秽之精6件套+蓄势待发
                .IfRunning(true).ThenCastElseContinue()
                ;

            //遁入暗影+恐惧6件套+蓄势待发+80码内1个怪时施放
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.DemonHunter_ShadowPower.Sno)).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 2 && (ctx.Skill.Player.GetSetItemCount(791249) >= 6 || ctx.Skill.Player.GetSetItemCount(254164) >= 6) && ctx.Skill.Player.Powers.UsedDemonHunterPowers.Preparation != null).ThenContinueElseNoCast()//遁入暗影+恐惧6件套或邪秽之精6件套+蓄势待发
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenCastElseContinue()//80码内1个怪
                ;

            // elusive ring 残影戒指
            CreateCastRule()
                .IfTrue(ctx => ElusiveRingEnabled && ctx.Skill.Player.Powers.UsedLegendaryPowers.ElusiveRing?.Active == true).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 40, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.ElusiveRing, 1, 30, 100).ThenCastElseContinue();

            //Bottomless Potion of Mutilation 卡杀回药水
            CreateCastRule()
                .IfTrue(ctx =>
                {
                    var buff = Hud.Game.Me.Powers.GetBuff(hud.Sno.SnoPowers.Generic_X1LegendaryGenericPotionPowerup.Sno);
                    return (LifeOnHitEnabled && buff != null && buff.TimeLeftSeconds[4] >= 4.6);
                }
                ).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenCastElseContinue();


            //娜塔亚2件套++80码内1个怪时施放
            CreateCastRule()//尖刺陷阱+娜塔亚2件+80码遇怪或施放尖刺陷阱时施放
                .IfCanCastSkill(100, 100, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(635131) >= 2 && ctx.Skill.Player.Powers.UsedDemonHunterPowers.SpikeTrap != null).ThenContinueElseNoCast()//娜塔亚2件+使用陷阱
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_female_cast_spiketrap_01 || ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_male_cast_spiketrap_01).ThenContinueElseNoCast()//施放尖刺陷阱
                .IfEnoughMonstersNearby(ctx => 80, ctx => 1).ThenCastElseContinue()//80码内有任意怪物时施放
                ;

            //夜魔化身+暗影4件套+使用暗影飞刀+80码内1个怪时施放
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSecondaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 0 && ctx.Skill.Player.GetSetItemCount(916931) >= 4 && (Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_male_cast_impale_01 || Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_female_cast_impale_01)).ThenContinueElseNoCast()///夜魔化身+暗影4件套+使用暗影飞刀
                .IfEliteOrBossIsNearby(ctx => 30).ThenContinueElseNoCast()//30码内精英或BOSS
                .IfCanCastSkill(4000, 5000, 10000).ThenCastElseContinue()//强制冷却
                ;
        }
        private bool LifeInWarning(IPlayer player)
        {
            return player.Defense.HealthPct <= 30 || (player.Powers.HealthPotionSkill.IsOnCooldown && (player.Defense.HealthPct <= 85))
                || (Hud.Game.IsEliteOnScreen && (player.Defense.HealthPct <= 75 || player.AvoidablesInRange.Count > 1));
        }
    }
}