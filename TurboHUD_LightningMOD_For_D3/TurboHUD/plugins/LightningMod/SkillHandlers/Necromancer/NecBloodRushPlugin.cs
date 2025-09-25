using System.Linq;
namespace Turbo.Plugins.LightningMod
{
    public class NecBloodRushPlugin : AbstractSkillHandler, ISkillHandler
    {
        public int SesourcesBelow { get; set; }
        private IScreenCoordinate PlayerScreenCoordinate = null;
        private IWorldCoordinate LastWorldCoordinate = null;
        private bool moveback = false;
        public NecBloodRushPlugin()
            : base(CastType.SimpleSkill, CastPhase.AutoCast, CastPhase.UseWpStart, CastPhase.Move, CastPhase.Attack, CastPhase.AttackIdle)
        {
            Enabled = false;
            SesourcesBelow = 80;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Necromancer_BloodRush;
            Rune = 4;
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
                .IfTrue(ctx =>
                {
                    return ctx.Skill.Player.Powers.CurrentSkills.Any(s => s.SnoPower.Sno == ctx.Hud.Sno.SnoPowers.Necromancer_Devour.Sno && s.Rune == 2) &&//吞噬 - 如饥似渴
                    ctx.Skill.Player.Powers.CurrentSkills.Any(s => s.SnoPower.Sno == ctx.Hud.Sno.SnoPowers.Necromancer_BoneSpear.Sno)//骨矛
                    ;
                }).ThenContinueElseNoCast()
                //.IfPrimaryResourcePercentageIsBelow(SesourcesBelow).ThenContinueElseNoCast()//能量低于特定百分比时有效
                .IfBossIsNearby(ctx => 50).ThenContinueElseNoCast()//BOSS在50码范围内
                .IfTrue(ctx =>
                {
                    IWorldCoordinate cursor = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    return cursor.XYZDistanceTo(ctx.Hud.Game.Me.FloorCoordinate) < 50;//鼠标在角色范围50码内
                }
                ).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Necromancer_Devour, 2, 1000, 1000).ThenContinueElseNoCast()//吞噬buff剩余不到1秒
                .IfTrue(ctx =>
                {
                    PlayerScreenCoordinate = ctx.Hud.Game.Me.FloorCoordinate.ToScreenCoordinate();
                    LastWorldCoordinate = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();//记录移动前鼠标位置
                    ctx.Hud.Interaction.MouseMove(PlayerScreenCoordinate.X, PlayerScreenCoordinate.Y);//移动鼠标到人物位置
                    moveback = true;
                    return true;
                }).ThenCastElseContinue()
                ;
            //(当前精魂值 + (11 * (1 + 安魂特效增幅)) - 200(噬血灵气固定200精魂上限)) / 6 = 每0.5秒获得的精魂，持续3秒共计6次
        }
    }
}