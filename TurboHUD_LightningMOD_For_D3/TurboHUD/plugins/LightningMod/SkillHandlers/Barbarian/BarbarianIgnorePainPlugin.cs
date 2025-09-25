namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class BarbarianIgnorePainPlugin : AbstractSkillHandler, ISkillHandler
	{
        private IPlayer DPS = null;
        private IPlayer shawang = null;
        public BarbarianIgnorePainPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_IgnorePain;

            CreateCastRule()//同仇敌忾
                .IfTrue(ctx => ctx.Skill.Rune == 2).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfHealthWarning(30, 60).ThenCastElseContinue()
                .IfNearbyPartyMemberIsInDanger(48, 30, 60, 40, true).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    var players = ctx.Hud.Game.Players.Where(p => !p.IsDead && p.SnoArea.Sno == ctx.Hud.Game.Me.SnoArea.Sno && p.CentralXyDistanceToMe <= 200);
                    return ctx.Skill.Rune == 2 && players.All(p => p.CentralXyDistanceToMe <= 50 && !p.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno));//队友都在50码内且不是灵魂状态
                }
                ).ThenCastElseContinue()
                .IfSpecificBuffIsActiveOnParty(Hud.Sno.SnoPowers.StoneGauntlets, 2, HeroClass.All, 50).ThenCastElseContinue()//岩石护手debuff
                .IfTrue(ctx =>
                {
                    if (ctx.Skill.Player.InGreaterRiftRank == 0) return false;//不在大秘境时不执行
                    var players = ctx.Hud.Game.Players.Where(p => !p.IsMe && !p.IsDead && !p.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno) &&//除了自己以外活着的队友
                    p.SnoArea.Sno == ctx.Hud.Game.Me.SnoArea.Sno && p.CoordinateKnown && //附近有效位置
                    p.Powers.UsedLegendaryGems.EsotericAlterationPrimary?.Active == false && //没有带转煞
                    p.Powers.UsedLegendaryGems.GemOfEfficaciousToxinPrimary?.Active == false && //没有带剧毒
                    p.Powers.UsedLegendaryPowers.OculusRing?.Active == false);//没有带神目
                    if (players == null) return false;
                    shawang = players.FirstOrDefault(p => p.Powers.UsedLegendaryGems.BaneOfTheStrickenPrimary?.Active == true);//带受罚的人是杀王位
                    if (shawang == null)
                    {
                        shawang = players.OrderByDescending(p => p.Offense.SheetDps).FirstOrDefault();//没有带受罚的人时取面板最高的人是杀王位
                    }
                    DPS = players.Where(p => p.Powers.UsedLegendaryGems.BaneOfTheStrickenPrimary?.Active == false).OrderByDescending(p => p.Offense.SheetDps).FirstOrDefault();//不带受罚且DPS最高的人是进度位
                    if (shawang == null)
                    {
                        DPS = players.OrderByDescending(p => p.Offense.SheetDps).FirstOrDefault();//都带受罚时取面板最高的人为进度位
                    }
                    bool cast = false;
                    if (Hud.Game.RiftPercentage < 100)
                    {
                        if(DPS != null && DPS.CoordinateKnown && DPS.CentralXyDistanceToMe < 50)//进度阶段进度位在50码内
                        {
                            cast = true;
                        }
                    }else
                    {
                        if(shawang != null && shawang.CoordinateKnown && shawang.CentralXyDistanceToMe < 50)//杀王阶段杀王位在50码内
                        {
                            cast = true;
                        }
                    }
                        return ctx.Skill.Rune == 2 && cast == true;//杀王或进度位在50码内时施放
                }
                ).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Ingeom.Sno) && ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.PrideOfCassius.Sno) && (ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno))).ThenContinueElseNoCast()//寅剑+卡修斯之傲+黄道或梅斧
                .IfSpecificBuffIsAboutToExpireOnParty(Hud.Sno.SnoPowers.Barbarian_IgnorePain, 0, 8000, 8000, HeroClass.All, 50).ThenCastElseContinue()//任意队友无视苦痛BUFF少于8秒时施放
                ;

            CreateCastRule()//威风八面符文
                .IfTrue(ctx => ctx.Skill.Rune == 3).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100, 500).ThenCastElseContinue()
                ;

            CreateCastRule()//其他符文
                .IfTrue(ctx => ctx.Skill.Rune != 2).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfHealthWarning(40, 80).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune != 3 && ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno, 0)).ThenNoCastElseContinue()//除威风八面符文时开启护盾塔后不使用
                .IfTrue(ctx =>
                ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.PrideOfCassius.Sno)//卡修斯之傲或黄道或梅斧
                ).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100,500).ThenCastElseContinue()
                ;
        }
    }
}