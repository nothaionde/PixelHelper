using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderSteedChargePlugin : AbstractSkillHandler, ISkillHandler
	{
        public bool NotWorkForDrawandQuarter { get; set; }
        private bool initialized = false;
        public CrusaderSteedChargePlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Collect, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_SteedCharge;

            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx => NotWorkForDrawandQuarter && ctx.Skill.Rune == 4).ThenNoCastElseContinue()//战马拖行符文时不生效
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Darklight.Sno) && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.VigilanteBelt.Sno)).ThenNoCastElseContinue()//装备黑暗之光和骑士腰带
                .IfTrue(ctx => ctx.Skill.Rune == 0 && ctx.Hud.Interaction.IsHotKeySet(ActionKey.Move) && ctx.Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)).ThenCastElseContinue()//尖刺马铠且按住强制移动时连续施放
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(447290)).ThenContinueElseNoCast()//战马套
                .IfTrue(ctx => ctx.Hud.Game.AliveMonsters.Any(m =>(m.CurHealth / m.MaxHealth) < 0.99 && m.CentralXyDistanceToMe < (ctx.Skill.Player.Powers.BuffIsActive(403468, 0) ? 60 : 15))).ThenContinueElseNoCast()//周围15码内至少1个怪血量低于99%,带贼神时为50码
                .IfFalse(ctx => ctx.Skill.Player.Powers.BuffIsActive(447291,1)).ThenCastElseContinue()
                ;

            CreateCastRule()//勇气天拳流
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Crusader_SteedCharge).ThenNoCastElseContinue()//骑马时
                .IfTrue(ctx => NotWorkForDrawandQuarter && ctx.Skill.Rune == 4).ThenNoCastElseContinue()//战马拖行符文时不生效
                .IfTrue(ctx => ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Darklight.Sno) && ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.VigilanteBelt.Sno)).ThenContinueElseNoCast()//装备黑暗之光和骑士腰带
                .IfTrue(ctx => 
                {
                    if(initialized == false)
                    {
                        initialized = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    }).ThenCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.Powers.GetBuff(Hud.Sno.SnoPowers.Crusader_SteedCharge.Sno)?.LastActive.ElapsedMilliseconds > 200).ThenCastElseContinue()//下马1秒后继续
                ;
        }
    }
}