// Based on GLQ_GreaterRiftPylonMarkerPlugin.cs  ( https://pastebin.com/iQeBWcTs )
using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Turbo.Plugins.RNN
{
	public class GRPylonMarker : BasePlugin, IInGameTopPainter, INewAreaHandler, IAfterCollectHandler, ICustomizer	
	{
		public Dictionary<ActorSnoEnum, PylonData> PylonsFound { get; set; } = new Dictionary<ActorSnoEnum, PylonData>();
		public Dictionary<uint, Tuple<string,ActorSnoEnum,ITexture>> BuffNA { get; set; }
		public Dictionary<uint, string> Lnn { get; set; }
		public int MyIndex { get; set; } = -1;
        public IFont FontUsed { get; set; }	
        public IFont FontLevel { get; set; }
		public IFont LeftFont { get; set; }
        public IFont FontOther { get; set; }
		public IBrush BrushLine { get; set; }
		public bool InRift { get; set; }
		public bool MapGR { get; set; }
		public bool Horizontal { get; set;}
		public float sizeFont { get; set; }
		public float widthLine { get; set; }
		public IBrush BorderBrush { get; set; }
		public IBrush BorderGreenBrush { get; set; }
		public bool ShowIcon { get; set; }
		public float SPname { get; set; } = 0f; 
		public float LineSpacing { get; set; }
		public bool PreSetSPname { get; set; }		
		
		public GRPylonMarker()
        {
            Enabled = true;
        }		
        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30001;
			
			Horizontal = true;
			ShowIcon = true;
			sizeFont = 8f;
			widthLine = 2f;
			LineSpacing = 1f;
			
			PreSetSPname = false;
			
			BorderBrush = Hud.Render.CreateBrush(250, 250, 100, 0, 1);
			BorderGreenBrush = Hud.Render.CreateBrush(250, 0, 220, 0, -1);
			BuffNA = new Dictionary<uint, Tuple<string,ActorSnoEnum,ITexture>>   // < buffsno, <nombre,actosno,ITexture> >
			{
				{ 262935,	new Tuple<string,ActorSnoEnum,ITexture>( "Сила",	ActorSnoEnum._x1_lr_shrine_damage, Hud.Texture.GetTexture(2350065673) )					},
				{ 403404,	new Tuple<string,ActorSnoEnum,ITexture>( "Молния",	ActorSnoEnum._x1_lr_shrine_electrified_tieredrift, Hud.Texture.GetTexture(1751750314) )	},
				{ 266258,	new Tuple<string,ActorSnoEnum,ITexture>( "Поддержка",	ActorSnoEnum._x1_lr_shrine_infinite_casting, Hud.Texture.GetTexture(3254056115) )		}, 
				{ 266254,	new Tuple<string,ActorSnoEnum,ITexture>( "Щит", 	ActorSnoEnum._x1_lr_shrine_invulnerable, Hud.Texture.GetTexture(455040081) )			},
				{ 266271,	new Tuple<string,ActorSnoEnum,ITexture>( "Скорость",	ActorSnoEnum._x1_lr_shrine_run_speed, Hud.Texture.GetTexture(1927801803) )				} 
			};
			Lnn = new Dictionary<uint,string>   // < SnoArea.Sno, Level string>
			{
				{288482, "Уровень 01"},	{288684, "Уровень 02"},	{288686, "Уровень 03"},	{288797, "Уровень 04"},	{288799, "Уровень 05"},
				{288801, "Уровень 06"},	{288803, "Уровень 07"},	{288809, "Уровень 08"},	{288812, "Уровень 09"},	{288813, "Уровень 10"},
				{0, "Уровень xx"}  // está añadido por precaución, por si en el futuro añaden más niveles
			};	
		}	
				
		public class PylonData 
		{
			public string Name { get; set; }
			public uint AreaSno { get; set; }
			public double Percent { get; set; }
			public uint BuffSno { get; set; }
			public bool Used { get; set; } 
			public PylonData( string name, uint area , double percent , uint buff , bool used)
			{
				this.Name = name;
				this.AreaSno = area;
				this.Percent = percent;
				this.BuffSno = buff;
				this.Used = used;						
			}
		}
		
        public void Customize()
		{
			FontUsed = Hud.Render.CreateFont("tahoma", sizeFont, 255, 170, 170, 170, false, false, 250, 0, 0, 0, true);
			FontLevel = Hud.Render.CreateFont("tahoma", sizeFont, 255, 235, 145, 235, false, false, 160, 0, 0, 0, true);
			FontOther = Hud.Render.CreateFont("tahoma", sizeFont, 255, 250, 50, 50, false, false, 160, 0, 0, 0, true);
			LeftFont = Hud.Render.CreateFont("tahoma", sizeFont - ( (sizeFont > 7)? 1.5f : 1f ), 255, 255, 255, 255, false, false, 160, 0, 0, 0, true);
			BrushLine = Hud.Render.CreateBrush(255, 125, 175, 240, widthLine);	
			if (PreSetSPname)
			{
				SPname = 0f; 
				foreach(var e in BuffNA)
				{
					var spname = FontLevel.GetTextLayout(e.Value.Item1).Metrics.Width;
					if  (SPname < spname) { SPname = spname; }
				}	
			}
			if (LineSpacing > 1.2f) { LineSpacing = 1.2f; }
			else if (LineSpacing < 0.8f) { LineSpacing = 0.8f; }
		}
		
		public void PylonsFoundAdd(uint buff , bool used)
		{			
			PylonsFound.Add(BuffNA[buff].Item2, new PylonData(BuffNA[buff].Item1,Hud.Game.Me.SnoArea.Sno,Hud.Game.RiftPercentage,buff,used));						
			if (!PreSetSPname)
			{
				var spname = FontLevel.GetTextLayout(BuffNA[buff].Item1).Metrics.Width;
				if  (SPname < spname) { SPname = spname; }			
			}
		}	
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{	
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				InRift = false;	MapGR = false; 
			}
			if ((area.HostAreaSno == 288482) || (area.Sno == 288482)) 
			{	
				if (Hud.Game.Me.InGreaterRift) { MapGR = true; }		
				if (!InRift) 		//Start New Rift (Nephalem o GR)
				{ 
					InRift = true; 					
					if (Hud.Game.Me.InGreaterRift) 
					{ 
						if (!PreSetSPname) { SPname = 0; } 
						PylonsFound.Clear(); 
					}					
				}
			}
			else { MapGR = false; }
 		}	
		
		public void AfterCollect()  
        {
			if (!Hud.Game.IsInGame) return;
			if (InRift && (Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492) && (q.State == QuestState.started || q.State == QuestState.completed )) == null))	{ InRift = false; }	
		}	
	
		public void ShowIconBuff(uint buff,float x, float y,float s)
		{
			BuffNA[buff].Item3.Draw(x , y, s, s); // texture draw
			if (Hud.Game.Me.Powers.BuffIsActive(buff, 0)) // check buff
			{
				var left = Hud.Game.Me.Powers.GetBuff(buff).TimeLeftSeconds[0];
				var layout = LeftFont.GetTextLayout(left.ToString( (left < 1)? "F1" : "F0") );
				LeftFont.DrawText(layout, x - layout.Metrics.Width - 3 , y + ((s - layout.Metrics.Height) / 2)); // counter							
				BorderGreenBrush.DrawRectangle(x + 1, y + 1, s - 2, s - 2); // border green
			}
			else
			{
				BorderBrush.DrawRectangle(x + 1, y + 1, s - 2, s - 2); // border orange
			}
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
			if ( (clipState != ClipState.BeforeClip) || !Hud.Game.IsInGame || !MapGR ) return;   // Hud.Game.SpecialArea está enlazado a la visibilidad de la barra de progreso...  ¿por qué?	
			
			foreach (var s in Hud.Game.Shrines)  // pilones que vemos
			{
				if  (PylonsFound.ContainsKey(s.SnoActor.Sno)) 
				{ 
					if ( (s.IsDisabled || s.IsOperated) && !PylonsFound[s.SnoActor.Sno].Used ) 	{ PylonsFound[s.SnoActor.Sno].Used = true; }
					if ( Hud.Game.Me.SnoArea.Sno != PylonsFound[s.SnoActor.Sno].AreaSno ) 	// to fix slow update of snoarea for pylon near the door
					{
						PylonsFound[s.SnoActor.Sno].AreaSno = Hud.Game.Me.SnoArea.Sno; 
					}  
				}
				else 
				{
					switch (s.SnoActor.Sno)   
					{
						case ActorSnoEnum._x1_lr_shrine_damage:					PylonsFoundAdd(262935,s.IsDisabled || s.IsOperated); break;		//330695 Power 
						case ActorSnoEnum._x1_lr_shrine_electrified_tieredrift: PylonsFoundAdd(403404,s.IsDisabled || s.IsOperated); break;		//398654 Conduit
						case ActorSnoEnum._x1_lr_shrine_infinite_casting:		PylonsFoundAdd(266258,s.IsDisabled || s.IsOperated); break;		//330697 Channeling	
						case ActorSnoEnum._x1_lr_shrine_invulnerable:			PylonsFoundAdd(266254,s.IsDisabled || s.IsOperated); break;		//330698 Shield
						case ActorSnoEnum._x1_lr_shrine_run_speed:				PylonsFoundAdd(266271,s.IsDisabled || s.IsOperated); break;		//330699 Speed								
					}
				}                                          
			}
			
			foreach (uint buff in BuffNA.Keys)  // bufos que recibimos , para marcar pilones usados y también para detectar pilones nuevos no vistos
			{
				if ( Hud.Game.Players.Any(p => p.HasValidActor && p.Powers.BuffIsActive(buff,0)) )  // Miro el buff de todos los jugadores , no sólo el mío (Hud.Game.Me.Powers.BuffIsActive(buff, 0)) 
				{
					var sno = BuffNA[buff].Item2;
					if (PylonsFound.ContainsKey(sno)) 
					{
						if (!PylonsFound[sno].Used) PylonsFound[sno].Used = true; 
					}
					else { PylonsFoundAdd(buff, true); }
				}
			}
				   
			if (Hud.Render.GreaterRiftBarUiElement.Visible) // Se mostrará información sólo si la barra de progreso está visible, pero recolectaremos datos siempre
			{
				var uiRect = Hud.Render.GreaterRiftBarUiElement.Rectangle;
				double lastPercent = 0; int num = 0; float bPos = 0; var Ypos = uiRect.Bottom + Hud.Window.Size.Height / 30; string texto = string.Empty; SharpDX.DirectWrite.TextLayout layout = null;
				foreach (var sno in PylonsFound.Keys)  
				{
					PylonData p = PylonsFound[sno]; 				
					IFont font = p.Used ? FontUsed : ( (p.AreaSno == Hud.Game.Me.SnoArea.Sno)? FontLevel : FontOther ) ;
					if (Horizontal) // modo Horizontal
					{
						bPos = uiRect.Left + uiRect.Width / 100 * (float) p.Percent;
						BrushLine.DrawLine(bPos, uiRect.Bottom, bPos, uiRect.Bottom + uiRect.Height * 0.3f , 0);	// Line Blue				
						layout = font.GetTextLayout(string.Format("{0:F1}%\t",p.Percent));	
						var h = layout.Metrics.Height; var x = uiRect.Left; var y = Ypos + num * (h * LineSpacing);
						if (ShowIcon)
						{	
							ShowIconBuff(p.BuffSno,x, y + (h * (1 - LineSpacing) + 1) / 2, h * LineSpacing - 1 ); 
							x += h * 1.5f;
						}												
						font.DrawText(layout, x, y);												
						x += layout.Metrics.Width;
						font.DrawText(font.GetTextLayout(p.Name), x, y);						
						x = x + SPname + h;	
						texto = (num > 0)? string.Format("[+{0:F1}%]",p.Percent - lastPercent) : "" ; 						
						texto = string.Format("{0}   {1}", Lnn[Lnn.ContainsKey(p.AreaSno) ? p.AreaSno : 0],texto); 
						layout = font.GetTextLayout(texto);						
						font.DrawText(layout, x, y);
					}
					else // modo Vertical
					{
						bPos = (float) (uiRect.Left + (uiRect.Width * p.Percent  / 100));					
						texto = (num > 0)? string.Format("[+{0:F0}%]",p.Percent - lastPercent) : "" ; 
						texto = string.Format("{0:F1}%\n{1}\n{2}\n{3}",p.Percent, p.Name, Lnn[Lnn.ContainsKey(p.AreaSno) ? p.AreaSno : 0],texto);
						layout = font.GetTextLayout(texto);	
						BrushLine.DrawLine(bPos, uiRect.Bottom, bPos, Ypos + layout.Metrics.Height / 2, 0);
						font.DrawText(layout, bPos - layout.Metrics.Width, Ypos);
					}
					lastPercent = p.Percent; num += 1;
				}
				if ( (num != 4) && (Hud.Game.RiftPercentage < 100)) // progreso que llevamos desde el último pilón
				{
					layout = FontLevel.GetTextLayout(string.Format("(+{0:F1}%)", Hud.Game.RiftPercentage - lastPercent));
					FontLevel.DrawText(layout, uiRect.Left + uiRect.Width / 2 - layout.Metrics.Width / 2, uiRect.Bottom + uiRect.Height * 0.25f);
				}							
			}	
		}		
	}	
}