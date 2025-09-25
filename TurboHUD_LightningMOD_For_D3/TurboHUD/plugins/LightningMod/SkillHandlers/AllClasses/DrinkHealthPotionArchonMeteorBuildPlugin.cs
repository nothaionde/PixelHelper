namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    using Turbo.Plugins.glq;

    public class DrinkHealthPotionArchonMeteorBuildPlugin : AbstractSkillHandler, ISkillHandler
    {
        public DrinkHealthPotionArchonMeteorBuildPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.Collect)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Generic_DrinkHealthPotion;


            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfTrue(IsPotionOnCoolDown).ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => Hud.Game.Me.Powers.BuffIsActive(488004) ||Hud.Game.Me.Powers.BuffIsActive(488036) || Hud.Game.Me.Powers.BuffIsActive(488037)).ThenContinueElseNoCast()//祭坛药水buff
                .IfTrue(ctx => isArchonMeteorBuild()).ThenContinueElseNoCast()//双黑法师专用
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wizard_Archon, 2).ThenNoCastElseContinue()//御法者激活时不施放
                .IfTrue(ctx => PublicClassPlugin.IsElementReadySoon(ctx.Hud, 2, ctx.Skill.Player, 1)).ThenCastElseContinue()//奥前2秒到奥元素，共2秒可施放
                ;
        }
        private bool IsPotionOnCoolDown(TestContext ctx)
        {
            bool IsOnCooldown;
            double Cooldown;
            Cooldown = (Hud.Game.Me.Powers.HealthPotionSkill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60d;
            IsOnCooldown = Cooldown <= 30 && Cooldown >= 0 ? true : false;
            return IsOnCooldown;
        }
        private bool isArchonMeteorBuild()
        {
            return Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheSwami.Sno) && //法尊
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.NilfursBoast.Sno) &&//尼芙尔的夸耀
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FazulasImprobableChain.Sno)//法祖拉的不可信之链
                ;
        }
    }
}