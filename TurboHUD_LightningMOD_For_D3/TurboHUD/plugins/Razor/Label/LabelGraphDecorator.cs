namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;
	using SharpDX.Direct2D1;
	using SharpDX.DirectWrite; //TextLayout

	using Turbo.Plugins.Default;

    public class LabelGraphDecorator : ILabelDecorator
    {
		//graph data
		public List<GraphLine> Lines { get; set; } //= new List<GraphLine>();
		public List<GraphZone> Zones { get; set; } //= new List<GraphZone>();
		
		//graph config
		public float GraphWidth { get; set; }
		public float GraphHeight { get; set; }
		public int GraphShownDuration { get; set; } = 30; //shown interval, in seconds
		public int GraphMaxDuration { get; set; } //recorded interval, in seconds - if not set, it becomes equal to GraphShownDuration
		public int YAxisMarkersCount { get; set; } = 5;
		public int XAxisMarkersCount { get; set; } = 6;
		public double? AbsoluteMin { get; set; } //optional, max minimum value to show (-1 = determined automatically by the data collected)
		public double? AbsoluteMax { get; set; } //optional, min maximum value to show (-1 = determined automatically by the data collected)
		public Func<double, string> DataFormat { get; set; } = (d) => d.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture); //BasePlugin.ValueToString((double)d, ValueFormat.LongNumber);
		public float AxisSpacing { get; set; } = 5; //space between graph border and the edge of the background drawn (cosmetic)
		//public bool SmoothPoints { get; set; } = false;
		public Func<bool> IsAutoPaused { get; set; } //optional, e.g. = () => Hud.Game.IsInTown
		
		public double SecondsPerPixel { get; private set; }
		public double ValuePerPixel { get; private set; }
		
		public IBrush GridBrush { get; set; }
		public IFont AxisFont { get; set; }
		public IFont AxisFontAlt { get; set; }
		public IFont TooltipFont { get; set; }
		
		//ilabeldecorator properties
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public IBrush BackgroundBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center;
		public float SpacingLeft { get; set; } //= 8;
		public float SpacingRight { get; set; } //= 8;
		public float SpacingTop { get; set; } //= 10;
		public float SpacingBottom { get; set; } //= 15;

		public float ContentWidth { get; set; }
		public float ContentHeight { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		public float LastX { get; private set; }
		public float LastY { get; private set; }
		
		public IController Hud { get; private set; }
		
		public DateTime? PausedTime { get; private set; }
		public DateTime? OldestTime { get; private set; }
		public bool ManuallyPaused { get; private set; } = false;

		private List<GraphLine> PausedLines = new List<GraphLine>();
		private List<GraphZone> PausedZones = new List<GraphZone>();
		//private LabelColumnDecorator Tooltip;
		private DateTime TooltipTime;
		private List<Tuple<object, string>> TooltipData;
		private bool UpdateOldestTime = false;
		
        public LabelGraphDecorator(IController hud)
        {
			Hud = hud;
			TooltipData = new List<Tuple<object, string>>();
			/*Tooltip*/Hint = new LabelColumnDecorator(hud,
				new LabelStringDecorator(hud, () => TooltipTime.ToString("T")) {SpacingTop = 2, SpacingLeft = 2, SpacingRight = 2, SpacingBottom = 2},
				new LabelTableDecorator(hud, 
					new LabelRowDecorator(hud,
						new LabelStringDecorator(hud) {Alignment = HorizontalAlign.Right, SpacingLeft = 5, SpacingRight = 5},
						new LabelStringDecorator(hud) {SpacingLeft = 5, SpacingRight = 5},
						new LabelStringDecorator(hud) {Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5}
					) {SpacingTop = 2, SpacingBottom = 2}
				) {
					
					OnFillRow = (row, index) => {
						if (index >= TooltipData.Count)
							return false;
						
						//Hud.Sound.Speak(index.ToString());
						if (TooltipData[index].Item1 is GraphLine)
						{
							GraphLine line = (GraphLine)TooltipData[index].Item1;
							//if (line.Font is object)
							{
								var label = (LabelStringDecorator)row.Labels[0];
								label.Font = line.Font;
								label.StaticText = line.Name;
								
								label = (LabelStringDecorator)row.Labels[1];
								label.Font = line.Font;
								label.StaticText = "▬";
								
								//double data = TooltipData[index].Item2;
								label = (LabelStringDecorator)row.Labels[2];
								label.Font = line.Font;
								label.StaticText = TooltipData[index].Item2;
							}
						}
						else
						{
							GraphZone zone = (GraphZone)TooltipData[index].Item1;
							var label = (LabelStringDecorator)row.Labels[0];
							label.Font = zone.Font;
							label.StaticText = zone.Name;
							
							label = (LabelStringDecorator)row.Labels[1];
							label.Font = zone.Font;
							label.StaticText = "■";
								
							//double data = TooltipData[index].Item2;
							label = (LabelStringDecorator)row.Labels[2];
							label.Font = null; //zone.Font;
							//label.StaticText = TooltipData[index].Item2;
						}
						
						return true;
					}
				}
			);
			//Hint = Tooltip;
        }
		
        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			if (Lines == null && !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				return;
			}
			
			if (GridBrush == null)
				GridBrush = Hud.Render.CreateBrush(45, 190, 190, 190, 1);
			if (AxisFont == null)
				AxisFont = Hud.Render.CreateFont("tahoma", 6.5f, 200, 211, 228, 255, false, false, 180, 0, 0, 0, true);
			//if (AxisFontAlt == null)
			//	AxisFontAlt = Hud.Render.CreateFont("tahoma", 6.5f, 200, 150, 150, 150, false, false, 180, 0, 0, 0, true);
			
			Visible = true;
			
			ContentWidth = GraphWidth;
			ContentHeight = GraphHeight;
			float h = ContentHeight + SpacingTop + SpacingBottom;
			float w = ContentWidth + SpacingLeft + SpacingRight;
			if (Width < w)
				Width = w;
			if (Height < h)
				Height = h;
			
			BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			BorderBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			
			
			Hovered = Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions

			var viewLines = PausedLines.Count > 0 ? PausedLines : Lines;
			var viewZones = PausedLines.Count > 0 ? PausedZones : Zones;
			
			//if (Lines is object && Lines.Count > 0)
			if (viewLines is object && viewLines.Count > 0)
			{
				var lines = viewLines.Where(ln => ln.Brush is object && ln.Points.Count > 0 && ln.Points.Any(p => p.Data != ln.GapValue));
				if (!lines.Any())
					return;
				
				if (UpdateOldestTime || !OldestTime.HasValue)
					OldestTime = lines.Max(ln => ln.Points[0].Time); //max, not min, because more active lines will flush data more recently than ones that are pretty inactive

				if (ManuallyPaused || (IsAutoPaused is object && IsAutoPaused()))
				{
					if (!PausedTime.HasValue)
						PausedTime = Hud.Time.Now;
				}
				else if (PausedTime.HasValue)
				{
					PausedTime = null;
				}
				
				/*if (IsAutoPaused is object)
				{
					if (IsAutoPaused())
					{
						if (!PausedTime.HasValue)
						{
							PausedTime = Hud.Time.Now;
							PausedStartTime = GetAbsoluteStartTime(lines);
						}
					}
					else if (!ManuallyPaused && PausedTime.HasValue)
						PausedTime = null;
				}*/	

				float x2 = x + SpacingLeft;
				float y2 = y + SpacingTop;

				//compute value range
				double max = (AbsoluteMax.HasValue ? AbsoluteMax.Value : lines.Max(ln => ln.Points.Max(p => p.Data)));
				double min = (AbsoluteMin.HasValue ? AbsoluteMin.Value : lines.Min(ln => ln.Points.Where(p => p.Data != ln.GapValue).Min(p => p.Data)));
				
				if (min == max) //no differring data points recorded
				{
					//center the value and set max and min such that valuePerPixel is approximately 1
					max += GraphHeight*0.5f;
					min -= GraphHeight*0.5f;
					if (AbsoluteMin.HasValue && min < AbsoluteMin.Value)
						min = AbsoluteMin.Value;
				}
				
				//compute pixel value
				double valuePerPixel = (max - min) / (double)GraphHeight;
				//if (valuePerPixel <= 0)
				//	return; //no differring data points recorded
				
				//draw in axis lines and labels based on scaling
				//float secondsPerPixel = 0;
				if (GridBrush is object)
				{
					//float x2 = x + SpacingLeft;
					//float y2 = y + SpacingTop;
					//float distance = GraphHeight / (float)(YAxisMarkersCount);
					float pixelsPerMarker = (GraphHeight / (float)YAxisMarkersCount); //(float)Math.Floor(GraphHeight / (float)(YAxisMarkersCount));
					float textWidth = 0;
					float textHeight = 0;
					float y3 = y2;
					float x3 = x2;
					double valuePerMarker = (max - min) / (double)(YAxisMarkersCount);
					
					for (int i = 0; i < YAxisMarkersCount+1; ++i)
					{
						GridBrush.DrawLine(x3, y3, x3 + GraphWidth, y3);

						double axisValue = max - (i * valuePerMarker); //max - (i * pixelsPerMarker * valuePerPixel);
						if (axisValue < min)
							axisValue = min;
						TextLayout label = AxisFont.GetTextLayout(DataFormat(axisValue));
						AxisFont.DrawText(label, x3 - label.Metrics.Width - AxisSpacing, y3 - label.Metrics.Height*0.5f);

						if (textWidth < label.Metrics.Width)
							textWidth = label.Metrics.Width;
						if (textHeight < label.Metrics.Height)
							textHeight = label.Metrics.Height;
					
						y3 += pixelsPerMarker;
					}
					
					//y2 = y + SpacingTop;
					//distance = GraphWidth / (float)(XAxisMarkersCount);
					pixelsPerMarker = GraphWidth / (float)XAxisMarkersCount; //(float)Math.Floor(GraphWidth / (float)(XAxisMarkersCount));
					//float timeInterval = GraphShownDuration / (float)(XAxisMarkersCount);
					float secondsPerMarker = (float)GraphShownDuration / (float)XAxisMarkersCount; //(float)Math.Floor(GraphShownDuration / (float)XAxisMarkersCount);
					SecondsPerPixel = (float)GraphShownDuration / GraphWidth; //secondsPerMarker / pixelsPerMarker;
					var font = AxisFontAlt ?? AxisFont;
					for (int i = 0; i < XAxisMarkersCount+1; ++i)
					{
						GridBrush.DrawLine(x3, y2, x3, y2 + GraphHeight);

						float axisValue = i * secondsPerMarker; //timeInterval;
						TextLayout label = font.GetTextLayout(axisValue.ToString("F0") + "s");
						font.DrawText(label, x3 - label.Metrics.Width*0.5f, y2 + GraphHeight + AxisSpacing);

						x3 += pixelsPerMarker;
					}
					
					//x2 = x + SpacingLeft; 
					//AxisLabelWidth = textWidth;
					//AxisLabelHeight = textHeight;
				}
				
				//timing
				DateTime endTime = PausedTime.HasValue ? PausedTime.Value : Hud.Time.Now; //IsPaused() ? PausedTime : Hud.Time.Now;
				DateTime startTime = endTime.AddSeconds(-1 * GraphShownDuration);
				//LabelDecorator.DebugWrite(PausedTime.ToString("T") + " vs " + endTime.ToString("T"), x + Width, y);
				//DateTime absoluteStartTime = lines.Min(ln => ln.Points.Where(p => p.Data != ln.GapValue).Min(p => p.Time));
				
				//calculate the point data for tooltip
				float hoveredX = -1;
				TooltipData.Clear();
				if (Hovered)
				{
					if (Hud.Window.CursorX < x2)
						hoveredX = x2;
					else if (Hud.Window.CursorX > x2 + GraphWidth)
						hoveredX = x2 + GraphWidth;
					else
						hoveredX = Hud.Window.CursorX;
					
					TooltipTime = endTime.AddMilliseconds(-1 * (hoveredX - x2) * SecondsPerPixel * 1000); //.ToString("T");
					//LabelDecorator.SetHint(this);
					LabelDecorator.SetHint(this);
				}

				
				//iterate each zone
				if (viewZones is object && viewZones.Count > 0)
				{
					foreach (GraphZone zone in viewZones)
					{
						//float? startX;
						//float? endX;
						ZonePoint drawPoint = null;
						bool hoveredFound = false;
						
						for (int i = zone.Points.Count - 1; i > -1; --i)
						{
							ZonePoint point = zone.Points[i];
							
							if (point.Time > endTime)
								continue;
							
							if (point.Data)
							{
								if (drawPoint is object)
								{
									if (drawPoint.Time < startTime)
										break;
								}
								
								bool truncate = point.Time < startTime;
								float startX = x2 + (float)((endTime - (truncate ? startTime : point.Time)).TotalSeconds / SecondsPerPixel);
								float endX = drawPoint is object ? x2 + (float)((endTime - drawPoint.Time).TotalSeconds / SecondsPerPixel) : x2;
									
								zone.Brush.DrawRectangle(endX, y2, startX - endX, GraphHeight);
								
								if (Hovered && !hoveredFound && hoveredX <= startX && hoveredX >= endX)
								{
									hoveredFound = true;
									
									if (zone.Font is object)
										TooltipData.Add(new Tuple<object, string>(zone, null));
								}
									
								if (truncate)
									break;
							}
							
							drawPoint = point;
						}
					}
				}
				
				//iterate each line
				foreach (GraphLine line in lines)
				{
					//if (line.Points.Count == 0)
					//	continue;
					
					//draw lines, managing pg/gs manually to allow for line breaks
					PathGeometry pg = null;
					GeometrySink gs = null;
					//DateTime endTime = line.Points[line.Points.Count - 1].Time;
					
					//draw it backwards
					DateTime drawTime = endTime;
					GraphPoint drawPoint = null;
					float hoveredY = 0;
					GraphPoint hoveredPoint = null;
					
					for (int i = line.Points.Count - 1; i > -1; --i)
					{
						GraphPoint point = line.Points[i];
						
						if (point.Time > endTime)
							continue;
						
						if (point.Data == line.GapValue)
						{
							//drawPoint = point;
							float pX = x2 + (float)((endTime - point.Time).TotalSeconds / SecondsPerPixel);
							//if (Hovered && !hoveredData.HasValue && pX >= hoveredX)
							//	hoveredData = line.GapValue;
							if (Hovered && hoveredPoint == null && pX >= hoveredX)
								hoveredPoint = point;
							
							if (pg is object)
							{
								gs.EndFigure(FigureEnd.Open);
								gs.Close();
								line.Brush.DrawGeometry(pg);
								gs.Dispose();
								pg.Dispose(); //have to manually garbage collect PathGeometry and GeometrySink without "using"
								gs = null;
								pg = null;
							}
						}
						else
						{
							bool truncate = point.Time < startTime;
							
							float pX = x2 + (float)((endTime - (truncate ? startTime : point.Time)).TotalSeconds / SecondsPerPixel);
							float pY = y2 + GraphHeight - (float)((point.Data - min) / valuePerPixel);
							//float pX = x2 + (float)((endTime - drawTime).TotalSeconds / SecondsPerPixel);
							//float pY = y2 + GraphHeight - (float)((point.Data - min) / valuePerPixel);

							//if (Hovered && !hoveredData.HasValue && pX >= hoveredX)
							if (Hovered && hoveredPoint == null && pX >= hoveredX)
							{
								hoveredY = pY;
								hoveredPoint = point;
							}
							
							if (pg == null)
							{
								pg = Hud.Render.CreateGeometry();
								gs = pg.Open();
								
								float startX = x2;
								if (drawPoint is object)
									startX += (float)((endTime - drawPoint.Time).TotalSeconds / SecondsPerPixel);
								gs.BeginFigure(new SharpDX.Vector2(startX, pY), FigureBegin.Filled);
							}
							else
							{
								//add a step if the points are more than 1 pixel apart
								float stepX = x2 + (float)((endTime - drawPoint.Time).TotalSeconds / SecondsPerPixel);
								if (pX - stepX > 1f)
									gs.AddLine(new SharpDX.Vector2(stepX, pY));
							}
							
							gs.AddLine(new SharpDX.Vector2(pX, pY));
							
							//drawPoint = point;
							if (truncate)
								break;
						}
						
						if (point.Time < startTime)
							break;

						drawPoint = point;
					}
					
					if (pg is object)
					{
						//Hud.Sound.Speak("end2");
						gs.EndFigure(FigureEnd.Open);
						gs.Close();
						line.Brush.DrawGeometry(pg);
						gs.Dispose();
						pg.Dispose(); //have to manually garbage collect PathGeometry and GeometrySink without "using"
						gs = null;
						pg = null;
					}
					
					//draw hovered point
					//if (hoveredData.HasValue && hoveredData.Value != line.GapValue)
					if (hoveredPoint is object && hoveredPoint.Data != line.GapValue)
					{
						line.Brush.DrawEllipse(hoveredX, (float)hoveredY, 5f, 5f);
						//TooltipData.Add(new Tuple<object, string>(line, (line.DataFormat is object ? line.DataFormat(hoveredData.Value) : DataFormat(hoveredData.Value)) + " " + hoveredTime.ToString()));
						
						if (line.Font is object)
							TooltipData.Add(new Tuple<object, string>(line, (line.DataFormat is object ? line.DataFormat(hoveredPoint.Data) : DataFormat(hoveredPoint.Data))));
					}
				}
			}
			
			if (TooltipData.Count > 0)
			{
				//couldn't set this in the constructor
				((LabelStringDecorator)((ILabelDecoratorCollection)Hint).Labels[0]).Font = AxisFont;
				
				LabelDecorator.SetHint(this);
			}

			Height = h;
			Width = w;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			ContentWidth = GraphWidth;
			ContentHeight = GraphHeight;
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
			SecondsPerPixel = GraphShownDuration / GraphWidth;
		}
		
		public void AddLine(GraphLine line)
		{
			if (Lines == null)
				Lines = new List<GraphLine>() {line};
			else if (!Lines.Contains(line))
				Lines.Add(line);
				
			if (PausedLines.Count > 0)
			{
				PausedLines.Clear();
				PausedZones.Clear();
			}
		}

		public void AddZone(GraphZone zone)
		{
			if (Zones == null)
				Zones = new List<GraphZone>() {zone};
			else if (!Zones.Contains(zone))
				Zones.Add(zone);
			
			if (PausedLines.Count > 0)
			{
				PausedLines.Clear();
				PausedZones.Clear();
			}
		}
		
		public void AddPoint(GraphLine line, DateTime time, double data)
		{
			if (line.Points.Count > 0)
			{
				if (line.Points[line.Points.Count - 1].Data == data)
					return;
				
				//Points.Add(new GraphPoint(time, Points[Points.Count - 1].Data));
				
				//only get rid of old data if not paused
				if (!PausedTime.HasValue && line.Points.Count > 1)
				{
					DateTime cutoff = line.Points[line.Points.Count - 1].Time.AddSeconds(-1 * (GraphMaxDuration > GraphShownDuration ? GraphMaxDuration : GraphShownDuration * 3));
					int index = line.Points.FindIndex(p => p.Time > cutoff);
					if (index > 1)
					{
						line.Points.RemoveRange(0, index-1);
						UpdateOldestTime = true;
					}
				}
			}
			
			//Hud.Sound.Speak("add "+data);
			line.Points.Add(new GraphPoint(time, data));
		}
		
		public void AddZonePoint(GraphZone zone, DateTime time, bool data)
		{
			if (zone.Points.Count > 0)
			{
				if (zone.Points[zone.Points.Count - 1].Data == data)
					return;
				
				//Points.Add(new GraphPoint(time, Points[Points.Count - 1].Data));
				
				if (zone.Points.Count > 1)
				{
					DateTime cutoff = zone.Points[zone.Points.Count - 1].Time.AddSeconds(-1 * (GraphMaxDuration > GraphShownDuration ? GraphMaxDuration : GraphShownDuration * 3));
					int index = zone.Points.FindIndex(p => p.Time > cutoff);
					if (index > 1)
						zone.Points.RemoveRange(0, index-1);
				}
			}
			
			zone.Points.Add(new ZonePoint(time, data));
		}
		
		public double GetX(DateTime time)
		{
			return (Hud.Time.Now - time).TotalSeconds / SecondsPerPixel;
		}

		public void TogglePause(bool pause)
		{
			//PausedTime = (pause ? Hud.Time.Now : null);
			if (pause) //PausedLines.Count == 0)
			{
				//Hud.Sound.Speak("pause");
					
				/*if (PausedLines.Count > 0)
				{
					PausedLines.Clear();
					PausedZones.Clear();
				}

				foreach (GraphLine line in Lines)
					PausedLines.Add(line.ToCopy());
				
				foreach (GraphZone zone in Zones)
					PausedZones.Add(zone.ToCopy());*/
				
				if (IsAutoPaused == null || !IsAutoPaused())
				{
					PausedTime = Hud.Time.Now;
					//PausedStartTime = GetAbsoluteStartTime();
					ManuallyPaused = true;
				}
			}
			else if (ManuallyPaused)
			{
				//Hud.Sound.Speak("unpause");
				PausedTime = null;
				ManuallyPaused = false;
				//PausedLines.Clear();
				//PausedZones.Clear();
			}
		}
		
		public bool IsPaused()
		{
			return PausedTime.HasValue || (IsAutoPaused is object && IsAutoPaused()); //PausedLines.Count > 0;
		}
		
		public bool IsOldestShown()
		{
			var time = PausedTime.HasValue ? PausedTime.Value : Hud.Time.Now;
			if (OldestTime.HasValue)
				return OldestTime.Value > time.AddSeconds(-1 * GraphShownDuration);

			return true; //PausedTime.HasValue && PausedStartTime.HasValue && PausedStartTime.Value >= PausedTime.Value.AddSeconds(-1 * GraphShownDuration);
		}
		
		public bool IsNewestShown()
		{
			return !PausedTime.HasValue || PausedTime.Value > Hud.Time.Now.AddSeconds(-1 * GraphShownDuration);
		}
		
		public void ShowOlder()
		{
			var intervalEndTime = (PausedTime.HasValue ? PausedTime.Value : Hud.Time.Now).AddSeconds(-1 * GraphShownDuration);
			//var intervalStartTime = PausedTime.AddSeconds(-1 * GraphShownDuration);
			
			//verify that there are any valid data points that occur before the interval end time
			if (Lines.Any(ln => ln.Points.Any(p => p.Time < intervalEndTime && p.Data != ln.GapValue))) //&& p.Time >= intervalStartTime 
			{
				PausedTime = intervalEndTime;
				ManuallyPaused = true;
				//PausedTime = null;
			}
		}
		
		public void ShowNewer()
		{
			if (!PausedTime.HasValue) //sanity check
				return;
			
			var intervalEndTime = PausedTime.Value.AddSeconds(GraphShownDuration);
			if (intervalEndTime >= Hud.Time.Now || !Lines.Any(ln => ln.Points.Any(p => p.Time < intervalEndTime && p.Data != ln.GapValue)))
			{
				PausedTime = null;
				ManuallyPaused = false;
			}
			else
			{
				PausedTime = intervalEndTime;
				ManuallyPaused = true;
			}
		}
		
		/*public DateTime GetAbsoluteStartTime(IEnumerable<GraphLine> lines)
		{
			return lines.Min(ln => ln.Points[0].Time);
		}*/
		
		public void Clear()
		{
			foreach (var line in Lines)
				line.Points.Clear();
			foreach (var zone in Zones)
				zone.Points.Clear();
			
			PausedLines.Clear();
			PausedZones.Clear();
			
			OldestTime = null;
			PausedTime = null;
			ManuallyPaused = false;
		}
	}
	
	public class GraphPoint {
		public DateTime Time { get; set; }
		public double Data { get; set; }
		public GraphPoint(DateTime time, double data)
		{
			Time = time;
			Data = data;
		}

		public override string ToString()
		{
			return "[" + Time.ToString("hh:mm:ss.fff tt") + "] " + Data.ToString();
		}
	}
	
	public class GraphLine
	{
		public string Name { get; set; }
		public List<GraphPoint> Points { get; set; } = new List<GraphPoint>();
		public double GapValue { get; set; } = double.MinValue;
		public IBrush Brush { get; set; }
		public IFont Font { get; set; }
		public Func<double, string> DataFormat { get; set; } //optional
		
		public GraphLine(string name)
		{
			Name = name;
		}
		
		public GraphLine ToCopy() //this is used to save paused state
		{
			return new GraphLine(Name) {
				Points = new List<GraphPoint>(Points),
				GapValue = GapValue,
				Brush = Brush,
				Font = Font,
				DataFormat = DataFormat
			};
		}
		
		public override string ToString()
		{
			return string.Join("\n", Points); //Name + "\n" + 
		}
		
		public string ToString(DateTime cutoff)
		{
			return string.Join("\n", Points.Where(p => p.Time > cutoff));
		}
	}
	
	public class GraphZone
	{
		public string Name { get; set; }
		public List<ZonePoint> Points { get; set; } = new List<ZonePoint>();
		public IBrush Brush { get; set; }
		public IFont Font { get; set; }
		
		public GraphZone(string name)
		{
			Name = name;
		}
		
		public GraphZone ToCopy() //this is used to save paused state
		{
			return new GraphZone(Name) {
				Points = new List<ZonePoint>(Points),
				Brush = Brush,
				Font = Font
			};
		}
		
		public override string ToString()
		{
			return string.Join("\n", Points); //Name + "\n" + 
		}
		
		public string ToString(DateTime cutoff)
		{
			return string.Join("\n", Points.Where(p => p.Time > cutoff));
		}
	}
	
	public class ZonePoint
	{
		public DateTime Time { get; set; }
		public bool Data { get; set; }
		public ZonePoint(DateTime time, bool data)
		{
			Time = time;
			Data = data;
		}

		public override string ToString()
		{
			//return Time.ToString() + " - " + Data.ToString();
			return "[" + Time.ToString("hh:mm:ss.fff tt") + "] " + Data.ToString();
		}
	}
}