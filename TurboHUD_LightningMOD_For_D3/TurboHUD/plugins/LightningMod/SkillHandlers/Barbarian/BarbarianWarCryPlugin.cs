using System.Linq;

namespace Turbo.Plugins.LightningMod
{
    public class BarbarianWarCryPlugin : AbstractSkillHandler, ISkillHandler
	{
        public int CastBelowFuryPercentage { get; set; }

        public BarbarianWarCryPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack, CastPhase.Attack)
        {
            Enabled = false;
            CastBelowFuryPercentage = 50;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_WarCry;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(500, 1000).ThenCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ChilaniksChain).ThenContinueElseNoCast()//加速腰带
                .IfPrimaryResourcePercentageIsBelow(CastBelowFuryPercentage).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.GetSetItemCount(786990) < 6 && ctx.Hud.Game.Players.All(p => !p.IsDead && p.HasValidActor && p.CentralXyDistanceToMe <= 100 && !p.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno)) && ctx.Hud.Game.ActorQuery.NearestBoss == null;//队友都在100码内且不是灵魂状态,BOSS不在附近时才生效
                }
                ).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Ingeom.Sno) && (ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno))).ThenContinueElseNoCast()//寅剑+黄道或梅斧
                .IfSpecificBuffIsAboutToExpireOnParty(Hud.Sno.SnoPowers.ChilaniksChain, 1, 8000, 8000, HeroClass.All, 100).ThenCastElseContinue()//任意队友腰带加速BUFF少于8秒时施放
                ;

            CreateCastRule()//旋风套时持续保持加速BUFF
                .IfInTown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(786990) >= 6).ThenContinueElseNoCast()//旋风套
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ChilaniksChain).ThenContinueElseNoCast()//加速腰带
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.ChilaniksChain, 1, 300, 500).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 0).ThenContinueElseNoCast()//怒气御体符文继续往下判断
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpireOnParty(Hud.Sno.SnoPowers.Barbarian_WarCry, 1, 200, 500, HeroClass.All, 100).ThenCastElseContinue()
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.BladeOfTheTribes).ThenContinueElseNoCast()//部族之刃
                .IfTrue(ctx =>
                {
                    var Animation = Hud.Game.Me.Animation;
                    return 
                    Animation == AnimSnoEnum._barbarian_female_dw_leap_attack_contactend ||
                    Animation == AnimSnoEnum._barbarian_male_dw_eq_contactend ||
                    Animation == AnimSnoEnum._barbarian_male_1hs_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_1ht_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_2hs_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_2ht_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_dw_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_stf_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_female_1hs_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_1ht_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_2hs_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_2ht_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_dw_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_stf_seismic_slam
                    ;//挑战或裂地斩
                }
                ).ThenCastElseContinue()
                ;
        }
    }
}