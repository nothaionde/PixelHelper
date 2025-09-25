using System.Linq;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class NecBoneArmorPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SpareResource { get; set; }
        public NecBoneArmorPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_BoneArmor;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 1 || (ctx.Skill.Rune == 2 && ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno) && ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.KrysbinsSentence.Sno))).ThenNoCastElseContinue()//限制免疫符文或者（白骨脱臼+元素戒+克利斯宾）时不使用该规则
                .IfTrue(ctx =>
                {
                    var monsters = ctx.Hud.Game.AliveMonsters.Where(m => !m.Invulnerable && !m.Invisible && (m.CentralXyDistanceToMe < 30 + m.RadiusBottom));
                    return monsters.Count() >= (ctx.Hud.Game.Me.Powers.BuffIsActive(476686, 0) ? 2 : 1) || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(30, false);//满足周围怪物最低要求的数量
                })
                .ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Player.Defense.HealthPct < 50 && ctx.Skill.Rune == 3).ThenNoCastElseContinue()//骨肉相连符文在50%血以下不使用
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(740281) >= 2 && ctx.Skill.Rune != 3).ThenCastElseContinue()//圣套时直接施放，不为骨肉相连符文
                .IfTrue(ctx => (ctx.Skill.Player.Powers.UsedNecromancerPowers.SkeletalMage?.Rune == 1 || ctx.Hud.Game.Me.Powers.BuffIsActive(484311)) && ctx.Skill.Player.Powers.BuffIsActive(476586) && ctx.Skill.Rune != 3).ThenCastElseContinue()//魂法+轮回镰刀时直接施放，且不为骨肉相连符文
                .IfTrue(ctx =>
                {
                    var buff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_BoneArmor.Sno);
                    return buff?.IconCounts[0] < (ctx.Hud.Game.Me.Powers.BuffIsActive(476686, 0) ? 15 : 10);
                }).ThenCastElseContinue()//层数不满时施放
                .IfBuffIsAboutToExpire(10000,15000).ThenCastElseContinue()//BUFF剩余10~15秒以下时施放
                ;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx =>  ctx.Skill.Rune == 2).ThenContinueElseNoCast()//该规则仅限白骨脱臼符文
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements).ThenContinueElseNoCast()//带元素戒指
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.KrysbinsSentence).ThenContinueElseNoCast()//带克利斯宾
                .IfTrue(ctx =>//其次保证骨甲减伤层数
                {
                    var monsters = ctx.Hud.Game.AliveMonsters.Where(m => !m.Invulnerable && !m.Invisible && m.CentralXyDistanceToMe < 30);
                    return monsters.Count() >= (ctx.Hud.Game.Me.Powers.BuffIsActive(476686, 0) ? 2 : 1) || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(30, false);//满足周围怪物最低要求的数量
                })
                .ThenContinueElseNoCast()
                .IfTrue(ctx =>//优先在爆发元素使用
                {
                    int CoeIndex = Hud.GetPlugin<PublicClassPlugin>().CoeIndex;
                    var CoeBuffLeftTime = PublicClassPlugin.GetBuffLeftTime(hud, ctx.Hud.Sno.SnoPowers.ConventionOfElements.Sno, PublicClassPlugin.GetHighestElement(Hud, ctx.Skill.Player, CoeIndex));//获取爆发元素剩余BUFF时间
                    return CoeBuffLeftTime > 0 && CoeBuffLeftTime < 3;
                }).ThenCastElseContinue()//元素时释放
                .IfTrue(ctx =>
                {
                    var buff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_BoneArmor.Sno);
                    return buff?.IconCounts[0] < (ctx.Hud.Game.Me.Powers.BuffIsActive(476686, 0) ? 15 : 10);
                }).ThenCastElseContinue()//层数不满时施放
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(476586) && ctx.Skill.Rune != 3).ThenCastElseContinue()//轮回镰刀时直接施放，不为骨肉相连符文
                .IfBuffIsAboutToExpire(1000, 1500).ThenCastElseContinue()//BUFF剩余即将过期时施放
                ;

            CreateCastRule()//增加规则当玩家装备勾玉宝石且没有装备受罚宝石，60%CDR以上，则玩家30码内有怪就释放骨甲。
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Stats.CooldownReduction >= 0.6).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.GogokOfSwiftnessPrimary).ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.BaneOfTheStrickenPrimary).ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 30, ctx => 1).ThenCastElseContinue()
                ;
        }
    }
}