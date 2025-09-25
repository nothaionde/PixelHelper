using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class WizardMirrorImagePlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardMirrorImagePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_MirrorImage;
            CreateCastRule()//对精英
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wizard_MirrorImage, 0).ThenNoCastElseContinue()
                .IfTrue(ctx => 
                ctx.Skill.Player.GetSetItemCount(84014) >= 6).ThenContinueElseNoCast()//火鸟6件
                .IfTrue(ctx =>
                {
                    double coe = PublicClassPlugin.GetHighestElementLeftSecond(Hud, ctx.Skill.Player, 1);
                    bool isCOE = ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno);
                    return (!isCOE || (isCOE && coe <= 11 && coe >= 0));//冰3~~电0
                }
                ).ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx => 15, false).ThenCastElseContinue()
                ;

            CreateCastRule()//对白怪
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wizard_MirrorImage, 0).ThenNoCastElseContinue()
                .IfTrue(ctx =>
                ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno) &&
                ctx.Skill.Player.GetSetItemCount(84014) >= 6).ThenContinueElseNoCast()//元素戒+火鸟6件
                .IfTrue(ctx =>
                {
                    double coe = PublicClassPlugin.GetHighestElementLeftSecond(Hud, ctx.Skill.Player, 1);
                    return (coe <= 9 && coe >= 0);//冰1~电0
                }
                ).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 15, ctx => 10).ThenCastElseContinue()
                ;
        }
    }
}