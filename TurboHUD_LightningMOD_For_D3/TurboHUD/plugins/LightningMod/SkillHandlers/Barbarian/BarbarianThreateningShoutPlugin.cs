namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    public class BarbarianThreateningShoutPlugin : AbstractSkillHandler, ISkillHandler
	{
        public int ActivationRange { get; set; }
        public int DensityLimit { get; set; }

        public BarbarianThreateningShoutPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.PreAttack)
        {
            Enabled = false;
            ActivationRange = 25;
            DensityLimit = 10;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_ThreateningShout;

            CreateCastRule()
                .IfTrue(ctx => ctx.Skill.Rune == 2).ThenNoCastElseContinue()//非恐怖收割符文
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfEliteOrBossIsNearby(ctx => ActivationRange).ThenCastElseContinue()
                .IfEnoughMonstersNearby(ctx => ActivationRange, ctx => ctx.Skill.Player.Powers.BuffIsActive(402458, 1) ? 1 : DensityLimit).ThenCastElseContinue();

            CreateCastRule()
                .IfTrue(ctx => ctx.Skill.Rune == 2).ThenContinueElseNoCast()//恐怖收割符文
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfBossIsNearby(ctx => ActivationRange).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    int playerConut = ctx.Hud.Game.Players.Count();
                    bool playIsNearby = playerConut == 1 ? true : ctx.Hud.Game.Players.Where(p => p.HasValidActor && p.NormalizedXyDistanceToMe < ActivationRange && !p.IsMe).Count() >= (playerConut - (playerConut > 2 ? 2 : 1));//靠近任意（最大玩家数-1）的玩家25码
                    var monsterWithHighestMonsterDensity = Hud.Game.AliveMonsters.Where(x => x.IsOnScreen).OrderByDescending(x => x.GetMonsterDensity(ActivationRange)).FirstOrDefault();
                    return (playIsNearby && monsterWithHighestMonsterDensity != null && monsterWithHighestMonsterDensity.NormalizedXyDistanceToMe <= ActivationRange && (monsterWithHighestMonsterDensity.GetMonsterDensity(25) + 1 >= DensityLimit || ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(25)));//靠近密度最高怪物25码
                }).ThenCastElseContinue();

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.BladeOfTheTribes).ThenContinueElseNoCast()//部族之刃
                .IfTrue(ctx =>
                {
                    var Animation = Hud.Game.Me.Animation;
                    return
                    Animation == AnimSnoEnum._barbarian_female_dw_leap_attack_contactend ||
                    Animation == AnimSnoEnum._barbarian_male_dw_eq_contactend ||
                    Animation == AnimSnoEnum._barbarian_male_1hs_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_1ht_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_2hs_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_2ht_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_dw_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_male_stf_seismic_slam_long ||
                    Animation == AnimSnoEnum._barbarian_female_1hs_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_1ht_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_2hs_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_2ht_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_dw_seismic_slam ||
                    Animation == AnimSnoEnum._barbarian_female_stf_seismic_slam
                    ;//挑战或裂地斩
                }
                ).ThenCastElseContinue()
                ;
        }
    }
}