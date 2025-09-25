using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecSimulacrumPlugin : AbstractSkillHandler, ISkillHandler
    {
        public float SecSpare { get; set; }
        public NecSimulacrumPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
            SecSpare = 6f;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_Simulacrum;
            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(472267) ? ctx.Skill.Player.Powers.GetBuff(472267)?.IconCounts[1] >= 300 : true).ThenContinueElseNoCast()//塔格奥4件时等叠满BUFF
                .IfTrue(ctx =>
                {
                    bool isBloodandBone = ctx.Skill.Rune == 3 || ctx.Skill.Player.Powers.BuffIsActive(484301);//燃烧狂欢节舞会2件或鲜血与白骨符文（2个分身）
                    int count = ctx.Hud.Game.Actors.Where(a => a.SummonerAcdDynamicId == ctx.Skill.Player.SummonerId && (a.SnoActor.Sno == (ActorSnoEnum)467053 || a.SnoActor.Sno == (ActorSnoEnum)484304)).Count();
                    return ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.HauntedVisions.Sno)//新版鬼灵面容
                    && (!ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno)//未激活血魂双分时
                     || (isBloodandBone && count < 2))//或者有使用鲜血白骨符文时不足两个分身
                    ;
                }).ThenCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum).ThenNoCastElseContinue()
                .IfTrue(ctx =>
                {
                    var buffLandOfTheDead = ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno);

                    return (buffLandOfTheDead?.TimeElapsedSeconds[0] < 1 && buffLandOfTheDead?.TimeElapsedSeconds[0] != 0) || //亡者领域施放1秒内
                    (
                    (120 - 
                    (ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Ingeom.Sno, 1) ? GetIngeom() : 0) //寅剑激活
                    ) * (1 - ctx.Skill.Player.Stats.CooldownReduction) //总CDR
                    <= ((ctx.Skill.Player.Powers.UsedLegendaryPowers.HauntedVisions?.Active == true ? 30 : 15) + SecSpare)
                    && (ctx.Skill.Player.Density.GetDensity(50)> 0));
                }).ThenCastElseContinue()
                ;
        }

        private uint GetIngeom()
        {
            uint Sec = 10;
            if (Hud.Game.Me.CubeSnoItem1 != null && Hud.Game.Me.CubeSnoItem1.Sno == Hud.Sno.SnoItems.Unique_Sword_1H_113_x1.Sno)
            {
                Sec = 10;
            }
            else
            {
                var Ingeom = Hud.Game.Items.FirstOrDefault(item => (item.Location == ItemLocation.RightHand || item.Location == ItemLocation.LeftHand) && item.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Sword_1H_113_x1.Sno);
                Sec = Ingeom == null ? 0 : (uint)Ingeom.Perfections.FirstOrDefault(p => p.Attribute.Code == "Item_Power_Passive").Cur;
            }
            return Sec;
        }
    }
}