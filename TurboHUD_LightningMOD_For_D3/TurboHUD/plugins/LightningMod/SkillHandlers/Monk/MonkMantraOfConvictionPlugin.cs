namespace Turbo.Plugins.LightningMod
{
    public class MonkMantraOfConvictionPlugin : AbstractSkillHandler, ISkillHandler
    {
        public MonkMantraOfConvictionPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack)
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_MantraOfConviction;

            CreateCastRule()
                .IfCanCastSkill(150, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Monk_Passive_MantraOfConvictionV2, 2).ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(50, ctx => 50).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var epiphanySkill = ctx.Skill.Player.Powers.UsedMonkPowers.Epiphany;
                    return epiphanySkill?.BuffIsActive == true;
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    // TODO simplify
                    return ctx.Hud.Game.IsEliteOnScreen || ctx.Hud.Game.IsGoblinOnScreen || ctx.Hud.Game.MaxPriorityOnScreen >= MonsterPriority.keywarden || ctx.Hud.Game.ActorQuery.NearestBoss != null;
                }).ThenCastElseContinue()
                ;
        }
    }
}