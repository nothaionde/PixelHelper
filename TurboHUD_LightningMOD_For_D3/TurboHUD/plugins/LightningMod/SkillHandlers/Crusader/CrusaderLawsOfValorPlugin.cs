namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;
    using System.Linq;
    using Turbo.Plugins.glq;
    public class CrusaderLawsOfValorPlugin : AbstractSkillHandler, ISkillHandler
	{
        public CrusaderLawsOfValorPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.PreAttack)
        {
            Enabled = false;
        }
        public HashSet<uint> ReflectMonsters = new HashSet<uint>()
        {
			//tested in patch 2.6.4
			5191, //Sand Dweller - gr - Buff_Visual_Effect	61	1
			5192, //Sand Dweller - adventure mode - Buff_Visual_Effect	61	1
			3981, //Vicious Magewraith - adventure mode
			3982, //Vicious Magewraith - Gr
			256022, //act 2 keywarden - Power_Buff_1_Visual_Effect_None	256026	0	power: Dervish_Whirlwind_Mortar_Prototype / Power_Buff_0_Visual_Effect_None	256026	1	power: Dervish_Whirlwind_Mortar_Prototype
			344389, //stonesinger gr or sand dweller elite
			418900, //stonesinger gr or sand dweller elite
			297730, //malthael

			//old snos from another plugin
			116075, //Sand Dweller - adventure mode
			26245, //Sand Dweller
			116305, //Sand Dweller
			196846, //Sand Dweller
			26066, //Dune Dervish
			26067, //Vicious Magewraith
			57063, //Rock Giant - adventure mode
			57064, //Sand Behemoth - adventure mode
			418901, //Sand Dweller - Gr 
		};
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_LawsOfValor;

            CreateCastRule()//一般规则
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx =>
                (ctx.Hud.Game.Me.Powers.BuffIsActive(310678) ||//律法无边
                ctx.Skill.Player.Stats.CooldownReduction >= 0.5 || ctx.Hud.Game.Me.Powers.BuffIsActive(402459)) && //CDR大于50或带了黄道
                (!isSanGuang() ||
                (isSanGuang() && (ctx.Hud.Game.RiftPercentage < 100 || !ctx.Skill.Player.InGreaterRift)))//大秘境进度阶段
                ).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Generic_X1CrusaderLawsOfValorPassive2, 6, 200, 500).ThenCastElseContinue()
                ;

            CreateCastRule()//一般规则-威嚇號令
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx=> ctx.Skill.Rune == 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                (ctx.Hud.Game.Me.Powers.BuffIsActive(402459) ? ctx.Skill.Player.Stats.CooldownReduction >= 0.5 : ctx.Skill.Player.Stats.CooldownReduction >= 0.7) && //带黄道时CDR高于50，否则CDR高于70
                (!isSanGuang() ||
                (isSanGuang() && (ctx.Hud.Game.RiftPercentage < 100 || !ctx.Skill.Player.InGreaterRift)))//大秘境进度阶段
                ).ThenContinueElseNoCast()
                .IfEliteIsNearby(ctx => 10).ThenCastElseContinue()//10码内精英
                .IfTrue(ctx => Hud.Game.AliveMonsters.Any(m =>m.CollisionCoordinate.XYDistanceTo(ctx.Skill.Player.CollisionCoordinate) < 10 && ReflectMonsters.Contains(m.SnoMonster.Sno))).ThenCastElseContinue()//10码内反弹怪
                ;

            CreateCastRule()//三光BOSS战规则
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx =>
                (isSanGuang() && ctx.Hud.Game.RiftPercentage == 100 && ctx.Hud.Game.ActorQuery.IsEliteOrBossCloserThan(30) && (PublicClassPlugin.IsElementReady (Hud, 0.5, ctx.Skill.Player, 4) || //大秘境BOSS阶段，神圣前0.5秒激活
                (ctx.Hud.Game.Me.Powers.BuffIsActive(310678) &&//律法无边
                ctx.Skill.Player.Stats.CooldownReduction >= 0.6518)))//无缝释放的基础条件
                ).ThenCastElseContinue()
                ;
        }

        private bool isSanGuang()
        {
            bool isHeavensFury = Hud.Game.Me.Powers.UsedCrusaderPowers.HeavensFury?.Rune == 4; //天堂之火
            bool isAegisofValor = Hud.Game.Me.GetSetItemCount(192736) >= 6;//勇气6件套
            bool isFateoftheFell = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FateOfTheFell.Sno, 0);//妖邪必败
            bool isConventionOfElements = Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 0);//元素戒指
            return isAegisofValor && isHeavensFury && isFateoftheFell && isConventionOfElements;
        }
    }
}