namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    using Turbo.Plugins.glq;
    public class CrusaderIronSkinPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderIronSkinPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_IronSkin;

            CreateCastRule()//其他符文
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 3//反伤符文时不生效
                   ).ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时不生效
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_IronSkin, 0).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Hud.Avoidance.CurrentValue).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.AvoidablesInRange.Any(x => x.AvoidableDefinition.InstantDeath)).ThenCastElseContinue()
                .IfHealthWarning(60, 80).ThenCastElseContinue()
                .IfTrue(ctx => (ctx.Skill.Player.Stats.CooldownReduction >= 0.5 || ctx.Hud.Game.Me.Powers.BuffIsActive(402459))).ThenContinueElseNoCast()//CDR大于50或带了黄道
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Crusader_IronSkin, 0, 50, 200).ThenCastElseContinue()
                ;
            CreateCastRule()//反伤之肤-轰击玩法
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(580748) >= 6 && ctx.Skill.Player.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Crusader_AkaratsChampion)?.Rune == 2).ThenNoCastElseContinue()//阿克汗6件+阿卡拉特勇士集结号令不使用此规则
               .IfTrue(ctx => ctx.Skill.Rune == 3 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.TheMortalDrama.Sno) &&//人世无常
                   ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno)//元素戒指
                   ).ThenContinueElseNoCast()
               .IfEnoughMonstersNearby(ctx => 30, ctx => 1).ThenContinueElseNoCast()
               .IfTrue(ctx =>
               {
                   return 
                   PublicClassPlugin.IsElementReady(hud, 3, ctx.Skill.Player, 6) || //物理爆发前3秒
                   (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno) && !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno)//吃了减耗塔或后骑马时不施放，否则骑马时也施放
                   );
               }
               ).ThenCastElseContinue()
               ;

            CreateCastRule()//反伤之肤-阿克汗轰击5222玩法
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno)).ThenNoCastElseContinue()//骑马不放
               .IfTrue(ctx => ctx.Skill.Rune == 3 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.TheMortalDrama.Sno)//人世无常
                   ).ThenContinueElseNoCast()
               .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(580748) >= 6 && ctx.Skill.Player.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Crusader_AkaratsChampion)?.Rune == 2).ThenContinueElseNoCast()//阿克汗6件+阿卡拉特勇士集结号令
               .IfEnoughMonstersNearby(ctx => 30, ctx => 1).ThenCastElseContinue()
               ;

            CreateCastRule()//反伤之肤-幻魔6件
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfTrue(ctx =>ctx.Skill.Player.GetSetItemCount(220113) >= 6).ThenContinueElseNoCast()//幻魔6件
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时不施放
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_IronSkin, 0).ThenNoCastElseContinue()//反伤之肤生效时不施放
               .IfEnoughMonstersNearby(ctx => 60, ctx => 1).ThenCastElseContinue()
               ;

        }
    }
}