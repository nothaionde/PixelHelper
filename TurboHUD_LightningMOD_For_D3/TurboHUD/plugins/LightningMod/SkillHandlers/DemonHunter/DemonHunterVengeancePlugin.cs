using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterVengeancePlugin : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterVengeancePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack, CastPhase.PreAttack)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Vengeance;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 1 || ctx.Skill.Rune == 3) && ctx.Skill.Player.Defense.HealthPct < 60).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(50, 100).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction * 100 > 75).ThenContinueElseNoCast()//大于75%CDR
                .IfBuffIsAboutToExpire(50, 100).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx => 100, false).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements).ThenContinueElseNoCast()//元素戒指
                .IfTrue(ctx => {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    double HighestElementLeft = PublicClassPlugin.GetHighestElementLeftSecond(hud, ctx.Skill.Player, CoeIndex);
                    return HighestElementLeft <= 16 && HighestElementLeft >= 15;//爆发1秒内施放
                }).ThenCastElseContinue()
                ;

            CreateCastRule()//黄道或黎明周围有精英或10个小怪施放，CDR29%以上时周围至少1个怪则施放
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(402459) || ctx.Skill.Player.Powers.BuffIsActive(446146, 0)).ThenContinueElseNoCast()//黄道激活时或黎明激活时
                .IfEliteOrBossIsNearby(ctx => 60).ThenCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 60, ctx => 10).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction * 100 > 29).ThenContinueElseNoCast()//大于29%CDR
                .IfBuffIsAboutToExpire(50, 100).ThenCastElseContinue()
                ;

            CreateCastRule()// 黄道激活时或黎明激活时且36 % CDR时持续施放
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(402459) || ctx.Skill.Player.Powers.BuffIsActive(446146, 0)).ThenContinueElseNoCast()//黄道激活时或黎明激活时
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction * 100 > 36).ThenContinueElseNoCast()//大于36%CDR
                .IfBuffIsAboutToExpire(50, 100).ThenCastElseContinue()
                ;

            CreateCastRule()//恨意迸发符文且黄道或黎明，低于30点憎恨时施放
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 4).ThenContinueElseNoCast()//恨意迸发
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(402459) || ctx.Skill.Player.Powers.BuffIsActive(446146, 0)).ThenContinueElseNoCast()//黄道激活时或黎明激活时
                .IfPrimaryResourceAmountIsBelow(ctx => 30).ThenContinueElseNoCast()//憎恨低于30点时
                .IfBuffIsAboutToExpire(50, 100).ThenCastElseContinue()
                ;
        }
    }
}