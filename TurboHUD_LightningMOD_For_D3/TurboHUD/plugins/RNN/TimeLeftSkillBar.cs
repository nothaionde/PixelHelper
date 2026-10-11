using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Collections.Generic;
using SharpDX;

namespace Turbo.Plugins.RNN
{
    public class TimeLeftSkillBar : BasePlugin, IInGameTopPainter
    {
		public Dictionary<uint,uint> ReplaceSno { get; set; }		
		public Dictionary<uint,int> FixedIndex { get; set; }
		public List<uint> ExcludeAll { get; set; }		
		public Dictionary<uint,List<int>> ExcludeThis { get; set; }	
		public IFont TimeLeftFont { get; set; }
		
        public TimeLeftSkillBar()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 31001;

			TimeLeftFont  = Hud.Render.CreateFont("arial", 8, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);	
			
			ReplaceSno = new Dictionary<uint,uint>  			
			{			
				{375049, 375050},	// Mantra Salvación		(Monk)
				{375082, 375083},	// Mantra Reprensión	(Monk)
				{373143, 373154},	// Mantra Curación		(Monk)
				{375088, 375089},	// Mantra Convicción	(Monk)
				{362102, 362118},	// Aliado Místico		(Monk)
				
				{342281, 342284},	// Ley Valor		(Crus)
				{342280, 342286},	// Ley Justicia		(Crus)
				{342279, 342299},	// Ley Esperanza	(Crus)				
			};
			
			FixedIndex = new Dictionary<uint,int>  			// Default Skill Index. Por defecto se muestran el primer indice con timeleft > 0 , aquí se puede fijar un indice concreto para la habilidad
			{						
				{130738, 3},	// Marcado Para Morir (DH)				
							
				{ 79528, 0},   	// Tolerancia Dolor	(Barb)
				{ 96296, 9},	// Torbellino		(Barb)

				{462255, 3},	// Parasitar (Necro)
				
				{317076, 6},	// Santuario Interior	(Monk)
				
				{375050, 1},	// Mantra Salvación		(Monk)
				{375083, 1},	// Mantra Reprensión	(Monk)
				{373154, 1},	// Mantra Curación		(Monk)
				{375089, 2},	// Mantra Convicción	(Monk)

				{342284, 6},	// Ley Valor		(Crus)
				{342286, 6},	// Ley Justicia 	(Crus)
				{342299, 6},	// Ley Esperanza	(Crus)	
				{325216, 0},	// Justicia 		(Crus) //  o excluir 7
				{286510, 1},	// Fulminar 		(Crus) //  o excluir 14
				{239042, 4},	// Ataque de barrido(Crus) //  o excluir 0
				{266951, 2},	// Escudo Bendito	(Crus) //  o excluir 7				
				{291804, 0},	// Piel Férrea		(Crus) //  o excluir 3		

				{134872, 2},	// Arconte		(Wiz)
				
				{117402, 4},	// Trance Vudú			(WD) // o excluir 1,3,2
				{108506, 3},	// Rafaga de espiritus	(WD) // o excluir el 1
				{ 67616, 0},	// Cosecha de Almas		(WD) // o excluir el 4
			};
				
			ExcludeAll = new List<uint>	 // Ignore Skill index (all)
			{
				 93395,	// Rayo de Hielo	(Wiz)  // o excluir 6
				  1765,	// Electrocutar		(Wiz)  // o excluir 1
				 91549,	// Desintegrar		(Wiz)  // o excluir 10
				134456,	// Rafaga			(Wiz)  // o excluir 2,8,9
				
				462089,		//Mago esqueletico (Necro)
				
				 77649,  	// Multishot		(DH)
				129216,		// Abrojos			(DH)
				131325,		// Flecha Elemental	(DH)
				129217,		// Centinela		(DH)
				
				362102, 	// Aliado Místico   (Monk)				
				362118,		// Aliado Místico R (Monk)
				 95940,		// Puños del trueno (Monk)
				121442,		// Vuelo del Dragón (Monk)
				
				266766,		// Martillo Bendito (Crus)
				273941,		// Consagración		(Crus), index 0 y 8  pero no parecen útiles	

				105963,		// Murciélagos		(WD) o excluir index 4
            };
			
			ExcludeThis = new Dictionary<uint,List<int>>	// Ignore skill Index (list)	
			{ 							
				{460757, new List<int>{5,1} },		// Devorar 			(Necro)  
				{453563, new List<int>{8} },		// Abosorver sangre (Necro) 
							
				{159169, new List<int>{2} },		// Arrollar			(Barb) ( active index = 0 , 1 ) 
				
				{ 97110, new List<int>{0,2} },		// Senda cien puños	(Monk) ( active index = 1 y 4 )
				{312736, new List<int>{0} },		// Embestida 		(monk) ( active index = 3 ,2 , 4)
				{ 83602, new List<int>{5} },		// Posesión 		(WD)
            };						
		}

		public void TimeLeftDraw( ActionKey key , double TimeLeftSec )
		{
			var ui = Hud.Render.GetPlayerSkillUiElement(key);
			var rect = new RectangleF((float)Math.Round(ui.Rectangle.X) + 0.5f, (float)Math.Round(ui.Rectangle.Y) + 0.5f, (float)Math.Round(ui.Rectangle.Width), (float)Math.Round(ui.Rectangle.Height));
			string text = string.Empty;
			if (TimeLeftSec < 1.0f) 
			{ 
				text = String.Format("{0:N1}",  TimeLeftSec ); 
			}
			else 
			{
				TimeLeftSec =  (int) (TimeLeftSec + 0.80);  // Redondeará a X si es menor  a X.20
				text = (TimeLeftSec < 60)? String.Format("{0:0}", TimeLeftSec ) : String.Format("{0:0}:{1:00}", (int) (TimeLeftSec / 60) ,  TimeLeftSec % 60 ) ;
			}
			var textLayout = TimeLeftFont.GetTextLayout(text) ;
			TimeLeftFont.DrawText(textLayout, rect.X + ((rect.Width - (float)Math.Ceiling(textLayout.Metrics.Width))/2), rect.Y + (((rect.Height / 2.5f) - textLayout.Metrics.Height)/2));					
		}	
		
        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden)   return;
            if (clipState != ClipState.BeforeClip)   return;
            if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map))  return;
			
            foreach (var skill in Hud.Game.Me.Powers.CurrentSkills)
            {	
				IBuff buff = null ;
				if (ReplaceSno.TryGetValue(skill.CurrentSnoPower.Sno, out var sno))
				{
					buff = Hud.Game.Me.Powers.AllBuffs.FirstOrDefault(b => b.SnoPower.Sno == sno );			
				}
				else { buff = skill.Buff; }
				if (buff == null) continue;				
				if (buff.Active)					
				{
					if (FixedIndex.TryGetValue(buff.SnoPower.Sno, out var index))  // Por si queremos fijar el índice para determinadas habilidades. 
					{ 
						if (buff.TimeLeftSeconds[index] > 0)
						{
							TimeLeftDraw( skill.Key, buff.TimeLeftSeconds[index]);		
						}
					}
					else  if (!ExcludeAll.Contains(buff.SnoPower.Sno))  // Habilidades para las que no se mostrará ningún timeleft , salvo si se prefijó
					{		
						bool skip = ExcludeThis.ContainsKey(buff.SnoPower.Sno) ? true:false;
						for (int i = 0; i < buff.TimeLeftSeconds.Length ;i++) 
						{
							if (buff.TimeLeftSeconds[i] > 0) 
							{
								if (skip && ExcludeThis[buff.SnoPower.Sno].Contains(i)) continue;	// Para excluir determinados indices en las comparaciones						
								TimeLeftDraw( skill.Key, buff.TimeLeftSeconds[i] );		
								break;		
							}
						}
					}						
				}	
            }
        }
    }
}