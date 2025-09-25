namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.Default;
    using SharpDX.DirectInput;
    using System.Linq;
    using System;
    using Turbo.Plugins.glq;
    public class MonkTempestRushPlugin : BasePlugin, IInGameTopPainter, IKeyEventHandler
    {
        public IKeyEvent ToggleKeyEvent { get; set; }
        public int StacksRelease { get; set; }
        public IFont InfoFont1 { get; private set; }
        public IFont InfoFont2 { get; private set; }
        public IFont InfoFont3 { get; private set; }
        private string str_running1;
        private string str_running2;
        private string str_tip;
        public bool isHideTip { get; set; }
        public bool Running { get; private set; }
        public bool Rotating { get; private set; }
        private bool isTempestRushMonk = false;
        private IPlayerSkill skillTempestRush = null;
        private IWatch DelayTimer;
        private IWatch DelayTimer2;
        private IMonster Monster = null;
        private IUiElement ChatUI;
        private IUiElement uiBossBattleRequestBox;
        private IUiElement uiBossBattleOpenBox;
        public MonkTempestRushPlugin()
        {
            Enabled = false;
            StacksRelease = 101;//大于100时不自动释放
            isHideTip = false;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F3, false, false, false);
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
            if (Hud.GetPlugin<PickUpPlugin>().WaitingforClick == false && ToggleKeyEvent.Matches(keyEvent) && keyEvent.IsPressed && isTempestRushMonk && !Hud.Inventory.InventoryMainUiElement.Visible)
            {
                if(Running)
                {
                    IWorldCoordinate coord = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate();
                    IMonster _monster = Hud.Game.AliveMonsters.Where(m => m.FloorCoordinate.XYDistanceTo(coord) < 15 && m.Rarity != ActorRarity.RareMinion).OrderByDescending(m => m.MaxHealth).FirstOrDefault();
                    //IMonster _monster = Hud.Game.SelectedMonster2;
                    if(_monster != null &&_monster == Monster)
                    {//锁定同一个目标再次快捷键时停止锁定该目标
                        Monster = null;
                        Rotating = false;
                        DelayTimer.Stop();
                        return;
                    }
                    else
                    {//如果目标为空或切换新目标则刷新目标
                        Monster = _monster;
                    }

                    if(Rotating)
                    {
                        if(Monster == null)
                        {//锁定状态选中空地时停止锁定
                            Rotating = false;
                            DelayTimer.Stop();
                            return;
                        }
                    }
                    else
                    {
                        if (Monster == null)
                        {//非锁定目标状态选中空地时停止旋转
                            Running = false;
                            DelayTimer.Stop();
                            DelayTimer2.Stop();
                            upTempestRush();
                            return;
                        }
                        else
                        {//切换锁定目标
                            Rotating = true;
                            DelayTimer.Start();
                            return;
                        }
                    }
                }
                else
                {
                    DelayTimer2.Start();
                    Running = true;
                }

            }
        }
//MonkTempestRushPlugin.cs

        private void upTempestRush()
        {
            if (skillTempestRush != null)
            {
                for (var i = 0; i < 50; i++)
                {
                    if (Hud.Interaction.IsContinuousActionStarted(skillTempestRush.Key))
                    {
                        Hud.Interaction.StopContinuousAction(skillTempestRush.Key);//弹起风雷冲
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
                str_running1 = "正在风雷冲，" + ToggleKeyEvent + "键围绕目标旋转或停止";
                str_running2 = "正在围绕目标旋转，" + ToggleKeyEvent + "键停止";
                str_tip = ToggleKeyEvent + "键开始风雷冲";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_running1 = "正在暴風奔襲，" + ToggleKeyEvent + "鍵圍繞目標旋轉或停止";
                str_running2 = "正在圍繞目標旋轉，" + ToggleKeyEvent + "鍵停止";
                str_tip = ToggleKeyEvent + "鍵開始暴風奔襲";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_running1 = "Стремительность Урагана," + ToggleKeyEvent + " включить или выключить вращение вокруг монстров";
                str_running2 = "Вращение вокруг монстров," + ToggleKeyEvent + " остановить";
                str_tip = ToggleKeyEvent + " Ключ для каста Стремительность Урагана";
            }
            else
            {
                str_running1 = "TempestRushing," + ToggleKeyEvent + " to rotating around the monster or stop";
                str_running2 = "Rotating around the monster," + ToggleKeyEvent + " to stop";
                str_tip = ToggleKeyEvent + " Key to cast TempestRush";
            }
            if (Hud.Inventory.InventoryMainUiElement.Visible || Hud.GetPlugin<PickUpPlugin>().WaitingforClick == true)
            {
                if (Running)
                {
                    upTempestRush();
                }
                return;
            }
            isTempestRushMonk = Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Monk &&
               Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower == Hud.Sno.SnoPowers.Monk_TempestRush)
                ;//获取自己是风雷冲武僧
            if (isTempestRushMonk)
            {
                skillTempestRush = Hud.Game.Me.Powers.UsedMonkPowers.TempestRush;
            }
            if (Hud.Game.IsInTown)
            {
                if(Running)
                {
                    Running = false;
                    upTempestRush();
                }
                return;
            }
            if (isTempestRushMonk)
            {
                if (!isHideTip)
                {
                    if (Running)
                    {
                        if (Rotating)
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
                || PublicClassPlugin.isHoverValidActor(Hud, 5)
                || Hud.Render.IsAnyBlockingUiElementVisible
                || !Hud.Window.CursorInsideGameWindow()
                || Hud.GetPlugin<PickUpPlugin>().isItemsAround == true
                || Hud.GetPlugin<OpenChestPlugin>().Clicking == true
                || (uiBossBattleRequestBox.Visible && Hud.Window.CursorInsideRect(uiBossBattleRequestBox.Rectangle.Left, uiBossBattleRequestBox.Rectangle.Top, uiBossBattleRequestBox.Rectangle.Width, uiBossBattleRequestBox.Rectangle.Height))
                || (uiBossBattleOpenBox.Visible && Hud.Window.CursorInsideRect(uiBossBattleOpenBox.Rectangle.Left, uiBossBattleOpenBox.Rectangle.Top, uiBossBattleOpenBox.Rectangle.Width, uiBossBattleOpenBox.Rectangle.Height))
                )
                {
                    upTempestRush();
                    return;
                }
                if(PublicClassPlugin.GetBuffCount(Hud, Hud.Sno.SnoPowers.Monk_TempestRush.Sno, 3) >= StacksRelease)
                {
                    if (Hud.Game.AliveMonsters.Where(x => !x.Invulnerable && !x.Invisible && x.CentralXyDistanceToMe < 15).Count() > 1)//15码内至少1个怪才释放风雷冲叠层
                    {
                        upTempestRush();
                        return;
                    }
                }
                if (!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_TempestRush.Sno) && DelayTimer2.IsRunning && DelayTimer2.ElapsedMilliseconds > 100)
                {
                    Hud.Interaction.StopContinuousAction(skillTempestRush.Key);
                    Hud.Interaction.StartContinuousAction(skillTempestRush.Key, skillTempestRush.Key == ActionKey.LeftSkill);//按下风雷冲
                    if(skillTempestRush.Key == ActionKey.LeftSkill) Hud.Interaction.StandStillUp();
                    DelayTimer2.Restart();
                }
                if (Rotating)
                {
                    if (Monster?.IsAlive == true && Monster.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < 200 && !Hud.Game.Me.IsDead && !Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno))//200码以内活着的目标且自己也活着
                    {
                        if (DelayTimer.ElapsedMilliseconds > 50 && !(Hud.Interaction.IsHotKeySet(ActionKey.Move) && Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)))
                        {
                            Hud.Interaction.MouseMove(Monster.FloorCoordinate.ToScreenCoordinate().X, Monster.FloorCoordinate.ToScreenCoordinate().Y);//鼠标点到目标
                            DelayTimer.Restart();
                        }
                    }
                    else
                    {
                        Rotating = false;
                        Monster = null;
                        DelayTimer.Stop();
                    }
                }
            }
        }
    }
}