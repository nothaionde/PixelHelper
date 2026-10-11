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
                .IfTrue(ctx => ctx.Skill.Rune == 2 || ctx.Skill.Rune == 4).ThenContinueElseNoCast()//眩目光速或光辉如炬符文
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

            CreateCastRule()//保持千飓6件
                .IfTrue(ctx =>
                {
                    if (ctx.Skill.Player.GetSetItemCount(755275) >= 6 && ctx.Skill.Player.GetSetItemCount(563257) >= 2)
                    {
                        return true;
                    }
                    return false;
                }).ThenContinueElseNoCast()
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
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Generic_P2ItemPassiveUniqueRing033, 2, 300, 500).ThenContinueElseNoCast()//千飓6件套buff剩余不到0.5秒
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Generic_P3ItemPassiveUniqueRing026, 1).ThenContinueElseNoCast()//神龙套buff激活
                .IfEnoughMonstersNearby(ctx => 12, ctx => 1).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    if (Hud.Game.Me.Animation.ToString().Contains("_rapidstrikes_"))//正在百烈拳
                    {
                        return true;
                    }
                    if (Hud.Game.Me.Animation.ToString().Contains("_debilitatingblows_"))//正在伏魔破
                    {
                        return true;
                    }
                    return false;
                }).ThenContinueElseNoCast()
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