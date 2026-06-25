using System.Linq;
using Turbo.Plugins.LightningMod;
namespace Turbo.Plugins.PixelDrama.SkillDefs
{
    public class DemonHunterSentryPlugin : AbstractSkillHandler, ISkillHandler
	{
        public DemonHunterSentryPlugin()
            : base(CastType.BuffSkill, CastPhase.AutoCast, CastPhase.Move, CastPhase.Attack)
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            AssignedSnoPower = Hud.Sno.SnoPowers.DemonHunter_Sentry;

            //CreateCastRule()
            //    .IfInTown().ThenNoCastElseContinue()
            //    .IfCastingIdentify().ThenNoCastElseContinue()
            //    .IfCastingPortal().ThenNoCastElseContinue()
            //    .IfPrimaryResourceIsEnough(0, ctx => 0).ThenContinueElseNoCast()
            //    .IfTrue(ctx =>
            //    {
            //        var players = Hud.Game.Players.Where(p => !p.IsDead && p.SnoArea.Sno == Hud.Game.Me.SnoArea.Sno/* && p != Hud.Game.Me*/);
            //    
            //        var cursorPos = ctx.Hud.Window.CreateScreenCoordinate(ctx.Hud.Window.CursorX, ctx.Hud.Window.CursorY).ToWorldCoordinate();
            //    
            //        foreach (var player in players)
            //    {
            //            //if (player.Powers.UsedLegendaryGems.EsotericAlterationPrimary?.Active == false
            //            //||  player.Powers.UsedLegendaryGems.GemOfEfficaciousToxinPrimary?.Active == false
            //            //|| player.Powers.UsedLegendaryPowers.OculusRing?.Active == false)
            //            //{
            //            //    continue;
            //            //}
            //            if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno) && !player.Powers.BuffIsActive(129217))
            //            {
            //                var playerScreenPos = player.FloorCoordinate.ToScreenCoordinate().ToWorldCoordinate();
            //    
            //                if (cursorPos.X >= playerScreenPos.X - 150 &&
            //                    cursorPos.X <= playerScreenPos.X + 150 &&
            //                    cursorPos.Y >= playerScreenPos.Y - 150 &&
            //                    cursorPos.Y <= playerScreenPos.Y + 150)
            //                {
            //                    return true;
            //                    return ctx.Hud.Game.AliveMonsters.Where(m => m.IsElite/* && m.Rarity != ActorRarity.RareMinion*/).Count() > 0;
            //                }
            //            }
            //        }
            //    
            //        return false;
            //    }
            //    ).ThenCastElseContinue();
        }
    }
}