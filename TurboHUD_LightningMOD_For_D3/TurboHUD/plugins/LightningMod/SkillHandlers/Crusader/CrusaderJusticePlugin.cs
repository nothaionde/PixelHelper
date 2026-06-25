namespace Turbo.Plugins.LightningMod
{
    public class CrusaderJusticePlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderJusticePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_Justice;
            //保持对戒
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue() 
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(359583, 0)).ThenContinueElseNoCast()//装备守心克己
                .IfEnoughMonstersInSector(ctx => ctx.Skill.Rune == 2 ? 70 : 30, ctx => 80, ctx => Hud.Window.Size.Height / 18.9f, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1, 1, 1500, 1500).ThenContinueElseNoCast()
                .IfCanCastSkill(90, 90, 3000).ThenCastElseContinue()
                ;

        }
    }
}