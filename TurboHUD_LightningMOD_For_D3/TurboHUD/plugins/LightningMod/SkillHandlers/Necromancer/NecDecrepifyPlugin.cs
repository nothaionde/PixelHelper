using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecDecrepifyPlugin : AbstractSkillHandler, ISkillHandler
    {
        public NecDecrepifyPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_Decrepify;
            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                    var isAnyUncursedMonster = Hud.Game.AliveMonsters.Any(m => (m.Rarity == ActorRarity.Boss || m.Rarity == ActorRarity.Rare || m.Rarity == ActorRarity.Champion || m.Rarity == ActorRarity.Unique || m.SnoMonster.Priority == MonsterPriority.goblin || m.SnoMonster.Priority == MonsterPriority.keywarden)  && !m.Invisible && !m.Invulnerable && !m.Illusion && !m.Cursed && (m.FloorCoordinate.XYZDistanceTo(cur.ToWorldCoordinate()) - m.RadiusBottom) <= 19  && 
                    (ctx.Skill.Rune == 4 ? true : !m.AffixSnoList.Any(a => a.Affix == MonsterAffix.Juggernaut)));//眩晕符文不对霸王生效
                    return isAnyUncursedMonster;
                }).ThenCastElseContinue()
                ;
        }
    }
}