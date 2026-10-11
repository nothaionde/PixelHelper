namespace Turbo.Plugins.Razor //<MyPluginsLocation> is the name of the folder in TurboHUD\plugins\ where this plugin file is located
{
	using System;
	using System.Linq;
	using System.Threading;
	using SharpDX.DirectWrite; //TextLayout

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Click;
	using Turbo.Plugins.Razor.Movable; 

	public class VolumeControls : BasePlugin, IMovable, ILeftClickHandler//, IAfterCollectHandler
	{
		public VolumeMode? DefaultVolumeMode = null; //optional, set this only if you want to override TH defaults //VolumeMode.AutoMasterAndEffects; //VolumeMode.Constant
		public double? DefaultVolumeMultiplier = null; //default is 3 //optional, set this only if you want to override TH defaults, only used when Volume Mode = VolumeMode.AutoMaster or Volume.AutoMasterAndEffects
		public int? DefaultConstantVolume = null; //0-100, default is 100 //optional, set this only if you want to override TH defaults, only used when Volume Mode = VolumeMode.Constant
		
		public double MultiplierIncrement { get; set; } = 0.5;
		public int ConstantIncrement { get; set; } = 10;
		//public string TTS_VolumeUp { get; set; } = "Volume Up";
		//public string TTS_VolumeDown { get; set; } = "Volume Down";
		//public string TTS_Unmute { get; set; } = "Un Mute";
		public string TTS_VolumeUp { get; set; } = "Громче";
		public string TTS_VolumeDown { get; set; } = "Тише";
		public string TTS_Unmute { get; set; } = "Включено";
		public int TTS_Interval { get; set; } = 1000; //don't play TTS again if triggered more frequently than once per TTS_Interval milliseconds
		
		public float StartingPositionX { get; set; } //optional
		public float StartingPositionY { get; set; } //optional

		public IFont IconFont { get; set; }
		public IFont RedFont { get; set; }
		public IFont GreenFont { get; set; }
		//public IFont GreyFont { get; set; }
		public IFont VolumeFont { get; set; }
		public IBrush BgBrush { get; set; }

		public string SpeakerSymbol { get; set; } = "🔉"; //🔊 🔉
		public string MuteSymbol { get; set; } = "🔇"; //🔇
		public string IncreaseSymbol { get; set; } = "🡹"; //🡅
		public string DecreaseSymbol { get; set; } = "🡻"; //🡇
		
		private Action ClickFunc;
		private Action IncrementMultiplier;
		private Action DecrementMultiplier;
		private Action IncrementConstant;
		private Action DecrementConstant;
		private Action ToggleMode;
		private Action ToggleMute;
		
		private float volume_width;
		private float control_width;
		private double last_volume_multiplier;
		private int last_volume_constant;
		
		private string last_said = null;
		
		public VolumeControls()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			
			IconFont = Hud.Render.CreateFont("tahoma", 8f, 225, 255, 255, 255, false, false, 175, 0, 0, 0, true);
			RedFont = Hud.Render.CreateFont("tahoma", 8f, 225, 255, 0, 0, false, false, 175, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", 8f, 225, 0, 255, 0, false, false, 175, 0, 0, 0, true);
			//GreyFont = Hud.Render.CreateFont("tahoma", 8f, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true);
			VolumeFont = Hud.Render.CreateFont("tahoma", 8f, 225, 255, 120, 0, false, false, 175, 0, 0, 0, true);
			
			BgBrush = Hud.Render.CreateBrush(125, 0, 0, 0, 0);
			
			IncrementMultiplier = () => { 
				Hud.Sound.VolumeMultiplier = Hud.Sound.VolumeMultiplier + MultiplierIncrement;
				Hud.Sound.IsSpeakEnabled = true;
				PlaySound(TTS_VolumeUp, TTS_Interval);
				
			};
			DecrementMultiplier = () => { 
				if (Hud.Sound.VolumeMultiplier - MultiplierIncrement > 0)
				{
					Hud.Sound.IsSpeakEnabled = true;

					Hud.Sound.VolumeMultiplier = Hud.Sound.VolumeMultiplier - MultiplierIncrement;
					PlaySound(TTS_VolumeDown, TTS_Interval);
				}
				else if (Hud.Sound.VolumeMultiplier != 0)
				{
					Hud.Sound.VolumeMultiplier = 0;
					Hud.Sound.IsSpeakEnabled = false;
				}
			};
			IncrementConstant = () => {
				Hud.Sound.IsSpeakEnabled = true;
				
				if (Hud.Sound.ConstantVolume + ConstantIncrement < 100)
				{
					
					Hud.Sound.ConstantVolume = Hud.Sound.ConstantVolume + ConstantIncrement;
					PlaySound(TTS_VolumeUp, TTS_Interval);
				}
				else if (Hud.Sound.ConstantVolume != 100)
				{
					Hud.Sound.ConstantVolume = 100;
					PlaySound(TTS_VolumeUp, TTS_Interval);
				}
			};
			DecrementConstant = () => {
				if (Hud.Sound.ConstantVolume - ConstantIncrement > 0)
				{
					Hud.Sound.ConstantVolume = Hud.Sound.ConstantVolume - ConstantIncrement;
					PlaySound(TTS_VolumeDown, TTS_Interval);
				}
				else if (Hud.Sound.ConstantVolume != 0)
				{
					Hud.Sound.ConstantVolume = 0;
					Hud.Sound.IsSpeakEnabled = false;
				}
			};
			ToggleMode = () => {
				Hud.Sound.VolumeMode = Hud.Sound.VolumeMode == VolumeMode.AutoMasterAndEffects ? VolumeMode.Constant : VolumeMode.AutoMasterAndEffects;
				PlaySound(Hud.Sound.VolumeMode.ToString(), TTS_Interval);
			};
			ToggleMute = () => {
				//if (Hud.Sound.VolumeMode == VolumeMode.Constant)
				//{
					if (Hud.Sound.VolumeMode == VolumeMode.Constant ? Hud.Sound.ConstantVolume == 0 : Hud.Sound.VolumeMultiplier == 0)
					{
						Hud.Sound.IsSpeakEnabled = true;

						Hud.Sound.VolumeMultiplier = last_volume_multiplier;
						Hud.Sound.ConstantVolume = last_volume_constant;
						PlaySound(TTS_Unmute);
					}
					else
					{
						last_volume_multiplier = Hud.Sound.VolumeMultiplier;
						Hud.Sound.VolumeMultiplier = 0;
						
						last_volume_constant = Hud.Sound.ConstantVolume;
						Hud.Sound.ConstantVolume = 0;
						
						Hud.Sound.IsSpeakEnabled = false;
					}
				/*}
				else
				{
					if (Hud.Sound.VolumeMultiplier == 0)
					{
						Hud.Sound.IsSpeakEnabled = true;

						Hud.Sound.VolumeMultiplier = last_volume_multiplier;
						Hud.Sound.ConstantVolume = last_volume_constant;
						PlaySound(TTS_Unmute);
					}
					else
					{
						last_volume_multiplier = Hud.Sound.VolumeMultiplier;
						Hud.Sound.VolumeMultiplier = 0;
						
						last_volume_constant = Hud.Sound.ConstantVolume;
						Hud.Sound.ConstantVolume = 0;
						
						Hud.Sound.IsSpeakEnabled = false;
					}
				}*/
			};
		}
		
		/*public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			foreach (IPlugin plugin in Hud.AllPlugins.Where(p => p.Enabled))
			{
				if (plugin is CoEAnnouncer)
				{
					//Type t = plugin.GetType();
					Hud.RunOnPlugin<typeof(t)>(p => {
						Hud.Sound.VolumeMultiplier = 9;
					});
				}
				else
				{
					//Type t = plugin.GetType();
					Hud.RunOnPlugin<typeof(t)>(p => {
						Hud.Sound.VolumeMultiplier = 1;
					});					
				}
			}
		}*/
		
		//required for IMovable, this function is called when MovableController first acknowledges this plugin's existence
		public void OnRegister(MovableController mover)
		{
			if (DefaultVolumeMode.HasValue && DefaultVolumeMode.Value != Hud.Sound.VolumeMode)
				Hud.Sound.VolumeMode = DefaultVolumeMode.Value;
			if (DefaultConstantVolume.HasValue && Hud.Sound.ConstantVolume != DefaultConstantVolume.Value)
				Hud.Sound.ConstantVolume = DefaultConstantVolume.Value;
			if (DefaultVolumeMultiplier.HasValue && Hud.Sound.VolumeMultiplier != DefaultVolumeMultiplier.Value)
				Hud.Sound.VolumeMultiplier = DefaultVolumeMultiplier.Value;

			//Initialize position and dimension elements of as many movable areas you want. The resize mode options are:
			//ResizeMode.Off - no resize
			//ResizeMode.On - free resize
			//ResizeMode.FixedRatio - keep the ratios of the starting dimensions
			//ResizeMode.Horizontal - resize horizontally only
			//ResizeMode.Vertical - resize vertically only
			
			//IUIElement.Rectangle works at this point in execution, so you can reference existing UI elements to calculate starting positions relative to them
			TextLayout layout = IconFont.GetTextLayout(" 100% ");
			volume_width = layout.Metrics.Width;
			layout = IconFont.GetTextLayout(IncreaseSymbol);
			control_width = layout.Metrics.Width;

			if (StartingPositionX == 0 && StartingPositionY == 0)
			{
				var uiRect = Hud.Render.InGameBottomHudUiElement.Rectangle;
				StartingPositionX = uiRect.Left + (uiRect.Width * 0.84f); //0.09f
				StartingPositionY = uiRect.Bottom - Hud.Window.Size.Height * 0.009f - (Hud.Window.Size.Height / 600) - layout.Metrics.Height - 5;
			}
			
			layout = IconFont.GetTextLayout(SpeakerSymbol);
			float width = layout.Metrics.Width + 5 + volume_width + 3 + control_width*2;
			//IconFont.GetTextLayout(SpeakerSymbol + " 100% 🡹 🡻");
			
			mover.CreateArea(
				this,
				"Panel", //area name
				new System.Drawing.RectangleF(StartingPositionX, StartingPositionY, width, layout.Metrics.Height), //position + dimensions
				true, //enabled at start? (visible upon creation)
				true, //save to config file?
				ResizeMode.Off //resize mode
			);
			
			/*mover.CreateArea(
				this,
				"Bars", //area name
				new RectangleF(x2, y2, w2, h2), //position + dimensions
				true, //enabled at start? (visible upon creation)
				true, //save to config file?
				ResizeMode.Horizontal, //resize mode
				ClipState.AfterClip //specify a different clipstate instead of the default (ClipState.BeforeClip) if desired
			);
			
			//creating a "temporary" movable area that is deleted when the disable command (Ctrl+X) is applied to it
			var temporaryArea = mover.CreateArea(
				this,
				"Temporary", //area name
				new RectangleF(x3, y3, w3, h3), //position + dimensions
				true, //enabled at start? (visible upon creation)
				false, //save to config file?
				ResizeMode.FixedRatio //resize mode
			);
			temporaryArea.DeleteOnDisable = true;*/
		}

		//required for IMovable, this is called whenever MovableController wants this plugin to draw something on the screen
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			
			if(Hud.Inventory.InventoryMainUiElement.Visible) 
			{
				return;
			}			
			
			//if the area is currently being moved (on the cursor) from its original position, deltaX and deltaY will be > 0
			//the movable area dimensions you can reference for drawing on the screen
			var x = area.Rectangle.X + deltaX;
			var y = area.Rectangle.Y + deltaY;
			var width = area.Rectangle.Width;
			var height = area.Rectangle.Height;
			
/*	// Раскоментировать для привязки без передвижного меню F12. User\MovablePluginConfig китайский ТХ
			var uiRect = Hud.Render.InGameBottomHudUiElement.Rectangle;
			
			TextLayout layout2 = IconFont.GetTextLayout(" 100% ");
			volume_width = layout2.Metrics.Width;
			layout2 = IconFont.GetTextLayout(IncreaseSymbol);
			control_width = layout2.Metrics.Width;
			
			x = uiRect.Left + (uiRect.Width * 0.84f); //0.09f
			y = uiRect.Bottom - Hud.Window.Size.Height * 0.009f - (Hud.Window.Size.Height / 600) - layout2.Metrics.Height - 5;			
								
			layout2 = IconFont.GetTextLayout(SpeakerSymbol);
			width = layout2.Metrics.Width + 5 + volume_width + 3 + control_width*2;			
			height = layout2.Metrics.Height;

			
*////////////////			
			
			
			BgBrush.DrawRectangle(x - 5, y - 2, width + 10, height + 6);
			
			bool isConstantMode = Hud.Sound.VolumeMode == VolumeMode.Constant;
			ClickFunc = null;
			
			//speaker icon
			TextLayout layout = IconFont.GetTextLayout(Hud.Sound.VolumeMultiplier == 0 ? MuteSymbol : SpeakerSymbol);
			IconFont.DrawText(layout, x, y);
			if (ClickFunc == null && !mover.EditMode && Hud.Window.CursorInsideRect(x, y, layout.Metrics.Width, layout.Metrics.Height))
				ClickFunc = ToggleMute;
			x += layout.Metrics.Width + 5;

			//two modes
			if (isConstantMode)
			{
				TextLayout volume = VolumeFont.GetTextLayout(Hud.Sound.ConstantVolume.ToString() + "%");
				VolumeFont.DrawText(volume, x + volume_width*0.5f - volume.Metrics.Width*0.5f, y + layout.Metrics.Height*0.5f - volume.Metrics.Height*0.5f);
				if (ClickFunc == null && !mover.EditMode && Hud.Window.CursorInsideRect(x, y, volume_width, layout.Metrics.Height))
					ClickFunc = ToggleMode;
				x += volume_width + 3; //volume.Metrics.Width + 5;
				
				if (Hud.Sound.ConstantVolume < 100)
				{
					if (Hud.Window.CursorInsideRect(x, y, control_width, layout.Metrics.Height))
					{
						TextLayout control = GreenFont.GetTextLayout(IncreaseSymbol);
						GreenFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
						if (ClickFunc == null && !mover.EditMode)
							ClickFunc = IncrementConstant;
					}
					else
					{
						TextLayout control = IconFont.GetTextLayout(IncreaseSymbol);
						IconFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
					}
					x += control_width;
				}
				
				if (Hud.Sound.ConstantVolume > 0)
				{
					if (Hud.Window.CursorInsideRect(x, y, control_width, layout.Metrics.Height))
					{
						TextLayout control = RedFont.GetTextLayout(DecreaseSymbol);
						RedFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
						if (ClickFunc == null && !mover.EditMode)
							ClickFunc = DecrementConstant;
					}
					else
					{
						TextLayout control = IconFont.GetTextLayout(DecreaseSymbol);
						IconFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
					}
				}
			}
			else
			{
				TextLayout volume = VolumeFont.GetTextLayout(Hud.Sound.VolumeMultiplier.ToString() + "x");
				VolumeFont.DrawText(volume, x + volume_width*0.5f - volume.Metrics.Width*0.5f, y + layout.Metrics.Height*0.5f - volume.Metrics.Height*0.5f);
				if (ClickFunc == null && !mover.EditMode && Hud.Window.CursorInsideRect(x, y, volume_width, layout.Metrics.Height))
					ClickFunc = ToggleMode;
				x += volume_width + 3;
				
				if (Hud.Window.CursorInsideRect(x, y, control_width, layout.Metrics.Height))
				{
					TextLayout control = GreenFont.GetTextLayout(IncreaseSymbol);
					GreenFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
					if (ClickFunc == null && !mover.EditMode)
						ClickFunc = IncrementMultiplier;
				}
				else
				{
					TextLayout control = IconFont.GetTextLayout(IncreaseSymbol);
					IconFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
				}
				x += control_width;
				
				if (Hud.Sound.VolumeMultiplier > 0)
				{
					if (Hud.Window.CursorInsideRect(x, y, control_width, layout.Metrics.Height))
					{
						TextLayout control = RedFont.GetTextLayout(DecreaseSymbol);
						RedFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
						if (ClickFunc == null && !mover.EditMode)
							ClickFunc = DecrementMultiplier;
					}
					else
					{
						TextLayout control = IconFont.GetTextLayout(DecreaseSymbol);
						IconFont.DrawText(control, x, y + layout.Metrics.Height*0.5f - control.Metrics.Height*0.5f);
					}
				}
			}

		}
		
		public void PlaySound(string tts, int limitRepeatInterval = 0)
		{
			if (Hud.Sound.LastSpeak.TimerTest(limitRepeatInterval))
				Hud.Sound.Speak(tts);
		}
		
		/*public void DelayedHint(string hint)
		{
			
		}*/
		
		public void OnLeftMouseDown()
		{
			if (ClickFunc is object)
			{
				ClickFunc();
			}
		}
		
		public void OnLeftMouseUp()
		{
			
		}
	}
}