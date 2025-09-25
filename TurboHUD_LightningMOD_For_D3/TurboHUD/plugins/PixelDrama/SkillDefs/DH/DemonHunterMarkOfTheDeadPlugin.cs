using System.Linq;
using System.Reflection;
using Turbo.Plugins;
using Turbo.Plugins.LightningMod;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class DemonHunterMarkOfTheDeadPlugin : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterMarkOfTheDeadPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_MarkedForDeath;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => {
                    return ctx.Skill.Player.Powers.BuffIsActive(488036) || ctx.Skill.Player.Powers.BuffIsActive(488037) || ctx.Skill.Player.Powers.BuffIsActive(488004); //祭坛喝药水buff已解锁
                }).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                    var isAnyUncursedMonster = Hud.Game.AliveMonsters.Any(m => (m.Rarity == ActorRarity.Boss || m.Rarity == ActorRarity.Rare || m.Rarity == ActorRarity.Champion 
                    || m.Rarity == ActorRarity.Unique || m.SnoMonster.Priority == MonsterPriority.goblin 
                    || m.SnoMonster.Priority == MonsterPriority.keywarden) &&
                    !m.AffixSnoList.Any(a => a.Affix == MonsterAffix.Juggernaut) &&
                    !m.Invisible && !m.Invulnerable && !m.Illusion && (m.FloorCoordinate.XYZDistanceTo(cur.ToWorldCoordinate()) - m.RadiusBottom) <= 19 &&
                    !m.MarkedForDeath);
                    return isAnyUncursedMonster;
                }).ThenCastElseContinue()
                ;


        }
    }
}