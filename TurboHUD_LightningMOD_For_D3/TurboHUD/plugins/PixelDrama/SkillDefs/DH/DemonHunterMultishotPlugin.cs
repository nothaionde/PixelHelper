using System.Linq;
using System.Reflection;
using Turbo.Plugins;
using Turbo.Plugins.LightningMod;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class DemonHunterMultishotPlugin : AbstractSkillHandler, ISkillHandler
    {
        public DemonHunterMultishotPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Multishot;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                    var cursorWorldPos = cur.ToWorldCoordinate();

                    return Hud.Game.AliveMonsters.Any(m =>
                        (m.Rarity == ActorRarity.Boss ||
                         m.Rarity == ActorRarity.Rare ||
                         m.Rarity == ActorRarity.Champion ||
                         m.Rarity == ActorRarity.Unique ||
                         m.SnoMonster.Priority == MonsterPriority.goblin ||
                         m.SnoMonster.Priority == MonsterPriority.keywarden)
                         && !m.AffixSnoList.Any(a => a.Affix == MonsterAffix.Juggernaut)
                         && !m.Invisible
                         && !m.Invulnerable
                         && !m.Illusion
                         && !m.Chilled
                         && (m.FloorCoordinate.XYZDistanceTo(cursorWorldPos) - m.RadiusBottom) <= 19);
                }).ThenCastElseContinue()
                ;


        }
    }
}