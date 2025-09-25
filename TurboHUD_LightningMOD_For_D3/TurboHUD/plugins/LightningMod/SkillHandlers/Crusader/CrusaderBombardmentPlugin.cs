namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.glq;
    public class CrusaderBombardmentPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderBombardmentPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_Bombardment;

            CreateCastRule()
               .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()//按键的间隔100~150延迟
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               .IfTrue(ctx => ctx.Skill.Rune == 0).ThenContinueElseNoCast()//尖刺桶
               .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(580748) >= 6 && ctx.Skill.Player.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Crusader_AkaratsChampion)?.Rune == 2).ThenNoCastElseContinue()//阿克汗6件+阿卡拉特勇士集结号令不使用此规则
               .IfTrue(ctx=> ctx.Skill.Player.GetSetItemCount(220113) >= 4).ThenNoCastElseContinue()//幻魔4件幻魔四件不使用此规则
               .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno)).ThenContinueElseNoCast()//元素戒指
               .IfEnoughMonstersNearby(ctx => 30, ctx => 1).ThenContinueElseNoCast()
               .IfTrue(ctx =>
               {
                   double LeftTime = PublicClassPlugin.GetHighestElementLeftSecondAssingedPlayer(hud, ctx.Skill.Player, 6);
                   if (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno))//减耗塔或（阿克汗6件+阿卡拉特勇士集结号令）
                   {
                       if(!ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno))//非骑马状态
                       {
                           return (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno) &&//人世无常
                            ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno)) ||//元素戒指
                            (LeftTime >= 15 || LeftTime <= 1)//电3~物理1秒之间的2秒
                             ;
                       }
                       else
                       {
                           return false;
                       }
                   }
                   else
                   {
                       return
                       ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno) &&//元素戒指
                       (LeftTime >= 15 || LeftTime <= 1)//电3~物理1秒之间的2秒
                       ;
                   }
               }
               ).ThenCastElseContinue()
               ;

            CreateCastRule()
               .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()//按键的间隔100~150延迟
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               .IfTrue(ctx=> ctx.Skill.Player.GetSetItemCount(220113) >= 4 && !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno)).ThenContinueElseNoCast()//幻魔4件且非骑马状态
               .IfEnoughMonstersNearbyCursor(ctx => 12, ctx => 1).ThenContinueElseNoCast()
               .IfSpecificBuffIsAboutToExpire(Hud.Sno.GetSnoPower(445639), 1, 500, 1000).ThenCastElseContinue()//保持幻魔4件减伤BUFF
               ;

            CreateCastRule()//反伤之肤-阿克汗轰击5222玩法
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()//按键的间隔100~150延迟
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               .IfTrue(ctx => ctx.Skill.Rune == 0).ThenContinueElseNoCast()//尖刺桶
               .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno)).ThenNoCastElseContinue()//骑马不放
               .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.TheMortalDrama.Sno)//人世无常
                   ).ThenContinueElseNoCast()
               .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(580748) >= 6 && ctx.Skill.Player.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Crusader_AkaratsChampion)?.Rune == 2 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Crusader_IronSkin.Sno)).ThenContinueElseNoCast()//阿克汗6件+阿卡拉特勇士集结号令+反伤之肤激活
               .IfEnoughMonstersNearby(ctx => 30, ctx => 1).ThenCastElseContinue()
               ;
        }
    }
}