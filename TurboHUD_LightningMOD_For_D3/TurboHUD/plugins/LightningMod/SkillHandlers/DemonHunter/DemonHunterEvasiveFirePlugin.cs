using System.Linq;
using System.Collections.Generic;
namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterEvasiveFirePlugin : AbstractSkillHandler, ISkillHandler
	{
        private List<uint> SpiketrapIds = new List<uint>{};
        private List<uint> OldIds = new List<uint> {};
        public DemonHunterEvasiveFirePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        private int getSpikeTrapMax()
        {
            int i = 4;
            if(Hud.Game.Me.Powers.UsedDemonHunterPowers.CustomEngineering != null)
            {
                i = 5;
            }
            if(Hud.Game.Me.Powers.UsedLegendaryPowers.TragOulCoils?.Active == true)
            {
                i = i * 2;
            }

            return i;
        }



        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_EvasiveFire;
            //保持对戒
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue() 
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 0 || ctx.Skill.Rune == 4) && ctx.Skill.Player.Powers.BuffIsActive(359583, 0)).ThenContinueElseNoCast()//强化硬甲或凝神射击 且 装备守心克己
                .IfEnoughMonstersInSector(ctx => 40, ctx => 80, ctx => ctx.Hud.Window.Size.Height / 11.33333f, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.Generic_ItemPassiveUniqueRing735x1, 1, 30, 100).ThenCastElseContinue()
                ;
            //保持明彻裹腕
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 0 || ctx.Skill.Rune == 4) && Hud.Game.Me.Powers.UsedLegendaryPowers.WrapsOfClarity?.Active == true).ThenContinueElseNoCast()//强化硬甲或凝神射击 且 装备明彻裹腕
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable, 0).ThenNoCastElseContinue()//护盾
                .IfEnoughMonstersNearby(ctx => 200, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(hud.Sno.SnoPowers.WrapsOfClarity, 1, 30, 300).ThenCastElseContinue()
                ;
            //保持憎恨
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 0 || ctx.Skill.Rune == 4)).ThenContinueElseNoCast()//强化硬甲或凝神射击
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting, 0).ThenNoCastElseContinue()//减耗
                .IfPrimaryResourcePercentageIsBelow(20).ThenCastElseContinue()
                ;
            //保持戒律
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 0 || ctx.Skill.Rune == 4) && Hud.Game.Me.GetSetItemCount(254164) >= 2).ThenContinueElseNoCast()//强化硬甲或凝神射击 且不洁两件套及以上
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting, 0).ThenNoCastElseContinue()//减耗
                .IfSecondaryResourcePercentageIsBelow(33).ThenCastElseContinue()
                ;

            //娜塔亚尖刺陷阱
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 0 || ctx.Skill.Rune == 4) && Hud.Game.Me.GetSetItemCount(635131) >= 2).ThenContinueElseNoCast()//强化硬甲或凝神射击 且娜塔亚两件套及以上
                .IfTrue(ctx => ctx.Skill.Player.Powers.UsedDemonHunterPowers.SpikeTrap?.Rune == 2).ThenContinueElseNoCast()//尖刺陷阱手动引爆符文
                .IfTrue(ctx =>
                {
                    //模拟计数地面未引爆的雷
                    var Actors = Hud.Game.Actors.Where(A => A.SnoActor.Sno == ActorSnoEnum._demonhunter_spiketrap_proxy && A.SummonerAcdDynamicId == ctx.Skill.Player.SummonerId);
                    var SpikeTraps = Actors.Where(A => !OldIds.Contains(A.AnnId) && !SpiketrapIds.Contains(A.AnnId));
                    foreach (var SpikeTrap in SpikeTraps)
                    {
                        SpiketrapIds.Add(SpikeTrap.AnnId);
                    }
                    if(SpiketrapIds.Count() >= getSpikeTrapMax())
                    {
                        foreach(var ids in SpiketrapIds)
                        {
                            OldIds.Add(ids);

                        }
                        SpiketrapIds.Clear();
                        for(int i = OldIds.Count - 1; i >= 0; i--)
                        {
                            if (Actors.Any(x => x.AnnId == OldIds[i]) == false)
                            {
                                OldIds.Remove(OldIds[i]);
                            }
                        }
                        return true;
                    }
                    return false;
                }).ThenContinueElseNoCast()//尖刺陷阱满了后施放
                .IfCanCastSkill(500, 500 ,5000).ThenCastElseContinue()//间隔0.5秒避免卡陷阱技能
                ;
        }
    }
}