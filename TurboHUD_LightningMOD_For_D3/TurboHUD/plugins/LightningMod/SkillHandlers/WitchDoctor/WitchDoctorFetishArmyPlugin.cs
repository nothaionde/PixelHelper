namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class WitchDoctorFetishArmyPlugin : AbstractSkillHandler, ISkillHandler
    {
        public WitchDoctorFetishArmyPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_FetishArmy;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .ForceCast().ForSpecificCombatRole(CombatRole.Support)
                .IfTrue(ctx => ctx.Phase == CastPhase.Move).ThenNoCastElseContinue().ForSpecificCombatRole(CombatRole.Default)
                .IfTrue(ctx =>
                {
                    var count = Hud.Game.Actors.Count(actor =>
                        actor.SnoActor.Sno == ActorSnoEnum._fetish_melee_a ||
                        actor.SnoActor.Sno == ActorSnoEnum._fetish_doublestack_shaman_a ||
                        actor.SnoActor.Sno == ActorSnoEnum._fetish_ranged_a ||
                        actor.SnoActor.Sno == ActorSnoEnum._fetish_melee_itempassive ||
                        actor.SnoActor.Sno == ActorSnoEnum._fetish_shaman_a ||
                        actor.SnoActor.Sno == ActorSnoEnum._fetish_skeleton_a);

                    if (ctx.Phase == CastPhase.UseTpStart)
                    {
                        return count <= 4;
                    }

                    switch (ctx.Skill.Rune)
                    {
                        case 1:
                            return count < 8;
                        case 2:
                        case 4:
                            return count < 7;
                    }

                    return count < 5;
                }).ThenCastElseContinue().ForSpecificCombatRole(CombatRole.Default);
        }
    }
}