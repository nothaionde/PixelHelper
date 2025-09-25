using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class WizardDisintegratePlugin : AbstractSkillHandler, ISkillHandler
    {
        public int Firebird { get; set; }
        public WizardDisintegratePlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
            Firebird = 44;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_Disintegrate;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfRunning().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(84014) >= 6).ThenContinueElseNoCast()//火鸟6件套
                .IfTrue(ctx => ctx.Skill.Player.InGreaterRiftRank > 0 && IsGuardianDead).ThenNoCastElseContinue()
                .IfTrue(ctx => (ctx.Hud.Interaction.IsHotKeySet(ActionKey.Move) && ctx.Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)) || ctx.Hud.Interaction.IsContinuousActionStarted(ctx.Skill.Player.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Wizard_Disintegrate).Key)).ThenNoCastElseContinue()//按住强制移动或瓦解射线时不施放
                .IfTrue(ctx => {
                    var buff = ctx.Skill.Player.Powers.GetBuff(485549);
                    if(buff != null && buff.IconCounts[3] >= 1 && buff.IconCounts[3] <= Firebird)
                    {
                        return true;
                    }
                    return false;
                    }).ThenCastElseContinue()//火鸟4件套>=1层且低于Firebird层时继续
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.TaegukPrimary.Sno)).ThenContinueElseNoCast()//太极
                .IfTrue(ctx => Hud.Game.Shrines.Any(x => !x.IsDisabled && !x.IsOperated && x.IsPylon) || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(200, false) || ctx.Hud.Game.ActorQuery.NearestGoblin != null).ThenContinueElseNoCast()//200码内有精英或BOSS或塔
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.TaegukPrimary, 1, 550, 550).ThenCastElseContinue()//太极BUFF快断了时施放
                ;
        }
        private bool IsGuardianDead
        {
            get
            {
                if (Hud.Game.Monsters.Any(m => m.Rarity == ActorRarity.Boss && !m.IsAlive))
                    return true;
                return riftQuest != null && (riftQuest.QuestStepId == 5 || riftQuest.QuestStepId == 10 || riftQuest.QuestStepId == 34 || riftQuest.QuestStepId == 46);
            }
        }
        private IQuest riftQuest
        {
            get
            {
                return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492);   // gr
            }
        }

    }
}