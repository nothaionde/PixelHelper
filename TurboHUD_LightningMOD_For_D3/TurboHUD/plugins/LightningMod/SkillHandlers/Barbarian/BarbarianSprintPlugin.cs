namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    public class BarbarianSprintPlugin : AbstractSkillHandler, ISkillHandler
    {
        public BarbarianSprintPlugin()
            : base(CastType.BuffSkill,  CastPhase.AutoCast, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_Sprint;

            CreateCastRule()//其他符文
                .IfTrue(ctx => ctx.Skill.Rune != 3).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfCanCastSkill(300, 400, 500).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(40, ctx => 20).ThenContinueElseNoCast()
                .IfRunning(true).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100,300).ThenCastElseContinue()//BUFF即将消失时自动保持BUFF
                ;

            CreateCastRule()//急行军符文
                .IfTrue(ctx => ctx.Skill.Rune == 3).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfCanCastSkill(300, 400, 500).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(40, ctx => 0).ThenContinueElseNoCast()
                .IfBossIsNearby(ctx => 50).ThenNoCastElseContinue()//BOSS在附近时不施放
                .IfTrue(ctx =>
                {
                    var players = ctx.Hud.Game.Players.Where(p => !p.IsDead && p.SnoArea.Sno == ctx.Hud.Game.Me.SnoArea.Sno && p.CentralXyDistanceToMe <= 200);
                    return players.All(p => p.CentralXyDistanceToMe <= 50 && !p.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno));//队友都在50码内
                }).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(500, 1000).ThenCastElseContinue()//BUFF即将消失时自动保持BUFF
                .IfPrimaryResourceIsEnough(70, ctx => 0).ThenContinueElseNoCast()//70%怒气以上
                .IfSpecificBuffIsAboutToExpireOnParty(Hud.Sno.SnoPowers.Barbarian_Sprint, 0, 2000, 2000, HeroClass.All, 50).ThenCastElseContinue()//任意队友加速BUFF少于2秒时施放
                ;
        }
    }
}