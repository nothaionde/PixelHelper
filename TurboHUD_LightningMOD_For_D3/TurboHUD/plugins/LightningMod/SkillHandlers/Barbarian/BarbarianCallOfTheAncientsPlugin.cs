namespace Turbo.Plugins.LightningMod
{
    public class BarbarianCallOfTheAncientsPlugin : AbstractSkillHandler, ISkillHandler
	{
        public BarbarianCallOfTheAncientsPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseTpStart, CastPhase.UseWpStart, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_CallOfTheAncients;
            
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    bool IKset = Hud.Game.Me.GetSetItemCount(671068) >= 2;
                    return IKset && ctx.Skill.BuffIsActive;
                    }
                ).ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    bool IKset = Hud.Game.Me.GetSetItemCount(671068) >= 2;
                    return (IKset) ||
                        (Hud.Game.IsEliteOnScreen) ||
                        Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno) //»ÆµÀ»òÃ·¸«
                        ||
                        (Hud.Game.MaxPriorityOnScreen >= MonsterPriority.keywarden) ||
                        (Hud.Game.ActorQuery.NearestBoss != null) ||
                        (ctx.Skill.Player.Defense.HealthPct <= 50);
                }).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(300, 500).ThenCastElseContinue()
                ;
        }
    }
}