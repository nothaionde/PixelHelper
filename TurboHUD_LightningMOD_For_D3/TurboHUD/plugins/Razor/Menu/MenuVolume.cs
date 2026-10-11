namespace Turbo.Plugins.Razor.Menu
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	//using Turbo.Plugins.Razor.Menu;
	//using Turbo.Plugins.Razor.Util; //Hud.Sno.GetExpToNextLevel

	public class MenuVolume : BasePlugin, IMenuAddon//, INewAreaHandler, ICustomizer, IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
	{
		public VolumeMode? DefaultVolumeMode { get; private set; } //optional, set this only if you want to override TH defaults //VolumeMode.AutoMasterAndEffects; //VolumeMode.Constant
		public double DefaultVolumeMultiplier { get; private set; } //default is 3 //optional, set this only if you want to override TH defaults, only used when Volume Mode = VolumeMode.AutoMaster or Volume.AutoMasterAndEffects
		public int DefaultConstantVolume { get; private set; } //0-100, default is 100 //optional, set this only if you want to override TH defaults, only used when Volume Mode = VolumeMode.Constant
		
		public double MultiplierIncrement { get; set; } = 0.5;
		public int ConstantIncrement { get; set; } = 10;
		public string TTS_VolumeUp { get; set; } = "Увеличить громкость";
		public string TTS_VolumeDown { get; set; } = "Уменьшить громкость";
		public string TTS_Unmute { get; set; } = "Включить звук";
		public int TTS_Interval { get; set; } = 1000; //don't play TTS again if triggered more frequently than once per TTS_Interval milliseconds

		public string SpeakerSymbol { get; set; } = "🔉"; //🔊
		public string MuteSymbol { get; set; } = "🔇";
		public string IncreaseSymbol { get; set; } = "🡹"; //🡅
		public string DecreaseSymbol { get; set; } = "🡻"; //🡇

		public IFont IconFont { get; set; }
		public IFont RedFont { get; set; }
		public IFont GreenFont { get; set; }
		//public IFont GreyFont { get; set; }
		public IFont VolumeFont { get; set; }
		public IBrush BgBrush { get; set; }
		
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }
		
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
		
        public MenuVolume()
        {
            Enabled = true;
			Priority = 0;
			DockId = "BottomLeft";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			DefaultVolumeMode = Hud.Sound.VolumeMode;
			DefaultConstantVolume = Hud.Sound.ConstantVolume;
			DefaultVolumeMultiplier = Hud.Sound.VolumeMultiplier;
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			if (!string.IsNullOrWhiteSpace(Config))
			{
				if (Config[0] == '1')
				{
					Hud.Sound.VolumeMode = VolumeMode.Constant;
					//DefaultVolumeMode = VolumeMode.Constant;
					if (int.TryParse(Config.Substring(2), out int vol))
						Hud.Sound.ConstantVolume = vol;
				}
				else
				{
					Hud.Sound.VolumeMode = VolumeMode.AutoMasterAndEffects;
					//DefaultVolumeMode = VolumeMode.AutoMasterAndEffects;
					if (double.TryParse(Config.Substring(2), out double vol))
						Hud.Sound.VolumeMultiplier = vol;
				}
			}
			
			/*if (DefaultVolumeMode.HasValue && DefaultVolumeMode.Value != Hud.Sound.VolumeMode)
				Hud.Sound.VolumeMode = DefaultVolumeMode.Value;
			if (DefaultConstantVolume.HasValue && Hud.Sound.ConstantVolume != DefaultConstantVolume.Value)
				Hud.Sound.ConstantVolume = DefaultConstantVolume.Value;
			if (DefaultVolumeMultiplier.HasValue && Hud.Sound.VolumeMultiplier != DefaultVolumeMultiplier.Value)
				Hud.Sound.VolumeMultiplier = DefaultVolumeMultiplier.Value;*/
			
			IconFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, 175, 0, 0, 0, true);
			RedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, 175, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, 175, 0, 0, 0, true);
			//GreyFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true);
			VolumeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 120, 0, false, false, 175, 0, 0, 0, true);
			
			//Label
			Label = new LabelStringDecorator(Hud) {
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize + 1, 255, 255, 255, 255, false, false, 175, 0, 0, 0, true),
				OnBeforeRender = (label) => {
					//muted
					if (Hud.Sound.VolumeMode == VolumeMode.Constant ? Hud.Sound.ConstantVolume == 0 : Hud.Sound.VolumeMultiplier == 0)
					{
						//((LabelStringDecorator)label).Font = RedFont;
						((LabelStringDecorator)label).StaticText = MuteSymbol;
					}
					else
					{
						//((LabelStringDecorator)label).Font = IconFont;
						((LabelStringDecorator)label).StaticText = SpeakerSymbol;
					}
					
					return true;
				}
			};
			
			TextLayout layout = IconFont.GetTextLayout(" 100% ");
			volume_width = layout.Metrics.Width * 1.3f;
			//layout = IconFont.GetTextLayout(IncreaseSymbol);
			//control_width = layout.Metrics.Width;
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ЗВУК") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreateReset((label) => {
							Hud.Sound.VolumeMode = DefaultVolumeMode.Value;
							Hud.Sound.VolumeMultiplier = DefaultVolumeMultiplier;
							Hud.Sound.ConstantVolume = DefaultConstantVolume;
							
							Config = string.Empty;
							plugin.Save();
						}),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud) {
						Hint = plugin.CreateHint("Нажмите, чтобы отключить звук"),
						SpacingRight = 2,
						OnBeforeRender = (label) => {
							//muted
							if (Hud.Sound.VolumeMode == VolumeMode.Constant ? Hud.Sound.ConstantVolume == 0 : Hud.Sound.VolumeMultiplier == 0)
							{
								((LabelStringDecorator)label).Font = RedFont;
								((LabelStringDecorator)label).StaticText = MuteSymbol;
							}
							else
							{
								((LabelStringDecorator)label).Font = IconFont;
								((LabelStringDecorator)label).StaticText = SpeakerSymbol;
							}
							
							return true;
						},
						OnClick = (label) => {
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
							
							Config = GenerateConfig();
							plugin.Save();
						}
					},
					new LabelStringDecorator(Hud, () => Hud.Sound.VolumeMode == VolumeMode.Constant ? Hud.Sound.ConstantVolume.ToString() + "%" : Hud.Sound.VolumeMultiplier.ToString() + "x") {
						Hint = plugin.CreateHint("Нажмите, чтобы переключить режим громкости"),
						Font = VolumeFont, 
						OnClick = (label) => {
							Hud.Sound.VolumeMode = Hud.Sound.VolumeMode == VolumeMode.AutoMasterAndEffects ? VolumeMode.Constant : VolumeMode.AutoMasterAndEffects;
							PlaySound(Hud.Sound.VolumeMode.ToString(), TTS_Interval);
							
							Config = GenerateConfig();
							plugin.Save();
						}
					},
					new LabelStringDecorator(Hud, IncreaseSymbol) {
						Hint = plugin.CreateHint("Увеличить громкость"),
						Font = IconFont,
						SpacingLeft = 2,
						SpacingRight = 2,
						HoveredFont = GreenFont,
						OnClick = (label) => {
							Hud.Sound.IsSpeakEnabled = true;

							if (Hud.Sound.VolumeMode == VolumeMode.AutoMasterAndEffects)
							{
								Hud.Sound.VolumeMultiplier = Hud.Sound.VolumeMultiplier + MultiplierIncrement;
								Hud.Sound.IsSpeakEnabled = true;
								PlaySound(TTS_VolumeUp, TTS_Interval);
							}
							else
							{
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
							}
							
							Config = GenerateConfig();
							plugin.Save();
						}
					},
					new LabelStringDecorator(Hud, DecreaseSymbol) {
						Hint = plugin.CreateHint("Уменьшить громкость"),
						Font = IconFont,
						SpacingLeft = 2,
						//SpacingRight = 2,
						HoveredFont = RedFont,
						OnClick = (label) => {
							if (Hud.Sound.VolumeMode == VolumeMode.AutoMasterAndEffects)
							{
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
							}
							else
							{
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
							}
							
							Config = GenerateConfig();
							plugin.Save();
						}
					}
				) {
					SpacingTop = 5,
					SpacingBottom = 5,
					SpacingLeft = 10,
					SpacingRight = 10,
					BackgroundBrush = plugin.BgBrush,
					OnBeforeRender = (label) => {
						//set the volume display width
						((LabelRowDecorator)label).Labels[1].Width = volume_width;
						//((LabelRowDecorator)label).Labels[2].Width = control_width;
						//((LabelRowDecorator)label).Labels[3].Width = control_width;
						
						//toggle increment/decrement based on volume boundaries
						
						return true;
					}
				}
			);
		}
		
		private void PlaySound(string tts, int limitRepeatInterval = 0)
		{
			if (Hud.Sound.LastSpeak.TimerTest(limitRepeatInterval))
				Hud.Sound.Speak(tts);
		}
		
		private string GenerateConfig()
		{
			if (Hud.Sound.VolumeMode == VolumeMode.Constant)
				return "1:" + Hud.Sound.ConstantVolume.ToString();
			
			return "0:" + Hud.Sound.VolumeMultiplier.ToString();
		}
	}
}