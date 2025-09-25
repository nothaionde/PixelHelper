using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class WizardFrostNovaPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SpareResource { get; set; }
        private IWatch CD;
        public WizardFrostNovaPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            CD = Hud.Time.CreateWatch();
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_FrostNova;
            CreateCastRule()
                .IfCanCastSkill(150, 250, 2000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune != 2 && ctx.Hud.Game.Players.Any(p => p.HasValidActor && !p.IsMe) && ctx.Hud.Game.Me.InGreaterRiftRank > 0 && ctx.Hud.Game.RiftPercentage >= 100).ThenNoCastElseContinue()//多人游戏且大秘境BOSS时冰冻迷雾以外符文不生效
                .IfEnoughMonstersNearby(ctx => 16, ctx => ctx.Skill.Rune == 4 ? 5 : 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    bool Obsidian = ctx.Skill.Player.Powers.BuffIsActive(hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno);//黄道
                    bool ColdSnap = ctx.Skill.Rune == 3;//极速冰冻符文
                    bool FrozenMist = ctx.Skill.Rune == 2;//冰冻迷雾符文
                    bool BoneChill = ctx.Skill.Rune == 0;//冻骨之寒符文
                    if (BoneChill)
                        return false;
                    if (FrozenMist)
                        return true;
                    if (!Obsidian)
                        return true;
                    if (Obsidian && (!CD.IsRunning || CD.ElapsedMilliseconds >= (ColdSnap ? 2500 : 1500)))
                    {
                        CD.Restart();
                        return true;
                    }
                    return false;
                }
                ).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Rune == 0).ThenContinueElseNoCast()//冻骨之寒符文时只在精英和BOSS附近施放
                .IfEliteOrBossIsNearby(ctx => 16).ThenCastElseContinue()
                ;
        }
    }
}