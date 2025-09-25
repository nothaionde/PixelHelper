namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.glq;

    public class NecSiphonBloodPlugin : AbstractSkillHandler, ISkillHandler
    {
        public NecSiphonBloodPlugin()
            : base(CastType.RangedChannelingSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_SiphonBlood;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 3)//Power Shift强力相移符文
                .IfTrue(ctx =>
                {
                    var monster = ctx.Hud.Game.SelectedMonster2;
                    var buffCount = PublicClassPlugin.GetBuffCount(ctx.Hud, Hud.Sno.SnoPowers.Necromancer_SiphonBlood.Sno, 10);
                    var buffTime = PublicClassPlugin.GetBuffLeftTime(ctx.Hud, Hud.Sno.SnoPowers.Necromancer_SiphonBlood.Sno, 10);
                    return monster != null && (buffCount < 10 || buffTime < 0.5d);
                }).ThenCastElseNoCast()
                ;


        }
    }
}