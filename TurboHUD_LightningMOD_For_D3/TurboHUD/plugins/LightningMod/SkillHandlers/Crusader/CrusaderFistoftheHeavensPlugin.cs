using System;
using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderFistoftheHeavensPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderFistoftheHeavensPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_FistOfTheHeavens;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => (int)Math.Ceiling(ctx.Skill.GetResourceRequirement())).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfEnoughMonstersNearbyCursor(ctx => 20, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>//保持勇气4件套减伤
                {
                    var set = Hud.Game.Me.GetSetItemCount(192736);
                    var yongqi4 = Hud.Game.Me.Powers.GetBuff(483655);
                    return set >= 4 && (yongqi4.IconCounts[1] < 50 || yongqi4.TimeLeftSeconds[1] < 0.5);
                }).ThenCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()//不在移动时保持勇气2件套效果
                .IfTrue(ctx =>
                {
                    var set = Hud.Game.Me.GetSetItemCount(192736);
                    var yongqi2 = Hud.Game.Me.Powers.GetBuff(483643);
                    return ((ctx.Skill.Player.Powers.CurrentSkills.Any(s => s.SnoPower.Sno == hud.Sno.SnoPowers.Crusader_HeavensFury.Sno) && set >= 2 && (yongqi2.IconCounts[1] < 3 || yongqi2.TimeLeftSeconds[1] < 0.5)));//保持勇气2件套增伤
                }).ThenCastElseContinue()
                ;
                

            CreateCastRule()//保持对戒克己
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => (int)Math.Ceiling(ctx.Skill.GetResourceRequirement())).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(956353) >= 2).ThenContinueElseNoCast()//装备了对戒
                .IfEnoughMonstersNearbyCursor(ctx => 20, ctx => 1).ThenContinueElseNoCast()//鼠标20码内有任意怪
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1, 2, 1000, 1000).ThenCastElseContinue()//触发对戒
                ;
        }
    }
}