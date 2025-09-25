namespace Turbo.Plugins.LightningMod
{
    public class NecLandOfTheDeadPlugin : AbstractSkillHandler, ISkillHandler
    {
        public float secMin { get; set; }
        public float secMax { get; set; }
        public NecLandOfTheDeadPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
            secMin = 0;
            secMax = 1;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_LandOfTheDead;
            CreateCastRule()
                .IfCanCastSkill(100, 150, 1000).ThenContinueElseNoCast()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Generic_PagesBuffInfiniteCasting, 0, 500, 2000, true).ThenCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead).ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.HauntedVisions).ThenNoCastElseContinue()//死灵面容
                .IfTrue(ctx =>
                {
                    var buff = ctx.Hud.Game.Me.Powers.GetBuff(ctx.Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno);

                    return buff?.TimeElapsedSeconds[1] > secMin && buff?.TimeElapsedSeconds[1] < secMax;
                }).ThenCastElseContinue()
                ;

            CreateCastRule()//死役玩法低CDR仅对精英和BOSS生效
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEliteOrBossIsNearby(ctx => 60).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    bool CDR = ctx.Hud.Game.Me.Stats.CooldownReduction <= 80;
                    bool isSetPestilence = ctx.Hud.Game.Me.GetSetItemCount(740282) >= 6;//死役6件套
                    return CDR && isSetPestilence && ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Necromancer_Passive_BloodIsPower.Sno) && (ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Ingeom.Sno));//死役+鲜血之力+黄道或寅剑
                }).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100, 500).ThenCastElseContinue()
                ;

                CreateCastRule()//死役玩法高CDR对小怪生效
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()//100码内无怪不开
                .IfTrue(ctx =>
                {
                    bool CDR = ctx.Hud.Game.Me.Stats.CooldownReduction > 80;
                    bool isSetPestilence = ctx.Hud.Game.Me.GetSetItemCount(740282) >= 6;//死役6件套
                    return CDR && isSetPestilence && ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Necromancer_Passive_BloodIsPower.Sno) && (ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.ObsidianRingOfTheZodiac.Sno) || ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.Ingeom.Sno));//死役+鲜血之力+黄道或寅剑
                }).ThenContinueElseNoCast()
                
                .IfBuffIsAboutToExpire(100, 500).ThenCastElseContinue()
                ;
            CreateCastRule()//辅助死灵
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()//100码内无怪不开
                .IfTrue(ctx =>
                {
                    return ctx.Hud.Game.Me.Powers.BuffIsActive(ctx.Hud.Sno.SnoPowers.MesserschmidtsReaver.Sno);//梅斧
                }).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100, 500).ThenCastElseContinue()
                ;
        }
    }
}