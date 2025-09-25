using System.Linq;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class CrusaderBlessedShieldPlugin : AbstractSkillHandler, ISkillHandler
	{
        private IScreenCoordinate LastScreenCoordinate = null;
        private bool moveback = false;
        public CrusaderBlessedShieldPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.Crusader_BlessedShield;
            CreateCastRule()
                .IfTrue(ctx =>
                {
                    if (moveback == true)
                    {
                        ctx.Hud.Interaction.MouseMove(LastScreenCoordinate.X, LastScreenCoordinate.Y);//移动到记录的鼠标位置
                        moveback = false;
                    }
                    return true;
                })
                .IfInTown().ThenNoCastElseContinue()
                .IfCastingIdentify().ThenNoCastElseContinue()
                .IfCastingPortal().ThenNoCastElseContinue() 
                .IfTrue(ctx => PublicClassPlugin.GetBuffCount(ctx.Hud, Hud.Sno.SnoPowers.Crusader_BlessedShield.Sno, 3) >= 100).ThenContinueElseNoCast()//祝福之盾100层
                .IfPrimaryResourceIsEnough(0,ctx => 0).ThenContinueElseNoCast()
                .IfSpecificBuffIsAboutToExpire(Hud.Sno.SnoPowers.Crusader_BlessedShield, 3, 1000, 1200).ThenContinueElseNoCast()
                .IfCanCastSkill(200, 200, 3000).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    IMonster monster = ctx.Hud.Game.AliveMonsters.Where(m => m.IsOnScreen && !m.Invisible && !m.Invulnerable && m.CentralXyDistanceToMe <= 50).OrderBy(m => m.NormalizedXyDistanceToMe).FirstOrDefault();
                    if (monster != null)
                    {
                        IScreenCoordinate monsterCoordinate = monster.ScreenCoordinate;
                        LastScreenCoordinate = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);//记录移动前鼠标位置
                        ctx.Hud.Interaction.MouseMove(monsterCoordinate.X, monsterCoordinate.Y);//移动鼠标到怪物位置
                        moveback = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    
                }).ThenCastElseContinue()
                ;

        }
    }
}