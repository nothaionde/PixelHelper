using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class WitchDoctorCorpseSpidersPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorCorpseSpidersPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_CorpseSpider;
            Rune = 1;
            CreateCastRule()
                .IfCanCastSkill(1000,1500,2000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfFalse(ctx =>
                {
                    bool nearbyCorpsespider = ctx.Hud.Game.Actors.Any(x => (x.SnoActor.Sno == ActorSnoEnum._witchdoctor_corpsespider_indigorune || x.SnoActor.Sno == ActorSnoEnum._p72_witchdoctor_corpsespider_indigorune) && x.SummonerAcdDynamicId == ctx.Hud.Game.Me.SummonerId);
                    return nearbyCorpsespider;
                }).ThenCastElseContinue()//判断尸蛛蛛后是否激活
                ;

        }
    }
}