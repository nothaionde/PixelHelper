/*

Merged LatencyHelper by Razorfish and LiveStats LatencyMeter by HaKache

*/

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
	
    public class MenuLatency : BasePlugin, IMenuAddon, IAfterCollectHandler, ICustomizer
	{
		public bool HideDefaultLatencyPlugin { get; set; } = true;
		public int LatencyMedium = 95;
		public int LatencyHigh = 155;
		
		/*public enum Threshold : int { 
			Low = 95, //40,
			Medium =  155, //70,
			High = 195, //100,
		};*/
		
		//public int Threshold_Danger { get; set; } = 190; //ms
		//public int Threshold_High { get; set; } = 150; //ms
		//public int Threshold_Medium { get; set; } = 95; //ms
		
		public Dictionary<string, Func<string[], bool>> RegionTags = new Dictionary<string, Func<string[], bool>>() { //optional, easy to see tags for the ip address
			{"W:", (ip) => ip.Length > 1 && ip[ip.Length - 2] == "30"}, //NA West
			{"W2:", (ip) => ip.Length > 1 && ip[ip.Length - 2] == "31"}, //NA West (Far)
			{"E:", (ip) => ip.Length > 1 && (ip[ip.Length - 2] == "15" || ip[ip.Length - 2] == "16")}, //NA East
		};
		private string IPAddress;
		private string _tag;
		
		//IMenuAddon
		public string Id { get; set; } //will be set by MenuPlugin
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; } //which dock does this plugin start in?
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public IBrush GraphBgBrush { get; set; }
		public IBrush MarkerBrush { get; set; }
		public IFont MarkerFont { get; set; }
		public IFont IconFont { get; set; }
		public IFont AvgFont { get; set; }
		//public IFont DmgFont { get; set; }
		//public IFont CritFont { get; set; }
		//public IFont MHPFont { get; set; }
		public IFont IPFont { get; set; }

		//public IFont HighFont { get; set; }
		//public IFont MediumFont { get; set; }
		//public IFont LowFont { get; set; }
		//public Dictionary<Threshold, IFont> LatencyFonts { get; set; }
		
		public int GraphDuration { get; set; } = 120; //in seconds		
		public float GraphWidth { get; set; } //Func<float>
		public float GraphHeight { get; set; } //Func<float>
		//public bool InvertGraph { get; set; } = true;
		public int YAxisMarkersCount { get; set; } = 5;
		public int XAxisMarkersCount { get; set; } = 6;

		//public List<GraphLine> GraphLines { get; private set; }
		public int HighestLatency { 
			get { return _maxLatency; }
			private set {
				if (_maxLatency != value)
				{
					_maxLatency = value;
					if (LatencyHigh < _maxLatency)
						MaxLatencyLabel.Font = LineHigh.Font;
					else if (LatencyMedium < _maxLatency)
						MaxLatencyLabel.Font = LineMedium.Font;
					else
						MaxLatencyLabel.Font = LineLow.Font;
				}
			}
		}
		private int _maxLatency = 0;
		private DateTime HighestLatencyTime;
		public int LowestLatency { 
			get { 
				if (_minLatency == int.MaxValue)
					return 0;
				return _minLatency;
			}
			private set {
				if (_minLatency != value)
				{
					_minLatency = value;
					if (LatencyHigh < _minLatency)
						MinLatencyLabel.Font = LineHigh.Font;
					else if (LatencyMedium < _minLatency)
						MinLatencyLabel.Font = LineMedium.Font;
					else
						MinLatencyLabel.Font = LineLow.Font;
				}
			}
		}
		private int _minLatency = int.MaxValue;
		private DateTime LowestLatencyTime;
		
		private GraphLine LineHigh;
		private GraphLine LineMedium;
		private GraphLine LineLow;
		private GraphLine LastLine;
		private GraphZone ZoneMinimized;
		private Dictionary<GraphLine, int> LineRank = new Dictionary<GraphLine, int>();
		private LabelGraphDecorator Graph;
		private LabelStringDecorator MinLatencyLabel;
		private LabelStringDecorator MaxLatencyLabel;
		private LabelStringDecorator AvgLatencyLabel;
		private LabelStringDecorator CurLatencyLabel;
		//private DateTime PreviousMoment;
		
        public MenuLatency()
		{
			Enabled = true;
			DockId = "BottomRight"; //"TopCenterDock";
			Priority = 10;
		}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
		}
		
		public void Customize()
		{
			if (HideDefaultLatencyPlugin)
				Hud.TogglePlugin<NetworkLatencyPlugin>(false);
		}
		
		public void OnRegister(MenuPlugin plugin)
		{
			IPFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 170, 150, 120, false, false, true);
			
			CurLatencyLabel = new LabelStringDecorator(Hud, () => Hud.Game.CurrentLatency.ToString("F0") + "ms") {SpacingTop = -1};
			Label = new LabelRowDecorator(Hud, 
				new LabelStringDecorator(Hud, GetRegionTag) {FuncCacheDuration = 1, Font = plugin.TitleFont, SpacingRight = 2, SpacingTop = 1}, 
				CurLatencyLabel,
				new LabelStringDecorator(Hud, "📶") {Font = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, true), SpacingLeft = 2}
			); //{SpacingLeft = 15, SpacingRight = 15};
			
			LineLow = new GraphLine("Низкая задержка")
			{
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 108, 196, 108, 1), //255, 73, 73
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 108, 196, 108, false, false, true),
			};
			LineRank.Add(LineLow, 1);
			LineMedium = new GraphLine("Средняя задержка")
			{
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 255, 245, 59, 1), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 245, 59, false, false, true),
			};
			LineRank.Add(LineMedium, 2);
			LineHigh = new GraphLine("Высокая задержка")
			{
				GapValue = -1,
				Brush = Hud.Render.CreateBrush(255, 255, 2, 2, 1), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 2, 2, false, false, true),
			};
			LineRank.Add(LineHigh, 3);
			
			ZoneMinimized = new GraphZone("Диабло Свернут")
			{
				Brush = Hud.Render.CreateBrush(75, 150, 150, 150, 0), 
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 100, 100, 100, false, false, true),
			};
			
			MinLatencyLabel = new LabelStringDecorator(Hud, () => LowestLatency.ToString("F0") + "ms") {Hint = plugin.CreateHint(() => LowestLatencyTime.ToString("T")), Alignment = HorizontalAlign.Left};
			AvgLatencyLabel = new LabelStringDecorator(Hud, () => Hud.Game.AverageLatency.ToString("F0") + "ms");
			MaxLatencyLabel = new LabelStringDecorator(Hud, () => HighestLatency.ToString("F0") + "ms") {Hint = plugin.CreateHint(() => HighestLatencyTime.ToString("T")), Alignment = HorizontalAlign.Right};
			Graph = new LabelGraphDecorator(Hud) {
				DataFormat = (d) => d.ToString("F0") + " ms", //, System.Globalization.CultureInfo.InvariantCulture
				GraphWidth = plugin.StandardGraphWidth,
				GraphHeight = plugin.StandardGraphHeight,
				GraphShownDuration = GraphDuration,
				GraphMaxDuration = GraphDuration * 5,
				Lines = LineRank.Keys.ToList(), //new List<GraphLine>() {LineLow, LineMedium, LineHigh},
				Zones = new List<GraphZone>() {ZoneMinimized},
				//GridBrush = Hud.Render.CreateBrush(45, 190, 190, 190, 1),
				//AxisFont = Hud.Render.CreateFont("tahoma", 6.5f, 200, 211, 228, 255, false, false, 180, 0, 0, 0, true),
				//AxisFontAlt = Hud.Render.CreateFont("tahoma", 6.5f, 200, 150, 150, 150, false, false, 180, 0, 0, 0, true),
				/*OnBeforeRender = (label) => { //debug
					LabelDecorator.DebugWrite(string.Join("\n", LineLow.Points), label.LastX + label.Width, label.LastY);
					LabelDecorator.DebugWrite(string.Join("\n", LineMedium.Points), label.LastX + label.Width*1.5f, label.LastY);
					//LabelDecorator.DebugWrite(string.Join("\n", LineMedium.Points), label.LastX + label.Width, label.LastY + 30);
					//LabelDecorator.DebugWrite(string.Join("\n", LineHigh.Points), label.LastX + label.Width, label.LastY + 60);
					return true;
				},*/
				BackgroundBrush = plugin.BgBrush,
				//SpacingTop = 5,
				SpacingLeft = 15,
				SpacingRight = 15,
				SpacingBottom = 25,
			};
			
			//var debug = plugin.CreatePagingControls(Graph);
			/*debug.OnBeforeRender = (lbl) => {
				return true;
			};*/
				
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ЗАДЕРЖКА") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreateReset(Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelAlignedDecorator(Hud,
					new LabelStringDecorator(Hud, "IP:") {Font = plugin.TitleFont, Alignment = HorizontalAlign.Left, SpacingRight = 3},
					new LabelStringDecorator(Hud, () => Hud.Game.ServerIpAddress) {Font = IPFont, Alignment = HorizontalAlign.Left, SpacingTop = -1},
					new LabelStringDecorator(Hud, "Текущий:") {Font = plugin.TitleFont, SpacingRight = 3},
					CurLatencyLabel,
					plugin.CreatePagingControls(Graph)
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 15, SpacingRight = 5},
				Graph,
				new LabelAlignedDecorator(Hud,
					new LabelStringDecorator(Hud, "Минимум:") {Font = plugin.TitleFont, Alignment = HorizontalAlign.Left, SpacingRight = 3},
					MinLatencyLabel,
					new LabelStringDecorator(Hud, "Средний:") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 3},
					AvgLatencyLabel,
					new LabelStringDecorator(Hud, "Максимум:") {Font = plugin.TitleFont, Alignment = HorizontalAlign.Right, SpacingRight = 3},
					MaxLatencyLabel
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 4}
			);
			
			/*Menu.Add(new MenuGraphDecorator(Hud) {
				Data = GraphLines,
				GraphDuration = GraphDuration,
				GraphWidth = GraphWidth,
				GraphHeight = GraphHeight,
				YAxisMarkersCount  = YAxisMarkersCount,
				XAxisMarkersCount = XAxisMarkersCount,
				NumberFormat = (d) => d.ToString("F0") + " ms",

				BackgroundBrush = Hud.Render.CreateBrush(125, 0, 0, 0, 0),
				GridBrush = Hud.Render.CreateBrush(45, 190, 190, 190, 1),
				GridFont = Hud.Render.CreateFont("tahoma", 6.5f, 200, 211, 228, 255, false, false, true),
				GridFontAlt = Hud.Render.CreateFont("tahoma", 6.5f, 200, 100, 100, 100, false, false, true),
			});

			CurLatencyUI = new MenuStringDecorator(() => {
				CurLatencyUI.TextFont = LatencyFonts[CurrentThreshold];
				return Hud.Game.CurrentLatency.ToString("F0") + "ms";				
			}) { TextFont = TextFont };
			
			Label = new MenuAlignedDecorator(
				new MenuStringDecorator(() => {
					if (!string.IsNullOrEmpty(Hud.Game.ServerIpAddress))
					{
						string[] elements = Hud.Game.ServerIpAddress.Split('.');
						if (elements.Length > 3)
						{
							switch (elements[2])
							{
								case "15":
									//return "E:";
								case "16":
									return "E:";
								case "30":
									//return "W:";
								case "31":
									return "W:";
								default:
									break;
							}
						}
					}
					
					return string.Empty;
				}) { TextFont = TextFont },
				CurLatencyUI,
				new MenuStringDecorator("📶") {TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, true), SpacingLeft = 2}
			) {
				SpacingLeft = 15, 
				SpacingRight = 15,
				//UseMaxDimensions = true
			};
			//new MenuStringDecorator("Latency") { TextFont = TextFont, Alignment = HorizontalAlign.Left };
			*/
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
				
				CurrentThreshold = GetThreshold((int)Hud.Game.CurrentLatency);
				PreviousThreshold = CurrentThreshold;
			}
		}*/
		
		public void AfterCollect()
		{
			//not in a game or not initialized yet
			if (!Hud.Game.IsInGame || Graph == null) //string.IsNullOrEmpty(Id))
				return;
			
			//determine which line to update
			GraphLine line = LineLow;
			if (LatencyHigh < Hud.Game.CurrentLatency)
			{
				line = LineHigh;
				CurLatencyLabel.Font = LineHigh.Font;
			}
			else if (LatencyMedium < Hud.Game.CurrentLatency)
			{
				line = LineMedium;
				CurLatencyLabel.Font = LineMedium.Font;
			}
			else
				CurLatencyLabel.Font = LineLow.Font;
			
			//record data changes as points
			Graph.AddPoint(line, Hud.Time.Now, Hud.Game.CurrentLatency);
			Graph.AddZonePoint(ZoneMinimized, Hud.Time.Now, !Hud.Window.IsForeground);
						
			//add connection line
			if (LastLine is object && LastLine != line && LastLine.Points.Count > 0)
			{
				if (LineRank[LastLine] != LineRank[line])
					Graph.AddPoint(line, Hud.Time.Now, LastLine.Points[LastLine.Points.Count - 1].Data); //Hud.Time.Now.AddMilliseconds(Math.Floor(-1 * Graph.SecondsPerPixel * 1000))
			}
			
			//end the not active line
			foreach (var ln in LineRank.Keys)
			{
				if (line != ln)
					Graph.AddPoint(ln, Hud.Time.Now, ln.GapValue); //ln.AddPoint(Hud.Time.Now, ln.GapValue);
			}
			
			//update min, max, avg
			if (_minLatency > Hud.Game.CurrentLatency)
			{
				LowestLatency = (int)Hud.Game.CurrentLatency;
				LowestLatencyTime = Hud.Time.Now;
			}
			if (_maxLatency < Hud.Game.CurrentLatency)
			{
				HighestLatency = (int)Hud.Game.CurrentLatency;
				HighestLatencyTime = Hud.Time.Now;
			}

			//average latency
			if (LatencyHigh < Hud.Game.AverageLatency)
				AvgLatencyLabel.Font = LineHigh.Font;
			else if (LatencyMedium < Hud.Game.AverageLatency)
				AvgLatencyLabel.Font = LineMedium.Font;
			else
				AvgLatencyLabel.Font = LineLow.Font;

			LastLine = line;
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
			Graph.Clear();
			LowestLatency = int.MaxValue;
			HighestLatency = 0;
		}
		
		public string GetRegionTag()
		{
			string str = Hud.Game.ServerIpAddress;
			if (IPAddress != str)
			{
				IPAddress = str;
				if (string.IsNullOrEmpty(str))
					_tag = null;
				else
				{
					string[] ip = str.Split('.');
					foreach (KeyValuePair<string, Func<string[], bool>> pair in RegionTags)
					{
						if (pair.Value.Invoke(ip))
						{
							_tag = pair.Key;
							break;
							//return pair.Key;
						}
					}
				}
			}
			
			return _tag;
		}
    }
}