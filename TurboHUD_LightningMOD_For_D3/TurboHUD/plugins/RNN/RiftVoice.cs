using Turbo.Plugins.Default;      
using System.Linq;
using System.Collections.Generic;

namespace Turbo.Plugins.RNN
{        
	public class RiftVoice : BasePlugin, ICustomizer, INewAreaHandler, IAfterCollectHandler
	{	
		private List<uint> IdGR { get;set; } = new List<uint> {13,16,34,46};
		private List<uint> IdNR { get;set; } = new List<uint> {1,3,10,5}; 
		private int TBoss { get; set; }
		private int TBossKilled { get; set; }
		private bool InGR { get; set; }
		private bool InNR { get; set; }		
			
		private int MyIndex { get; set; } = -1;
		private string NameBoss { get; set; } = null;
		
		public bool NotifyBoss { get; set; } 
		public int WaitingBoss { get; set; } 
		public string BossAlive { get; set; }
		public string BossIdAlive { get; set; }
		public string BossDead { get; set; }
		public string BossIdDead { get; set; }

		public bool NotifyGR { get; set; } 
		public string GROpen { get; set; }
		public string GRClosed { get; set; }

		public bool NotifyNR { get; set; } 
		public string NROpen { get; set; }
		public string NRClosed { get; set; }

		public SortedDictionary<int,string> NextProgressGR { get; set; } = new SortedDictionary<int,string>(); 
		private int NewTarget { get; set; } = 0;
		public bool NotifyProgressGR { get; set; }
		
		public  RiftVoice()
		{
			Enabled = true;                        
		}
		
		public override void Load(IController hud)
        {
            base.Load(hud);	
			Order = 30001;
			
			NotifyBoss = true;	
			WaitingBoss = 90; // Ticks (1 second = 60 ticks) . Attempts will be made to identify the Boss during this time, and if it succeeds, only the text "BossIdAlive" will be read.
			BossAlive = "The boss is here";  
			BossIdAlive = "The Boss is <Boss> "; // Boss was identified. <Boss> will be replaced by the Boss name
			BossDead = "The Boss is Dead";				
			BossIdDead = "<Boss> died";			// Boss was identified. <Boss> will be replaced by the Boss name
			
			NotifyGR = true;					// Notify when a GR opens or closes
			GROpen = "GR Open";
			GRClosed = "GR Closed";
			
			NotifyNR = true;					// Notify when a NR opens or closes
			NROpen = "Rift Open";
			NRClosed = "Rift CLosed";

			NotifyProgressGR = false;			// Notify when certain progress (NextProgressGR) is achieved in the GR			
		}

        public void Customize()   
		{				
			NextProgressGR.Add(int.MaxValue,"∞");
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{	
			if ((newGame) || (MyIndex != Hud.Game.Me.Index))
			{ 
				InGR = false; InNR = false; NameBoss = null; 
				MyIndex = Hud.Game.Me.Index; 		
			}
 		}
		public string FindNameBoss()
		{
			var m = Hud.Game.Markers.FirstOrDefault(k => (k.SnoActor != null) && k.SnoActor.Code.Contains("_Boss_"));
			if	(m != null) {  return m.Name; }
			else 
			{
				var b = Hud.Game.Monsters.FirstOrDefault(s => (s.Rarity == ActorRarity.Boss) && (s.SummonerAcdDynamicId == 0));
				if (b != null) 	{ return b.SnoMonster.NameLocalized; }
			}
			return null;								
		}

		public void AfterCollect()  
        {	
			if (!Hud.Game.IsInGame) return;
			var riftQuest = Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492) && (q.State == QuestState.started || q.State == QuestState.completed )) ;
			if (riftQuest != null)
			{			
				if (IdGR.Contains(riftQuest.QuestStepId)) 
				{
					if (!InGR)  // GR start
					{						
						InGR = true;
						if (NotifyGR && (Hud.Game.RiftPercentage == 0) && !string.IsNullOrEmpty(GROpen)) { Hud.Sound.Speak(GROpen); }
						TBoss = 0; TBossKilled = 0; NameBoss = null; NewTarget = 0;
						foreach(var d in NextProgressGR) 
						{
							if (d.Key > Hud.Game.RiftPercentage) { NewTarget = d.Key ; break; }
						}
					}

					if (riftQuest.QuestStepId == 13) 
					{
						if ( NotifyProgressGR && (Hud.Game.RiftPercentage >= NewTarget) )
						{
							if (NextProgressGR.TryGetValue(NewTarget, out var text))
							{
								text = text.Replace("<Progress>",Hud.Game.RiftPercentage.ToString("F1"));
								if (!string.IsNullOrEmpty(text)) { Hud.Sound.Speak(text); }
							}
							foreach(var d in NextProgressGR) 
							{
								if (d.Key > Hud.Game.RiftPercentage) { NewTarget = d.Key ; break; }
							}							
						}
						
					}						
					else if (riftQuest.QuestStepId == 16) //GR Boss Alive 
					{	
						if (TBoss < 2)
						{
							if (NameBoss != null)
							{
								TBoss = 2;  								
								if (NotifyBoss)
								{
									var MsgBoss = BossIdAlive.Replace("<Boss>", NameBoss);
									if (!string.IsNullOrEmpty(MsgBoss))  {  Hud.Sound.Speak(MsgBoss); }	
								}									
							}
							else
							{
								NameBoss = FindNameBoss();
								if (NameBoss == null) 	
								{
									if ( (TBoss == 0) && ((Hud.Game.CurrentGameTick - riftQuest.CreatedOn) > WaitingBoss) )
									{
										TBoss = 1;
										if ( NotifyBoss && !string.IsNullOrEmpty(BossAlive) )  {  Hud.Sound.Speak(BossAlive); }
									}
								} 		// método no estricto						
							}
						}						
					}
					else if ( (riftQuest.QuestStepId == 34) ) //GR Boss Dead , Kadala Appears
					{
						if (TBossKilled == 0) 
						{
							if (NameBoss == null) 	{ NameBoss = FindNameBoss(); }
							TBossKilled = 1;  						
							if (NotifyBoss)
							{
								var MsgBoss = (NameBoss == null)? BossDead : BossIdDead.Replace("<Boss>", NameBoss);
								if (!string.IsNullOrEmpty(MsgBoss))  {	Hud.Sound.Speak(MsgBoss); }
							}
						} 
					}
					else if (riftQuest.QuestStepId == 46) //GR kadala quit , Orek Appears						
					{

						if (TBossKilled == 0) 
						{
							if (NameBoss == null) 	{ NameBoss = FindNameBoss(); }
							TBossKilled = 1;  //GR Boss Dead , Kadala Appears
							
							if (NotifyBoss)
							{
								var MsgBoss = (NameBoss == null)? BossDead : BossIdDead.Replace("<Boss>", NameBoss);
								if (!string.IsNullOrEmpty(MsgBoss))  { Hud.Sound.Speak(MsgBoss); }
							}
						} 
					}															
				}
				else if (IdNR.Contains(riftQuest.QuestStepId))
				{
					if (!InNR) // NR (Nephalem Rift) Start
					{								
						if ((riftQuest.QuestStepId == 5) && InGR) return;
						InNR = true; 
						if (NotifyNR && (Hud.Game.RiftPercentage == 0) && !string.IsNullOrEmpty(NROpen)) Hud.Sound.Speak(NROpen); 
						TBoss = 0; TBossKilled = 0; NameBoss = null;
					}
					if (riftQuest.QuestStepId == 3) 
					{
						if (TBoss < 2)
						{
							if (NameBoss != null)
							{
								TBoss = 2;   
								if (NotifyBoss)
								{
									var MsgBoss = BossIdAlive.Replace("<Boss>", NameBoss);
									if (!string.IsNullOrEmpty(MsgBoss))  {  Hud.Sound.Speak(MsgBoss); }	
								}
							}
							else
							{
								NameBoss = FindNameBoss(); 
								if (NameBoss == null) 	
								{
									if ( (TBoss == 0) && ((Hud.Game.CurrentGameTick - riftQuest.CreatedOn) > WaitingBoss) )
									{
										TBoss = 1;
										if ( NotifyBoss && !string.IsNullOrEmpty(BossAlive) )  {  Hud.Sound.Speak(BossAlive); }
									}
								} 								
							}
						}						
					}
					else if ( (riftQuest.QuestStepId == 10) )
					{
						if (TBossKilled == 0)
						{
							if (NameBoss == null) 	{ NameBoss = FindNameBoss(); }							
							TBossKilled = 1;  //NR Boss Dead , Orek Appears
							if (NotifyBoss)
							{
								var MsgBoss = (NameBoss == null)? BossDead : BossIdDead.Replace("<Boss>", NameBoss);
								if (!string.IsNullOrEmpty(MsgBoss))  {	Hud.Sound.Speak(MsgBoss); }
							}
						}
					}									
				}
				else return;
			}			
			else 
			{
				if (InGR) 
				{
					InGR = false; NameBoss = null;
					if (NotifyGR && !string.IsNullOrEmpty(GRClosed)) Hud.Sound.Speak(GRClosed); 					
				}
				if (InNR) 
				{
					InNR = false; NameBoss = null;
					if (NotifyNR && !string.IsNullOrEmpty(NRClosed)) Hud.Sound.Speak(NRClosed); 					
				}
			}				
		}		
	}	
}