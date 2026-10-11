// Versión adaptada a Movable plugins system: https://www.ownedcore.com/forums/showthread.php?t=908809&p=4231469#post4231469
using System;
using System.Globalization;
using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Razor.Movable;
using System.Drawing;

namespace Turbo.Plugins.RNN
{
    public class TimersRiftBar : BasePlugin, INewAreaHandler, IAfterCollectHandler, IMovable // , IInGameTopPainter
    {
		public IBrush BgBrush { get; set; }	
		public IBrush Border1Brush { get; set; }
		public IBrush Border2Brush { get; set; }		
		public IBrush TimeBrush { get; set; }
		public IBrush ProgressBrush { get; set; }
		public IBrush ProgressBrushR { get; set; }
		public IBrush PercentBrush { get; set; }
		
		public IFont GRLeftFont { get; set; }
		public IFont GRElapsedFont { get; set; }
		public IFont PercentFont { get; set; }
		public IFont PercentFontR { get; set; }
				
		public IFont GreenFont { get; set; }
		public IFont RedFont { get; set; }
		
		public int TotalTicks { get; set; }
		public int TTotal { get; set; }
		public int TRift { get; set; }
		public int TBoss { get; set; }
		public int TBossKilled { get; set; }
		public int TOrek { get; set; }
		
		public int Deaths { get; set; }
		public int TDeathT { get; set; }
		public int TDeath { get; set; }
		
		public bool Dead { get; set; } 
		
		public bool InGR { get; set; }
		public bool InNR { get; set; }
		public bool ShowBar { get; set; }

		public bool nofiable { get; set; } = false;
		
		public string Symbol  { get; set; }

		public bool GRSolo { get; set; }
		public bool BarGRSolo { get; set; }
		public bool BarNRAlso { get; set; }
		
		public float Ox { get; set; }
		public float Oy { get; set; }
		public float Ax { get; set; }
		public float Ay { get; set; }	
		
		public string DeathCoordinate { get; set; }
		
		public List<uint> IdGR { get;set; } = new List<uint> {13,16,34,46};
		public List<uint> IdNR { get;set; } = new List<uint> {1,3,10,5};
		
		public Dictionary<string, bool> MarkIdReapp { get; set; } = new Dictionary<string, bool>();
		public int RiftPools { get; set; }
		private int MyIndex { get; set; } = -1;

		private int  EventTick { get; set; } = 0;
		
        public TimersRiftBar()
        {
            Enabled = true;
            Order = 30001;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			Ox = 0.915f; 	// 0....1 , OffsetX for Bar
			Oy = 0.86f;		// 0....1 , OffsetY for Bar
			Ax = 0.074f;	// 0....1 , % Width for Bar
			Ay = 0.02f;		// 0....1 , % Height for Bar
			BarGRSolo = false;	// In GR , Show Bar only when I play Solo	
			BarNRAlso = false;	// Show Bar also in Nephalem Rift
			
			TimeBrush = Hud.Render.CreateBrush(255, 180, 147, 109, 0);
			ProgressBrush = Hud.Render.CreateBrush(255, 150, 50, 190, 0);			
			ProgressBrushR = Hud.Render.CreateBrush(255, 145, 85, 0, 0);		
			Border1Brush = Hud.Render.CreateBrush(255, 50, 40, 35, 2);
			Border2Brush = Hud.Render.CreateBrush(255, 150, 100, 60, 2);
			BgBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 0);
			
			PercentBrush = Hud.Render.CreateBrush( 255, 200, 100, 200, 0);		
			PercentFont = Hud.Render.CreateFont("tahoma", 8, 250, 250, 150, 50, true, false, 160, 0, 0, 0, true);
			PercentFontR = Hud.Render.CreateFont("tahoma", 8, 250, 220, 220, 0, true, false, 160, 0, 0, 0, true);
			
			GRLeftFont = Hud.Render.CreateFont("tahoma", 7, 255, 180, 147, 109, true, false, 160, 0, 0, 0, true);
			GRElapsedFont = Hud.Render.CreateFont("tahoma", 8, 255, 180, 147, 109, true, false, 160, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", 7, 255, 0, 255, 0, true, false, 160, 0, 0, 0, true);
			RedFont = Hud.Render.CreateFont("tahoma", 7f, 255, 250, 100, 100, true, false, 160, 0, 0, 0, true);
			
			TotalTicks = 54000; //15 * 60 * 60;
						
        }

		public void OnNewArea(bool newGame, ISnoArea area)  
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				InGR = false; InNR = false; nofiable = false; TRift = 0; 
				ShowBar = false; 
			}
		}

		public void AfterCollect()  {
			if (!Hud.Game.IsInGame) { return; }
			var riftQuest = Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492 || q.SnoQuest.Sno == 416175) && (q.State == QuestState.started || q.State == QuestState.completed)) ;
			if ( (riftQuest != null) )
			{
				if (nofiable || (riftQuest.CreatedOn <= 0)) return; //For precaution, perhaps unnecessary
				if (IdGR.Contains(riftQuest.QuestStepId)) 
				{
					if (!InGR) 
					{	
						if ((riftQuest.QuestStepId != 13) && (riftQuest.QuestStepId != 16))  { nofiable = true; TRift = -1; return;   }

						InGR = true; GRSolo = true; Dead = false; Symbol = "⚔"; 
						Deaths = 0; TDeath = 0; TDeathT = 0;
						TTotal = 0; TBoss = 0; TBossKilled = 0; TOrek = -1;
						TRift = riftQuest.CreatedOn + 50;  // Inicialmente le damos este valor. CurrentTimedEventStartTick puede tardar un par de segundos en setearse. 
						ShowBar = true;
				
						foreach(IPlayer p in Hud.Game.Players) 
						{
							if (!p.IsMe &&  p.InGreaterRift) { GRSolo = false; if (BarGRSolo) ShowBar = false; break; }
						}
					}
					if (!ShowBar) return; 

					if (Hud.Game.Me.IsDeadSafeCheck)  //hay que InGRStartr a la coordenada de la tumba, eso además implica un retraso con respecto a la muerte real, añadida corrección de 18 ticks
					{	
						if (!Dead && ((Hud.Game.Me.SnoArea.HostAreaSno == 288482) || (Hud.Game.Me.SnoArea.Sno == 288482)) )  // I'm not an observer and I die inside the GR							
						{
							var hs = Hud.Game.HeadStones.FirstOrDefault(h => h.Player.IsMe);
							if (hs != null)
							{
								Dead = true; ++Deaths;	TDeath = Hud.Game.CurrentGameTick + 180 - 18; DeathCoordinate = hs.FloorCoordinate.ToString();
							}
						}
					}
					else 
					{
						if (Dead) 	
						{							
							Dead = false; 	
							if ( GRSolo && (Deaths > 1) && (Hud.Game.Me.FloorCoordinate.ToString() == DeathCoordinate) /*&& (TTotal < TotalTicks)*/) 
							{
								var t = (Deaths - 1) * 5;	t = TDeath + 60 * ((t > 30)? 30: t) - Hud.Game.CurrentGameTick;
								if (t > 0) { TDeathT += t; }
							}	
						}
					}				

					if ( Hud.Game.CurrentTimedEventStartTick != EventTick)  { TRift = Hud.Game.CurrentTimedEventStartTick; EventTick = Hud.Game.CurrentTimedEventStartTick; }	// CurrentTimedEventStartTick inicializado											
					TTotal = (Hud.Game.CurrentGameTick - TRift) + TDeathT;

					if (riftQuest.QuestStepId == 16) //Boss Alive
					{						
						if (TBoss == 0) 
						{
							TBoss = riftQuest.CreatedOn + TDeathT - TRift;
							Symbol = "⚔👿"; 
						}						
					}
					else if ( riftQuest.QuestStepId == 34 ) //Boss Dead and the GR was completed in time
					{
						if (TBossKilled == 0) 
						{
							TBossKilled = riftQuest.CreatedOn + TDeathT - TRift ;  
							if (TBoss > 0) { Symbol = "✔👸"; }
							else { TBoss = TBossKilled; Symbol = "⚠👸"; }  //A This line will not arrive, added by forecast (for restart TH).
						}  
					}
					else if (riftQuest.QuestStepId == 46) {  // Urshi quit (if the GR was completed in time). Orek appears 
						if (TOrek == -1) 
						{ 
							TOrek = 0;  
							if (TBoss > 0) 
							{
								if (TBoss == TBossKilled) { Symbol = "⚠🤴"; }  // si viene de "⚠👸"
								else 
								{
									Symbol = "✔🤴";
									if (TBossKilled == 0)  { TBossKilled = riftQuest.CreatedOn + TDeathT - TRift; }  // Si la falla no es completada en tiempo (15min), se saltará questepid 34 y debemos setearlo aquí
								}
							}
							else { TBoss = TTotal; TBossKilled = TBoss;  Symbol = "⚠🤴"; }  //A This line will not arrive, added by forecast (for restart TH).	
						}												
					}
				
					if (TBossKilled > 0) { TTotal = TBossKilled; }  // Freeze timers		
					
				}
				else if (BarNRAlso && IdNR.Contains(riftQuest.QuestStepId))
				{
					if (!InNR) 
					{								
						if ((riftQuest.QuestStepId == 5) && InGR) return;
						if (riftQuest.QuestStepId != 1) { nofiable = true; TRift = -1; return; }  //  restart TH is not viable if (QuestStepId != 1)
						InNR = true; TRift = riftQuest.CreatedOn;						
						Dead = false; Deaths = 0; Symbol = "⚔"; RiftPools = 0;						
						TTotal = 0; TBoss = 0; TBossKilled = 0; TOrek = -1; 
						MarkIdReapp.Clear();
						ShowBar = true;						
					}	

					if (Hud.Game.Me.IsDeadSafeCheck)   
					{	
						if (!Dead && ((Hud.Game.Me.SnoArea.HostAreaSno == 288482) || (Hud.Game.Me.SnoArea.Sno == 288482)) ) { Dead = true; ++Deaths; }
					}
					else 
					{
						if (Dead) { Dead = false; }
					}
					if ((Hud.Game.Me.SnoArea.HostAreaSno == 288482) || (Hud.Game.Me.SnoArea.Sno == 288482))  //  if (Hud.Game.IsInGame && (a || b) ) , moved the check to line 1
					{						
						var markers = Hud.Game.Markers.Where(s => s.IsPoolOfReflection);
						foreach (var marker in markers) 
						{
							if (MarkIdReapp.TryGetValue(marker.Id, out var r))				
							{							
								if (r == false) 
								{
									MarkIdReapp[marker.Id] = true;
									++RiftPools;
								}								
							}
							else MarkIdReapp.Add(marker.Id, false); // new marker detected -> saving (coordinate,reappear?)											
						}
					}
										
					TTotal = (Hud.Game.CurrentGameTick - TRift) ;
				
					if (riftQuest.QuestStepId == 3) //Boss Alive  
					{
						if (TBoss == 0) 
						{
							Symbol = "⚔👿"; TBoss = riftQuest.CreatedOn - TRift;    						
						} 
					}
					else if ( (riftQuest.QuestStepId == 10) ) //Boss Dead
					{
						if (TBossKilled == 0) 
						{
							Symbol = "🤴"; TBossKilled = riftQuest.CreatedOn - TRift;  
						} 
					}	
					else if (riftQuest.QuestStepId == 5)	// Orek , 30s
					{ 
						if (TOrek == -1) 
						{ 
							Symbol = "✔"; TOrek = riftQuest.CreatedOn - TRift; 
						} 
					}
				
					if (TOrek > 0) { TTotal = TOrek; }  // Freeze timers
															
				}			
			}
			else 
			{
				if (TRift != 0) { InGR = false; InNR = false; ShowBar = false; nofiable = false; TRift = 0; }
			}		
		}
		public void OnRegister(MovableController mover)
		{
			if (this.Enabled == false) return;
			mover.CreateArea(
				this,
				"Bar", //area name
				new RectangleF(Hud.Window.Size.Width * Ox, Hud.Window.Size.Height * Oy, Hud.Window.Size.Width * Ax, Hud.Window.Size.Height * Ay), //position + dimensions
				true, //enabled at start?
				true, //save changes to config file?
				ResizeMode.On //resizable?
			);			
		}
		
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			if (!Hud.Game.IsInGame) { return; }
			if (ShowBar)
			{			
				float x = area.Rectangle.X + deltaX;	float y = area.Rectangle.Y + deltaY;
				float w = area.Rectangle.Width;	float h = area.Rectangle.Height;
					
				BgBrush.DrawRectangle(x, y, w, h);
				var text = (Hud.Game.RiftPercentage).ToString("F1", CultureInfo.InvariantCulture) + "%";
				var textLayout = PercentFont.GetTextLayout(text);
				
				if (InGR) 
				{							
					if (TTotal > 0) 
					{
						TimeBrush.DrawRectangle(x, y, ((TTotal > TotalTicks)? w : (w*TTotal/TotalTicks)), h); 
						ProgressBrush.DrawRectangle(x, y, (float) (w * Hud.Game.RiftPercentage / 100), h);
						PercentFont.DrawText(textLayout, x + (w - textLayout.Metrics.Width) / 2, y + ((h - textLayout.Metrics.Height)/2));	
						if (TTotal < TotalTicks)
						{
							TimeBrush.DrawLine(x + (w*TTotal/TotalTicks) , y , x + (w*TTotal/TotalTicks), y + h - 1, 1);
							PercentBrush.DrawLine(x + (float)(w*Hud.Game.RiftPercentage / 100) , y , x + (float)(w*Hud.Game.RiftPercentage / 100), y + h - 1, 1);						
						}
						text = ValueToString((long) (TotalTicks - TTotal) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
					}
				}
				else
				{
					ProgressBrushR.DrawRectangle(x, y, (float) (w * Hud.Game.RiftPercentage / 100), h);
					PercentFontR.DrawText(textLayout, x + (w - textLayout.Metrics.Width) / 2, y + ((h - textLayout.Metrics.Height)/2));
					if (TTotal < TotalTicks)
					{					
						Border2Brush.DrawLine(x + (float)(w*Hud.Game.RiftPercentage / 100) , y , x + (float)(w*Hud.Game.RiftPercentage / 100), y + h - 1, 1);
					}
					text =  ((RiftPools == 0)? "No" : RiftPools.ToString()) + " Pools"; 					
				}		
				Border2Brush.DrawRectangle(x, y, w, h);
				Border1Brush.DrawRectangle(x-2, y-2, w+4, h+4);
				
				textLayout = GRLeftFont.GetTextLayout(text);
				GRLeftFont.DrawText(textLayout, x + w - textLayout.Metrics.Width , y +  h + 2 );
								
				text =  ValueToString((long)(TTotal) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime) ;
				textLayout = GRElapsedFont.GetTextLayout(text);	
				GRElapsedFont.DrawText(textLayout, x + (w - textLayout.Metrics.Width) / 2, y - textLayout.Metrics.Height - 2 );

				GRLeftFont.DrawText(GRLeftFont.GetTextLayout(Symbol), x , y - textLayout.Metrics.Height - 4 );
				
				textLayout = GRLeftFont.GetTextLayout("☠️ " + Deaths);
				GRLeftFont.DrawText(textLayout, x + w - textLayout.Metrics.Width, y - textLayout.Metrics.Height - 4 );					

				if  (TBoss > 0) 
				{
					text =  "💀" + " " + ValueToString((long)( ( (TBossKilled > 0)? TBossKilled : TTotal ) - TBoss) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
					GRLeftFont.DrawText(GRLeftFont.GetTextLayout(text), x , y + h + 2);
				}
				else 
				{ 
					if (InGR ) 
					{
						var progreso = ( (TotalTicks * Hud.Game.RiftPercentage / 100) - TTotal );
						text = ValueToString((long) Math.Abs(progreso) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
						if (progreso > 0) GreenFont.DrawText(GreenFont.GetTextLayout("+" + text), x , y + h + 2);
						else RedFont.DrawText(RedFont.GetTextLayout("-" + text), x , y + h + 2);
					}
				}												
			}									
		}
    }
}