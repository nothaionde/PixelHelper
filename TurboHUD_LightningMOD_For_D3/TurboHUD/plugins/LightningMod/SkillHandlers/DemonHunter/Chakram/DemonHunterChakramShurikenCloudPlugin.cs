namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterChakramShurikenCloudPlugin : AbstractSkillHandler, ISkillHandler
	{
        public DemonHunterChakramShurikenCloudPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Chakram;
            Rune = 4;

            CreateCastRule()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                //.IfCanCastBuff().ThenContinueElseNoCast()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(70, ctx => 10).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(20000, 50000).ThenCastElseContinue();
        }
    }
}