using Turbo.Plugins.glq;

namespace Turbo.Plugins.LightningMod
{
    public class WizardWaveofForcePlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardWaveofForcePlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_WaveOfForce;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 500)
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.FazulasImprobableChain).ThenContinueElseNoCast()//装备法祖拉的不可信之链
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.TheSwami).ThenContinueElseNoCast()//装备法尊
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.TheGrandVizier).ThenContinueElseNoCast()//装备大维兹尔之杖
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac).ThenContinueElseNoCast()//装备黄道
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wizard_Archon, 2).ThenNoCastElseContinue()//御法者激活时不施放
                .IfTrue(ctx => PublicClassPlugin.GetCooldownTimerOfArchon(hud) >= 3).ThenContinueElseNoCast()//御法者CD≥3秒时
                .IfTrue(ctx => PublicClassPlugin.GetBuffCount(ctx.Hud, hud.Sno.SnoPowers.Wizard_Passive_ArcaneDynamo.Sno,1) < 5).ThenContinueElseNoCast()//奥能迸发低于5层时继续
                .IfEnoughMonstersNearby(ctx => 20, ctx => 1).ThenCastElseContinue()
                ;
        }

    }
}