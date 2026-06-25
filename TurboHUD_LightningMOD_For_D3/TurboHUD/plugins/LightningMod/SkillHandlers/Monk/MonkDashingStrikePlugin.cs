namespace Turbo.Plugins.LightningMod
{
    public class MonkDashingStrikePlugin : AbstractSkillHandler, ISkillHandler
    {
        private IScreenCoordinate PlayerScreenCoordinate = null;
        private IWorldCoordinate LastWorldCoordinate = null;
        private bool moveback = false;
        public MonkDashingStrikePlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Monk_DashingStrike;
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
                .IfTrue(ctx => ctx.Skill.Rune == 2 || ctx.Skill.Rune == 4)//眩目光速或光辉如炬符文
                .IfBossIsNearby(ctx => 20).ThenContinueElseNoCast()//BOSS在20码范围内
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    return cursor.XYZDistanceTo(ctx.Hud.Game.Me.FloorCoordinate) < 20;//鼠标在角色范围20码内
                }
                ).ThenContinueElseNoCast()
                .IfBuffIsAboutToExpire(100, 300).ThenContinueElseNoCast()//疾风击buff剩余不到0.3秒
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