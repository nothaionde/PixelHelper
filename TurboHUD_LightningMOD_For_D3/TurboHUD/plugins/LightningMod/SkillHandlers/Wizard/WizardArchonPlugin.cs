using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class WizardArchonPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int buffStack { get; set; }
        public WizardArchonPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
            buffStack = 20;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Wizard_Archon;
            CreateCastRule()
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfCanCastSimple().ThenContinueElseNoCast()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfEnoughMonstersNearby(ctx => 100, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    bool active = false;
                    var buff = Hud.Game.Me.Powers.GetBuff(440235);
                    var BlackHole = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Wizard_BlackHole);
                    var BlackHoleBuff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Wizard_BlackHole.Sno);
                    var cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    int count = Hud.Game.AliveMonsters.Where(m => m.FloorCoordinate.XYDistanceTo(cursor) < 15 + m.RadiusBottom).Count();
                    if (buff != null && buff.IconCounts[0] >= buffStack -  
                    (BlackHole != null && !BlackHole.IsOnCooldown && 
                    (!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Wizard_BlackHole.Sno) || (BlackHoleBuff != null && ((BlackHoleBuff.TimeLeftSeconds[8] * BlackHoleBuff.IconCounts[8]) < (count * 10)))) //当前黑洞BUFF剩余时间* 层数获得当前增伤权重 少于 鼠标位置怪 * 10秒总增伤权重怪时释放黑洞
                    ? 1:0))
                    {
                        if(BlackHole != null && !BlackHole.IsOnCooldown) hud.Interaction.DoAction(BlackHole.Key);
                        active = true;
                    }
                    return active;
                }).ThenCastElseContinue();
        }
    }
}