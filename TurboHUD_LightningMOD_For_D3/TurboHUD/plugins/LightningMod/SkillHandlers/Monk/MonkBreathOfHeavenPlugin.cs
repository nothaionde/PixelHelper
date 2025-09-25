namespace Turbo.Plugins.LightningMod
{
    public class MonkBreathOfHeavenPlugin : AbstractSkillHandler, ISkillHandler
	{
        public MonkBreathOfHeavenPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_BreathOfHeaven;
            //芳华吐纳1 炽炎怒火2 光能灌注3 御风而行4
            CreateCastRule()
                .IfCanCastSkill(150, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfHealthPercentageIsBelow(ctx => 70).ThenCastElseContinue()
                .IfNearbyPartyMemberIsInDanger(12, 70, 80, 70, true).ThenCastElseContinue()//所有符文在12码队友危险时使用
                .IfTrue(ctx => ctx.Skill.Rune == 3).ThenCastElseContinue()//光能灌注
                .IfPrimaryResourcePercentageIsBelow(50).ThenContinueElseNoCast()//内力低于50%
                .IfTrue(ctx => (ctx.Hud.Game.Me.Stats.CooldownReduction > 0.65 && (ctx.Hud.Game.Me.Powers.BuffIsActive(402459) || ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.Ingeom.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.MesserschmidtsReaver.Sno))) || ctx.Hud.Game.Me.Stats.CooldownReduction > 0.73)//65 % CDR以上及带黄道/寅剑/梅斧或 73% CDR 时自动持续施放
                ;
            CreateCastRule()//炽炎怒火符文
                .IfTrue(ctx => ctx.Skill.Rune == 2).ThenContinueElseNoCast()
                .IfCanCastSkill(150, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => 40).ThenContinueElseNoCast()//有精英或BOSS时保持BUFF
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Monk_BreathOfHeaven, 0, 50, 100).ThenCastElseContinue()
                ;
            CreateCastRule()//御风而行符文
                .IfTrue(ctx => ctx.Skill.Rune == 4).ThenContinueElseNoCast()
                .IfCanCastSkill(150, 200, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => (ctx.Hud.Game.Me.Stats.CooldownReduction > 0.65 && (ctx.Hud.Game.Me.Powers.BuffIsActive(402459) || ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.Ingeom.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(hud.Sno.SnoPowers.MesserschmidtsReaver.Sno))) || ctx.Hud.Game.Me.Stats.CooldownReduction > 0.73).ThenCastElseContinue()//65%CDR以上及带黄道/寅剑/梅斧或 73% CDR 时自动持续施放
                ;
        }
    }
}