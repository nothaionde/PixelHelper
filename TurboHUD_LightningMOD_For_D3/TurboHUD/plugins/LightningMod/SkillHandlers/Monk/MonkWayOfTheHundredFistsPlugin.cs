namespace Turbo.Plugins.LightningMod
{
    public class MonkWayOfTheHundredFistsPlugin : AbstractSkillHandler, ISkillHandler
    {
        public MonkWayOfTheHundredFistsPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_WayOfTheHundredFists;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                ;
        }
    }
}