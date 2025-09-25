namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    using Turbo.Plugins.glq;
    public class DemonHunterCompanionPlugin : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterCompanionPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Companion;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction >= 0.69 || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno)).ThenCastElseContinue()//69CDR或黄道时持续保持
                .IfTrue(ctx => ctx.Skill.Rune == 3 && ctx.Skill.Player.Stats.ResourcePctHatred < 15//蝙蝠符文在憎恨低于15%时使用
                ).ThenCastElseContinue()
                .IfTrue(ctx => (ctx.Skill.Rune == 1 || ctx.Skill.Rune == 0 || ctx.Skill.Rune == 255) && ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40)//野猪蜘蛛符文在40码内有精英或Boss时使用
                ).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    int PartyCoeIndex = Hud.GetPlugin<PublicClassPlugin>().PartyCoeIndex;
                    if (ctx.Skill.Rune != 2 && Hud.Game.Me.GetSetItemCount(254427) < 2) return false;//非战狼符文且不带掠夺套
                    bool _cast;
                    var DPSPlayer = ctx.Hud.Game.Players.FirstOrDefault(p => p.InGreaterRift &&
                p.Powers.UsedLegendaryPowers.ConventionOfElements?.Active == true//元素戒指
                );

                    if (DPSPlayer != null)
                    {
                        double CoeLeftTime = PublicClassPlugin.GetHighestElementLeftSecondAssingedPlayer(Hud, DPSPlayer, PartyCoeIndex);//获取离队伍DPS最高元素倒计时
                        _cast = CoeLeftTime < 6 && CoeLeftTime > 0;//爆发元素前6秒
                    }
                    else if (Hud.Game.Me.Powers.BuffIsActive(430674))//元素戒
                    {
                        double CoeLeftTime = PublicClassPlugin.GetHighestElementLeftSecond(Hud, ctx.Skill.Player, CoeIndex);//获取离自己最高元素倒计时
                        _cast = CoeLeftTime < 6 && CoeLeftTime > 0 && ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40);
                    }
                    else
                    {
                        _cast = ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40, false);//遭遇精英时施放
                    }
                    return _cast;
                }).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 4 && ctx.Hud.Game.ActorQuery.NearestHealthGlobe != null && ctx.Hud.Game.ActorQuery.NearestHealthGlobe.NormalizedXyDistanceToMe <= 60 && ctx.Skill.Player.Defense.HealthPct < 60//雪貂符文在60码内有血球并且生命低于60%时使用
                ).ThenCastElseContinue()
                .IfTrue(ctx => Hud.Game.Me.GetSetItemCount(254427) >= 2 && !Hud.Game.Me.Powers.BuffIsActive(430674) &&(ctx.Skill.Player.Stats.ResourcePctHatred < 15 || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40) || (ctx.Hud.Game.ActorQuery.NearestHealthGlobe != null && ctx.Hud.Game.ActorQuery.NearestHealthGlobe.NormalizedXyDistanceToMe <= 60 && ctx.Skill.Player.Defense.HealthPct < 60))//掠夺套且不带元素戒时
                ).ThenCastElseContinue()
                ;
        }
    }
}