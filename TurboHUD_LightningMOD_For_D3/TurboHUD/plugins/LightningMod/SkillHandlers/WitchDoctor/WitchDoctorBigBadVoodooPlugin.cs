namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    public class WitchDoctorBigBadVoodooPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorBigBadVoodooPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var count = ctx.Hud.Game.Actors.Count(actor => actor.SummonerAcdDynamicId == ctx.Skill.Player.SummonerId &&
                    (actor.SnoActor.Sno == ActorSnoEnum._fetish_melee_a || 
                    actor.SnoActor.Sno == ActorSnoEnum._fetish_doublestack_shaman_a || 
                    actor.SnoActor.Sno == ActorSnoEnum._fetish_ranged_a || 
                    actor.SnoActor.Sno == ActorSnoEnum._fetish_melee_itempassive || 
                    actor.SnoActor.Sno == ActorSnoEnum._fetish_shaman_a || 
                    actor.SnoActor.Sno == ActorSnoEnum._fetish_skeleton_a || 
                    actor.SnoActor.Sno == ActorSnoEnum._fetish_melee_sycophants));
                    return (count > 3 && ctx.Hud.Game.Me.Powers.BuffIsActive(318724));//3个以上鬼娃并带星铁
                }).ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx => 40).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return (ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_Passive_GraveInjustice.Sno) && ctx.Hud.Game.Me.Powers.BuffIsActive(484128));//剥削死者和套蒙嘟噜咕2件
                }).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo, 4, 500, 1000).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.Stats.CooldownReduction > 0.6 && ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.WitchDoctor_Passive_GraveInjustice.Sno);//CDR高于60%且使用剥削死者被动
                }).ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx =>20, false).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var BigBadVoodooCount = ctx.Hud.Game.Actors.Count(actor => actor.SummonerAcdDynamicId == ctx.Skill.Player.SummonerId && actor.CentralXyDistanceToMe < 30 &&
                    (actor.SnoActor.Sno == ActorSnoEnum._witchdoctor_bigbadvoodoo_fetish ||
                    actor.SnoActor.Sno == ActorSnoEnum._witchdoctor_bigbadvoodoo_fetish_blue ||
                    actor.SnoActor.Sno == ActorSnoEnum._witchdoctor_bigbadvoodoo_fetish_red ||
                    actor.SnoActor.Sno == ActorSnoEnum._witchdoctor_bigbadvoodoo_fetish_yellow ||
                    actor.SnoActor.Sno == ActorSnoEnum._witchdoctor_bigbadvoodoo_fetish_purple))
                    ;
                    return (BigBadVoodooCount == 0);//未站在大巫毒内时施放
                }).ThenCastElseContinue()
                ;

        }
    }
}