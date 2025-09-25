using Turbo.Plugins.glq;

namespace Turbo.Plugins.LightningMod
{
    public class WizardExplosiveBlastPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WizardExplosiveBlastPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_ExplosiveBlast;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 500)
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => isArchonMeteorBuild()).ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => {
                    return ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.WandOfWoh.Sno) || //沃尔
                    (ctx.Skill.Player.Stats.ResourcePctPri >= 70 && !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno)) || //法力高于70%且不装备黄道
                    (ctx.Skill.Player.GetSetItemCount(84014) >= 6 && ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.Deathwish.Sno))//火鸟6+绝命
                    ;
                }).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(GetBlastRange, ctx => 1).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(GetBlastRange, true).ThenCastElseContinue();

            CreateCastRule()
                //保持无尽深渊BUFF
                .IfSpecificBuffIsActive(hud.Sno.SnoPowers.WandOfWoh).ThenNoCastElseContinue()
                .IfCanCastSkill(100, 150, 500)
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(hud.Sno.SnoPowers.OrbOfInfiniteDepth).ThenContinueElseNoCast()//无尽深渊法珠
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.Wizard_ExplosiveBlast, 1, 1700 ,2000, false, 4).ThenContinueElseNoCast()
                .IfTrue(ctx => PublicClassPlugin.GetBuffCount(ctx.Hud, hud.Sno.SnoPowers.Wizard_Passive_ArcaneDynamo.Sno, 1) == 5).ThenNoCastElseContinue()//奥能迸发5层时不施放
                .IfEnoughMonstersNearby(GetBlastRange, ctx => 1).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(GetBlastRange, true).ThenCastElseContinue();
            CreateCastRule()
                //双黑奥陨电元素自动施放
                .IfCanCastSkill(100, 150, 500)
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => isArchonMeteorBuild()).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wizard_Archon, 2).ThenNoCastElseContinue()//御法者激活时不施放
                //.IfTrue(ctx => PublicClassPlugin.IsElementReady(ctx.Hud, 4, ctx.Skill.Player,1)).ThenContinueElseNoCast()//奥前4秒到奥元素，共8秒可施放
                .IfTrue(ctx =>
                {
                    var coe = PublicClassPlugin.GetBuffLeftTime(ctx.Hud, Hud.Sno.SnoPowers.ConventionOfElements.Sno, 5);//电元素倒计时
                    return coe >= 2 && coe <= 3;
                }).ThenContinueElseNoCast()//电4-3施
                .IfEnoughMonstersNearby(GetBlastRange, ctx => 1).ThenCastElseContinue()
                .IfEliteOrBossIsNearby(GetBlastRange, true).ThenCastElseContinue();
        }

        private bool isArchonMeteorBuild()
        {
            return Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheSwami.Sno) && //法尊
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.OrbOfInfiniteDepth.Sno) && //无尽深渊法珠
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.NilfursBoast.Sno) &&//尼芙尔的夸耀
                Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FazulasImprobableChain.Sno)//法祖拉的不可信之链
                ;
        }
        private int GetBlastRange(TestContext ctx)
        {
            switch (ctx.Skill.Rune)
            {
                case 0: return 12; // short fuse
                case 1: return 18 + (int)((20 * (1 + Hud.Game.Me.Stats.MoveSpeed / 100)) * 1.5); // obliterate
                default: return 12 + (int)((20 * (1 + Hud.Game.Me.Stats.MoveSpeed / 100)) * 1.5); // everything else
            }
        }
    }
}