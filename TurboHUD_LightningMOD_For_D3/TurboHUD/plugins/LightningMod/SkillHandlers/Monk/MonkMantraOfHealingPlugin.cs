using System;
namespace Turbo.Plugins.LightningMod
{
    public class MonkMantraOfHealingPlugin : AbstractSkillHandler, ISkillHandler
	{
        public MonkMantraOfHealingPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud); 
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_MantraOfHealing;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfIdle().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx => 50).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    bool isAquilaCuirass = ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.AquilaCuirass.Sno);
                    if(isAquilaCuirass)
                    {
                        if(((1 - Math.Ceiling(ctx.Skill.GetResourceRequirement())) / ctx.Hud.Game.Me.Stats.ResourceMaxSpirit) * 100 > 95)
                        {
                            return true;//满足保持天鹰能量
                        }
                    }
                    else
                    {
                        if(ctx.Hud.Game.Me.Stats.ResourcePctSpirit > 90)
                        {
                            return true;//不穿天鹰时，能量高于90%则施放
                        }
                    }
                    return false;
                }
                ).ThenCastElseContinue()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Monk_Passive_MantraOfHealingV2, 1, 50, 500).ThenCastElseContinue()
                .IfSpecificBuffIsAboutToExpireOnParty(Hud.Sno.SnoPowers.Monk_Passive_MantraOfHealingV2, 1, 50, 500, Hud.Game.Me.Stats.ResourcePctSpirit >= 30 ? HeroClass.All : HeroClass.WitchDoctor, 60).ThenCastElseContinue()
                .IfSpecificBuffIsAboutToExpireOnParty(Hud.Sno.SnoPowers.Monk_Passive_MantraOfHealingV2, 1, 50, 500, Hud.Game.Me.Stats.ResourcePctSpirit >= 30 ? HeroClass.All : HeroClass.Necromancer, 60).ThenCastElseContinue()
                ;
        }
    }
} 