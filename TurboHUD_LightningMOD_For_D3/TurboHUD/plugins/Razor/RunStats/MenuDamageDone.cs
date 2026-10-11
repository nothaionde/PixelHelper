/*using Turbo.Plugins.Default;
using System.Drawing;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading;*/

namespace Turbo.Plugins.Razor.RunStats
{
	using SharpDX.DirectInput; //Key
	using SharpDX.DirectWrite; //TextLayout
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;
	using Turbo.Plugins.Razor.Util;
	
    public class MenuDamageDone : BasePlugin, IMenuAddon, IAfterCollectHandler, IMenuSaveHandler//, ICustomizer
	{
		public int DamageEventsShown { get; set; } = 15;
		//public int DamageEventsStored { get; set; } = 150; //10 pages
		
		public string TextViewLog { get; set; } = "Журнал";
		public string TextViewGraph { get; set; } = "График";
		public string TextHintViewLog { get; set; } = "Открыть журнал";
		public string TextHintViewGraph { get; set; } = "Открыть график";
		
		public IFont TextFont { get; set; } //label font
		public IFont TitleFont { get; set; }

		public IBrush GraphBgBrush { get; set; }
		public IFont DpsFont { get; set; }
		public IFont AvgFont { get; set; }
		public IFont DmgFont { get; set; }
		public IFont CritFont { get; set; }
		public IFont MHPFont { get; set; }

		//public IFont HighFont { get; set; }
		//public IFont MediumFont { get; set; }
		//public IFont LowFont { get; set; }
		//public Dictionary<Threshold, IFont> LatencyFonts { get; set; }
		
		public int GraphDuration { get; set; } = 30; //in seconds		
		public float GraphWidth { get; set; } //Func<float>
		public float GraphHeight { get; set; } //Func<float>
		public bool InvertGraph { get; set; } = true;
		public int YAxisMarkersCount { get; set; } = 5;
		public int XAxisMarkersCount { get; set; } = 6;

		public double HighestHit { get; private set; }
		//public double LowestHit { get; private set; }
		public double MaxNormalHit { get; private set; }
		public double MinNormalHit { get; private set; } = double.MaxValue;
		public double MaxCritHit { get; private set; }
		public double MinCritHit { get; private set; } = double.MaxValue;
		public DateTime MaxNormalTime { get; private set; }
		public DateTime MinNormalTime { get; private set; }
		public DateTime MaxCritTime { get; private set; }
		public DateTime MinCritTime { get; private set; }
		
		private LabelStringDecorator ViewLogButton;
		private LabelStringDecorator ViewGraphButton;
		
		private LabelGraphDecorator Graph;
		private GraphLine LineMonsterHPLoss;
		private GraphLine LineDamageDone;
		private GraphLine LineNormalDamage;
		private GraphLine LineCritDamage;
		private GraphLine LineCurDPS;
		private GraphLine LineAvgDPS;
		private GraphLine LineSheetDPS;
		private GraphZone ZoneOculus;
		
		private LabelTableDecorator Table;
		private LabelStringDecorator MinNormalLabel;
		private LabelStringDecorator MaxNormalLabel;
		private LabelStringDecorator MinCritLabel;
		private LabelStringDecorator MaxCritLabel;
		private double IntervalDamage;
		private double IntervalCritDamage;
		private double IntervalNormalDamage;
		
		public class DamageEvent
		{
			public DateTime Time { get; set; }
			public double TotalDamage { get; set; }
			public double CritDamage { get; set; }
			//public double NormalDamage{ get; set; }
			public string Breakdown { get; set; }
			
			public DamageEvent(DateTime time, double damage, double crit, string breakdown)
			{
				Time = time;
				TotalDamage = damage;
				CritDamage = crit;
				Breakdown = breakdown;
			}
			
			public override string ToString()
			{
				if (!string.IsNullOrEmpty(Breakdown))
					return "[" + Time.ToString("hh:mm:ss.fff tt") + "] " + string.Format("{0:n5}", TotalDamage) + " (" + Breakdown + ")";
				return "[" + Time.ToString("hh:mm:ss.fff tt") + "] " + string.Format("{0:n5}", TotalDamage);
			}
		}
		private List<DamageEvent> DamageEvents = new List<DamageEvent>();
		
		//IMenuAddon
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; } //which dock does this plugin start in?
		public string Id { get; set; } //will be set by MenuPlugin
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		//IMenuSaveHandler
		public bool SaveMode { get; set; } = false; //initially off
		public System.Func<bool> SaveCondition { get; set; } //called when SaveMode = true to check whether or not SaveToFile should be called
		//public string SavePath { get; set; }
		//public string SaveFileName { get; set; } = "DamageLog";
		//private int LastSaveTick = 0;
		private DateTime LastSaveTime;
		
		//bookkeeping
		private DateTime LastRecordedTime;
		private double LastSeenDamageDealtHit;
		private double FirstSeenDamageDealtAll;
		private double LastSeenDamageDealtAll;
		private double LastSeenDamageDealtCrit;
		private uint LastSeenHero;
		private IWatch ConduitPlayTime;
		//private bool ManualPause = false;
		
		private LabelColumnDecorator GraphUI;
		private LabelCanvasDecorator LogUI;
		private LabelRowDecorator PagingUI;
		private LabelRowDecorator SelectViewUI;
		private LabelRowDecorator CurrentDPSUI;
		
        public MenuDamageDone()
		{
			Enabled = true;
			DockId = "BottomCenter"; //"TopCenterDock";
			Priority = 20;
		}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			ConduitPlayTime = Hud.Time.CreateWatch();
		}
		
		/*public void Customize()
		{
			//if (HideDefaultLatencyPlugin)
			//	Hud.TogglePlugin<NetworkLatencyPlugin>(false);
		}*/
		
		public void OnRegister(MenuPlugin plugin)
		{
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 191, 255, false, false, 100, 0, 0, 0, true); //Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 135, 135, 135, false, false, true);
			TitleFont = Hud.Render.CreateFont("tahoma", 7f, 200, 211, 228, 255, false, false, 100, 0, 0, 0, true);
			DmgFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 211, 228, 255, false, false, 100, 0, 0, 0, true);
			DpsFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 91, 237, 59, false, false, 100, 0, 0, 0, true);
			AvgFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 250, 125, 0, false, false, 100, 0, 0, 0, true); //86, 172, 219 //107, 96, 255 //73, 255, 239 //84, 106, 255
			CritFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 211, 237, 59, false, false, 100, 0, 0, 0, true);
			MHPFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 73, 73, false, false, 100, 0, 0, 0, true);
			
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => /*ValueToString(Hud.Game.Me.Offense.SheetDps, ValueFormat.LongNumber)*/((double)Hud.Game.Me.Offense.SheetDps).ToHumanReadable(4)) {Font = TextFont, FuncCacheDuration = 2, SpacingRight = 2},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(218205305)) {TextureHeight = 30, ContentHeight = plugin.MenuHeight, ContentWidth = plugin.MenuHeight - 4, SpacingTop = 1, SpacingBottom = -1} //Hud.Texture.GetTexture(3983130311/*83494678452*/)) {TextureHeight = 38, ContentHeight = plugin.MenuHeight} //
			) {Hint = plugin.CreateHint("Лист ДПС")};
			
			//HintViewGraph = plugin.CreateHint(TextHintViewGraph);
			//HintViewLog = plugin.CreateHint(TextHintViewLog);
			
			LineDamageDone = new GraphLine("Нанесенный урон") 
			{ 
				/*DataFunc = () => {
					var delta = Hud.Tracker.Session.DamageDealtCrit - LastSeenDamageDealtCrit;
					LastSeenDamageDealtCrit = Hud.Tracker.Session.DamageDealtCrit;
					
					return (ConduitPlayTime.IsRunning && delta == 0 ? //damage data doesn't update while conduit is active
						-1 :
						delta
					);
				},*/
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 211, 237, 59, 1.5f), 
				Font = AvgFont 
			};
			LineNormalDamage = new GraphLine("Некритический урон") 
			{ 
				/*DataFunc = () => {
					var all = Hud.Tracker.Session.DamageDealtAll - LastSeenDamageDealtAll;
					var crit = Hud.Tracker.Session.DamageDealtCrit - LastSeenDamageDealtCrit; //LastSeenDamageDealtCrit is updated in another GraphLine, don't have to modify it here

					LastSeenDamageDealtAll = Hud.Tracker.Session.DamageDealtAll;
					
					return (all < crit ? 
						0 :
						(ConduitPlayTime.IsRunning && all == 0 ? //hud doesn't update damage data while conduit is active
							-1 :
							all - crit
						)
					);
				},*/
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 211, 228, 255, 1f), 
				Font = DmgFont
			};
			LineCritDamage = new GraphLine("Критический урон") 
			{ 
				/*DataFunc = () => {
					var delta = Hud.Tracker.Session.DamageDealtCrit - LastSeenDamageDealtCrit;
					LastSeenDamageDealtCrit = Hud.Tracker.Session.DamageDealtCrit;
					
					return (ConduitPlayTime.IsRunning && delta == 0 ? //damage data doesn't update while conduit is active
						-1 :
						delta
					);
				},*/
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 211, 237, 59, 1f), 
				Font = CritFont 
			};
			LineMonsterHPLoss = new GraphLine("Урон, нанесенный монстрам")
			{ 
				/*DataFunc = () => {
					var dps = Hud.Stat.MonsterHitpointDecreasePerfCounter.LastValue; //Monster HP Loss Rate
					
					return (dps < 0 ? //possibly a number overflow error
						-1 : 
						dps
					);
				},*/
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 255, 73, 73, 1), 
				Font = MHPFont,
			};
			LineCurDPS = new GraphLine("Текущий ДПС")
			{ 
				//Name = "Current DPS", 
				/*DataFunc = () => {
					var dps = Hud.Game.Me.Damage.CurrentDps;
					
					return (ConduitPlayTime.IsRunning && dps == 0 ? //hud doesn't update damage data while conduit is active
						-1 : 
						dps
					);
				},*/
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 91, 237, 59, 1), 
				Font = DpsFont
			};
			/*LineAvgDPS = new GraphLine("Average DPS")
			{ 
				//Name = "Current DPS", 
				DataFunc = () => {
					var dps = Hud.Game.Me.Damage.CurrentDps;
					
					return (ConduitPlayTime.IsRunning && dps == 0 ? //hud doesn't update damage data while conduit is active
						-1 : 
						dps
					);
				},
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 91, 237, 59, 1), 
				Font = AvgFont
			};*/
			LineSheetDPS = new GraphLine("Sheet DPS") 
			{ 
				//DataFunc = () => (double)Hud.Game.Me.Offense.SheetDps,
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 0, 191, 255, 1f), //250, 125, 0 //107, 96, 255
				Font = TextFont
			};
			
			ZoneOculus = new GraphZone("Oculus (Buff)")
			{
				Brush = Hud.Render.CreateBrush(35, 128, 255, 0, 0), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 128, 255, 0, false, false, true),
			};
			
			MinNormalLabel = new LabelStringDecorator(Hud, () => MinNormalHit == double.MaxValue ? "0" : MinNormalHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MinNormalTime.ToString("T")), Font = LineNormalDamage.Font, Alignment = HorizontalAlign.Left, SpacingRight = 3};
			MaxNormalLabel = new LabelStringDecorator(Hud, () => MaxNormalHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MaxNormalTime.ToString("T")), Font = LineNormalDamage.Font, Alignment = HorizontalAlign.Left};
			MinCritLabel = new LabelStringDecorator(Hud, () => MinCritHit == double.MaxValue ? "0" : MinCritHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MinCritTime.ToString("T")), Font = LineCritDamage.Font, Alignment = HorizontalAlign.Right, SpacingRight = 3};
			MaxCritLabel = new LabelStringDecorator(Hud, () => MaxCritHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MaxCritTime.ToString("T")), Font = LineCritDamage.Font, Alignment = HorizontalAlign.Right};
			
			Graph = new LabelGraphDecorator(Hud) {
				DataFormat = (d) => ValueToString(d, ValueFormat.LongNumber),
				GraphWidth = plugin.StandardGraphWidth,
				GraphHeight = plugin.StandardGraphHeight,
				GraphShownDuration = GraphDuration,
				GraphMaxDuration = GraphDuration * 40,
				AbsoluteMin = 0,
				Zones = new List<GraphZone>() {ZoneOculus}, //{ZoneOculus, ZoneCOE},
				Lines = new List<GraphLine>() {LineDamageDone, LineCritDamage, LineNormalDamage, LineMonsterHPLoss, LineCurDPS/*, LineSheetDPS*/},
				//BackgroundBrush = plugin.BgBrush,
				IsAutoPaused = () => Hud.Game.IsInTown, // || !Hud.Game.IsInGame,
				SpacingLeft = 15,
				SpacingRight = 15,
				SpacingBottom = 25,
			};
			
			Table = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud, 
					new LabelStringDecorator(Hud) {Font = DmgFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 1, SpacingBottom = 1},
					new LabelProgressBarDecorator(Hud, 
						new LabelStringDecorator(Hud) {Font = DmgFont, Alignment = HorizontalAlign.Left}
					) {Hint = plugin.CreateHint(string.Empty), BarBrush = Hud.Render.CreateBrush(100, 0, 191, 255, 0), /*BackgroundBrush = Hud.Render.CreateBrush(50, 211, 228, 255, 0), */SpacingLeft = 10, SpacingRight = 10, SpacingTop = 1, SpacingBottom = 1}
				) {SpacingTop = 1, SpacingBottom = 1}
			) {
				Enabled = false,
				SpacingLeft = 10,
				SpacingRight = 10,
				Alignment = HorizontalAlign.Left,
				//BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				OnFillRow = (row, index) => {
					if (index >= DamageEvents.Count)
						return false;
					
					var dEvent = DamageEvents[index];
					
					//time
					((LabelStringDecorator)row.Labels[0]).StaticText = dEvent.Time.ToString("hh:mm:ss.fff tt");
					
					//damage
					var progressUI = ((LabelProgressBarDecorator)row.Labels[1]); 
					progressUI.Progress = HighestHit > 0 ? (float)(dEvent.TotalDamage / HighestHit) : 0; //dEvent.CritDamage / 
					
					//progressUI.Hint
					string types = null;
					string hint = null;
					if (dEvent.CritDamage == 0)
					{
						types = "N";
						hint = "Normal";
					}
					else if (dEvent.TotalDamage == dEvent.CritDamage)
					{
						types = "C";
						hint = "Critical";
					}
					else
					{
						types = "N + C";
						hint = dEvent.Breakdown;
					}
					
					((LabelStringDecorator)progressUI.Labels[0]).StaticText = string.Format("{0:n0} ({1})", dEvent.TotalDamage, types); //ValueToString(dEvent.TotalDamage, ValueFormat.NormalNumber);
					((LabelStringDecorator)progressUI.Hint).StaticText = hint;
					
					return true;
				}
			};
			
			/*var pauseButton = plugin.CreatePause(Graph);
			//var pauseClick = pauseButton.OnClick;
			pauseButton.OnClick = (label) => {
				//pauseClick(label);
				Graph.TogglePause(!Graph.IsPaused());
				ManualPause = Graph.IsPaused();
				Hud.Sound.Speak(ManualPause.ToString());
			};*/
			//pauseClick = pauseButton.OnClick;
			
			/*Action<ILabelDecorator> unpauseManual = (label) => ManualPause = false;*/
			/*var pauseRender = pauseButton.OnBeforeRender;
			pauseButton.OnBeforeRender = (label) => {
				if (pauseRender(label))
				{
					if (Hud.Game.IsInTown)
					{
						//label.OnClick = unpauseManual;
						if (!ManualPause)
							((LabelStringDecorator)label).Font = plugin.DisabledFont;
						//((LabelStringDecorator)label).Font = ManualPause ? plugin.ResumeFont : plugin.DisabledFont;
						//label.Hint = unpauseManualHint;
					}
					else
					{
						//label.OnClick = pauseClick;
						//label.Hint = pauseHint;
					}
				
					//label.OnClick = (Hud.Game.IsInTown && Graph.IsPaused() ? unpauseManual : pauseClick);
					return true;
				}
				
				return false;
			};*/
			/*, (label) => {
					ManualPause = Graph.IsPaused();
					Hud.Sound.Speak("pause");
			});*/
			
			//var pauseHint = pauseButton.Hint;
			/*Action<ILabelDecorator> unpauseManual = (label) => ManualPause = false;
			LabelStringDecorator unpauseManualHint = plugin.CreateHint("Click to Resume (once you leave town)");
			pauseButton.OnBeforeRender = (label) => {
				pauseRender(label);
				
				if (Hud.Game.IsInTown && Graph.IsPaused())
				{
					label.OnClick = unpauseManual;
					label.Hint = unpauseManualHint;
				}
				else
				{
					label.OnClick = pauseClick;
					//label.Hint = pauseHint;
				}
				
				//label.OnClick = (Hud.Game.IsInTown && Graph.IsPaused() ? unpauseManual : pauseClick);
				return true;
			};*/
			
			ViewLogButton = new LabelStringDecorator(Hud, TextViewLog) {Hint = plugin.CreateHint(TextHintViewLog), OnClick = ToggleLog, Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5, SpacingBottom = 3}; //LineSheetDPS.Brush
			ViewGraphButton = new LabelStringDecorator(Hud, TextViewGraph) {Hint = plugin.CreateHint(TextHintViewGraph), OnClick = ToggleGraph, Font = TextFont, BorderBrush = LineSheetDPS.Brush, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5, SpacingBottom = 3};

			SelectViewUI = new LabelRowDecorator(Hud, ViewGraphButton, ViewLogButton) {Gap = 1, HoveredBrush = plugin.HighlightBrush, Alignment = HorizontalAlign.Left};
			CurrentDPSUI = new LabelRowDecorator(Hud, 
				new LabelStringDecorator(Hud, "ДПС:") {Hint = plugin.CreateHint("Текущий урон в секунду"), Font = DmgFont, SpacingLeft = 15, SpacingRight = 3},
				new LabelStringDecorator(Hud, () => Graph.DataFormat(Hud.Game.Me.Damage.CurrentDps)) {Hint = plugin.CreateHint("Текущий урон в секунду"), Font = LineCurDPS.Font, SpacingRight = 15}
			);
			
			PagingUI = plugin.CreatePagingControls(Graph);
			GraphUI = new LabelColumnDecorator(Hud,
				new LabelAlignedDecorator(Hud,
					SelectViewUI,
					CurrentDPSUI,
					PagingUI //plugin.CreatePagingControls(Graph)
				) {SpacingLeft = 15, SpacingRight = 15, SpacingTop = 2, SpacingBottom = 4},
				Graph,
				Table //LogUI
			) {BackgroundBrush = plugin.BgBrush};
			GraphUI.Resize();
			
			/*LogUI = new LabelCanvasDecorator(Hud, (canvas, x, y) => {
				Table.Paint(x, y);
				//GraphUI.Paint(x, y);
				canvas.ContentHeight = Table.Height < plugin.StandardGraphHeight ? plugin.StandardGraphHeight : Table.Height;
				canvas.ContentWidth = Table.Width < plugin.StandardGraphWidth ? plugin.StandardGraphWidth : Table.Width;
			}) {ContentWidth = plugin.StandardGraphWidth, ContentHeight = plugin.StandardGraphHeight};*/
				
			/*new LabelColumnDecorator(Hud,
				new LabelAlignedDecorator(Hud,
					SelectViewUI,
					CurrentDPSUI
				) {SpacingLeft = 5, SpacingTop = 2, SpacingBottom = 4},
				new LabelCanvasDecorator(Hud, (canvas, x, y) => {
					//Table.Paint(x, y);
					GraphUI.Paint(x, y);
					canvas.Height = Table.Height < Graph.Height ? Graph.Height : Table.Height;
					canvas.Width = Table.Width < Graph.Width ? Graph.Width : Table.Width;
				}) {Width = Graph.Width, Height = Graph.Height}
			) {BackgroundBrush = plugin.BgBrush};
			LogUI.Resize(); */
			
			
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "НАНЕСЕННЫЙ УЩЕРБ") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreateReset(Reset),
						plugin.CreateSave(this),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				GraphUI,
				new LabelAlignedDecorator(Hud,
					new LabelStringDecorator(Hud, "Минимум:") {Hint = plugin.CreateHint("Минимальное нормальное попадание"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Left, SpacingRight = 3},
					MinNormalLabel,
					new LabelStringDecorator(Hud, "Максимум:") {Hint = plugin.CreateHint("Максимальное нормальное попадание"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Left, SpacingRight = 3},
					MaxNormalLabel,
					new LabelStringDecorator(Hud, "Минимум:") {Hint = plugin.CreateHint("Минимальный критический удар"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Right, SpacingLeft = 15, SpacingRight = 3},
					MinCritLabel,
					new LabelStringDecorator(Hud, "Максимум:") {Hint = plugin.CreateHint("Максимальный критический удар"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Right},
					MaxCritLabel
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 4}
			);

			//IMenuSaveHandler
			SaveCondition = () => (Hud.Time.Now - LastSaveTime).TotalSeconds > 5;
		}
		
		/*public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				foreach (GraphLine line in GraphLines)
					line.Data.Clear();
				
				LastRecordedTick = 0;
				LowestLatency = 0;
				HighestLatency = 0;
			}
		}*/
		
		public void AfterCollect()
		{
			if (ConduitPlayTime == null)
				return;
			
			//not in a game or not initialized yet
			if (!Hud.Game.IsInGame || Hud.Game.IsInTown)
			{
				//auto pause when in town
				if (Hud.Game.IsInTown && Graph is object && !Graph.ManuallyPaused && !Graph.IsPaused())
					//Hud.Sound.Speak("auto pause");
					Graph.TogglePause(true);
				
				if (ConduitPlayTime.IsRunning)
					ConduitPlayTime.Stop();
				return;
			}
			
			//is conduit active?
			if (Hud.Game.Me.Powers.BuffIsActive(403404) || Hud.Game.Me.Powers.BuffIsActive(263029))
			{
				if (!ConduitPlayTime.IsRunning)
					ConduitPlayTime.Start();
			}
			
			if (LastSeenHero != Hud.Game.Me.HeroId)
			{
				LastSeenHero = Hud.Game.Me.HeroId;
				LastSeenDamageDealtAll = Hud.Game.CurrentHeroTotal.DamageDealtAll;
				LastSeenDamageDealtHit = LastSeenDamageDealtAll;
				//FirstSeenDamageDealtAll = LastSeenDamageDealtAll;
			}

			if (Graph is object)
			{
				//auto resume when not in town or loading screen or menus or manually paused
				if (!Graph.ManuallyPaused && Graph.IsPaused())
					Graph.TogglePause(false);
				
				//add data
				Graph.AddZonePoint(ZoneOculus, Hud.Time.Now, Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2));
				
				//all damage
				var damage = Hud.Tracker.Session.DamageDealtAll - LastSeenDamageDealtAll;
				if (damage > HighestHit)
					HighestHit = damage;
				//if (damage < LowestHit)
				//	LowestHit = damage;
				
				List<string> breakdown = new List<string>();

				//crit damage
				var crit = Hud.Tracker.Session.DamageDealtCrit - LastSeenDamageDealtCrit;
				var nonCrit = damage - crit;
				LastSeenDamageDealtCrit = Hud.Tracker.Session.DamageDealtCrit;
				if (crit > 0)
				{
					if (nonCrit == 0)
						breakdown.Add("critical");
					else
						breakdown.Add(string.Format("{0:n0} {1}", crit, "critical"));
					
					if (crit > MaxCritHit)
					{
						MaxCritHit = crit;
						MaxCritTime = Hud.Time.Now;
					}
					if (crit < MinCritHit)
					{
						MinCritHit = crit;
						MinCritTime = Hud.Time.Now;
					}
				}

				//non-crit damage
				if (nonCrit > 0)
				{
					if (crit == 0)
						breakdown.Add("normal");
					else
						breakdown.Add(string.Format("{0:n0} {1}", nonCrit, "normal"));
					
					if (nonCrit > MaxNormalHit)
					{
						MaxNormalHit = nonCrit;
						MaxNormalTime = Hud.Time.Now;
					}
					if (crit < MinNormalHit)
					{
						MinNormalHit = nonCrit;
						MinNormalTime = Hud.Time.Now;
					}
				}
				
				if (damage > 0)
				{
					//DamageEvents.Add(string.Format("[{0}] +{1} ({2})", Hud.Time.Now.ToString("hh:mm:ss.fff tt"), ValueToString(damage, ValueFormat.NormalNumber), string.Join(" + ", breakdown)));
					DamageEvents.Add(new DamageEvent(Hud.Time.Now, damage, crit, string.Join(" + ", breakdown)));
					
					//remove older entry to make room
					if (SaveMode)
					{
						if (DamageEvents.Count > DamageEventsShown && DamageEvents[0].Time < LastRecordedTime) //find one entry to remove
							DamageEvents.RemoveAt(0);
					}
					else if (DamageEvents.Count > DamageEventsShown)
						DamageEvents.RemoveAt(0);
					
					Graph.AddPoint(LineDamageDone, Hud.Time.Now, (ConduitPlayTime.IsRunning && damage == 0 ? LineDamageDone.GapValue : damage));
					Graph.AddPoint(LineCritDamage, Hud.Time.Now, (ConduitPlayTime.IsRunning && crit == 0 ? LineCritDamage.GapValue : crit));
					Graph.AddPoint(LineNormalDamage, Hud.Time.Now, (ConduitPlayTime.IsRunning && damage == 0 ? LineNormalDamage.GapValue : nonCrit));
					
					IntervalDamage += damage;
					IntervalCritDamage += crit;
					IntervalNormalDamage += nonCrit;
				}
				
				//int pixel = (int)Math.Floor(Graph.GetX(LastRecordedTime));
				if (Math.Floor(Graph.GetX(LastRecordedTime)) > 0) //pixel > 0) //(Hud.Time.Now - LastRecordedTime).TotalSeconds >= Graph.SecondsPerPixel)
				{
					if (IntervalDamage == 0)
						Graph.AddPoint(LineDamageDone, Hud.Time.Now, 0);
					if (IntervalCritDamage == 0)
						Graph.AddPoint(LineCritDamage, Hud.Time.Now, 0);
					if (IntervalNormalDamage == 0)
						Graph.AddPoint(LineNormalDamage, Hud.Time.Now, 0);
					
					IntervalDamage = 0;
					IntervalCritDamage = 0;
					IntervalNormalDamage = 0;
					LastRecordedTime = Hud.Time.Now;
					//LastPixel = pixel;
				}
				
				LastSeenDamageDealtAll = Hud.Tracker.Session.DamageDealtAll;

				//dps dealt to monsters
				damage = Hud.Stat.MonsterHitpointDecreasePerfCounter.LastValue;
				Graph.AddPoint(LineMonsterHPLoss, Hud.Time.Now, (damage < 0 ? LineMonsterHPLoss.GapValue : damage));
				
				//dps
				Graph.AddPoint(LineCurDPS, Hud.Time.Now, Hud.Game.Me.Damage.CurrentDps);
				Graph.AddPoint(LineSheetDPS, Hud.Time.Now, (double)Hud.Game.Me.Offense.SheetDps);
			}
		}
		
		/*public void OnKeyEvent(IKeyEvent keyEvent)
		{
			if (!Hud.Window.IsForeground) return; //only process the key event if hud is actively displayed
			
			if (keyEvent.IsPressed)
			{
				if (keyEvent.Key == Key.Left)
					InvertGraph = false;
				else if (keyEvent.Key == Key.Right)
					InvertGraph = true;
				else if (keyEvent.Key == Key.Down)
					SaveGraph();
				else if (keyEvent.Key == Key.Up)
					ToggleBetweenGraphs();
			}
		}*/
		
		public void Reset(ILabelDecorator label)
		{
			ConduitPlayTime.Reset();
			//HighestHit = 0;
			//LowestHit = 0; //this will always be zero, fix this!
			MinNormalHit = double.MaxValue;
			MaxNormalHit = 0;
			MinCritHit = double.MaxValue;
			MaxCritHit = 0;
			
			Graph.Clear();
		}
		
		public void ToggleGraph(ILabelDecorator label)
		{
			Graph.Enabled = !Graph.Enabled;
			PagingUI.Enabled = Graph.Enabled;
			SelectViewUI.Labels[0].BorderBrush = Graph.Enabled ? LineSheetDPS.Brush : null; 
			
			/*
			
			
			if (GraphUI.Labels[1] == Graph)
			{
				//GraphUI.Labels[1] = Table;
				//Table.Enabled = true;
				//PagingUI.Enabled = false;
				SelectViewUI.Labels[0].BorderBrush = null; 
				SelectViewUI.Labels[1].BorderBrush = LineSheetDPS.Brush; 
			}
			else
			{
				//GraphUI.Labels[1] = Graph;
				//PagingUI.Enabled = true;
				SelectViewUI.Labels[0].BorderBrush = LineSheetDPS.Brush; 
				SelectViewUI.Labels[1].BorderBrush = null;
			}
			
			if (((ILabelDecoratorCollection)Panel).Labels[1] == GraphUI)
			{
				GraphUI.Labels[1] = LogUI;
				SelectViewUI.Labels[0].BorderBrush = null; 
				SelectViewUI.Labels[1].BorderBrush = LineSheetDPS.Brush; 
			}
			else
			{
				((ILabelDecoratorCollection)Panel).Labels[1] = GraphUI;
				SelectViewUI.Labels[0].BorderBrush = LineSheetDPS.Brush; 
				SelectViewUI.Labels[1].BorderBrush = null;
			}*/
			
			//Hud.Sound.Speak("Toggle UI");
			/*if (((ILabelDecoratorCollection)Panel).Labels[2] == Graph)
			{
				((ILabelDecoratorCollection)Panel).Labels[2] = Table;
				((ILabelDecoratorCollection)((ILabelDecoratorCollection)Panel).Labels[1]).Labels[0] = ViewGraphButton;
				
				//hide paging controls
				((ILabelDecoratorCollection)((ILabelDecoratorCollection)Panel).Labels[1]).Labels[3].Enabled = false;
			}
			else
			{
				((ILabelDecoratorCollection)Panel).Labels[2] = Graph;
				((ILabelDecoratorCollection)((ILabelDecoratorCollection)Panel).Labels[1]).Labels[0] = ViewLogButton;
				
				//show paging controls
				((ILabelDecoratorCollection)((ILabelDecoratorCollection)Panel).Labels[1]).Labels[3].Enabled = true;
			}*/
		}
		
		public void ToggleLog(ILabelDecorator label)
		{
			Table.Enabled = !Table.Enabled;
			SelectViewUI.Labels[1].BorderBrush = Table.Enabled ? LineSheetDPS.Brush : null;
		}
		
		//https://stackoverflow.com/questions/620605/how-to-make-a-valid-windows-filename-from-an-arbitrary-string
		public void SaveToFile()
		{
			//Hud.Sound.Speak("Save To File");
			//System.IO.Path.GetInvalidFileNameChars().Aggregate(line.Name, (current, c) => current.Replace(c, '_'))
			//foreach (DamageEvent e in DamageEvents)
			string str = string.Join("\n", DamageEvents.Where(e => e.Time > LastSaveTime));
			if (!string.IsNullOrEmpty(str))
				Hud.TextLog.Log("DamageLog", str, false, true);
			
			foreach (var line in Graph.Lines)
			{
				str = line.ToString(LastSaveTime);
				if (!string.IsNullOrEmpty(str))
					Hud.TextLog.Log("DamageGraph_line_" + System.IO.Path.GetInvalidFileNameChars().Aggregate(line.Name.Replace(' ', '_'), (current, c) => current.Replace(c, '_')), str, false, true);
			}
			
			foreach (var zone in Graph.Zones)
			{
				str = zone.ToString(LastSaveTime);
				if (!string.IsNullOrEmpty(str))
					Hud.TextLog.Log("DamageGraph_zone_" + System.IO.Path.GetInvalidFileNameChars().Aggregate(zone.Name.Replace(' ', '_'), (current, c) => current.Replace(c, '_')), str, false, true);
			}

			LastSaveTime = Hud.Time.Now;
		}
    }
}