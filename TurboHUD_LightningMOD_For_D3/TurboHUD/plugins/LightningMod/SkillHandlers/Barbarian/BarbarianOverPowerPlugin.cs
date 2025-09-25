namespace Turbo.Plugins.LightningMod
{
    public class BarbarianOverPowerPlugin : AbstractSkillHandler, ISkillHandler
    {
        public BarbarianOverPowerPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_Overpower;

            CreateCastRule()
                .IfCanCastSkill(150, 200, 250)
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx=>
                {//劲力狂增符文至少30%以下怒气周围5个怪以上才使用
                    return
                    ctx.Skill.Rune == 3 &&
                    ctx.Skill.Player.Stats.ResourcePctFury <= 30 &&
                    ctx.Skill.Player.Density.GetDensity(9) >= 5;
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {//杀戮狂欢、占尽先机符文在Buff消失后才使用
                    return
                    (ctx.Skill.Rune == 0 || ctx.Skill.Rune == 4) &&
                    !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_Overpower.Sno);
                }).ThenCastElseContinue()
                //其他符文在周围有怪时使用
                .IfTrue(ctx =>
                {
                    return (ctx.Skill.Rune == 1 || ctx.Skill.Rune == 2) && (ctx.Skill.Player.Density.GetDensity(9) >= 1);
                }).ThenCastElseContinue()
                ;
        }
    }
}
 