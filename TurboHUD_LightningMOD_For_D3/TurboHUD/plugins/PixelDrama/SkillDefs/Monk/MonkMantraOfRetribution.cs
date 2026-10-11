using System.Linq;
using Turbo.Plugins.LightningMod;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class MonkMantraOfRetributionPlugin : AbstractSkillHandler, ISkillHandler
	{
        public MonkMantraOfRetributionPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud); 
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_MantraOfRetribution;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx => 50).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Monk_Passive_MantraOfRetributionV2, 1, 50, 500).ThenCastElseContinue()
                ;
        }
    }
} 