namespace Turbo.Plugins.LightningMod
{
    using System.Linq;
    using System;
    using Turbo.Plugins.Default;
    using SharpDX.DirectInput;
    using Turbo.Plugins.glq;
    public class DemonHunterStrafeHungeringArrowPlugin : BasePlugin, IInGameTopPainter, IKeyEventHandler
    {
        public IKeyEvent ToggleKeyEvent { get; set; }
        public IKeyEvent zhuisao { get; set; }
        public IFont InfoFont1 { get; private set; }
        public IFont InfoFont2 { get; private set; }
        public IFont InfoFont3 { get; private set; }
        private string str_running1;
        private string str_running2;
        private string str_running3;
        private string str_tip;
        private string str_tip2;
        public bool isHideTip { get; set; }
        public static bool Running { get; set; }
        public int dongneng { get; set; }
        private bool isStrafeDH = false;
        private int SetItemCount = 0;
        private IPlayerSkill skillStrafe = null;
        private IPlayerSkill skillPrimary = null;
        private IWatch DelayTimer;
        private IWatch DelayTimer2;
        private IMonster Monster = null;
        private bool isZhuisao = false;
        private int delaytime = 0;
        private IUiElement ChatUI;
        private IUiElement uiBossBattleRequestBox;
        private IUiElement uiBossBattleOpenBox;

        public DemonHunterStrafeHungeringArrowPlugin()
        {
            Enabled = false;
            dongneng = 21;
            isHideTip = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F3, false, false, false);
            zhuisao = Hud.Input.CreateKeyEvent(true, Key.F4, false, false, false);
            InfoFont1 = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            InfoFont2 = Hud.Render.CreateFont("tahoma", 8, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);
            InfoFont3 = Hud.Render.CreateFont("tahoma", 8, 255, 255, 0, 0, true, false, 255, 0, 0, 0, true);
            DelayTimer = Hud.Time.CreateWatch();
            DelayTimer2 = Hud.Time.CreateWatch();
            ChatUI = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
            uiBossBattleRequestBox = Hud.Render.RegisterUiElement("Root.NormalLayer.boss_join_party_main.LayoutRoot.Background.buttons", null, null);
            uiBossBattleOpenBox = Hud.Render.RegisterUiElement("Root.NormalLayer.boss_enter_main.stack.wrapper", null, null);
        

    }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (Hud.GetPlugin<PickUpPlugin>().WaitingforClick == false && ToggleKeyEvent.Matches(keyEvent) && keyEvent.IsPressed && isStrafeDH && !Hud.Inventory.InventoryMainUiElement.Visible)
            {
                if(Running)
                {
                    Running = false;
                    upStrafe();
                    DelayTimer.Stop();
                    DelayTimer2.Stop();
                }
                else if (SetItemCount >= 4)
                {
                    Running = true;
                    DelayTimer.Start();
                    DelayTimer2.Start();
                }
                else
                {
                    Running = true;
                    DelayTimer2.Start();
                }
            }
            if (zhuisao.Matches(keyEvent) && keyEvent.IsPressed && isStrafeDH && SetItemCount >= 4)
            {
                if (Running)
                {
                    isZhuisao = !isZhuisao;
                }
            }
        }
        private void upStrafe()
        {
            if (skillStrafe != null)
            {
                for (var i = 0; i < 50; i++)
                {
                    if(Hud.Interaction.IsContinuousActionStarted(skillStrafe.Key))
                    {
                        Hud.Interaction.StopContinuousAction(skillStrafe.Key);//弹起扫射
                    }
                    else
                    {
                        break;
                    }
                }

            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.AfterClip)
            {
                return;
            }
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_running1 = "正在扫射，" + ToggleKeyEvent + "键停止，" + zhuisao + "键开启高频模式";
                str_running2 = "正在高频模式，" + zhuisao + "键切换普通模式，"+ ToggleKeyEvent + "键停止";
                str_running3 = "正在扫射，" + ToggleKeyEvent + "键停止";
                str_tip = ToggleKeyEvent + "键开始扫射和主要技能";
                str_tip2 = ToggleKeyEvent + "键开始扫射";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_running1 = "正在掃射，" + ToggleKeyEvent + "鍵停止，" + zhuisao + "鍵開啟高頻模式";
                str_running2 = "正在高頻模式，" + zhuisao + "鍵切換普通模式，" + ToggleKeyEvent + "鍵停止";
                str_running3 = "正在掃射，" + ToggleKeyEvent + "鍵停止";
                str_tip = ToggleKeyEvent + "鍵開始掃射和主要技能";
                str_tip2 = ToggleKeyEvent + "鍵開始掃射";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_running1 = "Обстрел," + ToggleKeyEvent + " для остановки," + zhuisao + " для преключения в высокочастотный режим";
                str_running2 = "Базовое умение высокочастотный режим," + zhuisao + " для перехода в обычный режимe," + ToggleKeyEvent + " для остановки";
                str_running3 = "Обстрел," + ToggleKeyEvent + " для остановки";
                str_tip = ToggleKeyEvent + " Ключ для каста Обстрел и базового умения";
                str_tip2 = ToggleKeyEvent + " Ключ для каста Обстрел";
            }
            else
            {
                str_running1 = "Strafeing," + ToggleKeyEvent + " to stop," + zhuisao + " to switch to high frequency mode";
                str_running2 = "Primary skill high frequency mode," + zhuisao + " to switch to normal mode," + ToggleKeyEvent + " to stop";
                str_running3 = "Strafeing," + ToggleKeyEvent + " to stop";
                str_tip = ToggleKeyEvent + " Key to cast Strafe and Primary skill";
                str_tip2 = ToggleKeyEvent + " Key to cast Strafe";
            }
            
            if (Hud.Inventory.InventoryMainUiElement.Visible || Hud.GetPlugin<PickUpPlugin>().WaitingforClick == true)
            {
                if (Running)
                {
                    upStrafe();
                }
                return;
            }
                
            isStrafeDH = Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.DemonHunter &&
               Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower == Hud.Sno.SnoPowers.DemonHunter_Strafe);//获取自己是扫射DH
            if (isStrafeDH)
            {
                skillStrafe = Hud.Game.Me.Powers.UsedDemonHunterPowers.Strafe;
                skillPrimary = null;
                foreach (var skill in Hud.Game.Me.Powers.UsedSkills)
                {
                    switch (skill.SnoPower.Sno)
                    {
                        case 129215:
                            skillPrimary = Hud.Game.Me.Powers.UsedDemonHunterPowers.HungeringArrow;
                            break;
                        case 361936:
                            skillPrimary = Hud.Game.Me.Powers.UsedDemonHunterPowers.EntanglingShot;
                            break;
                        case 77552:
                            skillPrimary = Hud.Game.Me.Powers.UsedDemonHunterPowers.Bolas;
                            break;
                        case 377450:
                            skillPrimary = Hud.Game.Me.Powers.UsedDemonHunterPowers.EvasiveFire;
                            break;
                        case 86610:
                            skillPrimary = Hud.Game.Me.Powers.UsedDemonHunterPowers.Grenades;
                            break;
                        default:
                            skillPrimary = null;
                            break;
                    }
                    if (skillPrimary != null) break;
                }
            }
            if (!Hud.Game.IsInTown && !glq.PublicClassPlugin.IsGuardianDead(Hud))
            {
                Running = false;
            }
            else
            {
                Running = true;
            }
            if (Hud.Game.IsInTown)
            {
                if(Running)
                {
                    Running = false;
                    upStrafe();
                }
                return;
            }
            if (isStrafeDH)
            {
                SetItemCount = Hud.Game.Me.GetSetItemCount(791249);
                if (!isHideTip)
                {
                    if (SetItemCount >= 4)
                    {
                        if (Running)
                        {
                            if (isZhuisao)
                            {
                                InfoFont3.DrawText(str_running2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont3.GetTextLayout(str_running2).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                            }
                            else
                            {
                                InfoFont2.DrawText(str_running1, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont2.GetTextLayout(str_running1).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                            }
                        }
                        else
                        {
                            InfoFont1.DrawText(str_tip, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont1.GetTextLayout(str_tip).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                        }
                    }
                    else
                    {
                        if (Running)
                        {
                            InfoFont2.DrawText(str_running3, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont2.GetTextLayout(str_running3).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                        }
                        else
                        {
                            InfoFont1.DrawText(str_tip2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().X - InfoFont1.GetTextLayout(str_tip2).Metrics.Width / 2, Hud.Game.Me.FloorCoordinate.ToScreenCoordinate().Y, true);
                        }
                    }
                }
            }
            else
            {
                return;
            }
            if (Running)
            {
                if (Hud.Game.IsLoading
                || Hud.Game.IsPaused
                || Hud.Game.IsInTown
                || !Hud.Game.IsInGame
                || !Hud.Window.IsForeground
                || (!Hud.Render.MinimapUiElement.Visible)
                || Hud.Render.WorldMapUiElement.Visible
                || ChatUI.Visible
                || Hud.Render.ActMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Render.WorldMapUiElement.LastVisibleSystemTick > DateTime.Now.Ticks - (500 * 10000)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyAllWithCast.Sno)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyWithCast.Sno)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_IdentifyWithCastLegendary.Sno)
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_AxeOperateGizmo.Sno)//悬赏任务读条
                || Hud.Game.Me.AnimationState == AcdAnimationState.Transform
                || Hud.Game.Me.AnimationState == AcdAnimationState.CastingPortal
                || Hud.Game.Me.IsDead
                || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno)
                || PublicClassPlugin.isHoverValidActor(Hud,5)
                || Hud.Render.IsAnyBlockingUiElementVisible
                || !Hud.Window.CursorInsideGameWindow()
                || Hud.GetPlugin<PickUpPlugin>().isItemsAround == true
                || Hud.GetPlugin<OpenChestPlugin>().Clicking == true
                || (uiBossBattleRequestBox.Visible && Hud.Window.CursorInsideRect(uiBossBattleRequestBox.Rectangle.Left, uiBossBattleRequestBox.Rectangle.Top, uiBossBattleRequestBox.Rectangle.Width, uiBossBattleRequestBox.Rectangle.Height))
                || (uiBossBattleOpenBox.Visible && Hud.Window.CursorInsideRect(uiBossBattleOpenBox.Rectangle.Left, uiBossBattleOpenBox.Rectangle.Top, uiBossBattleOpenBox.Rectangle.Width, uiBossBattleOpenBox.Rectangle.Height))
                )
                {
                    upStrafe();
                    return;
                }
                if (!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.DemonHunter_Strafe.Sno, 0) && DelayTimer2.IsRunning && DelayTimer2.ElapsedMilliseconds > 50)
                {
                    Hud.Interaction.StopContinuousAction(skillStrafe.Key);
                    Hud.Interaction.StartContinuousAction(skillStrafe.Key, skillStrafe.Key == ActionKey.LeftSkill);//按下扫射
                    if(skillStrafe.Key == ActionKey.LeftSkill) Hud.Interaction.StandStillUp();
                    DelayTimer2.Restart();
                }
                if (SetItemCount >= 4 && isZhuisao)
                {
                    DoHungeringArrow();
                }
                else if (SetItemCount >= 4)
                {
                    if (Hud.Game.ActorQuery.OnScreenMonsterCount == 0)
                    {
                        if(PublicClassPlugin.GetBuffCount(Hud, 484289, 10) <= 16)
                            DoHungeringArrow();
                    }
                    else
                    {
                        if (PublicClassPlugin.GetBuffLeftTime(Hud, 484289, 10) <= dongneng)
                            DoHungeringArrow();
                    }
                }
            }
        }
        private void DoHungeringArrow()
        {
            if (DelayTimer.ElapsedMilliseconds > 30 && !(Hud.Interaction.IsHotKeySet(ActionKey.Move) && Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)) && !PublicClassPlugin.isHoverValidActor(Hud,10))
            {
                Hud.Interaction.DoActionAutoShift(skillPrimary.Key);//释放主动技能
                DelayTimer.Restart();
            }
        }
    }
}