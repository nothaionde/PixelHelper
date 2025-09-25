using Turbo.Plugins.glq;

namespace Turbo.Plugins.LightningMod
{
    public class WizardElectrocutePlugin : AbstractSkillHandler, ISkillHandler
	{
        public WizardElectrocutePlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_Electrocute;//电刑
            //保持对戒
            CreateCastRule()
                .IfCanCastSkill(100, 150, 500).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue() 
                .IfTrue(ctx=> ctx.Hud.Interaction.IsHotKeySet(ActionKey.Move) && ctx.Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)).ThenNoCastElseContinue()//按住强制移动时不生效
                .IfRunning().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(359583, 0)).ThenContinueElseNoCast()//装备守心克己
                .IfEnoughMonstersNearbyCursor(ctx => 10, ctx => 1).ThenContinueElseNoCast()//鼠标附近10码内至少有1个怪
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1, 1, 30, 100).ThenCastElseNoCast()
                ;
            //双黑奥陨电元素叠奥能迸发
            CreateCastRule()
                .IfCanCastSkill(50, 50, 500).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfTrue(ctx=> isArchonMeteorBuild())
                .IfTrue(ctx => ctx.Hud.Interaction.IsHotKeySet(ActionKey.Move) && ctx.Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)).ThenNoCastElseContinue()//按住强制移动时不生效
                .IfRunning().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wizard_Archon, 2).ThenNoCastElseContinue()//御法者激活时不施放
                .IfTrue(ctx => PublicClassPlugin.IsElementReadySoon(ctx.Hud, 3, ctx.Skill.Player, 1)).ThenContinueElseNoCast()//奥前3秒到奥元素，共3秒可继续
                .IfTrue(ctx => PublicClassPlugin.GetBuffCount(ctx.Hud, hud.Sno.SnoPowers.Wizard_Passive_ArcaneDynamo.Sno, 1) < 5).ThenCastElseNoCast()//奥能迸发不足5层时施放
            ;
        }
        private bool isArchonMeteorBuild()
        {
            return Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Wizard_Passive_ArcaneDynamo.Sno) && //奥能迸发
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheSwami.Sno) && //法尊
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.NilfursBoast.Sno) &&//尼芙尔的夸耀
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FazulasImprobableChain.Sno)//法祖拉的不可信之链
                ;
        }
    }
}