namespace Turbo.Plugins.LightningMod
{
    using System;

    public class WitchDoctorSoulHarvestPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int ActivationRange { get; set; }

        public WitchDoctorSoulHarvestPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.PreAttack, CastPhase.Attack, CastPhase.Move)
        {
            Enabled = false;
            ActivationRange = 18;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 18, ctx => 3).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => 18).ThenCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 18, ctx =>
                {
                    var stack = ctx.Skill.Buff != null && ctx.Skill.Buff.Active ? ctx.Skill.Buff.IconCounts[0] : 0;
                    return Math.Max(2, stack);
                }).ThenCastElseContinue();
        }
    }
}