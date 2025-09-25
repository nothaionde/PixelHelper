using System.Linq;
using Turbo.Plugins.LightningMod;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class CrusaiderMovable : AbstractSkillHandler, ISkillHandler
	{
        public CrusaiderMovable()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_SteedCharge;
            CreateCastRule()
                .IfInTown()
                .ThenContinueElseNoCast()
                .IfCastingIdentify()
                .ThenNoCastElseContinue()
                .IfOnCooldown()
                .ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    var AnimationState = Hud.Game.Me.AnimationState;
                    return AnimationState == AcdAnimationState.Running;
                })
                .ThenCastElseContinue();
        }
    }

    public class DemonHunterSmokeScreenMovable : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterSmokeScreenMovable()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_SmokeScreen;
            CreateCastRule()
                .IfInTown()
                .ThenContinueElseNoCast()
                .IfCastingIdentify()
                .ThenNoCastElseContinue()
                .IfOnCooldown()
                .ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    var AnimationState = Hud.Game.Me.AnimationState;
                    return AnimationState == AcdAnimationState.Running;
                })
                .ThenCastElseContinue();
        }
    }
}