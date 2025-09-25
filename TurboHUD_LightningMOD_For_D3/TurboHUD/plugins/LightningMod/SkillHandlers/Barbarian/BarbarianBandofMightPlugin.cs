namespace Turbo.Plugins.LightningMod
{
    using System;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.glq;
    public class BarbarianBandofMightPlugin : BasePlugin, IAfterCollectHandler
    {
        private IScreenCoordinate PlayerScreenCoordinate = null;
        private IWorldCoordinate LastWorldCoordinate = null;
        private IWatch Timer;
        public BarbarianBandofMightPlugin()
        {
            Enabled = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            Timer = Hud.Time.CreateWatch();
        }
        public void AfterCollect()
        {
            if (!Hud.Game.IsInGame
                ||Hud.Game.IsLoading
                || Hud.Game.Me.IsInTown
                || !Hud.Window.IsForeground
                || (!Hud.Render.MinimapUiElement.Visible)
                || Hud.Render.IsAnyBlockingUiElementVisible
                || Hud.Render.ActMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Render.WorldMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Game.Me.AnimationState == AcdAnimationState.Transform
                || PublicClassPlugin.isCasting(Hud)
                || Hud.Game.Me.IsDead
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno)
                || !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.BandOfMight.Sno)//力量戒指
                || Hud.Game.Me.Density.GetDensity(100) == 0//100码内至少有1个怪
                || (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.ChilaniksChain.Sno) && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.PrideOfCassius.Sno))//齐腰+卡腰的辅助蛮子
                )
            {
				if(Timer.IsRunning) Timer.Stop();
                return;
            }
            IPlayerSkill skill = null;
            var skillFuriousCharge = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Barbarian_FuriousCharge);//狂暴冲锋
            var skillGroundStomp = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Barbarian_GroundStomp);//大地践踏
            var skillLeap = Hud.Game.Me.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Barbarian_Leap);//跃击
            if (skillFuriousCharge == null && skillGroundStomp == null && skillLeap == null) return;
            if (skillGroundStomp != null)
                skill = skillGroundStomp;
            if (skillLeap != null)
                skill = skillLeap;
            if (skillFuriousCharge != null)
                skill = skillFuriousCharge;
            if (skill.IsOnCooldown) return;
            var buff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.BandOfMight.Sno);
            var isBastionsofWill = Hud.Game.Me.Powers.BuffIsActive(359583);//意志壁垒对戒
            var ShieldBuff = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno);
            var RestraintBuff = Hud.Game.Me.Powers.GetBuff(359583);
            if ((buff.TimeLeftSeconds[1] < 0.8 || (isBastionsofWill && RestraintBuff != null && RestraintBuff.TimeLeftSeconds[1] < 0.4)) && (isBastionsofWill || (ShieldBuff == null || (ShieldBuff != null && ShieldBuff.TimeLeftSeconds[0] < 1))) && (!Timer.IsRunning || Timer.ElapsedMilliseconds > 50))
            {
                
                if (skill.SnoPower.NameEnglish == "Furious Charge" && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_Passive_NoEscape.Sno) && Hud.Game.Me.InGreaterRiftRank > 0 && PublicClassPlugin.IsGuardianAlive(Hud))//狂暴冲锋+无处可逃被动+大秘境BOSS战
                {
                    PlayerScreenCoordinate = Hud.Game.Me.FloorCoordinate.ToScreenCoordinate();
                    LastWorldCoordinate = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();//记录移动前鼠标位置
                    Hud.Interaction.MouseMove(PlayerScreenCoordinate.X, PlayerScreenCoordinate.Y);//移动鼠标到人物位置
                    Hud.Interaction.DoActionAutoShift(skill.Key);
                    Hud.Interaction.MouseMove(LastWorldCoordinate.ToScreenCoordinate().X, LastWorldCoordinate.ToScreenCoordinate().Y);//移动到记录的鼠标位置
                }
                else
                {
                    Hud.Interaction.DoActionAutoShift(skill.Key);
                }
                Timer.Restart();
            }
        }
    }
}