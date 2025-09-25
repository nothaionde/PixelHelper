using Turbo.Plugins.glq;
using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderProvokePlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderProvokePlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_Provoke;

            CreateCastRule()//一般规则
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx => ctx.Skill.Rune == 4 && ctx.Skill.Player.GetSetItemCount(220113) >=2 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno) && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.TheMortalDrama.Sno) && ctx.Skill.Player.Powers.CurrentSkills.Any(x => x?.SnoPower == ctx.Hud.Sno.SnoPowers.Crusader_IronSkin && x?.Rune == 3)
                ).ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction >= 0.5).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    var v = (1 - (30 + 5 * ctx.Skill.Player.Density.GetDensity(10)) / ctx.Skill.Player.Stats.ResourceMaxWrath) * 100;
                    return
                        (ctx.Skill.Player.Stats.ResourcePctWrath <= (v > 0 ? v : 10));
                }).ThenCastElseContinue()
                .IfHealthWarning(70,80).ThenCastElseContinue()
                ;
            CreateCastRule()//5222轰击豆角
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 4 && ctx.Skill.Player.GetSetItemCount(220113) >= 2 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno) && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.TheMortalDrama.Sno) && ctx.Skill.Player.Powers.CurrentSkills.Any(x => x?.SnoPower == ctx.Hud.Sno.SnoPowers.Crusader_IronSkin && x?.Rune == 3)
                ).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_IronSkin, 0).ThenCastElseContinue()
                ;
        }
    }
}