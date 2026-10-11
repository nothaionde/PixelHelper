/*

July 14, 2021
	- fixed CountdownUI.BackgroundBrush changing StrokeWidth during countdown due to brush sharing with the DrawPhantasm/DrawPhantasmPaused functions
July 12, 2021
	- fixed phantasm count associated with each countdown bar
July 11, 2021
	- rewritten - active/inactive phantasm state via attribute checks


*/

namespace Turbo.Plugins.Razor
{
	using SharpDX.Direct2D1;
	using SharpDX.DirectInput;
	using SharpDX.DirectWrite;
	using System; //Math
	using System.Collections.Generic;
	using System.Drawing; //RectangleF
	using System.Globalization;
	using System.Linq;
	using System.Media;
	using System.Threading;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Movable;
	using Turbo.Plugins.Razor.Label;

    public class SpiritBarrageHelper : BasePlugin, IMovable, IAfterCollectHandler, INewAreaHandler, IInGameTopPainter //, IInGameWorldPainter //, IKeyEventHandler, IInGameTopPainter
	{
		public bool MarkPhantasms { get; set; } = true; //show automatically-selected DisplayedPlayer's phantasms (only you by default)
		public bool MarkOthersPhantasms { get; set; } = true; //DisplayedPlayer can be both you or other party members
		public bool MarkAllPhantasms { get; set; } = false; //show everyones phantasms instead of just the closest wd (or self)
		//public bool MarkPhantasmRange { get; set; } = true;
		public bool SaveBarrageDetails { get; set; } = true;
		public float MarkExplosionRadiusAtTimeRemaining { get; set; } = 2f; //seconds remaining before phantasm explosion, 0 = don't show explosion radius
		
		public int PhantasmCountCap { get; set; } = 3;
		public List<Phantasm> Phantasms { get; private set; } = new List<Phantasm>();
		//public List<Phantasm> UnassignedPhantasms { get; set; } = new List<Phantasm>();

		public List<Phantasm> DisplayedPhantasms { get; private set; } = new List<Phantasm>();
		public IPlayer DisplayedPlayer { get; set; } 
			/*get { return _player; }
			private set {
				if (_player != value)
				{
					_player = value;
					DisplayedPhantasms.Clear();
					if (_player is object)
					{
						DisplayedPhantasms.Clear();
						DisplayedPhantasms.AddRange(Phantasms.Where(p => p.SummonerId == _player.SummonerId && (p.DeathTick <= 0 || p.DeathTick >= Hud.Game.CurrentGameTick)).OrderByDescending(p => p.SpawnTick));
						//if (DisplayedPhantasms.Count > PhantasmCountCap)
						//	DisplayedPhantasms.RemoveRange(PhantasmCountCap, DisplayedPhantasms.Count - PhantasmCountCap); //All(p => p.DeathTick < Hud.Game.CurrentGameTick);
					}
				}
			}
		}
		private IPlayer _player;*/
		public int HistoryLength { get; set; } = 15; //per player
		
		public WorldDecoratorCollection RippleDecorator { get; set; }
		
		public class Phantasm {
			public uint Id { get; set; }
			public IWorldCoordinate Position { get; set;}
			public uint SummonerId { get; set; }
			public string SummonerName { get; set; }
			public bool Barber { get; set; } = false;
			public bool Gazing { get; set; } = false;
			public int DeathTick { get; set; } = -1; //gametick / 60 = seconds
			public int StartTick { get; set; } = -1; //gametick / 60 = seconds
			public int SpawnTick { get; set; } //gametick / 60 = seconds
			public DateTime Timestamp { get; set; }
			
			public bool Detonated { get; set; } = false; //whether or not this phantasm was detonated early
			
			public int Ticks { get; set; } = 0;

			//snapshot
			public int PainEnhancerStacks { get; set; } = 0;
			public int SacrificeStacks { get; set; } = 0;
			public int GruesomeStacks { get; set; } = 0;
			public float AttackSpeed { get; set; }
			public int FramesPerAttack { get; set; }
			
			//dynamic
			public int ConventionTicks { get; set; } = 0;
			public int OculusTicks { get; set; } = 0;
			public int EndlessSum { get; set; } = 0;
			public int SquirtsSum { get; set; } = 0;
			public int PowerPylonTicks { get; set; } = 0;
			public int ChannelingPylonTicks { get; set; } = 0;
			public int DensitySum { get; set; } = 0;
			public int ConfidenceTicks { get; set; } = 0;
			
			//result
			public double DamageDealt { get; set; }
			
			//bookkeeping
			public uint AreaSno { get; set; }
			public int LastSeen { get; set; }
			public int LastAccumulationTick { get; set; } = 0;
			public int LastAccumulationTickDensity { get; set; }
			
			//public Dictionary<uint, int> AccumulatedBuffs { get; set; } = new Dictionary<uint, int>(); //snopower.sno, number of accumulated buff ticks
			//public int LastAccumulatedTick { get; set; } = new Dictionary<uint, int>(); //snopower.sno, number of accumulated buff ticks
			public Phantasm() {}
		}
		
		//public WorldDecoratorCollection PhantasmDecorator { get; set; }
		//public WorldDecoratorCollection PhantasmExplosionDecorator { get; set; }
		public WorldDecoratorCollection PhantasmTimerDecorator { get; set; }
		public GroundFixedTimerDecorator PhantasmTimer { get; set; }
		
		//public int[] PhantasmRGB { get; set; } = new int[3] { 28, 104, 255 }; //8, 173, 255
		//public int[] PausedRGB { get; set; } = new int[3] { 206, 3, 252 };
		public IBrush PhantasmBrush { get; set; }
		public IBrush PausedBrush { get; set; }
		public IBrush ExplosionBrush { get; set; }
		public string PausedSymbol { get; set; } = "⏳";
		//public string TextNotApplicable { get; set; } = "-";
		public StringGeneratorFunc TextNotApplicable { get; set; } = () => "-";
		
		public float BarWidth { get; set; } = 35;
		public float BarHeight { get; set; } = 4;
		public float StartingPositionX { get; set; }
		public float StartingPositionY { get; set; }
		
		public IBrush HighlightBrush { get; set; }
		public IBrush DividerBrush { get; set; }
		//public IBrush TimerBrush { get; set; }
		public IBrush BgBrush { get; set; }
		public IBrush ShadowBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		public IFont CountFont { get; set; }
		public IFont TimerFont { get; set; }
		
		public IFont HintFont { get; set; }
		public IFont TimeFont { get; set; }
		public IFont TextFont { get; set; }
		
		public IBrush TimerHigh { get; set; }
		public IBrush TimerLow { get; set; }
		
		private float offsetX = 0;
		private float offsetY = 0;
		private int LastMaintenanceTick;
		private LabelTableDecorator Countdown;
		private LabelProgressBarDecorator CountdownBar;

        public SpiritBarrageHelper()
		{
			Enabled = true;
		}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//HighlightBrush = Plugin.HighlightBrush;
			DividerBrush = Hud.Render.CreateBrush(650, 255, 255, 255, 1);
			//TimerBrush = Hud.Render.CreateBrush(120, 51, 255, 204, 0);
			BgBrush = Hud.Render.CreateBrush(100, 0, 0, 0, 0);
			BorderBrush = Hud.Render.CreateBrush(100, 0, 0, 0, 3);
			ShadowBrush = Hud.Render.CreateBrush(75, 0, 0, 0, 0);
			//EssenceBrush = Hud.Render.CreateBrush(120, 40, 255, 237, 0);
			//EssenceFont = Hud.Render.CreateFont("tahoma", 7, 220, 40, 255, 237, true, false, 150, 0, 0, 0, true); //185, 185, 185
			CountFont = Hud.Render.CreateFont("tahoma", 9, 220, 255, 255, 255, true, false, 150, 0, 0, 0, true);
			TimerFont = Hud.Render.CreateFont("tahoma", 7, 200, 255, 255, 255, false, false, 150, 0, 0, 0, true);
			
			TextFont = Hud.Render.CreateFont("tahoma", 8f, 255, 245, 218, 66, false, false, true); //25, 225, 255
			HintFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 30, true, false, true);
			TimeFont = Hud.Render.CreateFont("tahoma", 8f, 190, 255, 255, 255, false, false, true);

			TimerHigh = Hud.Render.CreateBrush(255, 0, 255, 100, 0); //120 alpha
			TimerLow = Hud.Render.CreateBrush(255, 255, 0, 0, 0); //120 alpha
			
			StartingPositionX = Hud.Window.Size.Width * 0.5f; 
			StartingPositionY = Hud.Window.Size.Height * 0.65f;
			
			PhantasmBrush = Hud.Render.CreateBrush(255, 28, 104, 255, 0);
			PausedBrush = Hud.Render.CreateBrush(255, 206, 3, 252, 0);
			ExplosionBrush = Hud.Render.CreateBrush(200, 28, 104, 255, 3, SharpDX.Direct2D1.DashStyle.Dash);
			
			//checking for the presence of follower or pets
			Hud.Render.RegisterUiElement("Root.NormalLayer.portraits.stack.pet_stack.portrait_0", Hud.Render.GetUiElement("Root.NormalLayer.portraits.stack.party_stack.portrait_0"), null);
			//Hud.Render.RegisterUiElement("Root.NormalLayer.portraits.stack.pet_stack.portrait_0.icon", Hud.Render.GetUiElement("Root.NormalLayer.portraits.stack.party_stack.portrait_0"), null);
			
			//can pin the floater prior to initialization being complete because RunStatsPlugin doesn't process the command until it is about to start rendering
			//PinFloater(new RectangleF(Hud.Window.Size.Width * 0.5f, Hud.Window.Size.Height * 0.64f, 1, 1));
			/*if (offsetX == 0 || offsetY == 0) {
				TextLayout layout = CountFont.GetTextLayout("0000");
				offsetX = layout.Metrics.Width * 0.5f;
				offsetY = layout.Metrics.Height;
				
				//update the pin dimensions
				//Plugin.UpdateFloaterPosition(new RectangleF(x - offsetX + 2, y + offsetY - 3, offsetX*2 - 4 + w, offsetY*3 + 6));
				PinFloater(new RectangleF(StartingPositionX, StartingPositionY, offsetX*2 - 4 + Hud.Window.Size.Width * 0.00155f * BarWidth, offsetY*3 + 6));
			}*/
			
			PhantasmTimer = new GroundFixedTimerDecorator(Hud) //GroundFixedTimerDecorator(Hud)
			{
				CountDownFrom = 5f,
				TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 100, 255, 150, true, false, 128, 0, 0, 0, true),
				BackgroundBrushEmpty = Hud.Render.CreateBrush(100, 0, 0, 0, 0),
				BackgroundBrushFill = PhantasmBrush, //Hud.Render.CreateBrush(200, 50, 50, 255, 0),
				Radius = 25f,
			};
			PhantasmTimerDecorator = new WorldDecoratorCollection(PhantasmTimer);
			
			RippleDecorator = new WorldDecoratorCollection(
				new GroundRippleDecorator(Hud)
				{
					Brush = PhantasmBrush, //ExplosionBrush, //Hud.Render.CreateBrush(225, 0, 255, 0, 3),
					Radius = 15f,
					RadiusTransformator = new RippleRadiusTransformator(Hud, 1500),
				},
				new GroundRippleDecorator(Hud)
				{
					Brush = PhantasmBrush, //ExplosionBrush, //Hud.Render.CreateBrush(225, 0, 255, 0, 3),
					Radius = 15f,
					RadiusTransformator = new RippleRadiusTransformator(Hud, 1500) {Offset = 500},
				},
				new GroundRippleDecorator(Hud)
				{
					Brush = PhantasmBrush, //ExplosionBrush, //Hud.Render.CreateBrush(225, 0, 255, 0, 3),
					Radius = 15f,
					RadiusTransformator = new RippleRadiusTransformator(Hud, 1500) {Offset = 1000},
				},
				new GroundRippleDecorator(Hud)
				{
					Brush = PhantasmBrush, //ExplosionBrush, //Hud.Render.CreateBrush(225, 0, 255, 0, 3),
					Radius = 15f,
					RadiusTransformator = new RippleRadiusTransformator(Hud, 1500) {Offset = 1500},
				}
			);
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
				Phantasms.Clear();
		}
		
		public void AfterCollect()
		{
			//if (!Hud.Game.IsInGame) return;			
			if (!Hud.Game.IsInGame) //|| !Hud.Game.Players.Any(p => p.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor && p.Powers.UsedWitchDoctorPowers.SpiritBarrage is object))
			{
				DisplayedPlayer = null;
				return;
			}
			
			DisplayedPlayer = Hud.Game.Me.Powers.UsedWitchDoctorPowers.SpiritBarrage is object ? Hud.Game.Me : (MarkOthersPhantasms ? Hud.Game.Players.FirstOrDefault(p => !p.IsMe && p.Powers.UsedWitchDoctorPowers.SpiritBarrage is object) : null);
			if (DisplayedPlayer == null) //no witch doctors using Spirit Barrage in the game
			{
				if (DisplayedPhantasms.Count > 0)
					DisplayedPhantasms.Clear();
				return;
			}
			
			//List<uint> modifiedSummonerIds = new List<uint>();
			//bool updateDisplayed = false;
			foreach (IActor phant in Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel))
			{
				IPlayer player = Hud.Game.Players.FirstOrDefault(p => p.SummonerId == phant.SummonerAcdDynamicId);
				if (player == null) continue;
				
				Phantasm snapshot = Phantasms.FirstOrDefault(p => p.Id == phant.AnnId);
				if (snapshot is object)
				{
					snapshot.LastSeen = Hud.Game.CurrentGameTick;
					
					if (snapshot.StartTick <= 0)
					{
						//if (phant.GetAttributeValueAsInt(Hud.Sno.Attributes.Deleted_On_Server, uint.MaxValue, -1) == 1)
						//	snapshot.DeathTick = Hud.Game.CurrentGameTick;
						//else if (phant.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 186471, -1) == 1)
						if (phant.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 186471, -1) == 1)
						{
							snapshot.StartTick = Hud.Game.CurrentGameTick;
							snapshot.DeathTick = snapshot.StartTick + (snapshot.Gazing ? 10 : 5)*60;
						}
						
					}
					else if (Hud.Game.CurrentGameTick <= snapshot.DeathTick) //snapshot.StartTick > 0
					{
						//detonated early?
						/*if (phant.GetAttributeValueAsInt(Hud.Sno.Attributes.Deleted_On_Server, uint.MaxValue, -1) == 1)
						{
							snapshot.DeathTick = Hud.Game.CurrentGameTick;
							continue;
						}*/
						
						//poll data about the phantasm
						if (snapshot.Barber && SaveBarrageDetails && snapshot.StartTick > 0 && Hud.Game.CurrentGameTick - snapshot.LastAccumulationTick >= 30 && snapshot.Ticks <= (snapshot.Gazing? 20 : 10))
						{
							++snapshot.Ticks;
							snapshot.LastAccumulationTick = Hud.Game.CurrentGameTick;
							
							//density ticks
							var monstersInPhantasmRange = Hud.Game.AliveMonsters.Where(m => !m.Untargetable && !m.Invisible && (phant.FloorCoordinate.XYDistanceTo(m.FloorCoordinate) - m.RadiusBottom) <= 10f);
							//snapshot.DensitySum += monstersInPhantasmRange.Count();
							snapshot.LastAccumulationTickDensity = monstersInPhantasmRange.Count();
							snapshot.DensitySum += snapshot.LastAccumulationTickDensity;
							
							if (player.HasValidActor && player.CoordinateKnown) //&& player.SnoArea == Hud.Game.Me.SnoArea)
							{
								//cold coe (possible other element?)
								if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 2))
									++snapshot.ConventionTicks; //+= (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 2) ? 1 : 0);
								
								//oculus
								if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2))
									++snapshot.OculusTicks; //+= (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2) ? 1 : 0);
								
								//squirts
								if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.SquirtsNecklace.Sno, 5))
									snapshot.SquirtsSum += player.Powers.GetBuff(Hud.Sno.SnoPowers.SquirtsNecklace.Sno).IconCounts[5]; //+= (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.SquirtsNecklace.Sno, 5) ? Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.SquirtsNecklace.Sno).IconCounts[5] : 0);
								
								//endless walk
								if (player.Powers.BuffIsActive(447541, 2))
									snapshot.EndlessSum += player.Powers.GetBuff(447541).IconCounts[2]; //(Hud.Game.Me.Powers.BuffIsActive(447541, 2) ? Hud.Game.Me.Powers.GetBuff(447541).IconCounts[2] : 0);
								
								//power pylon
								if (player.Powers.BuffIsActive(262935))
									++snapshot.PowerPylonTicks; //+= (player.Powers.BuffIsActive(262935) ? 1 : 0);
								
								//channeling pylon
								if (player.Powers.BuffIsActive(266258))
									++snapshot.ChannelingPylonTicks; //+= (player.Powers.BuffIsActive(266258) ? 1 : 0);
								
								//confidence ritual coverage
								if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_Passive_ConfidenceRitual.Sno))
									snapshot.ConfidenceTicks += monstersInPhantasmRange.Count(m => (player.FloorCoordinate.XYDistanceTo(m.FloorCoordinate) - m.RadiusBottom) <= 20f); //(player.Powers.BuffIsActive(Hud.Sno.SnoPowers.BaneOfTheTrappedPrimary.Sno) ? 1 : 0);
							}
						}
					}
				}
				else if (phant.GetAttributeValueAsInt(Hud.Sno.Attributes.Deleted_On_Server, uint.MaxValue, -1) != 1)
				{
					//bool gazing = player.Powers.BuffIsActive(Hud.Sno.SnoPowers.GazingDemise.Sno);
					snapshot = new Phantasm() {
						Id = phant.AnnId,
						SummonerId = phant.SummonerAcdDynamicId,
						SummonerName = player.BattleTagAbovePortrait,
						Position = Hud.Window.CreateWorldCoordinate(phant.FloorCoordinate),
						Barber = player.Powers.BuffIsActive(Hud.Sno.SnoPowers.TheBarber.Sno),
						Gazing = player.Powers.BuffIsActive(Hud.Sno.SnoPowers.GazingDemise.Sno),
						AreaSno = player.SnoArea.Sno,
						
						SpawnTick = phant.CreatedAtInGameTick,
						StartTick = phant.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 186471, -1) == 1 ? Hud.Game.CurrentGameTick : -1,
						Timestamp = Hud.Time.Now.AddSeconds(-1 * (double)(Hud.Game.CurrentGameTick - phant.CreatedAtInGameTick) / 60d),
						LastSeen = Hud.Game.CurrentGameTick,

						//snapshots
						AttackSpeed = player.Offense.AttackSpeed,
						FramesPerAttack = GetFramesPerAttack(21f, 1.2f, player.Offense.AttackSpeed),
						PainEnhancerStacks = (player.Powers.BuffIsActive(403462) ? Hud.Game.AliveMonsters.Count(m => m.Attackable && m.FloorCoordinate.XYDistanceTo(player.FloorCoordinate) <= 20 && m.DotDpsApplied > 0 && m.GetAttributeValueAsInt(Hud.Sno.Attributes.Bleeding, uint.MaxValue) == 1) : 0),
						SacrificeStacks = (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_Sacrifice.Sno) ? player.Powers.GetBuff(Hud.Sno.SnoPowers.WitchDoctor_Sacrifice.Sno).IconCounts[0] : 0),
						GruesomeStacks = (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.Sno) ? player.Powers.GetBuff(Hud.Sno.SnoPowers.WitchDoctor_Passive_GruesomeFeast.Sno).IconCounts[1] : 0),
					};
					
					if (snapshot.StartTick > 0)
						snapshot.DeathTick = snapshot.StartTick + (snapshot.Gazing ? 10 : 5)*60;
					
					//add it to the list
					Phantasms.Add(snapshot);
					
					//truncate history as necessary
					if (Phantasms.Count > HistoryLength)
						Phantasms.RemoveAt(0);

					//check for early detonations
					//var phants = Phantasms.Where(p => p.SummonerId == snapshot.SummonerId && Hud.Game.Actors.Any(a => a.AnnId == p.Id)).OrderBy(p => p.SpawnTick).ToArray(); //(Hud.Game.CurrentGameTick < p.DeathTick || p.DeathTick <= 0)
					//if (phants.Length > PhantasmCountCap)
					//	phants[PhantasmCountCap].DeathTick = snapshot.SpawnTick;
					
					//if (snapshot.SummonerId == DisplayedPlayer.SummonerId)
					//	updateDisplayed = true;
				}
				
				
				//put this summoner in the queue to be checked for early detonations
				//if (!modifiedSummonerIds.Contains(phant.SummonerAcdDynamicId))
				//	modifiedSummonerIds.Add(phant.SummonerAcdDynamicId);
			}
			
			//if (updateDisplayed)
			//{
			//	DisplayedPhantasms.Clear();
			//	DisplayedPhantasms.AddRange(Phantasms.Where(p => p.SummonerId == DisplayedPlayer.SummonerId && (p.DeathTick <= 0 || p.DeathTick >= Hud.Game.CurrentGameTick)).OrderByDescending(p => p.SpawnTick));
				//if (DisplayedPhantasms.Count > PhantasmCountCap)
				//	DisplayedPhantasms.RemoveRange(PhantasmCountCap, DisplayedPhantasms.Count - PhantasmCountCap); //RemoveAll(p => p.DeathTick < Hud.Game.CurrentGameTick);
			//}
			
			//update displayed phantasms list
			//if (DisplayedPlayer is object)
			{
				DisplayedPhantasms.Clear();
				DisplayedPhantasms.AddRange(Phantasms.Where(p => p.SummonerId == DisplayedPlayer.SummonerId && (p.DeathTick > Hud.Game.CurrentGameTick || (p.DeathTick <= 0 && Hud.Game.CurrentGameTick - p.LastSeen < 5))).OrderByDescending(p => p.SpawnTick));
			}
			//else if (DisplayedPhantasms.Count > 0)
			//	DisplayedPhantasms.Clear();
			
			//Phantasms.RemoveAll(p => Hud.Game.CurrentGameTick - p.LastSeen > 60 * 5);
		}
		
		//drawing Floater
		public void OnRegister(MovableController mover)
		{
			TextLayout layout = CountFont.GetTextLayout("0000");
			offsetX = layout.Metrics.Width * 0.5f;
			offsetY = layout.Metrics.Height;
			
			//initialize position and dimension elements
			mover.CreateArea(
				this,
				"Bars", //area name
				new RectangleF(StartingPositionX, StartingPositionY, offsetX*2 - 4 + Hud.Window.Size.Width * 0.00155f * BarWidth, offsetY*3 + 6), //position + dimensions
				true, //enabled at start?
				true, //save to config file?
				ResizeMode.Horizontal
			);
			
			CountdownBar = new LabelProgressBarDecorator(Hud) {
				BarHeight = Hud.Window.Size.Height * 0.001667f * BarHeight, 
				BarWidth = offsetX*2 - 4 + Hud.Window.Size.Width * 0.00155f * BarWidth, 
				BarBrush = TimerHigh, 
				BarBrushUnderlay = TimerLow, 
				BackgroundBrush = BgBrush, 
				BorderBrush = BorderBrush, 
				SpacingLeft = 5, 
				SpacingRight = 5
			};
			
			Countdown = new LabelTableDecorator(Hud,
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud) {Font = TimerFont, SpacingLeft = 5, SpacingRight = 5},
					new LabelStringDecorator(Hud) {Font = CountFont, SpacingLeft = 5, SpacingRight = 13}, //5 + 5 + 3
					CountdownBar,
					new LabelStringDecorator(Hud) {Font = TimerFont, Alignment = HorizontalAlign.Left, SpacingLeft = 8, SpacingRight = 5}
				) {Alignment = HorizontalAlign.Left}
			) {
				OnFillRow = (row, index) => {
					if (index >= DisplayedPhantasms.Count || index >= PhantasmCountCap)
						return false;
					
					//return false;
					Phantasm phant = DisplayedPhantasms[index];
					/*if (phant.DeathTick > 0 && phant.DeathTick < Hud.Game.CurrentGameTick || Hud.Game.CurrentGameTick - phant.LastSeen > 5)
					{
						row.Enabled = false;
						return true;
					}
					
					row.Enabled = true;*/
					
					LabelStringDecorator label = (LabelStringDecorator)row.Labels[0];
					label.StaticText = phant.LastAccumulationTickDensity > 0 ? phant.LastAccumulationTickDensity.ToString() : null; //Hud.Game.CurrentGameTick.ToString() + " : " + phant.LastSeen + " : " + phant.DeathTick; //phant.StartTick.ToString(); //phant.Id.ToString(); //"--"; //Hud.Game.AliveMonsters.Count(m => !m.Untargetable && !m.Invisible && (phant.Position.XYDistanceTo(m.FloorCoordinate) - m.RadiusBottom) <= 15f).ToString();
					
					label = (LabelStringDecorator)row.Labels[1];
					label.StaticText = (Math.Min(DisplayedPhantasms.Count, PhantasmCountCap) - index).ToString();
					
					float duration = phant.Gazing ? 10 : 5;
					//float duration = (float)(phant.DeathTick - phant.StartTick) / 60f;
					float timeLeft = duration - (Hud.Game.CurrentGameTick - phant.StartTick)/60f; //(float)(phant.DeathTick - Hud.Game.CurrentGameTick) / 60f;
					//float timeLeftPct = timeLeft / duration;
					//LabelProgressBarDecorator pLabel = (LabelProgressBarDecorator)row.Labels[2];
					//pLabel.Progress = timeLeft / duration;
					
					label = (LabelStringDecorator)row.Labels[3];
					if (phant.StartTick <= 0) // || (phant.StartTick > Hud.Game.CurrentGameTick && phant.SpawnTick < Hud.Game.CurrentGameTick)
					{
						PausedBrush.StrokeWidth = 0;
						PausedBrush.Opacity = 1f;
						CountdownBar.Progress = 1f;
						CountdownBar.BarBrush = PausedBrush; //pLabel.BackgroundBrush = PausedBrush;
						label.StaticText = "Inactive"; //timeLeft.ToString(timeLeft < 1 ? "F1" : "F0") + "s";
					}
					else
					{
						CountdownBar.Progress = timeLeft / duration;
						CountdownBar.BarBrush = TimerHigh; //TimerHigh; //pLabel.BackgroundBrush = TimerHigh;
						TimerHigh.Opacity = CountdownBar.Progress; //pLabel.Progress;
						label.StaticText = timeLeft.ToString(timeLeft < 1 ? "F1" : "F0") + "s";
					}
					
					//label = (LabelStringDecorator)row.Labels[3];
					//label.StaticText = timeLeft.ToString(timeLeft < 1 ? "F1" : "F0") + "s"; //timeLeft.ToString("F0"); //
					
					return true;
				}
			};
		}
		
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			if (DisplayedPlayer == null)
				return;

			//call LabelDecorator.IsVisible to make sure the label dimensions (RowWidths) are initialized and result in drawable values
			if (LabelDecorator.IsVisible(Countdown))
			{
				float x = area.Rectangle.X + deltaX;
				float y = area.Rectangle.Y + deltaY;

				Countdown.RowWidths[2] = area.Rectangle.Width;
				BgBrush.StrokeWidth = 0;
				Countdown.Paint(x - (Countdown.RowWidths[0] + Countdown.RowWidths[1]), y);
			}

		}
		
		//public void PaintWorld(WorldLayer layer)
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip || (!MarkPhantasms && !MarkAllPhantasms))
				return;
			
			//figure out which of the spirit barrages to mark
			var actors = MarkAllPhantasms ? 
				Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel && a.GetAttributeValueAsInt(Hud.Sno.Attributes.Deleted_On_Server, uint.MaxValue, -1) != 1) :
				DisplayedPlayer is object ? Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._wd_spiritbarragerune_aoe_ghostmodel && a.SummonerAcdDynamicId == DisplayedPlayer.SummonerId && a.GetAttributeValueAsInt(Hud.Sno.Attributes.Deleted_On_Server, uint.MaxValue, -1) != 1) : null;
			
			if (actors == null)
				return;
			
			foreach (IActor phant in actors)
			{
				Phantasm snapshot = Phantasms.FirstOrDefault(p => p.Id == phant.AnnId);
				if (snapshot is object)
				{
					if (snapshot.StartTick <= 0)
						DrawPhantasmPaused(phant); //PhantasmPausedDecorator.Paint(WorldLayer.Ground, phant, phant.FloorCoordinate, null);
					else if (snapshot.DeathTick >= Hud.Game.CurrentGameTick)
					{
						DrawPhantasm(phant); //PhantasmDecorator.Paint(WorldLayer.Ground, phant, phant.FloorCoordinate, null);
						
						PhantasmBrush.StrokeWidth = 0;
						PhantasmBrush.Opacity = 1f;
						PhantasmTimer.CountDownFrom = snapshot.Gazing ? 10 : 5;
						PhantasmTimer.CreatedAtInGameTick = snapshot.StartTick;
						PhantasmTimerDecorator.Paint(WorldLayer.Ground, phant, phant.FloorCoordinate, null);
						
						if (snapshot.Barber && MarkExplosionRadiusAtTimeRemaining > 0 && snapshot.DeathTick >= Hud.Game.CurrentGameTick && snapshot.DeathTick - Hud.Game.CurrentGameTick < 60f*MarkExplosionRadiusAtTimeRemaining)
						{
							ExplosionBrush.DrawWorldEllipse(15f, -1, phant.FloorCoordinate);
							
							PhantasmBrush.StrokeWidth = 10f;
							PhantasmBrush.Opacity = 0.5f;
							RippleDecorator.Paint(WorldLayer.Ground, phant, phant.FloorCoordinate, null);
							
							PhantasmTimer.BackgroundBrushFill = TimerLow;
						}
						else
						{
							PhantasmBrush.StrokeWidth = 0;
							PhantasmBrush.Opacity = 1f;
							PhantasmTimer.BackgroundBrushFill = PhantasmBrush;
						}
						
						PhantasmTimer.CountDownFrom = snapshot.Gazing ? 10 : 5;
						PhantasmTimer.CreatedAtInGameTick = snapshot.StartTick;
						PhantasmTimerDecorator.Paint(WorldLayer.Ground, phant, phant.FloorCoordinate, null);

					}
				}
			}

		}
		
		public void DrawPhantasmPaused(IActor phant)
		{
			BgBrush.StrokeWidth = 5f;
			BgBrush.DrawWorldEllipse(10f, -1, phant.FloorCoordinate);

			PausedBrush.StrokeWidth = 0;
			PausedBrush.Opacity = 35f/255f;
			PausedBrush.DrawWorldEllipse(10f, -1, phant.FloorCoordinate);

			PausedBrush.StrokeWidth = 3f;
			PausedBrush.Opacity = 1f;
			PausedBrush.DrawWorldEllipse(10f, -1, phant.FloorCoordinate);
			
			//draw timer symbol
			//var pos = phant.FloorCoordinate.ToScreenCoordinate();
			BgBrush.StrokeWidth = 0;
			BgBrush.DrawEllipse(phant.ScreenCoordinate.X, phant.ScreenCoordinate.Y, 25f, 25f); //pos.X, pos.Y, 25f, 25f);
			
			PausedBrush.StrokeWidth = 0;
			PausedBrush.Opacity = 1f;						
			PausedBrush.DrawEllipse(phant.ScreenCoordinate.X, phant.ScreenCoordinate.Y, 24f, 24f); //pos.X, pos.Y, 24f, 24f);
			
			var layout = CountFont.GetTextLayout(PausedSymbol);
			CountFont.DrawText(layout, phant.ScreenCoordinate.X - layout.Metrics.Width*0.5f, phant.ScreenCoordinate.Y - layout.Metrics.Height*0.5f); //CountFont.GetTextLayout(PausedSymbol)
		}
		
		public void DrawPhantasm(IActor phant)
		{
			BgBrush.StrokeWidth = 5f;
			BgBrush.DrawWorldEllipse(10f, -1, phant.FloorCoordinate); //snapshot.Position

			PhantasmBrush.StrokeWidth = 0;
			PhantasmBrush.Opacity = 35f/255f;
			PhantasmBrush.DrawWorldEllipse(10f, -1, phant.FloorCoordinate); //snapshot.Position

			PhantasmBrush.StrokeWidth = 3f;
			PhantasmBrush.Opacity = 1f;
			PhantasmBrush.DrawWorldEllipse(10f, -1, phant.FloorCoordinate); //snapshot.Position
		}
		
		public int GetFramesPerAttack(float b_anim, float s_coeff, float aps)
		{
			//FPA = floor( (b_anim - 1) / b_anim * 60 / (APS * s_coeff) )
			return (int)Math.Floor( (b_anim - 1) / b_anim * 60 / (aps * s_coeff) );
		}
    }
}