using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderCondemnPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderCondemnPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_Condemn;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfPrimaryResourceIsEnough(ctx => (ctx.Hud.Game.Me.Powers.CurrentSkills.Any(s => s.SnoPower.Sno == ctx.Hud.Sno.SnoPowers.Crusader_Phalanx.Sno && s.Rune == 3) && ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.UnrelentingPhalanx.Sno)) ? 40 : 0, ctx => ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.FrydehrsWrath.Sno) ? 40 : 0).ThenContinueElseNoCast()//当装备或萃取不懈方阵+且技能栏有斗阵-铁血人墙符文时保留50能量，否则0
                .IfEnoughMonstersNearby(ctx => (int)(20 * (1 + Hud.Game.Me.Stats.MoveSpeed / 100)) * 3 + ((ctx.Skill.Rune == 1 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.FrydehrsWrath.Sno)) ? 10 : -15), ctx => 1).ThenCastElseContinue()
                ;
        }
    }
}