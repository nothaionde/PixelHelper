namespace Turbo.Plugins.LightningMod
{
    using System.Linq;

    public class WitchDoctorZombieDogsPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int dog { get; set; }
        public WitchDoctorZombieDogsPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
            dog = 3;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_SummonZombieDog;

            CreateCastRule()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    var count = Hud.Game.Actors.Count(actor => 
                    actor.SummonerAcdDynamicId == ctx.Hud.Game.Me.SummonerId &&
                    (actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_fire ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_healthglobe ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_healthlink ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_lifesteal ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedogrune_poison ||
                    actor.SnoActor.Sno == ActorSnoEnum._wd_zombiedog));
                    if (count >= dog) return false;

                    if ((count >= 1) && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheTallMansFinger.Sno)) return false;

                    return true;
                }).ThenCastElseContinue();
        }
    }
}