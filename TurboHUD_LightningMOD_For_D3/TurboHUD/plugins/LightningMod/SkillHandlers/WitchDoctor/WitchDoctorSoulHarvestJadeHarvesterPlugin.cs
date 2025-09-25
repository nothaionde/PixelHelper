namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    public class WitchDoctorSoulHarvestJadeHarvesterPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int ActivationRange { get; set; }

        public WitchDoctorSoulHarvestJadeHarvesterPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.PreAttack, CastPhase.Attack, CastPhase.Move)
        {
            Enabled = false;
            ActivationRange = 18;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 18, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return (Hud.Game.Me.GetSetItemCount(842970) >= 6);//玉魂6件
                }
                ).ThenContinueElseNoCast()
                .IfTrue(ctx =>//灵行进怪堆没减伤BUFF时优先考虑层数
                {
                    int Stacks = ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.SacredHarvester.Sno) == true ? 10 : 5;//收割刀
                    return (
                    !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest.Sno, 5) &&//没有6件套减伤BUFF
                    ctx.Skill.Player.Density.GetDensity(18) >= Stacks//满足最大层数
                    )
                    ;
                }).ThenCastElseContinue()
                .IfTrue(ctx =>
                {
                    var monsters = Hud.Game.AliveMonsters.Where(m => ((m.SummonerAcdDynamicId == 0 && m.IsElite) || !m.IsElite) && m.NormalizedXyDistanceToMe <= 18);//18码内除幻术以外怪
                    int Count = 0;
                    bool RingOfEmptiness = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.RingOfEmptiness.Sno, 0);//空虚之戒
                    foreach (var monster in monsters)
                    {
                        if(RingOfEmptiness)
                        {
                            if (monster.Haunted || monster.Locust) Count++;//虫群和蚀魂怪物计数
                        }
                       else
                        {
                            if (monster.Haunted) Count++;//虫群和蚀魂怪物计数
                        }
                    }
                    return (Count >= 1 && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest.Sno, 5));
                }
                ).ThenCastElseContinue()
                ;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 18, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    return (Hud.Game.Me.GetSetItemCount(842970) >= 6 && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest.Sno, 0));//玉魂6件及有收割BUFF
                }
                ).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.WitchDoctor_SoulHarvest, 5, 300, 500).ThenCastElseContinue()//优先考虑减伤buff
                ;

        }
    }
}