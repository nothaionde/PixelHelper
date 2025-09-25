namespace Turbo.Plugins.LightningMod
{
    public class BarbarianBattleRagePlugin : AbstractSkillHandler, ISkillHandler
    {
        public BarbarianBattleRagePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_BattleRage;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(10, ctx => 20).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(10000, 15000).ThenCastElseContinue();
        }
    }
}