namespace Turbo.Plugins.LightningMod
{
    public class CrusaderFallingSwordPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderFallingSwordPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_FallingSword;
            CreateCastRule()
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.FaithfulMemory).ThenContinueElseNoCast()//忠贞回忆
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_FallingSword, 9).ThenNoCastElseContinue()
               .IfEnoughMonstersNearbyCursor(ctx => 14, ctx => 1).ThenContinueElseNoCast()
               .IfCanCastSkill(300, 500, 1000).ThenCastElseContinue()
               ;

            CreateCastRule()
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
               .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.FaithfulMemory).ThenContinueElseNoCast()//忠贞回忆
               .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Crusader_FallingSword, 9, 2000, 2300, true).ThenContinueElseNoCast()
               .IfCanCastSkill(300, 500, 1000).ThenCastElseContinue()
               ;
        }
    }
}