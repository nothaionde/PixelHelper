namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterHungeringArrowPlugin : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterHungeringArrowPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_HungeringArrow;
            //保持对戒
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue() 
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(359583, 0)).ThenContinueElseNoCast()//装备守心克己
                .IfEnoughMonstersInSector(ctx => 40, ctx => 80, ctx => Hud.Window.Size.Height / 11.33333f , ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1, 1, 100, 300).ThenCastElseContinue()
                ;
            //保持明彻裹腕
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => Hud.Game.Me.Powers.UsedLegendaryPowers.WrapsOfClarity?.Active == true).ThenContinueElseNoCast()//装备明彻裹腕
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable, 0).ThenNoCastElseContinue()//护盾
                .IfEnoughMonstersNearby(ctx => 200, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.WrapsOfClarity, 1, 30, 300).ThenCastElseContinue()
                ;
        }
    }
}