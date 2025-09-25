using System.Linq;
using Turbo.Plugins.LightningMod;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class CrusaderSteedChargePlugin : AbstractSkillHandler, ISkillHandler
    {
        public bool NotWorkForDrawandQuarter { get; set; }
        private bool initialized = false;
        private IFont debugFont;
        public CrusaderSteedChargePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_SteedCharge;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    var skillsToWaitFor = new[]
                    {
                        Hud.Sno.SnoPowers.Crusader_AkaratsChampion,
                        Hud.Sno.SnoPowers.Crusader_IronSkin,
                        Hud.Sno.SnoPowers.Crusader_LawsOfHope,
                        Hud.Sno.SnoPowers.Crusader_LawsOfValor,
                    };
                    return skillsToWaitFor.Any(sno =>
                    {
                        var skill = ctx.Skill.Player.Powers.GetUsedSkill(sno);
                        return skill != null && !skill.IsOnCooldown;
                    });
                }).ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()
                .IfTrue(ctx => NotWorkForDrawandQuarter && ctx.Skill.Rune == 4).ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Darklight.Sno) && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.VigilanteBelt.Sno)).ThenNoCastElseContinue()
                .ThenCastElseContinue()
                ;
        }
    }
}