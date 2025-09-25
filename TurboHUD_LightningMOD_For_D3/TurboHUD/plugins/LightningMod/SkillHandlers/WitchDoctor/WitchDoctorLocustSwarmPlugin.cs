using System.Linq;
using Turbo.Plugins.glq;
namespace Turbo.Plugins.LightningMod
{
    public class WitchDoctorLocustSwarmPlugin : AbstractSkillHandler, ISkillHandler
	{
        private IScreenCoordinate LastScreenCoordinate = null;
        private bool moveback = false;
        public WitchDoctorLocustSwarmPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.WitchDoctor_LocustSwarm;
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
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.RingOfEmptiness).ThenContinueElseNoCast()//虚空戒指
                .IfSpecificBuffIsActive(Hud.Sno.SnoPowers.Wormwood).ThenNoCastElseContinue()//虫杖
                .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
                .IfCanCastSkill(1000, 1000, 3000).ThenContinueElseNoCast()
                .IfTrue(ctx =>
                {
                    IMonster monster = ctx.Hud.Game.AliveMonsters.Where(m => (!m.Locust && (m.IsElite || m.Rarity == ActorRarity.Boss)) && m.IsOnScreen && !m.Invisible && !m.Invulnerable && m.NormalizedXyDistanceToMe <= 20).OrderBy(m => m.NormalizedXyDistanceToMe).FirstOrDefault();
                    if (monster != null)
                    {
                        IScreenCoordinate monsterCoordinate = monster.ScreenCoordinate;
                        LastScreenCoordinate = ctx.Hud.Window.CreateScreenCoordinate(ctx.Hud.Window.CursorX, ctx.Hud.Window.CursorY);//记录移动前鼠标位置
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