// ShrineType.PoolOfReflection, ShrineType.HealingWell
// ShrineType.PowerPylon, ShrineType.ConduitPylon, ShrineType.ChannelingPylon, ShrineType.ShieldPylon, ShrineType.SpeedPylon
// ShrineType.BlessedShrine, ShrineType.EnlightenedShrine, ShrineType.FortuneShrine, ShrineType.FrenziedShrine, ShrineType.FleetingShrine, ShrineType.EmpoweredShrine
// ShrineType.BanditShrine

using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Turbo.Plugins.RNN
{
    public class OtherShrinePlugin : BasePlugin, IAfterCollectHandler, IInGameWorldPainter, INewAreaHandler, ICustomizer
    {
        public WorldDecoratorCollection QuestDecorator { get; set; }
        public WorldDecoratorCollection KeywardenDecorator { get; set; }
        public WorldDecoratorCollection BossDecorator { get; set; }
		public WorldDecoratorCollection OtherMarkerDecorator { get; set; }
		public WorldDecoratorCollection DefaultMarkerDecorator { get; set; }
		public bool ShowMarkers { get; set;}
		public int LineToMarkerRange { get; set; }
		public float Opacity { get; set; }

		private WorldDecoratorCollection DecoratorRectangle { get; set; }
		private WorldDecoratorCollection DecoratorRectangleGray { get; set; }
		private WorldDecoratorCollection DecoratorCircle { get; set; }
		private WorldDecoratorCollection DecoratorCircleGray { get; set; }
		private WorldDecoratorCollection DecoratorHealing { get; set; }
		private WorldDecoratorCollection DecoratorDefault { get; set; }
		private WorldDecoratorCollection LinePylon { get; set; }
		private WorldDecoratorCollection LinePool { get; set; }

		private WorldDecoratorCollection LineUnknownShrine { get; set; }
		private WorldDecoratorCollection LineUndefinedShrine { get; set; }

		private TopLabelDecorator DecoratorPylonNemesys { get; set; }
		private TopLabelDecorator DecoratorPopupON { get; set; }
		private TopLabelDecorator DecoratorPopupOFF { get; set; }

		private Dictionary<string, bool> ShrineUsedCoord { get; set; } = new Dictionary<string, bool>();
		private Dictionary<string, bool> MarkIdReapp { get; set; } = new Dictionary<string, bool>();
		private Dictionary<ShrineType, ShrineData> ShrinesData { get; set; } = new Dictionary<ShrineType, ShrineData>();

		private Dictionary<uint, ShrineType> PylonBuffType { get; set; }  = new Dictionary<uint, ShrineType>
		{
			{262935,ShrineType.PowerPylon}, {403404,ShrineType.ConduitPylon}, {266258,ShrineType.ChannelingPylon}, {266254,ShrineType.ShieldPylon}, {266271,ShrineType.SpeedPylon}, // Power, Conduit, Chann, Shield, Speed
			{278268,ShrineType.BlessedShrine}, {030476,ShrineType.BlessedShrine}, {278269,ShrineType.EnlightenedShrine}, {030477,ShrineType.EnlightenedShrine}, {263029,ShrineType.ConduitPylon}, // protección, experiencia
			{278270,ShrineType.FortuneShrine}, {030478,ShrineType.FortuneShrine}, {278271,ShrineType.FrenziedShrine}, {030479,ShrineType.FrenziedShrine}, {260348,ShrineType.FleetingShrine}, // fortuna, frenético, fugaz
			{260349,ShrineType.EmpoweredShrine}	 // potenciado
		};
		private List<ShrineType> PylonsGR { get; set; } = new List<ShrineType>()
		{
			ShrineType.PowerPylon, ShrineType.ConduitPylon, ShrineType.ChannelingPylon, ShrineType.ShieldPylon, ShrineType.SpeedPylon
		};

		private Dictionary<ActorSnoEnum, ShrineType> SnoTypeDic { get; set; } = new Dictionary<ActorSnoEnum,ShrineType>
		{
			// _shrine_global = 135384, // _shrine_global_glow /*156680*/
			{ ActorSnoEnum._poolofreflection /*373463*/, ShrineType.PoolOfReflection },
			{ ActorSnoEnum._healthwell_global /*138989*/, ShrineType.HealingWell },

			{ ActorSnoEnum._x1_lr_shrine_damage /*330695*/, ShrineType.PowerPylon },
			{ ActorSnoEnum._x1_lr_shrine_electrified /*330696*/, ShrineType.ConduitPylon },
			{ ActorSnoEnum._x1_lr_shrine_infinite_casting /*330697*/, ShrineType.ChannelingPylon },
			{ ActorSnoEnum._x1_lr_shrine_invulnerable /*330698*/, ShrineType.ShieldPylon },
			{ ActorSnoEnum._x1_lr_shrine_run_speed /*330699*/, ShrineType.SpeedPylon },

			{ ActorSnoEnum._shrine_global_blessed /*176074*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._shrine_global_enlightened /*176075*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._shrine_global_fortune /*176076*/, ShrineType.FortuneShrine },
			{ ActorSnoEnum._shrine_global_frenzied /*176077*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._shrine_global_hoarder /*260346*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._shrine_global_reloaded /*260347*/, ShrineType.EmpoweredShrine },

			{ ActorSnoEnum._a4_heaven_shrine_global_blessed /*225025*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_enlightened /*225030*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_fortune /*225027*/, ShrineType.FortuneShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_frenzied /*225028*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_hoarder /*260344*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_reloaded /*260345*/, ShrineType.EmpoweredShrine },

			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_blessed /*225261*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_enlightened /*225262*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_fortune /*225263*/, ShrineType.FortuneShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_frenzied /*225266*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_hoarder /*260342*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_reloaded /*260343*/, ShrineType.EmpoweredShrine },

			{ ActorSnoEnum._p43_ad_shrine_global_blessed /*455251*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_enlightened /*455252*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_frenzied /*455253*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_hoarder /*455254*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_reloaded /*455256*/, ShrineType.EmpoweredShrine },

			{ ActorSnoEnum._shrine_treasuregoblin /*269349*/, ShrineType.BanditShrine },
			{ ActorSnoEnum._a4_heaven_shrine_treasuregoblin /*434409*/, ShrineType.BanditShrine },
			{ ActorSnoEnum._p43_ad_shrine_treasuregoblin /*455256*/, ShrineType.BanditShrine },
			{ ActorSnoEnum._p73_shrine_treasuregoblin /*455256*/, ShrineType.BanditShrine }
		};

		private string PylonNemesys { get; set;} = string.Empty;
		private string TextoPopup { get; set; }	= string.Empty;
		private IFont TextoFont { get; set; }

		private int PGRn { get; set; }
		private int MyIndex { get; set; } = -1;
		private bool InRift { get; set; }

		public bool NotifyInTown { get; set; }
		public bool TTSViewPylon { get; set;}
		public bool TTSBuffPylon { get; set;}
		public string TTSViewPoolText { get; set;}

		public string LabelHealingWells	 { get; set; }
		public string LabelPoolOfReflection { get; set; }
		public string LabelPossiblePylon { get; set; }
		public bool LabelPylonExchange { get; set; }

		public bool HiddenPylonUsed { get; set; }
		public bool LineToPylon { get; set; }
		public float LineToPylonWidth { get; set; }
		public float LineToPylonRange { get; set; }
		public float LineToPoolRange { get; set; }
		public bool SquarePylon { get; set; }

		public bool LineDifferentToUnknownShrine { get; set; } = true;  // Draw a gray line on the minimap to unidentified Sanctuary (Sanctuary)

		public bool ShowPylonSpawn { get; set; }
		public bool ShowPopup { get; set;}

		public float xPopup { get; set;}
		public float yPopup { get; set;}
		public float FontSizePopup { get; set; }

		public bool NemesisAlert { get; set; }

		private class ShrineData
		{
			public string LabelG { get; set; }
			public string LabelM { get; set; }
			public string TTSView { get; set; }
			public string TTSBuffON { get; set; }
			public string TTSBuffOFF { get; set; }
			public string PopupBuff { get; set; }
			public bool GR { get; set; } = false;
			public bool Buff { get; set; } = false;
			public int Finish { get; set; } = 0;
			public WorldDecoratorCollection ShrineDecorator { get; set; } = null;

			public void Set(string labelg, string labelm, string ttsview  , string ttson , string ttsoff, string popup)
			{
				this.LabelG = labelg;
				this.LabelM = labelm;
				this.TTSView = ttsview;
				this.TTSBuffON = ttson;
				this.TTSBuffOFF = ttsoff;
				this.PopupBuff = popup;
			}
			public void Reset()
			{
				this.GR = false;
				this.Buff = false;
				this.Finish = 0;
			}
		}

		public class PopupPylon : IQueueItem
        {
			public bool On { get; set; }
            public string Text { get; set; }
            public DateTime QueuedOn { get; private set; }
            public TimeSpan LifeTime { get; private set; }

            public PopupPylon(bool on, string text, TimeSpan duration)
            {
				this.On = on;
                this.Text = text;
                this.LifeTime = duration;
                this.QueuedOn = DateTime.Now;
            }
        }

        public void Config(ShrineType t, string labelg, string labelm, string ttsview  , string ttson , string ttsoff, string popup)
		{
			ShrinesData[t].Set(labelg, labelm, ttsview, ttson, ttsoff, popup);
        }

		private WorldDecoratorCollection CreateShrineDecorator(float Gs = 7f, float Ms = 6f, int o = 192, int r = 255, int g = 255, int b = 55)
		{
			return  new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud)
				{
					BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
					BorderBrush = Hud.Render.CreateBrush(o, r, g, b, 1),
					TextFont = Hud.Render.CreateFont("tahoma", Gs, o, r, g, b, false, false, 128, 0, 0, 0, true),
				},
				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", Ms, o, r, g, b, false, false, 128, 0, 0, 0, true),
					RadiusOffset = 5f,
				},
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, r, g, b, 2),
					ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
					Radius = 4.0f,
					ShapePainter = new RectangleShapePainter(Hud),
				}
			);
		}

		public void ModifyShrineDecorator(ShrineType shine, float GroundFontSize = 7f, float MapFontSize = 6f, int opacity = 192, int red = 255, int green = 255, int blue = 55 )
		{
			ShrinesData[shine].ShrineDecorator = CreateShrineDecorator( GroundFontSize, MapFontSize , opacity , red , green, blue);
		}

		public OtherShrinePlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30001;

			LabelHealingWells = ""; 		// Text on the minimap for HealingWells (null for namelocalized,"" for nothing)
			LabelPoolOfReflection = "Pool"; // Text on the minimap for pools (null for namelocalized,"" for nothing)
			LabelPossiblePylon = "Pylon?";  // Text for the labels of the places where possible pylons may appear
			LabelPylonExchange = false;  	// Exchange labels on map and mini map. If it is false they will always be shown

			NotifyInTown = false;  	    // Enable or disable TTS/Popup in Town
			TTSViewPylon = true; 	 	// Notify with voice (TTS) the Pylons and Sanctuaries
			TTSBuffPylon = true;  	 	// Notify with voice (TTS) When you receive a buff (and lose) from a pylon/Sanctuary
			TTSViewPoolText = "Pool";   // Notify with voice (TTS) the pools of Reflection, will read the indicated text. Set to "" (or null) for not TTS

			SquarePylon = true;			// Show Square Yellow/Gray on pylons/shrines
			HiddenPylonUsed = false; 	// Hide or show (gray) decorators for used pylons
			LineToPylon = true;  		// Draw a yellow line on the minimap to the PoolOfReflection/Pylon/Sanctuary detected
			LineToPylonWidth = 2f;		// Line width
			LineToPylonRange = 700;		// Only draw the line if you are less than this distance
			LineToPoolRange = 0;		// Only draw the line if you are less than this distance For unlimited distance set to 0

			ShowPylonSpawn = true;		// Hide or not the decorators of the possible Pylons. In case someone uses another complement for it
			ShowPopup = true;     		// Popup when you receive (or lose) a buff from a Pylon/Sanctuary

			xPopup = 0.5f;			// 0f ... 1f  Popup Coordinate x
			yPopup = 0.81f;			// 0f ... 1f  Popup Coordinate y
			FontSizePopup = 9f;		// Size font for popup text

			Opacity = 0.75f;			// 0f..1f opacity for all textures(of pylons/shrines/pools/wells, other markers) on the map when they're not on screen

			ShowMarkers = true;			// Show markers other than Pylons/Shrines/Pools/Wells. Assign the value "false" if you want to use another plugin for this
			LineToMarkerRange = 700;	// Only draw the line if you are less than this distance

			NemesisAlert = true;

			foreach (ShrineType shrine in Enum.GetValues(typeof(ShrineType)))
			{
				string msg = shrine.ToString().Replace("Shrine","").Replace("Pylon","") ;
				ShrinesData.Add(shrine,new ShrineData());
				ShrinesData[shrine].Set(null, null, null, msg + " Active",msg + " Lost", msg);
				//ShrinesData[shrine].ShrineDecorator = CreateShrineDecorator();
			}

			TextoFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 50, true, false, false);

			DecoratorDefault = new WorldDecoratorCollection ();

			DecoratorPylonNemesys = new TopLabelDecorator(Hud)
			{
				TextFont = TextoFont,
				BackgroundTexture1 = Hud.Texture.ButtonTextureOrange,
				BackgroundTexture2 = Hud.Texture.BackgroundTextureOrange,
				BackgroundTextureOpacity1 = 1.0f,
				BackgroundTextureOpacity2 = 1.0f,
				TextFunc = () =>  PylonNemesys,
				HintFunc = () => "Players with Nemesys",
			};

			DecoratorRectangle = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud)
				{
					BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
					BorderBrush = Hud.Render.CreateBrush(192, 255, 255, 55, 1),
					TextFont = Hud.Render.CreateFont("tahoma", 7f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
				},

				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
					RadiusOffset = 5f,
				},
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 255, 255, 64, 2),
					ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
					Radius = 4.0f,
					ShapePainter = new RectangleShapePainter(Hud),
				}
			);

			DecoratorRectangleGray = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud)
				{
					BackgroundBrush = Hud.Render.CreateBrush(255, 20, 20, 20, 0),
					BorderBrush = Hud.Render.CreateBrush(192, 150, 150, 150, 1),
					TextFont = Hud.Render.CreateFont("tahoma", 7f, 200, 155, 155, 155, false, false, 128, 0, 0, 0, true),
				},
				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 155, 155, 155, false, false, 128, 0, 0, 0, true),
					RadiusOffset = 5f,
				},
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 150, 150, 150, 2),
					ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
					Radius = 4.0f,
					ShapePainter = new RectangleShapePainter(Hud),
				}
			);

			DecoratorCircle = new WorldDecoratorCollection(
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                },
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 64, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                }
			);

			DecoratorCircleGray = new WorldDecoratorCollection(
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 155, 155, 155, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                },
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 150, 150, 150, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                }
			);

			DecoratorHealing = new WorldDecoratorCollection(
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 220, 255, 64, 64, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5f,
                },
				new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 64, 64, 2),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 4.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                }
			);

            QuestDecorator = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(192, 255, 255, 55, -1),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 10.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 255, 255, 0, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 10,
                    Up = true,
                },
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(120, 255, 255, 55, -1),
                    ShapePainter = new LineFromMeShapePainter(Hud)
                }
            );

            KeywardenDecorator = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(192, 238, 130, 238, -1),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 10.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 255, 20, 255, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 10,
                    Up = true,
                },
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(120, 238, 130, 238, -1),
                    ShapePainter = new LineFromMeShapePainter(Hud)
                }
            );

            BossDecorator = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(192, 180, 0, 0, -1),
                    ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
                    Radius = 10.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 155, 0, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 10,
                    Up = true,
                },
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(120, 180, 0, 0, -1),
                    ShapePainter = new LineFromMeShapePainter(Hud)
                }
            );
			OtherMarkerDecorator = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(192, 55, 155, 255, -1),
					ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
					Radius = 10.0f,
					ShapePainter = new CircleShapePainter(Hud),
				},
				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 55, 155, 255, false, false, 128, 0, 0, 0, true),
					RadiusOffset = 10,
					Up = true,
				},
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(192, 55, 155, 255, -1),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
			DefaultMarkerDecorator = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(190, 255, 255, 255, -1),
					ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
					Radius = 10.0f,
					ShapePainter = new CircleShapePainter(Hud),
				},
				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 255, 255, 255, false, false, 128, 0, 0, 0, true),
					RadiusOffset = 10,
					Up = true,
				},
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(192, 255, 255, 255, -1),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
		}

        public void Customize()
        {
            Hud.TogglePlugin<MarkerPlugin>(false);
			LineUnknownShrine = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 150, 155, 155, LineToPylonWidth,SharpDX.Direct2D1.DashStyle.Dash),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
			LineUndefinedShrine = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 150, 155, 155, LineToPylonWidth,SharpDX.Direct2D1.DashStyle.DashDot),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
			LinePylon = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 255, 200, 50, LineToPylonWidth,SharpDX.Direct2D1.DashStyle.Dash),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
			LinePool = new WorldDecoratorCollection(
				new MapShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 255, 255, 0, LineToPylonWidth,SharpDX.Direct2D1.DashStyle.Dash),
					ShapePainter = new LineFromMeShapePainter(Hud)
				}
			);
			DecoratorPopupON = new TopLabelDecorator(Hud)
			{
				BorderBrush = Hud.Render.CreateBrush(255, 180, 147, 109, -1),
				BackgroundBrush = Hud.Render.CreateBrush(200, 10, 10, 10, 0),
				TextFont = Hud.Render.CreateFont("tahoma", FontSizePopup, 255, 0, 250, 0, true, false, false),
				TextFunc = () =>  TextoPopup
			};
			DecoratorPopupOFF = new TopLabelDecorator(Hud)
			{
				BorderBrush = Hud.Render.CreateBrush(255, 180, 147, 109, -1),
				BackgroundBrush = Hud.Render.CreateBrush(200, 10, 10, 10, 0),
				TextFont = Hud.Render.CreateFont("tahoma", FontSizePopup, 255, 250, 0, 0, true, false, false),
				TextFunc = () =>  TextoPopup
			};
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				InRift = false;
				foreach (var p in ShrinesData.Keys)	{ ShrinesData[p].Reset(); }
				MarkIdReapp.Clear(); ShrineUsedCoord.Clear();
			}
			if ((area.HostAreaSno == 288482) || (area.Sno == 288482))
			{
				if (!InRift) 		//Start Map New Rift (Nephalem o GR)
				{
					InRift = true;
					if (Hud.Game.Me.InGreaterRift) // new GR
					{
						PGRn = 0 ;
						foreach (var p in ShrinesData.Keys)	{ ShrinesData[p].Reset();  }
					}
				}
			}
		}

		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame) return;
			if (InRift && (Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492) && (q.State == QuestState.started || q.State == QuestState.completed )) == null))	{ InRift = false; }

			var actors = Hud.Game.Actors.Where(a => a.GizmoType == GizmoType.PowerUp && (a.SnoActor.Kind == ActorKind.Shrine || a.SnoActor.Kind == ActorKind.None) && a.FloorCoordinate.IsValid);
			foreach (var actor in actors) // para paliar el problema de marker.IsUsed con los santuarios conocidos y desconocidos
			{
				var sid = actor.WorldId + "|" + actor.FloorCoordinate.ToString();
				if (!ShrineUsedCoord.TryGetValue(sid, out var isUsed))
				{
					ShrineUsedCoord.Add(sid, actor.GetAttributeValue(Hud.Sno.Attributes.Gizmo_State, 1048575) == 1);
				}
				else if (!isUsed && actor.GetAttributeValue(Hud.Sno.Attributes.Gizmo_State, 1048575) == 1)
				{
					ShrineUsedCoord[sid] = true;
				}
			}
			var markers = Hud.Game.Markers.Where(s => s.FloorCoordinate.IsValid && (s.IsPylon || s.IsShrine || s.IsPoolOfReflection) && s.SnoActor != null);
			foreach (var marker in markers)
			{
				if (MarkIdReapp.TryGetValue(marker.Id, out var r))
				{
					if (r == false)
					{						
						ActorSnoEnum fixActor = (marker.IsShrine && Hud.Game.Me.SnoArea.Sno == 486520)? ActorSnoEnum._p73_shrine_treasuregoblin: marker.SnoActor.Sno; //  echoing Nightmwares => _shrine_global is bandit, for early detection
						if (SnoTypeDic.TryGetValue(fixActor, out var stype))
						{
							MarkIdReapp[marker.Id] = true;
							var used = marker.IsShrine? (ShrineUsedCoord.TryGetValue(marker.WorldId + "|" + marker.FloorCoordinate.ToString(),out var isUsed)?isUsed:false) : marker.IsUsed; // marker.IsUsed puede dar un valor erróneo cuando marker es un sanc
							if (!used)
							{
								string shrineName = string.Empty;
								if (marker.IsPoolOfReflection)
								{
									shrineName = TTSViewPoolText ?? string.Empty;
								}
								else
								{
									if ( PylonsGR.Contains(stype) && !ShrinesData[stype].GR ) // marker.IsPylon
									{
										ShrinesData[stype].GR = true;
										PGRn++;
									}
									shrineName = (TTSViewPylon)? ShrinesData[stype].TTSView: string.Empty;
								}
								if (shrineName != string.Empty)
								{
									Hud.Sound.Speak( shrineName ?? marker.SnoActor.NameLocalized ); // if == null localized name  , if == "" (string.Empty) no TTS
								}
							}
						}
					}
				}
				else
				{
					MarkIdReapp.Add(marker.Id, false);
				}
			}
			foreach (uint buff in PylonBuffType.Keys)
			{
				if (Hud.Game.Me.Powers.GetBuff(buff) == null) continue;
				var stype = PylonBuffType[buff];
				if ( Hud.Game.Me.Powers.BuffIsActive(buff,0)  )
				{
					var bfinish = Hud.Game.CurrentGameTick + (int) (60 * Hud.Game.Me.Powers.GetBuff(buff).TimeLeftSeconds[0]);
					if ( !ShrinesData[stype].Buff || (bfinish > (ShrinesData[stype].Finish + 60)) )
					{
						if (Hud.Game.Me.Powers.GetBuff(buff).TimeLeftSeconds[0] == 0) { continue; } // Patch Posible bug. Recibes bufo y TimeLeftSeconds es 0, ¿no inicializado aún?
						ShrinesData[stype].Buff = true;
						ShrinesData[stype].Finish = bfinish;
						if (Hud.Game.Me.Powers.GetBuff(buff).TimeElapsedSeconds[0] < 1)
						{
							var msg = ShrinesData[stype].TTSBuffON;
							if (TTSBuffPylon && !string.IsNullOrEmpty(msg)) 	// null or "" -> no TTS
							{
								if (NotifyInTown || !Hud.Game.IsInTown) { Hud.Sound.Speak(msg);	}
							}
							msg = ShrinesData[stype].PopupBuff;
							if (ShowPopup && !string.IsNullOrEmpty(msg))  // null or "" -> no popup
							{
								if (NotifyInTown || !Hud.Game.IsInTown) { Hud.Queue.AddItem( new PopupPylon(true, msg, new TimeSpan(0, 0, 0, 0, 3000)) ); }
							}
						}
						if ( PylonsGR.Contains(stype) && !ShrinesData[stype].GR )
						{
							PGRn++;
							ShrinesData[stype].GR = true;
						}
					}
				}
				else if (ShrinesData[stype].Buff)
				{
					var t = Hud.Game.Me.Powers.GetBuff(buff).LastActive.ElapsedMilliseconds;
					if ( (Hud.Game.CurrentGameTick > ShrinesData[stype].Finish) || ((t > 200) && (t < 1000)) )  // Patch, erratic behavior BuffIsActive/LastActiveIwatch
					{
						ShrinesData[stype].Buff = false;
						ShrinesData[stype].Finish = Hud.Game.CurrentGameTick - 1;
						var msg = ShrinesData[stype].TTSBuffOFF;
						if (TTSBuffPylon && !string.IsNullOrEmpty(msg))
						{
							if (NotifyInTown || !Hud.Game.IsInTown)		{ Hud.Sound.Speak(msg);	}
						}
						msg = ShrinesData[stype].PopupBuff;
						if (ShowPopup && !string.IsNullOrEmpty(msg))
						{
							if (NotifyInTown || !Hud.Game.IsInTown)		{ Hud.Queue.AddItem(new PopupPylon(false, msg, new TimeSpan(0, 0, 0, 0, 3000))); }
						}
					}
				}
			}
		}

        public void PaintWorld(WorldLayer layer)
        {
			if (!Hud.Game.IsInGame) return;
			if (layer != WorldLayer.Ground) return;
			if (!Hud.Game.IsInTown)
			{
				var markers = Hud.Game.Markers.Where(s => s.FloorCoordinate.IsValid && s.SnoActor != null);
				foreach (var marker in markers)
				{
					Hud.Render.GetMinimapCoordinates(marker.FloorCoordinate.X, marker.FloorCoordinate.Y, out var mapX, out var mapY);
					if (Hud.SceneReveal.MinimapClip && Hud.Render.MinimapUiElement.Visible && !Hud.Render.MinimapUiElement.Rectangle.Contains(mapX,mapY)) continue;
					var texture = Hud.Texture.GetTexture(marker.TextureSno, marker.TextureFrameIndex);
					if (texture != null)
					{
						var width = texture.Width * Hud.Render.MinimapScale; 	// Radius = 1
						var height = texture.Height * Hud.Render.MinimapScale;	//
						texture.Draw(mapX - (width / 2), mapY - (height / 2), width, height, marker.FloorCoordinate.IsOnScreen() ? 1f : Opacity); // Podría también hacer depender la opacidad de "used"
					}
					if (marker.IsHealingWell)
					{
						if (!marker.IsUsed)
						{
							DecoratorHealing.ToggleDecorators<MapShapeDecorator>(texture == null);
							DecoratorHealing.Paint(WorldLayer.Map, null, marker.FloorCoordinate, LabelHealingWells ?? marker.SnoActor.NameLocalized);
						}
					}
					else if (marker.IsPoolOfReflection)
					{
						if (!marker.IsUsed)
						{
							if (LineToPylon && (LineToPoolRange == 0 || marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < LineToPoolRange))
							{
								LinePool.Paint(WorldLayer.Map, null, marker.FloorCoordinate, "");
							}
							DecoratorCircle.ToggleDecorators<MapShapeDecorator>(texture == null);
							DecoratorCircle.Paint(WorldLayer.Map, null, marker.FloorCoordinate, LabelPoolOfReflection ?? marker.SnoActor.NameLocalized);
						}
					}
					else if (marker.IsPylon || marker.IsShrine)
					{
						var used = marker.IsPylon? marker.IsUsed : (ShrineUsedCoord.TryGetValue(marker.WorldId + "|" + marker.FloorCoordinate.ToString(),out var isUsed)?isUsed:false);  // marker.IsUsed puede dar un valor erróneo cuando marker es un sanc
						if (!used)
						{
							if (Hud.Game.Me.SnoArea.Sno != 486520)	// Desactivo líneas y némesis para las pesadillas resonadoras.
							{
								if (NemesisAlert && marker.FloorCoordinate.IsOnScreen() && !Hud.Game.Me.Powers.BuffIsActive(318820))
								{
									var players = Hud.Game.Players.Where(p => p.Powers.BuffIsActive(318820) );
									PylonNemesys = string.Empty;
									foreach (var player in players)
									{
										PylonNemesys += ((PylonNemesys == string.Empty)? "":"\n") + player.BattleTagAbovePortrait ;
									}
									var l = TextoFont.GetTextLayout(PylonNemesys) ; var wi = l.Metrics.Width * 1.5f ; var he = l.Metrics.Height * 1.4f;
									DecoratorPylonNemesys.Paint(marker.FloorCoordinate.ToScreenCoordinate().X - wi/2 , marker.FloorCoordinate.ToScreenCoordinate().Y - he - 12, wi, he, HorizontalAlign.Center);
								}
								if (LineToPylon && (marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < LineToPylonRange))
								{
									if (marker.IsShrine && LineDifferentToUnknownShrine && !ShrineUsedCoord.ContainsKey(marker.WorldId + "|" + marker.FloorCoordinate.ToString()))
									{
										if (marker.SnoActor.Kind != ActorKind.Shrine) LineUndefinedShrine.Paint(WorldLayer.Map, null, marker.FloorCoordinate, "");
										else LineUnknownShrine.Paint(WorldLayer.Map, null, marker.FloorCoordinate, "");
									}
									else LinePylon.Paint(WorldLayer.Map, null, marker.FloorCoordinate, "");
								}
							}
						}
						else if (HiddenPylonUsed) continue;
						ActorSnoEnum fixActor = (marker.IsShrine && Hud.Game.Me.SnoArea.Sno == 486520)? ActorSnoEnum._p73_shrine_treasuregoblin: marker.SnoActor.Sno;
						if ( SnoTypeDic.TryGetValue(fixActor, out var stype) )
						{
							bool cerca = (marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < 35)? true: false;
							DecoratorDefault = used ? DecoratorRectangleGray : (ShrinesData[stype].ShrineDecorator ?? DecoratorRectangle) ;
							if ((!LabelPylonExchange && marker.FloorCoordinate.IsOnScreen()) || cerca) //Mostrar GroundLabelDecorator cuando estás cerca
							{
								DecoratorDefault.Paint(WorldLayer.Ground, null, marker.FloorCoordinate, (ShrinesData[stype].LabelG)?? marker.SnoActor.NameLocalized);
							}
							if (!LabelPylonExchange || !cerca) // Mostrar MapLabelDecorator cuando no estás cerca
							{
								DecoratorDefault.ToggleDecorators<MapShapeDecorator>(SquarePylon);
								DecoratorDefault.ToggleDecorators<MapLabelDecorator>(true);
								DecoratorDefault.Paint(WorldLayer.Map, null, marker.FloorCoordinate, (ShrinesData[stype].LabelM)?? marker.SnoActor.NameLocalized);
							}
						}
					}
					else if (ShowMarkers)
					{
						if (marker.SnoQuest != null)								{ DecoratorDefault = QuestDecorator;			}
						else if (marker.SnoActor != null)
						{
							if (marker.SnoActor.Code.Contains("_Boss_")) 			{ DecoratorDefault = BossDecorator;				}
							else if (marker.SnoActor.Code.EndsWith("_Unique_Uber")) { DecoratorDefault = KeywardenDecorator;		}
							else 													{ DecoratorDefault = OtherMarkerDecorator;		}
						}
						else 														{ DecoratorDefault = DefaultMarkerDecorator;	}

						DecoratorDefault.Decorators[2].Enabled = (marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) > LineToMarkerRange)?false:true;
						DecoratorDefault.Paint(WorldLayer.Map, null, marker.FloorCoordinate, marker.Name);
					}
				}
				if (ShowPylonSpawn)
				{
					var riftPylonSpawnPoints = Hud.Game.Actors.Where(s => s.SnoActor.Sno == ActorSnoEnum._markerlocation_tieredriftpylon);
					foreach (var actor in riftPylonSpawnPoints)
					{
						if ((Hud.Game.RiftPercentage < 100) && (PGRn < 4))
						{
							DecoratorDefault = DecoratorRectangle;  		//DecoratorDefault = DecoratorRectangle; (for rectangle) or DecoratorDefault = DecoratorCircle; (for circle)
							DecoratorDefault.ToggleDecorators<MapLabelDecorator>(true);
						}
						else
						{
							DecoratorDefault = DecoratorRectangleGray; 		//DecoratorDefault = DecoratorRectangleGray; (for rectangle) or DecoratorDefault = DecoratorCircleGray; (for circle)
							DecoratorDefault.ToggleDecorators<MapLabelDecorator>(false); // replace HayPylon with true for show always the label
						}
						DecoratorDefault.ToggleDecorators<MapShapeDecorator>(true);
						DecoratorDefault.Paint(WorldLayer.Map, actor, actor.FloorCoordinate, LabelPossiblePylon );
					}
				}
			}
			else if (!NotifyInTown) return;
			float h = FontSizePopup + Hud.Window.Size.Height * 0.04f;
			float y = Hud.Window.Size.Height * yPopup - h / 2;
			foreach (PopupPylon p in Hud.Queue.GetItems<PopupPylon>().Take(7))
			{
				TextoPopup = p.Text;
				float w = p.Text.Length*FontSizePopup + Hud.Window.Size.Width * 0.04f;
				float x = Hud.Window.Size.Width * xPopup - w / 2;
				if (p.On) { DecoratorPopupON.Paint(x, y , w, h, HorizontalAlign.Center); }
				else { DecoratorPopupOFF.Paint(x, y , w, h, HorizontalAlign.Center); }
				if (y < Hud.Window.Size.Height/2) y += h;
				else y -= h;
			}
		}
	}
}