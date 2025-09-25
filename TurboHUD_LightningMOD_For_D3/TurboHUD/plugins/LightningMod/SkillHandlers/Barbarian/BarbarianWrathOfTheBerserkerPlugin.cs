using System.Linq;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class BarbarianWrathOfTheBerserkerPlugin : AbstractSkillHandler, ISkillHandler
    {
        public BarbarianWrathOfTheBerserkerPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting, 0, 500, 2000, true).ThenCastElseContinue()//减耗塔结束前施放一次
                .IfBuffIsAboutToExpire(300, 500).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    bool isCOE = ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno);
                    double HighestElementLeft = PublicClassPlugin.GetHighestElementLeftSecond(hud, ctx.Skill.Player, CoeIndex);
                    var IKset = Hud.Game.Me.GetSetItemCount(671068) >= 4;//不朽4件套
                    return (IKset) || (Hud.Game.Me.Powers.UsedPassives.Any(p => p.Sno == Hud.Sno.SnoPowers.Barbarian_Passive_BoonOfBulKathos.Sno) && ctx.Skill.Player.Stats.CooldownReduction >= 0.75) //布尔凯索的恩泽被动 且 CDR高于75
                    || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) //黄道
                    
                    || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno) //梅斧
                    ||(isCOE ? ((HighestElementLeft <= 16 && HighestElementLeft >= 15) || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Ingeom.Sno)) && Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40, false) : Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40, false)) //遭遇精英或BOSS，装备元素戒指时只在爆发前1秒施放
                    || (ctx.Skill.Player.Defense.HealthPct <= 30)//血量低于30%
                        ;
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    bool isCOE = ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno);
                    bool NinetySet = Hud.Game.Me.GetSetItemCount(397674) >= 6;
                    bool RaekorSet = Hud.Game.Me.GetSetItemCount(749637) >= 6;
                    return (NinetySet || RaekorSet) && isCOE && Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40, false) && PublicClassPlugin.IsElementReady(ctx.Hud, 0.1, ctx.Skill.Player, CoeIndex) //遭遇精英或BOSS，装备元素戒指时只在爆发前0.1秒施放
                        ;
                }).ThenCastElseContinue()
                ;

        }
    }
}