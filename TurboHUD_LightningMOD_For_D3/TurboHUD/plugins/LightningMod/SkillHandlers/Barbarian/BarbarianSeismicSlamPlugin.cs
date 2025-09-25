namespace Turbo.Plugins.LightningMod
{
    public class BarbarianSeismicSlamPlugin : AbstractSkillHandler, ISkillHandler
    {
        public BarbarianSeismicSlamPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_SeismicSlam;
            Rune = 1;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfPrimaryResourceAmountIsAbove(ctx => 30).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var set = Hud.Game.Me.GetSetItemCount(671068) == 6 && Hud.Game.Me.GetSetItemCount(749637) == 4;//满足6+4时
                    return (set);
                }).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1.Sno, 0) && !ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1.Sno, 2)).ThenCastElseContinue()//带对戒并且守心BUFF消失后施放
                .IfSpecificSkillOnCooldown(Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker).ThenContinueElseNoCast()//狂暴之怒冷却时
                .IfPrimaryResourcePercentageIsAbove(98).ThenCastElseContinue()//怒气高于98%

                ;
        }
    }
}