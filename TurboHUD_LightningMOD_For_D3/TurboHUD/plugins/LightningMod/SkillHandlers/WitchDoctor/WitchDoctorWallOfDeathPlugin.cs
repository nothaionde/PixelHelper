using System.Linq;

namespace Turbo.Plugins.LightningMod
{

    public class WitchDoctorWallOfDeathPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorWallOfDeathPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_WallOfDeath;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx => Hud.Game.Me.GetSetItemCount(860188) >= 6).ThenContinueElseNoCast()//魔牙6件套
                .IfEnoughMonstersNearby(ctx => 200, ctx => 1).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Generic_P3ItemPassiveUniqueRing010, 1, 300, 500).ThenCastElseContinue()//6件套特效
                .IfTrue(ctx => {
                    bool CommuningWithSpirits = ctx.Skill.Rune == 2;//谕魂符文
                    var cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                    var isAnyUncursedMonster = Hud.Game.AliveMonsters.Any(m => (m.Rarity == ActorRarity.Boss || m.Rarity == ActorRarity.Rare || m.Rarity == ActorRarity.Champion || m.Rarity == ActorRarity.Unique || m.SnoMonster.Priority == MonsterPriority.goblin || m.SnoMonster.Priority == MonsterPriority.keywarden) && !m.Invisible && !m.Invulnerable && !m.Illusion && m.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_C, 134837) != 1 && (m.FloorCoordinate.XYZDistanceTo(cur.ToWorldCoordinate()) - m.RadiusBottom) <= 19 && !m.AffixSnoList.Any(a => a.Affix == MonsterAffix.Juggernaut));
                    return CommuningWithSpirits && isAnyUncursedMonster;
                }).ThenCastElseContinue()
                ;

        }
    }
}