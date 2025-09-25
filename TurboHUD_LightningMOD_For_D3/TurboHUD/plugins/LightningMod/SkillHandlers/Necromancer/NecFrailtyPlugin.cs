using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecFrailtyPlugin : AbstractSkillHandler, ISkillHandler
    {
        public NecFrailtyPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_Frailty;
            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx => ctx.Skill.Rune == 4).ThenNoCastElseContinue()//光环符文不生效
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                    var isAnyMonsterUnhealthy = Hud.Game.AliveMonsters.Any(m => (
                    m.Rarity == ActorRarity.Boss || 
                    m.Rarity == ActorRarity.Rare || 
                    m.Rarity == ActorRarity.Champion || 
                    m.Rarity == ActorRarity.Unique ||
                    m.SnoMonster.Priority == MonsterPriority.goblin || 
                    m.SnoMonster.Priority == MonsterPriority.keywarden
                    )  && !m.Invisible && !m.Invulnerable && !m.Illusion  && (m.FloorCoordinate.XYZDistanceTo(cur.ToWorldCoordinate()) - m.RadiusBottom) <= 18  && (m.CurHealth / m.MaxHealth) < (ctx.Skill.Rune != 0 ? 0.15 : 0.18) &&
                    (ctx.Skill.Rune != 0 ? true : 
                    ctx.Skill.Player.Defense.HealthPct >= 30));//饥渴坟墓时自身血量不低于30%
                    return isAnyMonsterUnhealthy;
                }).ThenCastElseContinue()
                ;
        }
    }
}