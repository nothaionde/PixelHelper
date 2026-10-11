using System.Linq;
using Turbo.Plugins.LightningMod;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class Monk_FistsOfThunderPlugin : AbstractSkillHandler, ISkillHandler
    {
        public Monk_FistsOfThunderPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_FistsOfThunder;
            CreateCastRule()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if (ctx.Skill.Player.GetSetItemCount(755275) >= 6 && ctx.Skill.Player.GetSetItemCount(563257) >= 2)
                    {
                        return true;
                    }
                    return false;
                })
                .ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if (Hud.Game.Me.Animation.ToString().Contains("_rapidstrikes_"))
                    {
                        return true;
                    }
                    return false;
                })
                .ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var comboBuff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Monk_Passive_CombinationStrike.Sno);
                    if (comboBuff == null || !comboBuff.Active)
                        return true;

                    int targetIndex = 4;
                    if (targetIndex < comboBuff.TimeLeftSeconds.Length)
                    {
                        double timeLeft = comboBuff.TimeLeftSeconds[targetIndex];
                        int count = comboBuff.IconCounts[targetIndex];

                        if (count <= 0 || timeLeft < 1.5)
                            return true;
                    }
                    return false;
                })
                .ThenCastElseContinue()
                ;
        }
    }
}