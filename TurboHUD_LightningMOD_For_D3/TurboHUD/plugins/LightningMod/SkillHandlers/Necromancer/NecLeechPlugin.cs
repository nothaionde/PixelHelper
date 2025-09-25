namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class NecLeechPlugin : AbstractSkillHandler, ISkillHandler
    {
        public NecLeechPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_Leech;

            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 4)//鲜血药剂符文
                .IfTrue(ctx => { 
                    return ctx.Skill.Player.Powers.BuffIsActive(488036) || ctx.Skill.Player.Powers.BuffIsActive(488037) || ctx.Skill.Player.Powers.BuffIsActive(488004); //祭坛喝药水buff已解锁
                }).ThenContinueElseNoCast()
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var cur = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
                    var isAnyUncursedMonster = Hud.Game.AliveMonsters.Any(m => (m.Rarity == ActorRarity.Boss || m.Rarity == ActorRarity.Rare || m.Rarity == ActorRarity.Champion || m.Rarity == ActorRarity.Unique || m.SnoMonster.Priority == MonsterPriority.goblin || m.SnoMonster.Priority == MonsterPriority.keywarden) && !m.Invisible && !m.Invulnerable && !m.Illusion && !m.Cursed && (m.FloorCoordinate.XYZDistanceTo(cur.ToWorldCoordinate()) - m.RadiusBottom) <= 19);
                    return isAnyUncursedMonster;
                }).ThenCastElseContinue()
                ;


        }
    }
}