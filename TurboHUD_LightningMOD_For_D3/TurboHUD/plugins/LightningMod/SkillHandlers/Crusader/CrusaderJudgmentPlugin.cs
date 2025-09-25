using System;
using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderJudgmentPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderJudgmentPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_Judgment;
            CreateCastRule()//通用规则
                .IfCanCastSkill(500, 1000, 15000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(580748) >= 2).ThenNoCastElseContinue()//阿克汗两件以上时不适用通用规则
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = ctx.Hud.Window.CreateScreenCoordinate(ctx.Hud.Window.CursorX, ctx.Hud.Window.CursorY).ToWorldCoordinate();
                    bool Result = ctx.Hud.Game.AliveMonsters.Any(m => ((m.IsElite && m.Rarity != ActorRarity.RareMinion) || m.SnoMonster.Priority == MonsterPriority.goblin) && m.FloorCoordinate.XYZDistanceTo(cursor) <= 20 && !m.Invulnerable && !m.Invisible && !m.Illusion &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None, 267600) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_A, 267600) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_B, 267600) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_C, 267600) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_D, 267600) != 1 &&
                    m.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_E, 267600) != 1
                    );
                    return Result;
                }).ThenCastElseContinue()
                ;
            CreateCastRule()//阿克汗
               .IfCanCastSkill(100, 100, 15000).ThenContinueElseNoCast()
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(580748) >= 2 && Hud.Game.Me.AnimationState == AcdAnimationState.Attacking).ThenContinueElseNoCast()//阿克汗两件以上且攻击时时适用
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
               .IfTrue(ctx =>
               {
                   IWorldCoordinate cursor = ctx.Hud.Window.CreateScreenCoordinate(ctx.Hud.Window.CursorX, ctx.Hud.Window.CursorY).ToWorldCoordinate();
                   bool isJudgment = ctx.Hud.Game.AliveMonsters.Any(m => m.FloorCoordinate.XYZDistanceTo(cursor) <= 20 && !m.Invulnerable && !m.Invisible && !m.Illusion
                   );
                   return isJudgment;
               }).ThenCastElseContinue()
               ;
        }
    }
}