namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    public class MonkSerenityPlugin : AbstractSkillHandler, ISkillHandler
	{
        public MonkSerenityPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_Serenity;
            CreateCastRule()
                .IfCanCastSkill(50, 100, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()//100码内至少1个怪
                .IfTrue(ctx =>//宁静致远
                {
                    return ctx.Skill.Rune == 3;
                }
                ).ThenContinueElseNoCast()
                .IfNearbyPartyMemberIsInDanger(45, 60, 80, 70, true).ThenCastElseContinue()//队友危险时施放
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction >= 0.7).ThenContinueElseNoCast()//CD超过70%时继续
                .IfTrue(ctx =>
                {
                    var players = ctx.Hud.Game.Players.Where(p => (p.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor || p.HeroClassDefinition.HeroClass == HeroClass.Necromancer || p.HeroClassDefinition.HeroClass == HeroClass.Crusader || p.HeroClassDefinition.HeroClass == HeroClass.Wizard) && 
                    !p.IsDead && p.InGreaterRift);//死灵或巫医或圣教军或法师
                    if (players == null)
                    {//没有死灵或巫医或圣教军或法师的时候则保护其他队友
                        players = ctx.Hud.Game.Players.Where(p => !p.IsDead && p.InGreaterRift);//其他玩家
                    }
                    if (players == null)
                    {
                        return false;//没有队友时返回false
                    }
                    bool CloseToThePlayer = false;
                    foreach (var player in players)
                    {
                        if(player.CentralXyDistanceToMe <= 45)
                        {
                            //优先保护的队友在45码内
                            CloseToThePlayer = true;
                            break;
                        }
                    }
                    return CloseToThePlayer;
                }).ThenCastElseContinue()
                ;
            CreateCastRule()//所有符文
                .IfCanCastSkill(50, 100, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.BuffIsActive).ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 60, ctx => 1).ThenContinueElseNoCast()//60码内至少1个怪
                .IfHealthWarning(60, 80).ThenCastElseContinue()//血量过低自动施放
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable).ThenNoCastElseContinue()//护盾塔不生效
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements).ThenNoCastElseContinue()//带元素戒指不生效
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.SquirtsNecklace.Sno) || Hud.Game.Me.Stats.CooldownReduction >= 0.5 && (
                ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Ingeom.Sno) || 
                ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno) || 
                ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting.Sno) ||
                ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno))).ThenCastElseContinue()//CDR大于50%且装备寅剑或梅斧或黄道或吃了减耗塔时施放
                ;
        }
    }
}