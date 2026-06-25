namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterRainofVengeancePlugin : AbstractSkillHandler, ISkillHandler
	{
        public DemonHunterRainofVengeancePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Caltrops;

            CreateCastRule()//尖刺陷阱+娜塔亚2件+12码遇怪或施放尖刺陷阱时施放
               .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(635131) >= 2 && ctx.Skill.Player.Powers.UsedDemonHunterPowers.SpikeTrap != null).ThenContinueElseNoCast()
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
               .IfTrue(ctx => ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_female_cast_spiketrap_01 || ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_male_cast_spiketrap_01).ThenCastElseContinue()
               .IfEliteOrBossIsNearby(ctx => 12).ThenCastElseContinue()//12码内有精英或BOSS时施放
               ;

            CreateCastRule()//施放三刀时施放
               .IfInTown().ThenNoCastElseContinue()
               .IfCastingIdentify().ThenNoCastElseContinue()
               .IfCastingPortal().ThenNoCastElseContinue()
               .IfOnCooldown().ThenNoCastElseContinue()
               .IfCanCastSimple().ThenContinueElseNoCast()
               .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
               .IfTrue(ctx => ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_male_cast_impale_01 || ctx.Hud.Game.Me.Animation == AnimSnoEnum._demonhunter_female_cast_impale_01).ThenContinueElseNoCast()//施放三刀时
               .IfTrue(ctx => {
                   var Vault = Hud.Game.Me.Powers.UsedDemonHunterPowers.Vault;
                   if(Vault == null)
                   {
                       return false;
                   }
                   return Hud.Interaction.IsContinuousActionStarted(Vault.Key);
                   }).ThenNoCastElseContinue()//翻滚时不放
               .IfCanCastSkill(500,500,1000).ThenContinueElseNoCast()
               .IfEnoughMonstersNearby(ctx => 60, ctx => 1).ThenCastElseContinue()
               ;

        }
    }
}