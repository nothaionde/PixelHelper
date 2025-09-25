namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterImpalePlugin : AbstractSkillHandler, ISkillHandler
	{
        public DemonHunterImpalePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Impale;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue() 
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfAttacking().ThenNoCastElseContinue()
                .IfTrue(ctx => Hud.Game.Me.Powers.UsedDemonHunterPowers.Vault != null).ThenContinueElseNoCast()//ÌÚÔ¾
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ChainOfShadows, 0).ThenContinueElseNoCast()//°µÓ°Ö®Á´
                .IfPrimaryResourceIsEnough(0 , ctx => 0).ThenContinueElseNoCast()
                .IfSecondaryResourcePercentageIsBelow(90).ThenContinueElseNoCast()
				.IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.ChainOfShadows, 1, 30, 100).ThenCastElseContinue();
        }
    }
}