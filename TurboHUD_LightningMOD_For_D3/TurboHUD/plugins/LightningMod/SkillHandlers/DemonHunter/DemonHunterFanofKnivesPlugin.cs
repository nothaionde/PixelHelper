using System.Linq;
using System.Collections.Generic;
namespace Turbo.Plugins.LightningMod
{
    public class DemonHunterFanofKnivesPlugin : AbstractSkillHandler, ISkillHandler
	{
        private List<uint> SpiketrapIds = new List<uint> { };
        private List<uint> OldIds = new List<uint> { };
        public DemonHunterFanofKnivesPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }
        private int getSpikeTrapMax()
        {
            int i = 4;
            if (Hud.Game.Me.Powers.UsedDemonHunterPowers.CustomEngineering != null)
            {
                i = 5;
            }
            if (Hud.Game.Me.Powers.UsedLegendaryPowers.TragOulCoils?.Active == true)
            {
                i = i * 2;
            }

            return i;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_FanOfKnives;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 4).ThenContinueElseNoCast()//刀刃护甲
                .IfBuffIsAboutToExpire(100, 200).ThenContinueElseNoCast()
                .IfEnoughMonstersNearby(ctx => 40, ctx => 1).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction * 100 > 40).ThenCastElseContinue()//CDR大于40%
                ;
            //娜塔亚尖刺陷阱
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => (ctx.Skill.Rune == 0) && Hud.Game.Me.GetSetItemCount(635131) >= 6).ThenContinueElseNoCast()//刀扇大师且娜塔亚6件套
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Powers.UsedDemonHunterPowers.SpikeTrap?.Rune != 2).ThenContinueElseNoCast()//尖刺陷阱
                .IfTrue(ctx =>
                {
                    //模拟计数地面未引爆的雷
                    var Actors = Hud.Game.Actors.Where(A => (A.SnoActor.Sno == ActorSnoEnum._demonhunter_spiketrap_proxy || A.SnoActor.Sno == ActorSnoEnum._demonhunter_spiketraprune_multitrap_proxy || A.SnoActor.Sno == ActorSnoEnum._demonhunter_spiketraprune_damage_proxy || A.SnoActor.Sno == ActorSnoEnum._demonhunter_spiketraprune_chainlightning_proxy) && A.SummonerAcdDynamicId == ctx.Skill.Player.SummonerId);
                    var SpikeTraps = Actors.Where(A => !OldIds.Contains(A.AnnId) && !SpiketrapIds.Contains(A.AnnId));
                    foreach (var SpikeTrap in SpikeTraps)
                    {
                        SpiketrapIds.Add(SpikeTrap.AnnId);
                    }
                    if (SpiketrapIds.Count() >= getSpikeTrapMax())
                    {
                        foreach (var id in SpiketrapIds)
                        {
                            OldIds.Add(id);

                        }
                        SpiketrapIds.Clear();
                        for (int i = OldIds.Count - 1; i >= 0; i--)
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
                .IfCanCastSkill(500, 500, 5000).ThenCastElseContinue()//间隔0.5秒避免卡陷阱技能
                ;

        }
    }
}