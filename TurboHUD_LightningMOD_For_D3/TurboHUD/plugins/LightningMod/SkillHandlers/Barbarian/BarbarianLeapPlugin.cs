using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class BarbarianLeapPlugin : AbstractSkillHandler, ISkillHandler
    {
        private IScreenCoordinate PlayerScreenCoordinate = null;
        private IWorldCoordinate LastWorldCoordinate = null;
        private bool moveback = false;
        public BarbarianLeapPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Barbarian_Leap;
            CreateCastRule()
                .IfTrue(ctx =>
                {
                    if (moveback == true)
                    {
                        ctx.Hud.Interaction.MouseMove(LastWorldCoordinate.ToScreenCoordinate().X, LastWorldCoordinate.ToScreenCoordinate().Y);//移动到记录的鼠标位置
                        moveback = false;
                    }
                    return true;
                })
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue()
                .IfOnCooldown().ThenNoCastElseContinue()
                .IfTrue(ctx => ctx.Skill.Player.GetSetItemCount(612542) >= 4 && !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.LutSocks.Sno) && !ctx.Skill.Player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno)).ThenContinueElseNoCast()//大地4件及以上且不装备跳跳鞋且不激活护盾塔
                .IfEnoughMonstersNearby(ctx => 60,ctx => 1).ThenContinueElseNoCast()//60码范围内有任意怪
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Barbarian_Leap, 2, 800, 1000).ThenContinueElseNoCast()//跳斩护甲buff剩余不到0.8秒
                .IfCanCastSkill(800, 800, 1000).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    PlayerScreenCoordinate = ctx.Hud.Game.Me.FloorCoordinate.ToScreenCoordinate();
                    LastWorldCoordinate = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();//记录移动前鼠标位置
                    ctx.Hud.Interaction.MouseMove(PlayerScreenCoordinate.X, PlayerScreenCoordinate.Y);//移动鼠标到人物位置
                    moveback = true;
                    return true;
                }).ThenCastElseContinue()
                ;
        }
    }
}