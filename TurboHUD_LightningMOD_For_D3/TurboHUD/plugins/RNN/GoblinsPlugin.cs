using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;

namespace Turbo.Plugins.RNN
{
    public class GoblinsPlugin : BasePlugin, IInGameWorldPainter, ICustomizer, INewAreaHandler, IAfterCollectHandler, IMonsterKilledHandler
	{
		private int MyIndex { get; set; } = -1;
		private int LastTick { get; set; } = 0;
		private int TotalGoblins { get; set; } = 0;
		private bool foundGoblin { get; set; } = false;

		private Dictionary<double, bool> MaxHealthReapp { get; set; } = new Dictionary<double, bool>();
		private Dictionary<ActorSnoEnum, GoblinData> GoblinsDic { get; set; } = new Dictionary<ActorSnoEnum, GoblinData>();
		private Dictionary<string, int> GoblinsFound { get; set; } = new Dictionary<string, int>();

        public WorldDecoratorCollection PortalDecorator { get; set; }
        public WorldDecoratorCollection MalevolentDecorator { get; set; }
        public WorldDecoratorCollection BloodDecorator { get; set; }
        public WorldDecoratorCollection OdiousDecorator { get; set; }
        public WorldDecoratorCollection GemDecorator { get; set; }
        public WorldDecoratorCollection GelatinousDecorator { get; set; }
        public WorldDecoratorCollection GildedDecorator { get; set; }
        public WorldDecoratorCollection InsufferableDecorator { get; set; }
        public WorldDecoratorCollection MenageristDecorator { get; set; }
        public WorldDecoratorCollection RainbowDecorator { get; set; }
        public WorldDecoratorCollection FiendDecorator { get; set; }
        public WorldDecoratorCollection TreasureDecorator { get; set; }
		public WorldDecoratorCollection DevilhandDecorator { get; set; }
		public WorldDecoratorCollection VisionDecorator { get; set; }

		private ITexture BGTexture { get; set; }
		private ITexture IMGTexture { get; set; }
		private float widthBG { get; set; } = 0;
		private float widthIMG { get; set; } = 0;
		private bool fixLoad  {get; set; } = true;
		private IFont FontText { get; set; }

        public bool EnableSpeak { get; set; }
        public string MalevolentSpeak { get; set; }
        public string BloodSpeak { get; set; }
        public string OdiousSpeak { get; set; }
        public string GemSpeak { get; set; }
        public string GelatinousSpeak { get; set; }
		public string GelatinousSplit1Speak { get; set; }
		public string GelatinousSplit2Speak { get; set; }
        public string GildedSpeak { get; set; }
        public string InsufferableSpeak { get; set; }
        public string MenageristSpeak { get; set; }
        public string RainbowSpeak { get; set; }
        public string FiendSpeak { get; set; }
        public string TreasureSpeak { get; set; }
		public string DevilhandSpeak { get; set; }

		public string VisionSpeak { get; set; }
		public string TextVisionGoblin { get; set; }

		public bool ShowCounter { get; set; }
		public float xPor { get; set; }
		public float yPor { get; set; }
		public float SizeMultiplier { get; set; }
		public float Opacity { get; set; }

        public GoblinsPlugin()
        {
            Enabled = true;
        }

		public class GoblinData
		{
			public int Amount { get; set; } = 0;
			public string TextSpeak { get; set; } = null;
			public WorldDecoratorCollection Decorator { get; set; }

			public GoblinData(string textspeak, WorldDecoratorCollection decorator)
			{
				this.TextSpeak = textspeak;
				this.Decorator = decorator;
			}
		}

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30001;

			EnableSpeak = true;
										// null -> NameLocalized , "" -> not Speak
			MalevolentSpeak = null;		// "Malevolent Tormentor"
			BloodSpeak = null;			// "Blood Thief"
			OdiousSpeak = null;			// "Odious Collector"
			GemSpeak = null;			// "Gem Hoarder"
			GelatinousSpeak = null;		// "Gelatinous Sire"
			GelatinousSplit1Speak = "";	// "" -> not Speak
			GelatinousSplit2Speak = "";
			GildedSpeak = null;			// "Gilded Baron"
			InsufferableSpeak = null;	// "Insufferable Miscreant"
			MenageristSpeak = null;		// "Menagerist Goblin"
			RainbowSpeak = null;		// "Rainbow Goblin"
			FiendSpeak = null;			// "Treasure Fiend"
			TreasureSpeak = null;		// "Treasure Goblin"
			DevilhandSpeak = null;		// "Snitchley"

			VisionSpeak = "";			// "Vision Goblin" speak when found. "" for nothing (ex. if you are installed EnmityVisionplugin)
			TextVisionGoblin = "Vision Goblin"; // to differentiate it from the treasure goblins, will be used instead of namelocalized for report and decorators

			ShowCounter = true;
			xPor = 0.324f;			// To set the x coordinate of the icon
			yPor = 0.850f;			// To set the y coordinate of the icon
			Opacity = 0.80f;		// 0f..1f  Opacity for icon texture
			SizeMultiplier = 0.5f;	// Size multiplier for icon


			BGTexture = Hud.Texture.GetTexture(672253601);  // 679903159 , 672253601
			IMGTexture = Hud.Texture.GetTexture(2605755780);
			CreateDecorators();
		}

		public void Customize()
        {
			Hud.TogglePlugin<GoblinPlugin>(false);

            GoblinsDic.Add(	ActorSnoEnum._p1_treasuregoblin_tentacle_a,	new GoblinData(RainbowSpeak, RainbowDecorator) );
            GoblinsDic.Add(	ActorSnoEnum._p1_treasuregobin_a_unique_greedminion, new GoblinData(FiendSpeak, FiendDecorator) );
		    GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_a,	new GoblinData(TreasureSpeak, TreasureDecorator) );	// "Treasure Goblin"
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_b,	new GoblinData(OdiousSpeak, OdiousDecorator)	);	// "Odious Collector"
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_c, new GoblinData(GemSpeak, GemDecorator)	);			// "Gem Hoarder"
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_c_unique_devilshand, new GoblinData(DevilhandSpeak, DevilhandDecorator) );
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_d_splitter, new GoblinData(GelatinousSpeak, GelatinousDecorator)	);			// Gelatinous Sire
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_d_splitter_02,	new GoblinData(GelatinousSplit1Speak, GelatinousDecorator) );	// Gelatinous Spawn
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_d_splitter_03,	new GoblinData(GelatinousSplit2Speak, GelatinousDecorator) );	// Gelatinous Spawn
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_e,	new GoblinData(InsufferableSpeak, InsufferableDecorator) );	// "Insufferable Miscreant"
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_f,	new GoblinData(BloodSpeak, BloodDecorator) );			// "Blood Thief"
			GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_h,	new GoblinData(MalevolentSpeak, MalevolentDecorator) );	// "Malevolent Tormentor"
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_j,	new GoblinData(GildedSpeak, GildedDecorator) );			// "Gilded Baron"
            GoblinsDic.Add(	ActorSnoEnum._treasuregoblin_k,	new GoblinData(MenageristSpeak, MenageristDecorator) );
										// -- Vision --"
			GoblinsDic.Add(	ActorSnoEnum._p76_treasuregoblin_a,	GoblinsDic[ActorSnoEnum._treasuregoblin_a] );	// Value by reference
            GoblinsDic.Add(	ActorSnoEnum._p76_treasuregoblin_b,	GoblinsDic[ActorSnoEnum._treasuregoblin_b] );
            GoblinsDic.Add(	ActorSnoEnum._p76_treasuregoblin_c,	GoblinsDic[ActorSnoEnum._treasuregoblin_c] );
            GoblinsDic.Add(	ActorSnoEnum._p76_treasuregoblin_e,	GoblinsDic[ActorSnoEnum._treasuregoblin_e] );
			GoblinsDic.Add(	ActorSnoEnum._p76_treasuregoblin_f,	GoblinsDic[ActorSnoEnum._treasuregoblin_f] );
			GoblinsDic.Add(	ActorSnoEnum._p76_treasuregoblin_a_noportal, new GoblinData(VisionSpeak, VisionDecorator) );
										// -- NightMares --
			GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_a,	GoblinsDic[ActorSnoEnum._treasuregoblin_a] );
            GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_b,	GoblinsDic[ActorSnoEnum._treasuregoblin_b] );
            GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_c,	GoblinsDic[ActorSnoEnum._treasuregoblin_c] );
            GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_e,	GoblinsDic[ActorSnoEnum._treasuregoblin_e] );
            GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_f,	GoblinsDic[ActorSnoEnum._treasuregoblin_f] );
            GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_h,	GoblinsDic[ActorSnoEnum._treasuregoblin_h] );
            GoblinsDic.Add(	ActorSnoEnum._p73_treasuregoblin_j,	GoblinsDic[ActorSnoEnum._treasuregoblin_j] );

			FontText = Hud.Render.CreateFont("arial", 18f * SizeMultiplier, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
        }

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )
			{
				MyIndex = Hud.Game.Me.Index;
				MaxHealthReapp.Clear();
				GoblinsFound.Clear();
				foreach(var d in GoblinsDic) { d.Value.Amount = 0; }
				LastTick = 0;
				foundGoblin = false;
				TotalGoblins = 0;
			}
		}

		private string GoblinsText()
		{
			string text = string.Format("[ {0} Goblins ]", TotalGoblins);
			foreach(var d in GoblinsFound.OrderByDescending(o => o.Value))
			{
				text += string.Format("\n{0} {1}", d.Value, d.Key);
			}
			return text;
		}

        private void CreateDecorators()
        {
			float offsetH = 5.0f , size1 = 8f , size2 = 6f;

			PortalDecorator = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(180, 255, 255, 255, 0),
					Radius = 8.0f
				},
                new MapShapeDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(180, 120, 0, 0, 0),
					Radius = 2.5f
				}
			);
			MalevolentDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 183, 91, 0, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 183, 91, 0, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			BloodDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 255, 0, 128, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 255, 255, 255, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 255, 0, 128, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			OdiousDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 0, 255, 0, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 0, 255, 0, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			GemDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 220, 220, 220, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 220, 220, 220, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			GelatinousDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(200, 50, 50, 200, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 255, 255, 255, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 62, 158, 255, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			GildedDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 255, 255, 0, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 255, 255, 0, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			InsufferableDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 64, 128, 128, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 64, 128, 128, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			MenageristDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(180, 56, 1, 185, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 255, 255, 255, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 200, 25, 1, 185, true, false, 128, 255, 255, 255, true),
					RadiusOffset = offsetH,
				}
			);
			RainbowDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(160, 196, 107, 255, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 196, 107, 255, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			FiendDecorator	= new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(160, 255, 163, 15, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 255, 163, 15, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			TreasureDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(200, 255, 255, 128, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 255, 255, 128, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			DevilhandDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(200, 255, 128, 255, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 0, 0, 0, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 255, 128, 255, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);
			VisionDecorator = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud) {
					BackgroundBrush = Hud.Render.CreateBrush(200, 50, 150, 250, 0),
					TextFont = Hud.Render.CreateFont("tahoma", size1, 255, 255, 255, 255, true, false, false)
				},
				new MapLabelDecorator(Hud) {
					LabelFont = Hud.Render.CreateFont("tahoma", size2, 255, 62, 158, 255, false, false, 128, 0, 0, 0, true),
					RadiusOffset = offsetH,
				}
			);

		}

		public void AfterCollect()
        {
			if (!Hud.Game.IsInGame || Hud.Game.Me.InGreaterRiftRank > 0) return;

			var goblins = Hud.Game.AliveMonsters.Where(x => x.SnoMonster.Priority == MonsterPriority.goblin);
			foreach (var goblin in goblins)
			{
				if (goblin.MaxHealth > 0)
				{
					if (MaxHealthReapp.TryGetValue(goblin.MaxHealth, out var r))
					{
						if (r == false)
						{
							MaxHealthReapp[goblin.MaxHealth] = true;
							LastTick = Hud.Game.CurrentGameTick;
							foundGoblin = true;
							var name = (goblin.SnoActor.Sno == ActorSnoEnum._p76_treasuregoblin_a_noportal && !string.IsNullOrEmpty(TextVisionGoblin))?TextVisionGoblin:goblin.SnoMonster.NameLocalized;
							if (!GoblinsDic.TryGetValue(goblin.SnoActor.Sno,out var data))
							{
								//Hud.TextLog.Log("GoblinsData",string.Format("?? \"{0}\" => A:{1} , M:{2} , \"{3}\" , \"{4}\" , P:{5} ",
								//goblin.SnoActor.Sno,(int)goblin.SnoActor.Sno,goblin.SnoMonster.Sno,goblin.SnoMonster.NameLocalized,goblin.SnoMonster.NameEnglish, goblin.SnoMonster.Priority),false,true);
								data = GoblinsDic[ActorSnoEnum._treasuregoblin_a];
							}
							else
							{
								if (data.TextSpeak == null) { data.TextSpeak = name; }
							}
							data.Amount += 1;
							if ( goblin.SnoActor.Sno != ActorSnoEnum._treasuregoblin_d_splitter_02 && goblin.SnoActor.Sno != ActorSnoEnum._treasuregoblin_d_splitter_03 )
							{
								TotalGoblins += 1;
								if (GoblinsFound.TryGetValue(name,out var c))	{ GoblinsFound[name] = c + 1; }
								else { GoblinsFound.Add(name,1); }
							}
						}
					}
					else { MaxHealthReapp.Add(goblin.MaxHealth, false);	}
				}
			}
			if (foundGoblin && EnableSpeak && Hud.Game.CurrentGameTick - LastTick > 60)
			{
				foreach(var d in GoblinsDic)
				{
					var n = d.Value.Amount;
					if (n > 0)
					{
						var msg = d.Value.TextSpeak ?? "Goblin";
						if (!string.IsNullOrEmpty(msg))
						{
							Hud.Sound.Speak( ((n > 1)? n.ToString() : "") + msg);
						}
						d.Value.Amount = 0;
					}
				}
				foundGoblin = false;
			}
		}

		public void OnMonsterKilled(IMonster monster)
		{
			if (monster.SnoMonster.Priority == MonsterPriority.goblin && monster.MaxHealth > 0)
			{
				if (MaxHealthReapp.TryGetValue(monster.MaxHealth, out var r) /*&& r*/)
				{
					var n = MaxHealthReapp.Count;
					MaxHealthReapp.Remove(monster.MaxHealth);
					//Hud.TextLog.Log("GoblinsKill",string.Format("?? \"{0}\" => A:{1} , M:{2} , \"{3}\" , \"{4}\" , P:{5} , Reapp:{6}, MaxHealth:{7}, Count:{8}",
					//monster.SnoActor.Sno,(int)monster.SnoActor.Sno,monster.SnoMonster.Sno,monster.SnoMonster.NameLocalized,monster.SnoMonster.NameEnglish, monster.SnoMonster.Priority, r, monster.MaxHealth, MaxHealthReapp.Count
					//),false,true);
				}
			}
		}

        public void PaintWorld(WorldLayer layer)
        {
			if ( (!Hud.Game.IsInGame) || (Hud.Game.Me.InGreaterRiftRank > 0) ) return;

            var portals = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._treasuregoblin_portal_open);
            foreach (var actor in portals)
            {
                PortalDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
            }

            var goblins = Hud.Game.AliveMonsters.Where(x => x.SnoMonster.Priority == MonsterPriority.goblin);
			foreach (var goblin in goblins)
			{
				if (!GoblinsDic.TryGetValue(goblin.SnoActor.Sno,out var data))
				{
					data = GoblinsDic[ActorSnoEnum._treasuregoblin_a];
				}
				(data.Decorator).Paint(layer, goblin, goblin.FloorCoordinate, (goblin.SnoActor.Sno == ActorSnoEnum._p76_treasuregoblin_a_noportal)?TextVisionGoblin:goblin.SnoMonster.NameLocalized);
			}

			if (ShowCounter)
			{
				var x = Hud.Window.Size.Width * xPor; var y = Hud.Window.Size.Height * yPor;
				widthBG = BGTexture.Width *  Hud.Window.Size.Height/1200.0f * SizeMultiplier; widthIMG = IMGTexture.Width *  Hud.Window.Size.Height/1200.0f * SizeMultiplier;
				BGTexture.Draw(x, y, widthBG, widthBG, Opacity);
				IMGTexture.Draw(x + (widthBG - widthIMG)/2, y + (widthBG - widthIMG)/2, widthIMG, widthIMG, Opacity);
				if (Hud.Window.CursorInsideRect(x, y, widthBG, widthBG))
				{
					Hud.Render.SetHint(GoblinsText());
				}
				var layout = FontText.GetTextLayout(TotalGoblins.ToString());
				FontText.DrawText(layout,x + (widthBG - layout.Metrics.Width)/2,y + (widthBG - layout.Metrics.Height)/2);
			}
		}
	}
}