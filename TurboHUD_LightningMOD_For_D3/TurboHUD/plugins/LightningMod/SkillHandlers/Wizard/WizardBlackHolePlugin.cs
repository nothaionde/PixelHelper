using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class WizardBlackHolePlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SpareResource { get; set; }
        private int Stacks = 0;
        public WizardBlackHolePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_BlackHole;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.StoneOfJordan).ThenNoCastElseContinue()//带乔丹时无效
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx => 80).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                ctx.Skill.Player.GetSetItemCount(84014) >= 6 && ctx.Skill.Player.Powers.UsedLegendaryPowers.Deathwish?.Active == true).ThenNoCastElseContinue()//火鸟6件时无效
                .IfTrue(ctx =>
                {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    bool IsElementReady = PublicClassPlugin.IsElementReady(hud, ctx.Skill.Rune == 4 ? 2 : 2, ctx.Skill.Player, CoeIndex);//法术窃取和绝对零度爆发前和爆发期间都可以使用
                    return IsElementReady;
                }).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var buff = ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Wizard_BlackHole.Sno);
                    if (buff != null)
                    {
                        Stacks = buff.IconCounts[8] != 0 ? buff.IconCounts[8] : buff.IconCounts[5];
                    }
                    return true;
                })
                .IfEnoughMonstersNearbyCursor(ctx => 15, ctx => Stacks).ThenContinueElseNoCast()
                .IfEnoughMonstersNearbyCursor(ctx => 15, ctx => 5).ThenCastElseContinue()
                .IfEliteOrBossNearbyCursor(ctx => 10).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfTrue(ctx => !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno) || ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneOfJordan.Sno)).ThenContinueElseNoCast()//不带元素戒或带乔丹时继续
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx =>0).ThenContinueElseNoCast()
                .IfTrue(ctx =>ctx.Skill.Rune == 3 || ctx.Skill.Rune == 4).ThenContinueElseNoCast()//法术窃取或绝对零度
                .IfEliteOrBossNearbyCursor(ctx => 10).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var buff = ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Wizard_BlackHole.Sno);
                    if (buff != null)
                    {
                        Stacks = buff.IconCounts[8] != 0 ? buff.IconCounts[8] : buff.IconCounts[5];
                    }
                    return true;
                })
                .IfEnoughMonstersNearbyCursor(ctx => 15, ctx => Stacks).ThenCastElseContinue()
                .IfBuffIsAboutToExpire(1000, 1500).ThenCastElseContinue()
                ;

        }
    }
}