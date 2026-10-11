// Based on gjuz's Poolstate plugin ( https://www.ownedcore.com/forums/diablo-3/turbohud/turbohud-community-plugins/613631-international-gz-poolstate.html )
using Turbo.Plugins.Default;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Turbo.Plugins.RNN
{
	public class AreaPoolsDeaths : BasePlugin, IInGameTopPainter, IAfterCollectHandler, INewAreaHandler
	{
        private Dictionary<uint, string> EspecialSno { get; set; }
        private Dictionary<uint, string> ActFix { get; set; }
		private Dictionary<uint, string> Lnn { get; set; }

		private IFont DefaultFont { get; set; }
		private IFont PortraitDataIndexFont { get; set; }

		public IFont BlancoFont { get; set; }
 		public IFont VerdeFont { get; set; }
		public IFont RojoFont { get; set; }
		public IFont SelectFont { get; set; }		
		
		private PlayerData[] DataIndex { get; set; } = new PlayerData[4];
		private int MyIndex { get; set; } = -1;
		private int TotalPlayers { get; set; } = 0;
		private bool InRift { get; set; }
		private int InGame { get; set; } = 0;
		public int NGMute { get; set; } = 300;
		
		public const string DeathsTotalSymbol = "☠";
		public const string DeathsInRiftSymbol = "🕈"; 
		public const string HasPoolSymbol = "⬟";
		public const string EmptyPoolSymbol = "⬠"; 
		
		public bool ShowArea { get; set; }
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		
		public bool TTSDeaths { get; set; }
		public string MsgDeaths { get; set; }
		public bool WavDeaths { get; set; }
		public string WavFile { get; set; }		
		
		public bool DebugArea { get; set; } = false;
		
		public AreaPoolsDeaths() 
		{
			Enabled = true;
		}

		public override void Load(IController hud) {
			base.Load(hud);
			Order = 30001;

            ShowArea = false;	// Show or hide information about the Area/Map (Lnn,Co,Va,VA,Tx,Ux,Ax,Wh)
			OffsetX = 0.05f;	// 0f...1f Set the x coordinate, relative to the portrait
			OffsetY = 0.12f;	// 0f...1f Set the y coordinate, relative to the portrait
			TTSDeaths = true;	// Notify (TTS) the deaths of other players
			MsgDeaths = "<Player> Умер";	// <Player> is replaced by the name of the player
			WavDeaths = false;	// Notify (sound file) the deaths of other players
			WavFile = "notification_9.wav";  // File to be played. It must be in the Sounds\ folder
			
            DefaultFont = Hud.Render.CreateFont("tahoma", 8f, 255, 180, 147, 109, false, false, true);
			PortraitDataIndexFont = Hud.Render.CreateFont("tahoma", 7f, 255, 180, 147, 109, false, false, true);	
		
 		    BlancoFont = Hud.Render.CreateFont("tahoma", 8f, 250, 255, 255, 255, false, false, 250, 0, 0, 0, true);
		    VerdeFont = Hud.Render.CreateFont("tahoma", 8f, 250, 0, 250, 0, false, false, 250, 0, 0, 0, true);
		    RojoFont = Hud.Render.CreateFont("tahoma", 9f, 250, 255, 0, 0, false, false, 250, 0, 0, 0, true);
			SelectFont = DefaultFont;
			
			Lnn = new Dictionary<uint,string>   // < SnoArea.Sno, Level string>
			{
				{288482, "L01"},	{288684, "L02"},	{288686, "L03"},	{288797, "L04"},	{288799, "L05"},
				{288801, "L06"},	{288803, "L07"},	{288809, "L08"},	{288812, "L09"},	{288813, "L10"},
				{0, "Lxx"}  // está añadido por precaución, por si en el futuro añaden más niveles
			};	
			
			EspecialSno = new Dictionary<uint, string> 
			{
				{380773, "Va"} , {380774, "Va"}, // Vault
				{483058, "VA"} , {483085, "VA"}, // Vault Ancient
				{434650, "Co"}, // Cow
				{257116, "U"}, {256767, "U1"}, {256106, "U2"}, {256742, "U3"}, {374239, "U4"}, // Ubers
				{332339, "T1"}, {168314, "T2"}, {92945, "T¾"}, {270011, "T5"}, //Town
				{409094, "WH"} //  Whimsydale
			};
					 
	 	     // (SnoArea.Sno,act) para conocer el acto al que pertenecen los mapas nuevos, al parecer no tienen asociado un acto en TH
			ActFix = new Dictionary<uint, string>
			{
				{ 63666, "A1"}, {445426, "A1"}, //1
				{456638, "A2"}, {460671, "A2"}, {464092, "A2"}, {464830, "A2"}, {465885, "A2"}, {467383, "A2"}, //2
				{444307, "A3"}, {445762, "A3"}, //3
				{444396, "A4"}, {445792, "A4"}, {446367, "A4"}, {446550, "A4"}, {448011, "A4"}, {448039, "A4"},  //4
				{464063, "A4"}, {464065, "A4"}, {464066, "A4"},                                   
				{464810, "A4"}, {464820, "A4"}, {464821, "A4"}, {464822, "A4"}, {464857, "A4"}, {464858, "A4"},                   
				{464865, "A4"}, {464867, "A4"}, {464868, "A4"}, {464870, "A4"}, {464871, "A4"}, {464873, "A4"},
				{464874, "A4"}, {464875, "A4"}, {464882, "A4"}, {464886, "A4"}, {464889, "A4"}, {464890, "A4"},                        
				{464940, "A4"}, {464941, "A4"}, {464942, "A4"}, {464943, "A4"}, {464944, "A4"},
				{475854, "A4"}, {475856, "A4"}, 
				{448391, "A5"}, {448368, "A5"}, {448375, "A5"}, {448398, "A5"}, {448404,"A5"},  {448411,"A5"}, //5
				{     0, "A?"},
			};
			
			for( var i = 0; i < 4; i++)
			{
				DataIndex[i] = new PlayerData{ BonusPool = long.MinValue, DeathsTotal = 0,DeathsInRift = 0, Alive = true, TicksDeath = 0 };
			}		
		}

		public class PlayerData 
		{
			public long BonusPool { get; set; }
			public int DeathsTotal { get; set; }
			public int DeathsInRift { get; set; }
			public bool Alive { get; set; }
			public int TicksDeath { get; set; }
			
			public void Reset()
			{
				this.BonusPool = long.MinValue;
				this.DeathsTotal = 0;
				this.DeathsInRift = 0;					
				this.Alive = true;		
				this.TicksDeath = 0;				
			}			
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   
			{
				MyIndex = Hud.Game.Me.Index;
				TotalPlayers = Hud.Game.NumberOfPlayersInGame;	
				
				for(int i = 0; i < 4; i++) 	{ DataIndex[i].Reset(); }
				var players = Hud.Game.Players.Where(p => p.HasValidActor);
				foreach (var player in players)
				{
					DataIndex[player.Index].BonusPool = player.BonusPoolRemaining;
				}
				InRift = false;	
				InGame = Hud.Game.CurrentGameTick;	// NG Tick , For Mute				
			}
 		}	

        public void Play_Sound(string Sonido)
        {
			var playSonido = Hud.Sound.LoadSoundPlayer(Sonido);            
			ThreadPool.QueueUserWorkItem(state =>
			{
				try  { playSonido.PlaySync(); }
				catch (Exception)  {   } 
			}	);
		}
		
		public void AfterCollect()  
        {
			if (!Hud.Game.IsInGame) return;  // si se quita, NumberOfPlayersInGame da valores falsos
			if (InRift ^ (Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492) && (q.State == QuestState.started || q.State == QuestState.completed )) != null))
			{
				InRift = !InRift;
				for(int i = 0; i < 4; i++) { DataIndex[i].DeathsInRift = 0; }
			}

			if (TotalPlayers != Hud.Game.NumberOfPlayersInGame) 
			{
				TotalPlayers = Hud.Game.NumberOfPlayersInGame;
				PlayersIndex_Part();				
			}
			
			foreach (var player in Hud.Game.Players)
			{
				if (player.IsDeadSafeCheck)
				{
					if (DataIndex[player.Index].Alive)
					{
						if (DataIndex[player.Index].TicksDeath == 0) { DataIndex[player.Index].TicksDeath = Hud.Game.CurrentGameTick; }	//testing , añadida protección extra
						else if (Math.Abs(Hud.Game.CurrentGameTick - DataIndex[player.Index].TicksDeath) > 12)  
						{
							DataIndex[player.Index].Alive = false;
							DataIndex[player.Index].DeathsTotal +=  1;
							if (InRift) { DataIndex[player.Index].DeathsInRift +=  1; }					
							DataIndex[player.Index].BonusPool = 0;							
							if (!player.IsMe)
							{
								if ( (InGame + NGMute) < Hud.Game.CurrentGameTick  ) 
								{
									if (TTSDeaths) { Hud.Sound.Speak(MsgDeaths.Replace("<Player>",player.BattleTagAbovePortrait)); }
									if (WavDeaths) { Play_Sound(WavFile); } 
								}								
							}
						}
					}
				}
				else 
				{
					if (!DataIndex[player.Index].Alive) {	DataIndex[player.Index].Alive = true;	}
						//else //testing 
						//{ 
						//	var t = Math.Abs(Hud.Game.CurrentGameTick - DataIndex[player.Index].TicksDeath);
						//	if (t < 12) { Hud.Sound.Speak("Revisa Muertes " + t + " , "+ player.BattleTagAbovePortrait); } 
						//}
					if (DataIndex[player.Index].TicksDeath != 0) { DataIndex[player.Index].TicksDeath = 0; }
					if (player.HasValidActor)	{	DataIndex[player.Index].BonusPool = player.BonusPoolRemaining;	}
				}
			}
		}	

		public void PlayersIndex_Part()
		{
			for(int i = 0; i < 4; i++) 
			{
				var part = true;
				foreach (IPlayer p in Hud.Game.Players) 
				{
					if (p.Index == i) { part = false; break; }
				}
				if (part) { DataIndex[i].Reset(); }
			}
		}
		
		public void PaintTopInGame(ClipState clipState) {
			if (clipState != ClipState.BeforeClip) return;			
			if (!Hud.Game.IsInGame) return;
			
			foreach (var player in Hud.Game.Players) 
			{
				var pt = player.PortraitUiElement.Rectangle;
				float XPos = pt.X + Hud.Window.Size.Width * OffsetX;
				float YPos = pt.Y + Hud.Window.Size.Height * OffsetY;	
				if (DebugArea) 
				{
					if (ActFix.TryGetValue(player.SnoArea.Sno,out var Acto))   { SelectFont = (player.SnoArea.Act > 0)?RojoFont:VerdeFont; }
					else if (EspecialSno.TryGetValue(player.SnoArea.Sno,out Acto)) { SelectFont = (player.SnoArea.Act > 0)?BlancoFont:RojoFont; }
					else   { Acto = "A?"; SelectFont = (player.SnoArea.Act > 0)?BlancoFont:RojoFont; }
					Acto = player.SnoArea.Act + "/" + Acto; 
					SelectFont.DrawText(SelectFont.GetTextLayout(Acto + " - " + player.SnoArea.Sno + " / " + player.SnoArea.NameLocalized),XPos ,YPos + 17);
				}
				if (ShowArea)
				{
					string PtId  ;
					if ((player.SnoArea.HostAreaSno == 288482) || (player.SnoArea.Sno == 288482))
					{
						if (!Lnn.TryGetValue(player.SnoArea.Sno, out PtId)) PtId = Lnn[0];
					}
					else if  (!EspecialSno.TryGetValue(player.SnoArea.Sno, out PtId))
					{	
						if (player.SnoArea.Act == 0)  { if (!ActFix.TryGetValue(player.SnoArea.Sno, out PtId)) PtId = ActFix[0];  } 
						else {  PtId = "A" + player.SnoArea.Act;  }
					}  
					var layout = DefaultFont.GetTextLayout( PtId);
					DefaultFont.DrawText(layout, XPos , YPos );					
					XPos += layout.Metrics.Height * 1.7f;  // XPos += layout.Metrics.Width + layout.Metrics.Height * 0.35f;
				}			
				long bonus = DataIndex[player.Index].BonusPool;
				float _pool = (bonus >= 0)?((bonus > 0)?10*((float)bonus/player.ParagonExpToNextLevel):0f):float.PositiveInfinity;
				string s = string.Format("{0} {1:0.##}  {2} {3} ", (bonus>0)? HasPoolSymbol:EmptyPoolSymbol, _pool, DeathsTotalSymbol, DataIndex[player.Index].DeathsTotal);
				if (InRift)
				{
					 s += string.Format(" [{0}{1}", DeathsInRiftSymbol, DataIndex[player.Index].DeathsInRift);
					 if (player.InGreaterRift) s += string.Format( " {0}s", (DataIndex[player.Index].DeathsInRift > 6)? 30:( 5 * (DataIndex[player.Index].DeathsInRift - (player.IsDead?1:0)) ) );
					 s += "]";
				}	
				PortraitDataIndexFont.DrawText(PortraitDataIndexFont.GetTextLayout(s), XPos , YPos); 							
			}
		}
	}
}