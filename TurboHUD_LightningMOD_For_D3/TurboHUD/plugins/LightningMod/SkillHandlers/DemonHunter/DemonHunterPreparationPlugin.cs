namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterPreparationPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int ResourceConsumed { get; set; }
        public DemonHunterPreparationPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
            ResourceConsumed = 30;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Preparation;

            CreateCastRule()
                .IfTrue(ctx => ctx.Skill.Rune == 0).ThenNoCastElseContinue()//惩罚符文不施放
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 3 && ctx.Skill.Player.Defense.HealthPct <= 40).ThenCastElseContinue()//战伤处理符文
                .IfTrue(ctx =>
                {
                    return (ctx.Skill.Player.Stats.ResourceCurDiscipline <= ctx.Skill.Player.Stats.ResourceMaxDiscipline - ResourceConsumed);//消耗了30点戒律后自动施放
                }).ThenCastElseContinue()
                ;
        }
    }
}