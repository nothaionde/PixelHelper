namespace Turbo.Plugins.LightningMod
{
    public class WizardDiamondSkinPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardDiamondSkinPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_DiamondSkin;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.Stats.CooldownReduction >= 0.6 && (ctx.Skill.Rune == 1 ? true : (ctx.Hud.Game.Me.Powers.BuffIsActive(402459) || ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.MesserschmidtsReaver.Sno)));
                }).ThenCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Ingeom, 1).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100, 500).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => (ctx.Skill.Rune == 3) && ((ctx.Skill.Player.AnimationState == AcdAnimationState.Attacking || ctx.Skill.Player.AnimationState == AcdAnimationState.Channeling) && (ctx.Skill.Player.Powers.UsedLegendaryPowers.AetherWalker?.Active == true ? true : (ctx.Skill.Player.Animation != AnimSnoEnum._wizard_female_hth_spellcast_teleport && ctx.Skill.Player.Animation != AnimSnoEnum._wizard_male_hth_spellcast_teleport && ctx.Skill.Player.Animation != AnimSnoEnum._wizard_female_archon_cast_teleport_01 && ctx.Skill.Player.Animation != AnimSnoEnum._wizard_male_archon_cast_teleport_01)))).ThenCastElseContinue()//节能棱镜-不装备以太时传送不施放否则任何技能施放时施放
                .IfTrue(ctx => (ctx.Skill.Rune == 1) && (ctx.Skill.Player.AnimationState == AcdAnimationState.Attacking || ctx.Skill.Player.AnimationState == AcdAnimationState.Channeling)).ThenCastElseContinue()//耐久体肤
                .IfTrue(ctx => ctx.Skill.Rune == 0 && ctx.Skill.Player.AnimationState == AcdAnimationState.Running).ThenCastElseContinue()//镜光体肤
                ;
        }
    }
}