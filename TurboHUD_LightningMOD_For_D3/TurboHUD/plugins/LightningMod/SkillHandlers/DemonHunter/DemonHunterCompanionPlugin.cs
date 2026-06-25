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
                .IfCanCastSimple().ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction >= 0.69 || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno)).ThenCastElseContinue()//69CDR»ò»ÆµÀÊ±³ÖÐø±£³Ö
                .IfTrue(ctx => ctx.Skill.Rune == 3 && ctx.Skill.Player.Stats.ResourcePctHatred < 15//òùòð·ûÎÄÔÚÔ÷ºÞµÍÓÚ15%Ê±Ê¹ÓÃ
                ).ThenCastElseContinue()
                .IfTrue(ctx => (ctx.Skill.Rune == 1 || ctx.Skill.Rune == 0 || ctx.Skill.Rune == 255) && ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40)//Ò°ÖíÖ©Öë·ûÎÄÔÚ40ÂëÄÚÓÐ¾«Ó¢»òBossÊ±Ê¹ÓÃ
                ).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    int PartyCoeIndex = Hud.GetPlugin<PublicClassPlugin>().PartyCoeIndex;
                    if (ctx.Skill.Rune != 2 && Hud.Game.Me.GetSetItemCount(254427) < 2) return false;//·ÇÕ½ÀÇ·ûÎÄÇÒ²»´øÂÓ¶áÌ×
                    bool _cast;
                    var DPSPlayer = ctx.Hud.Game.Players.FirstOrDefault(p => p.InGreaterRift &&
                p.Powers.UsedLegendaryPowers.ConventionOfElements?.Active == true//ÔªËØ½äÖ¸
                );

                    if (DPSPlayer != null)
                    {
                        double CoeLeftTime = PublicClassPlugin.GetHighestElementLeftSecondAssingedPlayer(Hud, DPSPlayer, PartyCoeIndex);//»ñÈ¡Àë¶ÓÎéDPS×î¸ßÔªËØµ¹¼ÆÊ±
                        _cast = CoeLeftTime < 6 && CoeLeftTime > 0;//±¬·¢ÔªËØÇ°6Ãë
                    }
                    else if (Hud.Game.Me.Powers.BuffIsActive(430674))//ÔªËØ½ä
                    {
                        double CoeLeftTime = PublicClassPlugin.GetHighestElementLeftSecond(Hud, ctx.Skill.Player, CoeIndex);//»ñÈ¡Àë×Ô¼º×î¸ßÔªËØµ¹¼ÆÊ±
                        _cast = CoeLeftTime < 6 && CoeLeftTime > 0 && ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40);
                    }
                    else
                    {
                        _cast = ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40, false);//ÔâÓö¾«Ó¢Ê±Ê©·Å
                    }
                    return _cast;
                }).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 4 && ctx.Hud.Game.ActorQuery.NearestHealthGlobe != null && ctx.Hud.Game.ActorQuery.NearestHealthGlobe.NormalizedXyDistanceToMe <= 60 && ctx.Skill.Player.Defense.HealthPct < 60//Ñ©õõ·ûÎÄÔÚ60ÂëÄÚÓÐÑªÇò²¢ÇÒÉúÃüµÍÓÚ60%Ê±Ê¹ÓÃ
                ).ThenCastElseContinue()
                .IfTrue(ctx => Hud.Game.Me.GetSetItemCount(254427) >= 2 && !Hud.Game.Me.Powers.BuffIsActive(430674) &&(ctx.Skill.Player.Stats.ResourcePctHatred < 15 || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(40) || (ctx.Hud.Game.ActorQuery.NearestHealthGlobe != null && ctx.Hud.Game.ActorQuery.NearestHealthGlobe.NormalizedXyDistanceToMe <= 60 && ctx.Skill.Player.Defense.HealthPct < 60))//ÂÓ¶áÌ×ÇÒ²»´øÔªËØ½äÊ±
                ).ThenCastElseContinue()
                ;
        }
    }
}