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
	
    public class MenuHealth : BasePlugin, IMenuAddon, IAfterCollectHandler, IMenuSaveHandler//, ICustomizer
	{
		public int DamageEventsShown { get; set; } = 15;
		//public int DamageEventsStored { get; set; } = 150; //10 pages
		
		public string TextViewLog { get; set; } = "Инспектор";
		public string TextViewGraph { get; set; } = "График";
		public string TextHintViewLog { get; set; } = "Нажмите, чтобы открыть инспектора";
		public string TextHintViewGraph { get; set; } = "Нажмите, чтобы открыть графика";
		
		//public IFont TextFont { get; set; } //label font
		//public IFont TitleFont { get; set; }

		public IBrush GraphBgBrush { get; set; }
		public IBrush ButtonBorderBrush { get; set; }
		public IFont TextFont { get; set; } 
		public IFont DefaultFont { get; set; } //label font
		public IFont IconFont { get; set; }
		public IFont DTPSFont { get; set; }
		public IFont ShieldFont { get; set; }
		public IFont SancFont { get; set; }

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
		private LabelStringDecorator CurrentHealth;
		
		private LabelGraphDecorator Graph;
		private GraphLine LineHealth;
		private GraphLine LineShield;
		private GraphLine LineHPS;
		private GraphLine LineDTPS;
		private GraphZone ZoneProc;
		private GraphZone ZoneImmune;
		
		private ImmunityHelper ImmunityPlugin;
		private Razor.Proc.PartyProcTracker ProcPlugin;
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
		private double LastSeenDamageReduction;
		private double FirstSeenDamageDealtAll;
		private double LastSeenDamageDealtAll;
		private double LastSeenDamageDealtCrit;
		private uint LastSeenHero;
		private IWatch ConduitPlayTime;
		//private bool ManualPause = false;
		
        public MenuHealth()
		{
			Enabled = true;
			DockId = "BottomCenter"; //"TopCenterDock";
			Priority = 10;
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
			ImmunityPlugin = Hud.GetPlugin<ImmunityHelper>();
			ProcPlugin = Hud.GetPlugin<Razor.Proc.PartyProcTracker>();
			
			ButtonBorderBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 1f);
			//TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 191, 255, false, false, 100, 0, 0, 0, true); //Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 135, 135, 135, false, false, true);
			//TitleFont = Hud.Render.CreateFont("tahoma", 7f, 200, 211, 228, 255, false, false, 100, 0, 0, 0, true);
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 215, 250, 247, false, false, 100, 0, 0, 0, true);
			DefaultFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 210, 250, 247, false, false, 100, 0, 0, 0, true);
			IconFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 242, 15, 15, false, false, 100, 0, 0, 0, true);
			ShieldFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 3, 123, 252, false, false, 100, 0, 0, 0, true); //125, 125, 255
			DTPSFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 238, 244, 66, false, false, 100, 0, 0, 0, true);
			
			CurrentHealth = new LabelStringDecorator(Hud, () => ((double)Hud.Game.Me.Defense.HealthCur).ToHumanReadable(4)) {Font = IconFont, SpacingRight = 2}; //, FuncCacheDuration = 1
			
			Label = new LabelRowDecorator(Hud,
				//new LabelStringDecorator(Hud, () => (LastSeenDamageReduction * 100).ToHumanReadable(4) + "%") {Font = TextFont, FuncCacheDuration = 2},
				CurrentHealth,
				new LabelStringDecorator(Hud, "🖤") {Font = Hud.Render.CreateFont("tahoma", 7f, 255, 242, 15, 15, false, false, 100, 0, 0, 0, true)} //, SpacingLeft = -2
			);// {Hint = plugin.CreateHint("Damage Reduction")};
			
			//HintViewGraph = plugin.CreateHint(TextHintViewGraph);
			//HintViewLog = plugin.CreateHint(TextHintViewLog);
			
			LineHealth = new GraphLine("Здоровье") 
			{ 
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 215, 250, 247, 1.5f), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 215, 250, 247, false, false, 100, 0, 0, 0, true)
			};
			LineShield = new GraphLine("Щит")
			{ 
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 3, 123, 252, 1), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 3, 123, 252, false, false, 100, 0, 0, 0, true)
			};
			LineHPS = new GraphLine("Текущее исцеление в секунду")
			{ 
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 91, 237, 59, 1), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 91, 237, 59, false, false, 100, 0, 0, 0, true)
			};
			LineDTPS = new GraphLine("Текущий урон, получаемый в секунду") 
			{ 
				//DataFunc = () => (double)Hud.Game.Me.Offense.SheetDps,
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 242, 15, 1, 1f), //250, 125, 0 //107, 96, 255
				Font = IconFont
			};
			
			ZoneProc = new GraphZone("Обман Смерти")
			{
				Brush = Hud.Render.CreateBrush(35, 255, 0, 0, 0), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true),
			};
			ZoneImmune = new GraphZone("Иммуннитет")
			{
				Brush = Hud.Render.CreateBrush(35, 98, 247, 252, 0), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 98, 247, 252, false, false, true),
			};
			
			//MinNormalLabel = new LabelStringDecorator(Hud, () => MinNormalHit == double.MaxValue ? "0" : MinNormalHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MinNormalTime.ToString("T")), Font = LineNormalDamage.Font, Alignment = HorizontalAlign.Left, SpacingRight = 3};
			//MaxNormalLabel = new LabelStringDecorator(Hud, () => MaxNormalHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MaxNormalTime.ToString("T")), Font = LineNormalDamage.Font, Alignment = HorizontalAlign.Left};
			//MinCritLabel = new LabelStringDecorator(Hud, () => MinCritHit == double.MaxValue ? "0" : MinCritHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MinCritTime.ToString("T")), Font = LineCritDamage.Font, Alignment = HorizontalAlign.Right, SpacingRight = 3};
			//MaxCritLabel = new LabelStringDecorator(Hud, () => MaxCritHit.ToHumanReadable(4)) {Hint = plugin.CreateHint(() => MaxCritTime.ToString("T")), Font = LineCritDamage.Font, Alignment = HorizontalAlign.Right};
			
			Graph = new LabelGraphDecorator(Hud) {
				DataFormat = (d) => d.ToHumanReadable(4),
				GraphWidth = plugin.StandardGraphWidth,
				GraphHeight = plugin.StandardGraphHeight,
				GraphShownDuration = GraphDuration,
				GraphMaxDuration = GraphDuration * 40,
				//AbsoluteMin = 0,
				//AbsoluteMax = 100,
				Zones = new List<GraphZone>() {ZoneProc, ZoneImmune},
				Lines = new List<GraphLine>() {LineHealth, LineShield, LineHPS, LineDTPS},
				BackgroundBrush = plugin.BgBrush,
				//IsAutoPaused = () => Hud.Game.IsInTown, // || !Hud.Game.IsInGame,
				SpacingLeft = 15,
				SpacingRight = 15,
				SpacingBottom = 25,
			};
			
			/*Table = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud, 
					new LabelStringDecorator(Hud) {Font = DmgFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 1, SpacingBottom = 1},
					new LabelProgressBarDecorator(Hud, 
						new LabelStringDecorator(Hud) {Font = DmgFont, Alignment = HorizontalAlign.Left}
					) {BackgroundBrush = Hud.Render.CreateBrush(100, 0, 191, 255, 0), SpacingLeft = 10, SpacingRight = 10, SpacingTop = 1, SpacingBottom = 1}
				) {SpacingTop = 1, SpacingBottom = 1}
			) {
				SpacingLeft = 10,
				SpacingRight = 10,
				Alignment = HorizontalAlign.Left,
				BackgroundBrush = plugin.BgBrush,
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
					((LabelStringDecorator)progressUI.Labels[0]).StaticText = string.Format("{0:n0} ({1})", dEvent.TotalDamage, dEvent.Breakdown); //ValueToString(dEvent.TotalDamage, ValueFormat.NormalNumber);
					return true;
				}
			};*/
			
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
			
			// ViewLogButton = new LabelStringDecorator(Hud, TextViewLog) {Hint = plugin.CreateHint(TextHintViewLog), OnClick = ToggleGraphLog, Font = plugin.TitleFont, BorderBrush = ButtonBorderBrush, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 4};
			// ViewGraphButton = new LabelStringDecorator(Hud, TextViewGraph) {Hint = plugin.CreateHint(TextHintViewGraph), OnClick = ToggleGraphLog, Font = plugin.TitleFont, BorderBrush = ButtonBorderBrush, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 4};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ЗДОРОВЬЕ") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreateReset(Reset),
						plugin.CreateSave(this),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelAlignedDecorator(Hud,
					new LabelStringDecorator(Hud, () => ProcPlugin.GetExtraLivesString(Hud.Game.Me)) {Hint = plugin.CreateHint("Дополнительные жизни"), Font = ProcPlugin.LifeFont, Alignment = HorizontalAlign.Left},
					new LabelStringDecorator(Hud, "Здоровье:") {Hint = plugin.CreateHint("Текущее здоровье"), Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 3},
					CurrentHealth, //new LabelStringDecorator(Hud, () => (LastSeenDamageReduction * 100).ToHumanReadable(4) + "%") {Hint = plugin.CreateHint("Current Damage Per Second"), Font = LineCurDPS.Font, SpacingRight = 15},
					//pauseButton
					plugin.CreatePagingControls(Graph)
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 4},
				Graph/*,
				new LabelAlignedDecorator(Hud,
					new LabelStringDecorator(Hud, "Min:") {Hint = plugin.CreateHint("Minimum Normal Hit"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Left, SpacingRight = 3},
					MinNormalLabel,
					new LabelStringDecorator(Hud, "Max:") {Hint = plugin.CreateHint("Maximum Normal Hit"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Left, SpacingRight = 3},
					MaxNormalLabel,
					new LabelStringDecorator(Hud, "Min:") {Hint = plugin.CreateHint("Minimum Critical Hit"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Right, SpacingLeft = 15, SpacingRight = 3},
					MinCritLabel,
					new LabelStringDecorator(Hud, "Max:") {Hint = plugin.CreateHint("Maximum Critical Hit"), Font = plugin.TitleFont, Alignment = HorizontalAlign.Right},
					MaxCritLabel
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 4}*/
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
			//not in a game or not initialized yet
			if (!Hud.Game.IsInGame)
				return;
			
			
			/*if (LastSeenHero != Hud.Game.Me.HeroId)
			{
				LastSeenHero = Hud.Game.Me.HeroId;
				//LastSeenDamageReduction = Hud.Game.CurrentHeroTotal.DamageDealtAll;
				//LastSeenDamageDealtHit = LastSeenDamageDealtAll;
				//FirstSeenDamageDealtAll = LastSeenDamageDealtAll;
				
			}*/
			
			Graph.AddPoint(LineHealth, Hud.Time.Now, (double)Hud.Game.Me.Defense.HealthCur);
			Graph.AddPoint(LineShield, Hud.Time.Now, (double)Hud.Game.Me.Defense.CurShield);
			Graph.AddPoint(LineHPS, Hud.Time.Now, Hud.Game.Me.Defense.CurrentHealingPerSecond);
			Graph.AddPoint(LineDTPS, Hud.Time.Now, Hud.Game.Me.Defense.CurrentDamageTakenPerSecond);
			
			Graph.AddZonePoint(ZoneProc, Hud.Time.Now, ProcPlugin.ExtraLivesRemaining(Hud.Game.Me) == 0);
			Graph.AddZonePoint(ZoneImmune, Hud.Time.Now, ImmunityPlugin.IsImmune);
			
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
		
		public double GetAncientParthansDR()
		{
			if (!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.AncientParthanDefenders.Sno))
				return -1;
					
			int count = Hud.Game.AliveMonsters.Count(m => (m.Stunned || m.Frozen) && m.NormalizedXyDistanceToMe <= 25);
			if (count < 1)
				return -1;
					
			double percent = 0.10; //old value
			if (Hud.Game.Me.HasCubedItem(Hud.Sno.SnoPowers.AncientParthanDefenders.Sno)) //Hud.Game.Me.CubeSnoItem2?.LegendaryPower.Sno == Hud.Sno.SnoPowers.AncientParthanDefenders.Sno)
				percent = 0.12;
			else
			{
				var parthans = Hud.Game.Items.FirstOrDefault(item => item.Location == ItemLocation.Bracers)?.Perfections.FirstOrDefault(p => p.Attribute.Code == "Item_Power_Passive");
				if (parthans != null)
					percent = parthans.Cur;
			}
			
			return (1 - Math.Pow((1 - percent), count)) * 100;
		}
		
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
		
		public void ToggleGraphLog(ILabelDecorator label)
		{
			//Hud.Sound.Speak("Toggle UI");
			if (((ILabelDecoratorCollection)Panel).Labels[2] == Graph)
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
			}
		}
		
		//https://stackoverflow.com/questions/620605/how-to-make-a-valid-windows-filename-from-an-arbitrary-string
		public void SaveToFile()
		{
			//Hud.Sound.Speak("Save To File");
			//System.IO.Path.GetInvalidFileNameChars().Aggregate(line.Name, (current, c) => current.Replace(c, '_'))
			//foreach (DamageEvent e in DamageEvents)
			string str = string.Join("\n", DamageEvents.Where(e => e.Time > LastSaveTime));
			if (!string.IsNullOrEmpty(str))
				Hud.TextLog.Log("HealthLog", str, false, true);
			
			foreach (var line in Graph.Lines)
			{
				str = line.ToString(LastSaveTime);
				if (!string.IsNullOrEmpty(str))
					Hud.TextLog.Log("HealthGraph_line_" + System.IO.Path.GetInvalidFileNameChars().Aggregate(line.Name.Replace(' ', '_'), (current, c) => current.Replace(c, '_')), str, false, true);
			}
			
			foreach (var zone in Graph.Zones)
			{
				str = zone.ToString(LastSaveTime);
				if (!string.IsNullOrEmpty(str))
					Hud.TextLog.Log("HealthGraph_zone_" + System.IO.Path.GetInvalidFileNameChars().Aggregate(zone.Name.Replace(' ', '_'), (current, c) => current.Replace(c, '_')), str, false, true);
			}

			LastSaveTime = Hud.Time.Now;
		}
    }
}