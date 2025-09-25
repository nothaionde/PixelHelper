namespace Turbo.Plugins.LightningMod
{
    public class BarbarianRendPlugin : AbstractSkillHandler, ISkillHandler
	{
        public BarbarianRendPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_Rend;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfCanCastSkill(3300, 4400, 500).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(20, ctx => 20)
                .IfEliteOrBossIsNearby(ctx => ctx.Skill.Rune == 0 ? 18 : 12).ThenCastElseContinue()
                .IfEnoughMonstersNearby(ctx => ctx.Skill.Rune == 0 ? 18 : 12, ctx =>
                {
                    if (ctx.Phase == CastPhase.PreAttack)
                    {
                        if (ctx.Skill.Player.Stats.ResourcePctFury >= 60) return 4;
                        if (ctx.Skill.Player.Stats.ResourcePctFury >= 70) return 3;
                        if (ctx.Skill.Player.Stats.ResourcePctFury >= 80) return 2;
                        if (ctx.Skill.Player.Stats.ResourcePctFury >= 90) return 1;
                    }
                    return 5;
                }).ThenCastElseContinue();
        }
    }
}