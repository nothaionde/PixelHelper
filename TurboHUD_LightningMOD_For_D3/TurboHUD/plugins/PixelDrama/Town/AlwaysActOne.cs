using System;
using System.Linq;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using Turbo.Plugins.Default;
using Turbo.Plugins.PixelDrama;

namespace Turbo.Plugins.PixelDrama.Town
{
	public class AlwaysActOne : BasePlugin, IAfterCollectHandler, INewAreaHandler, IKeyEventHandler
	{
		public static IntPtr D3Hwnd = IntPtr.Zero;

		[DllImport("USER32.DLL")]
		private static extern IntPtr FindWindow(string ClassName, string WindowText);

		[DllImport("USER32.DLL")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

		public static void CMouseLClick(int x, int y)
		{
			SetCursorPos(x, y);
			mouse_event(6U, 0, 0, 0U, IntPtr.Zero);
		}

		public static void SendPressKey(IntPtr ptr, Keys key)
		{
			SendMessage(ptr, 256U, (IntPtr)((int)key), IntPtr.Zero);
			SendMessage(ptr, 257U, (IntPtr)((int)key), IntPtr.Zero);
		}

		public static void SendReleaseKey(Keys key)
		{
			SendMessage(D3Hwnd, 257U, (IntPtr)((int)key), IntPtr.Zero);
		}

		public void mouseLClickUiE(IUiElement uie)	// D3Hwnd
		{
			var x = (int) (uie.Rectangle.X + uie.Rectangle.Width/2.0f);
			var y = (int) (uie.Rectangle.Y + uie.Rectangle.Height/2.0f);
			IntPtr lParam = (IntPtr)(y << 16 | (x & 65535));
			SendMessage(D3Hwnd, 513U, (IntPtr)1, lParam);
			SendMessage(D3Hwnd, 514U, (IntPtr)1, lParam);
		}

		protected IUiElement ActOne;
		protected IUiElement TownOne;
		protected IUiElement ZoomOut;
		protected IUiElement ResumeGame;

		private bool _doTeleport { get; set; } = false;
		private bool teleporting { get; set; } = false;
		private long msLapseAction { get; set; } = 0;
		public long msLapseMin { get; set; } = 50;
		public int maxDistanceToWapypoint = 35;

		private bool doTeleport
		{
			get 
			{ 
				return _doTeleport; 
			}
			set
			{
				_doTeleport = value;
				teleporting = false;
				msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
			}
		}

		public Keys keyWaypointMap { get; set; } = Keys.M; // Waypoint

        public AlwaysActOne()
        {
            Enabled = true;
        }

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				if (!Hud.Input.IsKeyDown(Keys.LControlKey))		// Left control prevents teleport after the new game
				{
					if (area.IsTown && Hud.Game.CurrentAct != 1)
					{
						doTeleport = true;
					}
				}
			}
		}

		public override void Load(IController hud)
		{
			base.Load(hud);

			D3Hwnd = FindWindow("D3 Main Window Class", null); // D3Hwnd = FindWindow(null, "Diablo III");

			TownOne = Hud.Render.RegisterUiElement("Root.NormalLayer.WaypointMap_main.LayoutRoot.OverlayContainer.POI.entry 0.LayoutRoot.Town", null, null);
			ActOne = Hud.Render.RegisterUiElement("Root.NormalLayer.WaypointMap_main.LayoutRoot.OverlayContainer.WorldMap.Act1Open.LayoutRoot.Name", null, null);
			ZoomOut = Hud.Render.RegisterUiElement("Root.NormalLayer.WaypointMap_main.LayoutRoot.OverlayContainer.Zoom.ZoomOut", null, null);
			ResumeGame = Hud.Render.RegisterUiElement("Root.NormalLayer.gamemenu_dialog.gamemenu_bkgrnd.button_resumeGame", null, null);
		}

		public void OnKeyEvent(IKeyEvent keyEvent)
		{
			if (keyEvent.IsPressed)
			{
				if (keyEvent.ControlPressed && keyEvent.ShiftPressed)
				{
					if (keyEvent.Key == Key.A)
					{
						if (!Hud.Game.IsInTown || Hud.Game.CurrentAct != 1)
						{
							doTeleport = true;
						}
					}
				}
			}
		}

		public void AfterCollect()
		{
			if (!PixelHelperSettings.Instance.AlwaysActOne || !Helper._helper.isHelperOn)
			{
				return;
			}
			if (!doTeleport || !Hud.Game.IsInGame)	return;		// !Hud.Window.IsForeground || Hud.Render.UiHidden (are not really needed for teleport after NG)
			if (ResumeGame.Visible)								// Esc => cancels the process, it will also be useful if there is a bug in the code and it goes into a loop
			{
				doTeleport = false;
			}
			else if (Hud.Game.CurrentRealTimeMilliseconds - msLapseAction > msLapseMin) 	// action inteRval 50ms
			{
				if (Hud.Game.Me.AnimationState == AcdAnimationState.Idle)
				{
					if (Hud.Render.WorldMapUiElement.Visible)
					{
						if (Hud.Render.ActMapUiElement.Visible)								// Steep 3
						{
							if (ActOne.Visible)
							{
								mouseLClickUiE(ActOne);
							}
						}
						else
						{
							if (Hud.Game.ActMapCurrentAct == BountyAct.A1)					// Steep 4 , Final
							{
								if (TownOne.Visible)
								{
									mouseLClickUiE(TownOne);
									teleporting = true;										// teleporting to Act 1 , the map disappears, it is Necessary to disable any possible action
								}
							}
							else 															// Steep 2
							{
								if (ZoomOut.Visible)
								{
									mouseLClickUiE(ZoomOut);
								}
							}
						}
					}
					else																	// Steep 1
					{
						if (teleporting)
						{
							doTeleport = false;
						}
						else if (Hud.Game.CurrentRealTimeMilliseconds - msLapseAction > 150)
						{
							SendReleaseKey(Keys.LControlKey); SendReleaseKey(Keys.LShiftKey); SendReleaseKey(Keys.RControlKey);	SendReleaseKey(Keys.RShiftKey);

							var wp  = Hud.Game.Actors.FirstOrDefault(a => a.SnoActor.Kind == ActorKind.Waypoint && a.CentralXyDistanceToMe < maxDistanceToWapypoint);
							if (wp != null && !Hud.Game.IsInTown)
							{
								if (Hud.Window.IsForeground)
								{
									CMouseLClick( (int) wp.FloorCoordinate.ToScreenCoordinate().X, (int) wp.FloorCoordinate.ToScreenCoordinate().Y); // i tried SeNdMessage before, but i managed to make it work much better this way
								}
							}
							else
							{
								SendPressKey(D3Hwnd, keyWaypointMap);					// Hud.Interaction.DoAction(ActionKey.WaypointMap);
							}
						}
						else return;
					}
				}
				msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
			}
		}
	}
}