using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX.DirectInput;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.PixelDrama.Town
{
    public class AlwaysUseYourParagon
    {
        public static IntPtr D3Hwnd = IntPtr.Zero;

        [DllImport("USER32.DLL")]
        private static extern IntPtr FindWindow(string ClassName, string WindowText);

        [DllImport("USER32.DLL")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam
        );

        public static void SendPressKey(IntPtr ptr, Keys key)
        {
            SendMessage(ptr, 256U, (IntPtr)((int)key), IntPtr.Zero);
            SendMessage(ptr, 257U, (IntPtr)((int)key), IntPtr.Zero);
        }

        public static void SendHoldKey(Keys key)
        {
            SendMessage(D3Hwnd, 256U, (IntPtr)((int)key), IntPtr.Zero);
        }

        public static void SendReleaseKey(Keys key)
        {
            SendMessage(D3Hwnd, 257U, (IntPtr)((int)key), IntPtr.Zero);
        }

        public void mouseLClickUiE(IUiElement uie) // D3Hwnd
        {
            var x = (int)(uie.Rectangle.X + uie.Rectangle.Width / 2.0f);
            var y = (int)(uie.Rectangle.Y + uie.Rectangle.Height / 2.0f);
            IntPtr lParam = (IntPtr)(y << 16 | (x & 65535));
            SendMessage(D3Hwnd, 513U, (IntPtr)1, lParam);
            SendMessage(D3Hwnd, 514U, (IntPtr)1, lParam);
        }

        private readonly IController Hud;
        protected IUiElement PARAGONUI;
        protected IUiElement PARAGON_TAB_ONE;
        protected IUiElement PARAGON_BUTTON_MAIN;
        protected IUiElement PARAGON_BUTTON_VIT;
        protected IUiElement PARAGON_BUTTON_SPEED;
        protected IUiElement PARAGON_BUTTON_ACCEPT;
        protected IUiElement PARAGON_BUTTON_CANCEL;
        protected IUiElement PARAGON_BUTTON_OPEN;

        private bool EnableParagonButton { get; set; } = false;
        public bool doParagon { get; set; } = false;
        private long msLapseAction { get; set; } = 0;
        private uint LastAreaSno { get; set; } = 0;

        public long msLapseMin { get; set; } = 50;
        public bool MaxSpeed { get; set; }
        public int ParagonMin { get; set; }
        public bool AlsoInNewGame { get; set; }
        public bool SupportVit { get; set; }

        public Keys KeyParagonWindow { get; set; } = Keys.P; // ParagonWindow

        public AlwaysUseYourParagon(IController hud)
        {
            Hud = hud;

            ParagonMin = 800; // Minimum Paragon required for this plugin to start working.
            SupportVit = true; // Increases Vit instead of the Main Stat if you have toxin or icebLink equipped.
            MaxSpeed = true; // Increase movement speed first
            AlsoInNewGame = true; // Work also when you enter a new game.

            D3Hwnd = FindWindow("D3 Main Window Class", null); // D3Hwnd = FindWindow(null, "Diablo III");
            PARAGONUI = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect",
                null,
                null,
                0f,
                0f
            );
            PARAGON_TAB_ONE = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect.tab_1",
                null,
                null,
                0f,
                0f
            );
            PARAGON_BUTTON_MAIN = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect.Bonuses.bonus0.IncreaseStat",
                null,
                null,
                0f,
                0f
            );
            PARAGON_BUTTON_VIT = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect.Bonuses.bonus1.IncreaseStat",
                null,
                null,
                0f,
                0f
            );
            PARAGON_BUTTON_SPEED = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect.Bonuses.bonus2.IncreaseStat",
                null,
                null,
                0f,
                0f
            );
            PARAGON_BUTTON_ACCEPT = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect.AcceptParagonPointsButton",
                null,
                null,
                0f,
                0f
            );
            PARAGON_BUTTON_CANCEL = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect.CancelParagonPointsButton",
                null,
                null,
                0f,
                0f
            );
            PARAGON_BUTTON_OPEN = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.game_notify_dialog_backgroundScreen.dlg_new_paragon.button",
                null,
                null
            );
        }

        public void OnNewArea(bool newGame, ISnoArea area) // bug ??
        {
            if (LastAreaSno != area.Sno)
                LastAreaSno = area.Sno;
            else if (!newGame)
                return; // Fix ??

            if (area.IsTown)
            {
                if (
                    Hud.Game.Me.ParagonPointsAvailable[0] > 0
                    && Hud.Game.Me.CurrentLevelParagon >= ParagonMin
                )
                {
                    msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
                    if (newGame)
                    {
                        if (AlsoInNewGame)
                        {
                            doParagon = true;
                            if (Hud.Game.CurrentAct != 1)
                            {
                                var pl = Hud.AllPlugins.FirstOrDefault(p =>
                                    p.GetType().Name == "AlwaysActOne"
                                ); // So that it doesn't interfere too much with AlwaysActOne
                                if (pl != null && pl.Enabled == true)
                                {
                                    msLapseAction += 10000;
                                }
                            }
                        }
                    }
                    else
                    {
                        doParagon = true;
                    }
                }
                EnableParagonButton = !PARAGON_BUTTON_OPEN.Visible;
            }
        }

        public void TryToUseParagons()
        {
            if (!Hud.Game.IsInGame || Hud.Game.IsPaused)
                return;
            if (doParagon)
            {
                if (Hud.Game.CurrentRealTimeMilliseconds - msLapseAction > msLapseMin) // action inteRval 50ms
                {
                    msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
                    //if (Hud.Game.Me.AnimationState == AcdAnimationState.Idle)
                    {
                        if (PARAGON_TAB_ONE.Visible) // PARAGONUI.Visible && PARAGON_TAB_ONE.Visible
                        {
                            EnableParagonButton = true;
                            if (PARAGON_TAB_ONE.AnimState != 13)
                            {
                                mouseLClickUiE(PARAGON_TAB_ONE);
                            }
                            else
                            {
                                if (MaxSpeed && PARAGON_BUTTON_SPEED.Visible)
                                {
                                    SendHoldKey(Keys.ControlKey);
                                    mouseLClickUiE(PARAGON_BUTTON_SPEED);
                                    SendReleaseKey(Keys.ControlKey);
                                }
                                else
                                {
                                    if (PARAGON_BUTTON_MAIN.Visible) //  doesN't matter if I check PARAGON_BUTTON_MAIN or PARAGON_BUTTON_VIT
                                    {
                                        IUiElement UiEStat;
                                        int i;

                                        if (
                                            SupportVit
                                            && (
                                                Hud.Game.Me.Powers.BuffIsActive(428354)
                                                || Hud.Game.Me.Powers.BuffIsActive(403556)
                                            )
                                        ) // Has the IceBlink/ToxiN Gem equipped => Increase Vit
                                        {
                                            UiEStat = PARAGON_BUTTON_VIT;
                                            i = Hud.Game.Me.ParagonPointsAvailable[1] / 100 + 1;
                                        }
                                        else
                                        {
                                            UiEStat = PARAGON_BUTTON_MAIN;
                                            i = Hud.Game.Me.ParagonPointsAvailable[0] / 100 + 1;
                                        }

                                        SendHoldKey(Keys.ControlKey);
                                        while (i-- > 0)
                                        {
                                            mouseLClickUiE(UiEStat);
                                        }
                                        SendReleaseKey(Keys.ControlKey);
                                    }
                                    else
                                    {
                                        if (
                                            PARAGON_BUTTON_ACCEPT.Visible
                                            && PARAGON_BUTTON_ACCEPT.AnimState != 38
                                        )
                                        {
                                            mouseLClickUiE(PARAGON_BUTTON_ACCEPT);
                                        }
                                        else if (PARAGON_BUTTON_CANCEL.Visible)
                                        {
                                            mouseLClickUiE(PARAGON_BUTTON_CANCEL);
                                        }
                                        doParagon = false;
                                    }
                                }
                            }
                        }
                        else if (Hud.Game.MapMode == MapMode.Minimap)
                        {
                            SendPressKey(D3Hwnd, KeyParagonWindow); // Hud.Interaction.DoAction(ActionKey.ParagonWindow);
                        }
                    }
                }
            }
            else if (Hud.Game.IsInTown)
            {
                if (
                    EnableParagonButton
                    && PARAGON_BUTTON_OPEN.Visible
                    && !Hud.Inventory.InventoryMainUiElement.Visible
                )
                {
                    if (
                        Hud.Game.Me.ParagonPointsAvailable[0] > 0
                        && Hud.Game.Me.CurrentLevelParagon >= ParagonMin
                    )
                    {
                        doParagon = true;
                        msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
                    }
                    EnableParagonButton = false;
                }
            }
        }
    }
}
