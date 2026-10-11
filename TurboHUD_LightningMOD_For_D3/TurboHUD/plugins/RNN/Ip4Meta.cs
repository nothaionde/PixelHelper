// This is Version 2
// Version 1 can be found here: https://pastebin.com/bY86KJcM 

using Turbo.Plugins.Default;
using System;
using System.Linq;

namespace Turbo.Plugins.RNN
{	
	public class Ip4Meta : BasePlugin, IInGameTopPainter, INewAreaHandler
	{
		private IFont TextFontRedOk { get; set; }
		private IFont TextFontRedNo { get; set; }
		private IFont TextFontGreendOk { get; set; } 
		private IFont TextFontGreenNo { get; set; } 
		
		private TopLabelDecorator RedDecorator { get; set; }
		private TopLabelDecorator GreenDecorator { get; set; }
		private string Tiempo  { get; set; } = "0:00";
		private string IPrS { get; set; } = "IP";
		private int[,] Datos { get; set;} =  new int[,] { {0,0},{0,0},{0,0},{0,0} };
		private bool ActiveGR { get; set; } = false;
		
		public bool OnlyGR { get; set; }
		public bool OnlyWithRune { get; set; }
		
		public bool OnlyTrusted { get; set; } = false; 

		public float Offx { get; set; }
		public float Offy { get; set; }	
		
		private int DataIndex_Num { get; set; }	
		public int MyIndex { get; set; } = -1;
			
		public Ip4Meta()	
		{
			Enabled = true;
		}

	    public override void Load(IController hud)
		{
			base.Load(hud);	
			Order = 30001;
			
			Offx = 0.32f;
			Offy = 0.60f;			
			OnlyGR = true;			// Only show in GR
			OnlyWithRune = true;	// Only show when there is a Barb with the rune "Mob Rule" (Ignore Pain for allies).	
				
			TextFontRedOk		= Hud.Render.CreateFont("tahoma", 6, 255, 255, 90, 90, true, false, false);
			TextFontRedNo		= Hud.Render.CreateFont("tahoma", 6, 255, 250, 150, 0, true, false, false);
			TextFontGreendOk	= Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 0, true, false, false);
			TextFontGreenNo		= Hud.Render.CreateFont("tahoma", 6, 255, 200, 200, 200, true, false, false);

			RedDecorator = new TopLabelDecorator(Hud)  
			{
				TextFont = TextFontRedOk,
				BackgroundTexture1 = Hud.Texture.ButtonTextureBlue,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureBlue,
				BackgroundTextureOpacity1 = 10.0f,
				BackgroundTextureOpacity2 = 10.0f,
				TextFunc = () =>  Tiempo, 
				HintFunc = () => "Sin IP",
			};
			GreenDecorator = new TopLabelDecorator(Hud)   
			{
				TextFont = TextFontGreendOk,
				BackgroundTexture1 = Hud.Texture.ButtonTextureGray,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureGreen,
				BackgroundTextureOpacity1 = 10.0f,
				BackgroundTextureOpacity2 = 10.0f,
				TextFunc = () => IPrS,
				HintFunc = () => "Con IP",	
			};			
		}
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )  
			{
				MyIndex = Hud.Game.Me.Index;
				DataIndex_Num = Hud.Game.NumberOfPlayersInGame;
				for (var i = 0; i < 4; i++) 
				{
					Datos[i,0] = 0;  // Tiempo (tick) de finalización de IP. Si es cero es que nunca la ha recibido.
					Datos[i,1] = Hud.Game.CurrentGameTick; // Sirve para medir el tiempo que ha estado alejado. Será cero si se mantiene en las proximidades (HasValidActor) y no ha estado alejado más de 5s segundos.
				}
				ActiveGR = false;			
			}		
		}
		
		public void PlayersIndex_Part()
		{
			for(int i = 0; i < 4; i++) 
			{
				var part = true;
				foreach (IPlayer p in Hud.Game.Players) 
				{
					if (p.Index == i)	{ part = false;	break; }
				}
				if (part) 
				{ 
					Datos[i,0] = 0; 
					Datos[i,1] = Hud.Game.CurrentGameTick; 
				}
			}
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden)   return;
            if (clipState != ClipState.BeforeClip)   return;			
			if (!Hud.Game.Me.IsInGame) return;
			if (DataIndex_Num != Hud.Game.NumberOfPlayersInGame) 
			{
				if (Hud.Game.NumberOfPlayersInGame < DataIndex_Num) { PlayersIndex_Part(); }
				DataIndex_Num = Hud.Game.NumberOfPlayersInGame;
			}
			
			if (Hud.Game.Me.InGreaterRift) 
			{
				if (!ActiveGR) 
				{
					ActiveGR = true;
					foreach (var pl in Hud.Game.Players) 	
					{
						Datos[pl.Index,0] = pl.InGreaterRift ? Hud.Game.CurrentGameTick : 0 ;	// Para establecer el tiempo sin ip a 0 cuando entramos en una GR. Asignando 0 oculto estado de la ip de los que no entren
						Datos[pl.Index,1] = Hud.Game.CurrentGameTick; 
					}						
				}
			}
			else
			{
				if (ActiveGR) { ActiveGR = false; }
				if (OnlyGR) return;
			}	
						  
			var BarbsWithIp = Hud.Game.Players.Where(p => p.Powers.UsedSkills.Any(s => s.SnoPower.Sno == 79528));
			if (!BarbsWithIp.Any()) return;			
			
			var BarbsWithIpRune = BarbsWithIp.Where(p => p.Powers.GetUsedSkill(Hud.Sno.SnoPowers.Barbarian_IgnorePain).Rune == 2);		
			if (OnlyWithRune && !BarbsWithIpRune.Any()) return;

			bool OthersBarbsWithIpRune = BarbsWithIpRune.Any(p => p != Hud.Game.Me);
			
			foreach (var player in Hud.Game.Players)
			{			
				if (player.Powers.GetBuff(79528) == null) continue;	
				int pos = (player.Powers.GetBuff(79528).TimeLeftSeconds[1] > player.Powers.GetBuff(79528).TimeLeftSeconds[0])? 1:0; //[0] ip dada por mi, [1] ip dada por otro. Nos quedamos con el mayor
				long ultima = player.Powers.GetBuff(79528).LastActive.ElapsedMilliseconds;  // Tiempo transcurrido desde la última vez que lo vimos con ip, avanza sin parar.
				if (ultima == 0) continue;
				long queda = (long) (1000 * player.Powers.GetBuff(79528).TimeLeftSeconds[pos]) ;  // Si nos alejamos se detiene					
				var portrait = player.PortraitUiElement.Rectangle;	
				var PlayerOnlyIpFromMe = !OthersBarbsWithIpRune && (player.IsMe || !BarbsWithIp.Contains(player)) ;
				if ((queda > ultima) && (!player.IsDead))
				{ 		
					long IPms = queda - ultima ; 
					if (player.HasValidActor) 
					{
						var FinishTick = Hud.Game.CurrentGameTick + (int) (IPms * 60 / 1000);
						if (Datos[player.Index,0] < FinishTick)  { Datos[player.Index,0] = FinishTick; }						
						if	(Datos[player.Index,1] != 0) { Datos[player.Index,1] = 0; } 
						GreenDecorator.TextFont = TextFontGreendOk; 
					}
					else 
					{ 
							if (Datos[player.Index,1] == 0)	{ Datos[player.Index,1] = Hud.Game.CurrentGameTick; }
							if (PlayerOnlyIpFromMe)	{ GreenDecorator.TextFont = TextFontGreendOk; }	
							else
							{
								if (OnlyTrusted) continue;
								GreenDecorator.TextFont = TextFontGreenNo; 
							}
					}
					IPrS = (IPms < 5001)? string.Format("{0:0}s",(int) (IPms + 999) /1000) : "IP"; 
					GreenDecorator.Paint(portrait.Left + portrait.Width * Offx , portrait.Top + portrait.Height*  Offy, portrait.Width * 0.34f, portrait.Height * 0.12f, HorizontalAlign.Center);		
				}
				else
				{	
					if (Datos[player.Index,0] == 0) continue;  // Nunca ha recibido Ip
					if (player.IsDead) 
					{
						if (Datos[player.Index,0] > Hud.Game.CurrentGameTick)	{ Datos[player.Index,0] = Hud.Game.CurrentGameTick;	}
					}	
					if (player.IsMe || PlayerOnlyIpFromMe)	{ RedDecorator.TextFont = TextFontRedOk ; }
					else 
					{
						if (Datos[player.Index,1] == 0)
						{
							if (!player.HasValidActor) Datos[player.Index,1] = Hud.Game.CurrentGameTick;
							RedDecorator.TextFont = TextFontRedOk;
						}
						else
						{
							if (Hud.Game.CurrentGameTick - Datos[player.Index,1] > 300 )	// 5s de margen para considerar fiable su tiempo sin ip
							{ 
								if (OnlyTrusted) continue;
								RedDecorator.TextFont = TextFontRedNo; 
							}
							else
							{
								if (player.HasValidActor)	{ Datos[player.Index,1] = 0; }
								RedDecorator.TextFont = TextFontRedOk;
							}
						}						
					}
					long time = (Hud.Game.CurrentGameTick - Datos[player.Index,0]) / 60;
					if (time > 599) Tiempo = "∞";
					else	Tiempo = String.Format("{0:0}:{1:00}", (int) time / 60 , time % 60 ) ;
					RedDecorator.Paint(portrait.Left + portrait.Width * Offx, portrait.Top + portrait.Height*  Offy, portrait.Width * 0.34f, portrait.Height * 0.12f, HorizontalAlign.Center);	
				}
			}
		}				
	}
}