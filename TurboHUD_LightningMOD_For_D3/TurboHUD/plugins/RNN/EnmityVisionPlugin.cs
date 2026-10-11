using System;
using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Turbo.Plugins.RNN
{
	public class EnmityVisionPlugin : BasePlugin, INewAreaHandler, IAfterCollectHandler, IInGameWorldPainter, IInGameTopPainter
	{
		public bool DebugEnabled { get; set; } = false;
		private uint DebugTA { get; set; } = 0;
		private Dictionary<uint,uint> Area_Waypoint_Dict { get; set; } = new Dictionary<uint,uint> // { Áreas, waypoint} recogidas de forma automática mediante el log, y añadidas por mí tras ser comprobadas (deben ser recorridas bajo ciertas condiciones)
		{
			{100854, 19943}, {445426, 19943}, {1199, 19943}, {135952, 19943}, {19940, 93632}, {82566, 93632}, {82567, 93632}, {80116, 93632}, {62726, 78572}, {83265, 72712}, {83264, 72712}, {154588, 72712}, {102300, 19952}, {165798, 19952},
			{119893, 19952}, {134002, 19952}, {82372, 19952},  {82373, 19952}, {149669, 19952},  {60397, 19953}, {60396, 19953}, {101351, 91133}, {91324, 91133}, {60714, 19780}, {19938, 91133}, {19933, 91133}, {19929, 91133},  {19789, 19787},
			{87907, 19783}, {19935, 119870}, {19825, 19835}, {148903, 19835}, {19826, 19835}, {195268, 19835}, {19839, 63666}, {464092, 456638}, {467383, 456638}, {19837, 19836}, {159588, 19836}, {220805, 19836}, {176058, 19800}, {19798, 19800},
			{80592, 19800}, {19797, 19800}, {60194, 19800}, {196047, 53834}, {123182, 53834}, {111487, 53834}, {111671, 53834}, {218968, 53834}, {432994, 62752}, {432999, 62752}, {192694, 62752}, {433000, 62752}, {433002, 62752}, {465885, 460671},
			{111516, 119306}, {139274, 119653}, {119656, 119653}, {112565, 155048}, {191078, 155048}, {221703, 155048}, {112580, 69504}, {80792, 80791}, {174666, 154644}, {221752, 154644}, {112548, 154644}, {92960, 93173}, {430336, 428494},
			{436319, 428494}, {109531, 109526}, {464858, 464857}, {464865, 464857}, {464873, 464857}, {464867, 464857}, {464868, 464857}, {464870, 464857}, {464875, 464857}, {464874, 464857}, {464871, 464857}, {409375, 409001}, {464820, 464065},
			{464810, 464065}, {464821, 464065}, {464822, 464065}, {109149, 109514}, {143648, 109514}, {446367, 409001}, {201668, 409517}, {201670, 409517}, {198564, 109538}, {215396, 109538}, {205434, 109540}, {109563, 109540}, {153669, 109540},
			{215235, 109540}, {464882, 464063}, {464890, 475854}, {464889, 475854}, {464886, 475854}, {464944, 475856}, {464943, 475856}, {464942, 475856}, {464940, 464066}, {464941, 464066},	{287220, 283567}, {245964, 258142}, {340533, 271234},
			{448404, 338602}, {367831, 338602}, {339397, 338602}, {349787, 459863}, {330576, 459863}, {432439, 427763}, {432440, 427763}, {424374, 427763}, {276705, 263493}, {430876, 263493},	{430875, 263493}, {336217, 263493}, {357577, 263493},
			{308487, 263493}, {336846, 261758}, {336847, 261758}, {314782, 261758}, {338956, 261758}, {158594, 57425}, {218511, 93103}, {446550, 109540},	{129375, 93632}, {235003, 258142}, {336577, 258142}, {448368, 338602}, {322645, 338602},
			{170118, 19836}, {19838, 19836}, {182258, 154644}, {221753, 154644}, {111232, 136448}, {197623, 69504}, {85202, 80791}, {444307, 428494}, {445762, 428494}, {182274, 154644}, {221754, 154644}, {189345, 155048}, {220904, 154644},
			{87832, 19775}, {94672, 19775}, {205424, 69504}, {169494, 57425}, {194239, 57425}, {182502, 155048}, {221761, 155048}, {338931, 261758}, {338970, 261758}, {448398, 283567}, {221702, 155048}, {220905, 154644},
			{121418, 19836}, {148905, 19835}, {62575, 57425}, {194242, 57425}, {448011, 409001}, {351802, 261758}, {62574, 57425}, {161105, 57425}, {19801, 63666}, {222577, 63666}, {134920, 93632}, {111667, 53834}, {218969, 53834}, {201671, 409517},
			{448375, 338602}, {445792, 409512}, {448411, 283567}, {179567, 19952}, {166127, 63666}, {135237, 19954}, {194232, 19954}, {90881, 19776}, {82326, 93632}, {82327, 93632},
			{257116, 119653}, {256767, 332339}, {374239, 332339}, {256106, 332339}, {256742, 332339}, // Ubers 446367 // BL {448391, 261758}, {448039, 109538}
		};

		protected IUiElement uiShowPortal;
		protected IUiElement uiCountdownLabel;
		protected IUiElement uiCountdownTimer;

		public WorldDecoratorCollection EnmityPortalLabel { get; set; }

		public bool ShowWaypoint { get; set; }

		public bool EnableSpeak { get; set; }

		public string SpeakOpenPortalUI { get; set; }
		public string SpeakOpenPortalArea { get; set; }
		public string SpeakClosingPortal { get; set; }
		public string SpeakClosedPortal { get; set; }
		public string SpeakChestFound { get; set; }
		public string SpeakGoblinFound { get; set; }
		public string SpeakFirstPortal { get; set; }
		public string SpeakNextPortal { get; set; }
		public string SpeakNextPortalOrder { get; set; }

		public string LabelFirstPortal { get; set; }
		public string LabelNextPortal { get; set; }
		public string LabelNextPortalOrder { get; set; }

		public bool EnableTextScreen { get; set; }

		public string TextScreen { get; set; }
		public string TextLocUnKnown { get; set; }
		public string TextNoPortal { get; set; }
		public string TextMoreLeveles { get; set; }
		public string TextClosing { get; set; }
		public string TextTimeFormat { get; set; }
		public string TextGoblin { get; set; }
		public string TextChest { get; set; }
		public string TextChestOpen { get; set; }
		public string TextLevelMe { get; set; }
		public string TextNoLevelMe { get; set; }

		public float OffsetX { get; set; } = 0;
		public float OffsetY { get; set; } = 0;
		public string TextLastVision { get; set; }

		public bool SoundEnabled { get; set; }
		public string FileSound { get; set; }

		private int PortalCountFound { get; set;} = 0;
		private int EnmityPortalStartTick { get; set;} = 0;
		private int EnmityPortalState { get; set;} = 0;
		private uint PortalAnnIdLast { get; set;} = 0;
		private int LevelMe { get; set;} = 0;

		private int ClosedPortalUITick { get; set;} = 0;
		private int OpenChestTick { get; set;} = 0;
		private const int maxLevels = 15;
		private const int TicksForAutoClose = 1920;			// OpenChestTick + 30 seg + 2 margen (32 * 60)
		private const int TicksForDetectNewOPenUI = 900;	// El dialog que notifica sobre una nueva visión dura 8 seg aprox, con poner 10 seg (600 ticks) deberia bastar, aunque  pongo un poco más (15seg)
		private const int ticksDelayUI = 30;				// Retrasar el anuncio de una nueva Visión mediante UI para dar tiempo a aparecer el portal de entrada cercano.
		private int EnmityPortalAct { get; set; } = -1;
		private string EnmityPortalArea { get; set; } = string.Empty;
		private uint EnmityWaypoint { get; set; } = 0;

		private delegate int CounDownTime();
		private CounDownTime CounDownTimeFunc;

 		public IFont FontTextShow { get; set; }
		public IFont FontTextHide { get; set; }
		public uint TextureId { get; set; } = 3122228893;

		private bool _EnmityPortal { get; set; } = false;
		public bool EnmityPortal
		{
			get { return _EnmityPortal; }
			set
			{
				if (_EnmityPortal != value)
				{
					_EnmityPortal = value;
					foreach(var d in EnmityAreasDicc.Keys.ToList()) { EnmityAreasDicc[d] = 0; }
					EnmityPortalsOrderDicc.Clear();
					PortalCountFound = 0;
					EnmityPortalState = 0;
					EnmityPortalAct = -1;
					EnmityPortalArea = string.Empty;
					EnmityWaypoint = 0;
					EnmityPortalStartTick = 0;
					PortalAnnIdLast = 0;
					LevelMe = 0;
				}
			}
		}

		private Dictionary<uint,int> EnmityAreasDicc { get; set; } = new Dictionary<uint,int>
		{
			{488740, 0 },	//	P76_NestingNewTristram_SeasonTheme
			{488741, 0 },	//	P76_NestingOldTristram_SeasonTheme
			{488742, 0 },	//	P76_NestingGraveyard_Act1_SeasonTheme
			{488743, 0 },	//	P76_NestingBloodPit_SeasonTheme	Normal
			{488745, 0 },	//	P76_NestingBoneyard_SeasonTheme	Normal
			{488746, 0 },	//	P76_NestingMoor_Sacrifice_SeasonTheme
			{488767, 0 },	//	P76_NestingAlcarnus_SeasonTheme
			{488770, 0 },	//	P76_NestingChurch_SeasonTheme
			{488799, 0 },	//	P76_NestingSescheron_SeasonTheme
			{488800, 0 },	//	P76_NestingForestTower_SeasonTheme
			{488870, 0 },	//	P76_NestingRam_SeasonTheme
			{488871, 0 },	//	P76_NestingSpire_SeasonTheme
			{488872, 0 },	//	P76_NestingHell_SeasonTheme
			{488873, 0 },	//	P76_NestingGideonsCourt_SeasonTheme
			{488874, 0 },	//	P76_NestingBog_SeasonTheme
		};

		private HashSet<ActorSnoEnum> EnmityPortals { get; set; } = new HashSet<ActorSnoEnum> {
			ActorSnoEnum._p76_portal_oldtristram_seasontheme,
			ActorSnoEnum._p76_portal_newtristram_seasontheme,
			ActorSnoEnum._p76_portal_graveyardact1_seasontheme,
			ActorSnoEnum._p76_portal_bloodpit_seasontheme,
			ActorSnoEnum._p76_portal_moors_sacrifice_seasontheme,
			ActorSnoEnum._p76_portal_boneyard_seasontheme,
			ActorSnoEnum._p76_portal_church_seasontheme,
			ActorSnoEnum._p76_portal_alcarnus_seasontheme,
			ActorSnoEnum._p76_portal_sescheron_seasontheme,
			ActorSnoEnum._p76_portal_foresttower_seasontheme,
			ActorSnoEnum._p76_portal_bog_seasontheme,
			ActorSnoEnum._p76_portal_gideonscourt_seasontheme,
			ActorSnoEnum._p76_portal_spire_seasontheme,
			ActorSnoEnum._p76_portal_hell_seasontheme,
			ActorSnoEnum._p76_portal_ram_seasontheme,
		};

		private Dictionary<uint,int> EnmityPortalsOrderDicc { get; set; } = new Dictionary<uint,int>();

		public EnmityVisionPlugin()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);

			ShowWaypoint = true;

			SoundEnabled = true;				// Alert sound for a new Vision
			FileSound = "notification_9.wav";	// File to be played. It must be in the Sounds\ folder

			EnableSpeak = true;	// false to disable all speak
			SpeakOpenPortalUI = "A portal opens away from you"; // null or empty not to speak, vision is detected via UI
			SpeakOpenPortalArea = "A portal opens near you";	// null or empty not to speak, vision is detected via portal. A nearby portal is typically detected before the UI notification
			SpeakClosingPortal = "The portal is closing";		// null or empty not to speak
			SpeakClosedPortal = "The Portal is closed";			// null or empty not to speak
			SpeakChestFound = "Chest of Visions";				// null or empty not to speak
			SpeakGoblinFound = "Goblin of Visions";				// null or empty not to speak
			SpeakFirstPortal = "Entrance Found";	// Speak when the entry portal is found after the vision is detected via UI
			SpeakNextPortalOrder = "Level {0}";		// Speak portals , {0} = Level number
			SpeakNextPortal  = "Next Level";		// Speak portals. Show When the number of levels cannot be set

			LabelFirstPortal = "Entrance";			// Show when the entry Portal has been found
			LabelNextPortalOrder = "Level {0}";		// {0} = Level number
			LabelNextPortal  = "Next Level";		// Show when the number of levels cannot be set

			EnableTextScreen = true;					// Show TextScreen line
			TextScreen  = "Enmity Vision: A{0} - {1} ({2} ago, {3}{4} levels, {5}). {6}"; // {0}=Act, {1}=Area NameLocalized, {2}=time, {3}=Levels, {4}=TextMoreLeveles, {5}=TextLevelMe/TextNoLevelMe, {6}=TextGoblin/TextChest/TextChestOpen/TextClosing
			TextLocUnKnown  = "\"Location unknown\"";	// Portal opened but not detected.
			TextNoPortal = "Untracked Visions.";		// No active portal.
			TextMoreLeveles = " or more";				// show if there is a possibility of more levels
			TextClosing = "Closing ... {0}s";			// show if the portal is closing. {0}=time
			TextGoblin = "Goblin found";
			TextChest = "Chest found";
			TextChestOpen = "Chest open";
			TextLevelMe = "in the {0}th";				// Shows the level you are at within the vision. {0}=level
			TextNoLevelMe = "out";						// You are not in an area of Vision

			TextTimeFormat = "{0:0}m{1:0}s";				// Format for Time since vision was detected // {0} = minutes, {1} = seconds

			OffsetX = 0.055f;	// 0f...1f Set the x coordinate for TextScreen
			OffsetY = 0.001f;	// 0f...1f Set the y coordinate for TextScreen

			EnmityPortalLabel = new WorldDecoratorCollection(
				new GroundLabelDecorator(Hud)
				{
					ForceOnScreen= false,
					BorderBrush = Hud.Render.CreateBrush(255, 193, 226, 255, 1),
					BackgroundBrush = Hud.Render.CreateBrush(220, 73, 89, 255, 0),
					TextFont = Hud.Render.CreateFont("tahoma", 7, 220, 193, 226, 255, true, false, false)
				},
				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", 5, 220, 193, 226, 255, false, false, true),
					RadiusOffset = 18,
				}
			);

			FontTextShow = Hud.Render.CreateFont("tahoma", 8f, 250, 0, 250, 0, false, false, 250, 0, 0, 0, true);
			FontTextHide = Hud.Render.CreateFont("tahoma", 8f, 50, 0, 250, 0, false, false, false);

			uiShowPortal = Hud.Render.RegisterUiElement("Root.NormalLayer.game_notify_dialog_backgroundScreen.game_text_line0", null, null);
			uiCountdownLabel = Hud.Render.RegisterUiElement("Root.NormalLayer.Countdown_main.count_down.timer_label", null, null);
			uiCountdownTimer = Hud.Render.RegisterUiElement("Root.NormalLayer.Countdown_main.count_down.timer", null, null);

			CounDownTimeFunc = () => {
										var t = 0;
										if (uiCountdownTimer?.Visible == true)
										{
											Int32.TryParse(uiCountdownTimer.ReadText(Encoding.UTF8, true), out t);
										}
										return t;
									};
		}

		public void Play_Sound(string playFile)
        {
            var playSonido = Hud.Sound.LoadSoundPlayer(playFile);
            ThreadPool.QueueUserWorkItem(state =>
            {
               try  { playSonido.PlaySync(); }
               catch (Exception)  {   }
            });
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
            {
				EnmityPortal = false;
				ClosedPortalUITick = 0;
				OpenChestTick = 0;
            }

			if (!area.IsTown && area.Act != 0 /*&& !EnmityAreasDicc.ContainsKey(area.Sno)*/) // Recopila/aprende nuevas áreas en cada sesión de TH, las iré añadiendo de forma permanente tras ser comprobadas
			{
				if (Hud.Game.ActMapWaypoints.FirstOrDefault(w => w.TargetSnoArea.Sno == Hud.Game.Me.SnoArea.Sno) != null)	DebugTA = area.Sno;
				else if (!Area_Waypoint_Dict.ContainsKey(area.Sno) && DebugTA != 0)
				{
					Area_Waypoint_Dict.Add(area.Sno, DebugTA); // Añade temporalmente al diccionario las nuevas áreas.
					if (DebugEnabled) // Serán añadidas al diccionario una vez comprobadas , las guardo en un log
					{
						Hud.TextLog.Log("EnmityAreasNEW",string.Format("{0}{2}, {3}{1}, // {4}, A{5} - {6}", "{", "}", area.Sno, DebugTA, Hud.Game.Me.WorldSno, Hud.Game.Me.Scene.SnoArea.Act, Hud.Game.Me.Scene.SnoArea.NameLocalized),false,true);
						Hud.Sound.Speak("Nueva área");
					}
				}
			}
        }

		public void AfterCollect()	// EnmityPortalState => 1- Visión detectada con Portal, 2- Vision detectada con UI, 3- Goblin Found, 4- Chest Found, 5- Chest Open, 6- Dialog UI Closing Vision
		{
			if (Hud.Game.IsLoading || Hud.Game.IsPaused || !Hud.Game.IsInGame /*|| !Hud.Game.Me.Hero.Seasonal*/) return;

			var portal = Hud.Game.Portals.FirstOrDefault(p => p.ActorAvailable && EnmityPortals.Contains(p.SnoActor.Sno)); 		// p.ActorAvailable es mecesario (en ocasiones se detectan portales adicionales falsos en ubicaciones random)
			if (portal != null && portal.AnnId != 0 && portal.Scene != null && Hud.Game.CurrentGameTick > ClosedPortalUITick)	// Solo habrá uno, no es necesario usar un bucle
			{
				if (EnmityAreasDicc.TryGetValue(portal.TargetArea.Sno, out var registered))	// el destino del portal es uno de las áreas de las visiones
				{
					if (!EnmityAreasDicc.ContainsKey(portal.Scene.SnoArea.Sno)) // es el portal de entrada (ha aparecido en un área que no coincide con ninguna de las áreas de visiones)
					{
						if (EnmityPortal && PortalAnnIdLast != 0 && portal.AnnId != PortalAnnIdLast)
						{  // Por precaución, por si la detección de cierre mediante Dialog UI (uiCountdownLabel) no funcionase correctamente. El AnnId del portal de entrada no puede coincidir con el de la anterior visión.
							EnmityPortal = false;
							registered = 0;
						}
						if (registered == 0) // portal no fue registrado anteriormente
						{
							if (EnmityPortal == false)
							{
								EnmityPortal = true;
								EnmityPortalState = 1;
								EnmityPortalStartTick = Hud.Game.CurrentGameTick;
								if (EnableSpeak)
								{
									if (!string.IsNullOrEmpty(SpeakOpenPortalArea))		Hud.Sound.Speak(SpeakOpenPortalArea);	// Detección de una nueva visión mediante un portal de entrada cercano
									else if (!string.IsNullOrEmpty(SpeakFirstPortal))	Hud.Sound.Speak(SpeakFirstPortal);		// Si no se ha definido un mensaje para la detección cercana pasará a anunciar el portal de entrada
								}
								if (SoundEnabled) Play_Sound(FileSound);
							}
							else
							{
								if (EnableSpeak && !string.IsNullOrEmpty(SpeakFirstPortal))		Hud.Sound.Speak(SpeakFirstPortal);	// Anuncia un portal de entrada encontrado instantes despues de detectar la visión mediante UI
							}
							PortalCountFound = 1;
							PortalAnnIdLast = portal.AnnId;
							EnmityAreasDicc[portal.TargetArea.Sno] = PortalCountFound;
							EnmityPortalsOrderDicc[portal.AnnId] = 0;

							EnmityPortalAct = portal.Scene.SnoArea.Act;
							EnmityPortalArea = portal.Scene.SnoArea.NameLocalized;

							if (Hud.Game.ActMapWaypoints.FirstOrDefault(w => w.TargetSnoArea.Sno == Hud.Game.Me.SnoArea.Sno) != null)
							{
								EnmityWaypoint = Hud.Game.Me.SnoArea.Sno;
							}
							else if (Area_Waypoint_Dict.TryGetValue(Hud.Game.Me.SnoArea.Sno, out var r))
							{
								EnmityWaypoint = r;
							}
							else EnmityWaypoint = 0;
						}
					}
					else
					{
						if (registered == 0)  // no hay portal registrado con el mismo área de destino
						{
							if (EnmityAreasDicc[portal.Scene.SnoArea.Sno] != 0) // sólo voy registrando portales consecutivos, en cadena (ya hubo un portal anterior con destino igual al área donde aparece éste)
							{
								PortalCountFound++;
								EnmityAreasDicc[portal.TargetArea.Sno] = PortalCountFound;
								EnmityPortalsOrderDicc[portal.AnnId] = PortalCountFound;
								if (EnableSpeak && !string.IsNullOrEmpty(SpeakNextPortalOrder))	Hud.Sound.Speak(string.Format(SpeakNextPortalOrder, PortalCountFound));
							}
							else if (!EnmityPortalsOrderDicc.ContainsKey(portal.AnnId))
							{
								EnmityPortalsOrderDicc.Add(portal.AnnId, -1);
								if (EnableSpeak && !string.IsNullOrEmpty(SpeakNextPortal))		Hud.Sound.Speak(SpeakNextPortal);
							}
						}
					}
				}
			}
			if (uiShowPortal?.Visible == true)
			{
				var text = uiShowPortal.ReadText(Encoding.UTF8, true); // Root.NormalLayer.game_notify_dialog_backgroundScreen.game_text_line0
				if (!string.IsNullOrEmpty(text) && Regex.IsMatch(text,@"(reality!|realidad!|Realität|réalité|realtà!|rzeczywistości!|realidade!|форму!|현실로|一股恨意化為了現實！)"))
				{
					if (EnmityPortal && (Hud.Game.CurrentGameTick - EnmityPortalStartTick > TicksForDetectNewOPenUI) ) EnmityPortal = false; // Por precaución, por si la detección de cierre mediante Dialog UI (uiCountdownLabel) no funcionase correctamente.
					if (EnmityPortal == false)	// El dialog notificando una nueva visión dura 8 seg aproximadamente (y el de cierre 30seg). Con poner 10 seg (600 ticks) deberia bastar, aunque pondré un poco más (15seg, o sea 900 ticks)
					{
						if (EnmityPortalStartTick == 0) EnmityPortalStartTick = Hud.Game.CurrentGameTick;
						else if (Hud.Game.CurrentGameTick - EnmityPortalStartTick > ticksDelayUI) // Retrasar durante breves instantes (30 ticks por defecto) el anuncio de una nueva Visión mediante UI para dar tiempo a aparecer un portal de entrada cercano.
						{
							EnmityPortal = true;
							EnmityPortalStartTick = Hud.Game.CurrentGameTick;
							EnmityPortalAct = -1;
							EnmityPortalState = 2;
							if (EnableSpeak && !string.IsNullOrEmpty(SpeakOpenPortalUI))	Hud.Sound.Speak(SpeakOpenPortalUI); //  Detección de una nueva visión mediante UI, con casi toda probabilidad la puerta de entrada no es detectable aún.
							if (SoundEnabled) Play_Sound(FileSound);
						}
					}
				}
			}
			if (EnmityPortal)
			{
				if (Hud.Game.Me.Scene != null)	// Para evitar posibles excepciones: Hud.Game.Me.Scene ser null, por ej cuando acabamos de cruzar un portal cualquiera.
				{
					if (EnmityAreasDicc.TryGetValue(Hud.Game.Me.Scene.SnoArea.Sno, out var mylevel))
					{
						LevelMe = mylevel;
						if (EnmityPortalState < 5) // Find Chest (Find Goblin)
						{
							var chest = Hud.Game.Actors.FirstOrDefault(c => c.SnoActor.Sno == ActorSnoEnum._p76_chest);
							if (chest != null )
							{
								if (chest.IsDisabled || chest.IsOperated)
								{
									EnmityPortalState = 5;	// Chest Open
									OpenChestTick = Hud.Game.CurrentGameTick;
								}
								else if (EnmityPortalState < 4)
								{
									EnmityPortalState = 4;	// Chest Found
									if (EnableSpeak && !string.IsNullOrEmpty(SpeakChestFound)) Hud.Sound.Speak(SpeakChestFound);
								}

							}
							else if (EnmityPortalState < 3) // Find Goblin
							{
								if (Hud.Game.AliveMonsters.Any(a => a.SnoActor.Sno == ActorSnoEnum._p76_treasuregoblin_a_noportal))
								{
									EnmityPortalState = 3;	// Goblin Found
									if (EnableSpeak && !string.IsNullOrEmpty(SpeakGoblinFound)) Hud.Sound.Speak(SpeakGoblinFound); //105371
								}
							}
						}
					}
					else LevelMe = 0;
				}
				if (EnmityPortalState == 5 && (Hud.Game.CurrentGameTick - OpenChestTick > TicksForAutoClose))	EnmityPortal = false; // Por precaución, por si la detección de cierre mediante Dialog UI no funcionase correctamente.
				if (uiCountdownLabel?.Visible == true)
				{
					if (EnmityPortalState != 6)
					{
						var text = uiCountdownLabel.ReadText(Encoding.UTF8, true); // Root.NormalLayer.Countdown_main.count_down.timer_label
						if (!string.IsNullOrEmpty(text) && Regex.IsMatch(text,@"\b(vision|visión|Visione|Wizja|visão|Видение|환영|幻象結束時間尚餘)\b", RegexOptions.IgnoreCase))
						{
							EnmityPortalState = 6;
							if (EnableSpeak && !string.IsNullOrEmpty(SpeakClosingPortal))	Hud.Sound.Speak(SpeakClosingPortal);
						}
					}
				}
				else  if (EnmityPortalState == 6)
				{
					EnmityPortal = false;
					ClosedPortalUITick =  Hud.Game.CurrentGameTick + 90;  // Tras cerrarse el dialog, el portal puede seguir detectándose durante unos instantes y así evito interpretarlo como una nueva visión. Normalmente sólo es detectable durante 2-6 ticks
					if (EnableSpeak && !string.IsNullOrEmpty(SpeakClosedPortal)) Hud.Sound.Speak(SpeakClosedPortal);
				}
			}
		}

		public void PaintTopInGame(ClipState clipState)
		{
			if (!Hud.Game.IsInGame /*|| !Hud.Game.Me.Hero.Seasonal*/) return;
			if (clipState == ClipState.AfterClip)
			{
				if (EnmityPortal && ShowWaypoint && EnmityWaypoint != 0 && Hud.Game.MapMode == MapMode.WaypointMap )
				{
					var waypoint = Hud.Game.ActMapWaypoints.FirstOrDefault(w => w.BountyAct == Hud.Game.ActMapCurrentAct && w.TargetSnoArea.Sno == EnmityWaypoint);
					if (waypoint != null)
					{
						var width = 220 * Hud.Window.HeightUiRatio;	var height = 100 * Hud.Window.HeightUiRatio;
						var x = Hud.Render.WorldMapUiElement.Rectangle.X + waypoint.CoordinateOnMapUiElement.X * Hud.Window.HeightUiRatio + width * 0.315f;
						var y = Hud.Render.WorldMapUiElement.Rectangle.Y + waypoint.CoordinateOnMapUiElement.Y * Hud.Window.HeightUiRatio + height * 0.11f;
						var texture = Hud.Texture.GetTexture(TextureId);
						texture.Draw( x, y, width * 0.09f, width * 0.11f);
					}
				}
			}
			else if (clipState == ClipState.BeforeClip)
			{
				if (EnableTextScreen)
				{
					if (EnmityPortal)
					{
						int t = (Hud.Game.CurrentGameTick - EnmityPortalStartTick)/ 60;
						string text = string.Format(TextScreen,	// {0}=Act, {1}=Area NameLocalized, {2}=time, {3}=Levels, {4}=TextMoreLeveles, {5}=TextGoblin, {6}=TextChest/TextChestOpen, {7}=TextClosing, {8}=state (for debug)
							(EnmityPortalAct < 1)? "?" : EnmityPortalAct.ToString(),
							(EnmityPortalAct < 0)? TextLocUnKnown : EnmityPortalArea,
							(EnmityPortalStartTick == 0) ? "??" : string.Format(TextTimeFormat, t/60, t%60),
							(PortalCountFound == 0)? 1 : PortalCountFound,
							(EnmityPortalState < 3 && PortalCountFound < maxLevels)? TextMoreLeveles : "",
							(LevelMe == 0)? TextNoLevelMe : string.Format(TextLevelMe, LevelMe),
							(EnmityPortalState == 3)? TextGoblin
								: EnmityPortalState == 4? TextChest
									: EnmityPortalState == 5? TextChestOpen
										: EnmityPortalState == 6? string.Format(TextClosing, CounDownTimeFunc())
											: ""
						);
						FontTextShow.DrawText(FontTextShow.GetTextLayout(text), Hud.Window.Size.Width * OffsetX, Hud.Window.Size.Height * OffsetY);
					}
					else
					{
						FontTextHide.DrawText(FontTextHide.GetTextLayout(string.Format(TextNoPortal)), Hud.Window.Size.Width * OffsetX, Hud.Window.Size.Height * OffsetY);
					}
				}
			}
		}

        public void PaintWorld(WorldLayer layer)
		{
			if (layer != WorldLayer.Ground || !Hud.Game.IsInGame /*|| !Hud.Game.Me.Hero.Seasonal*/) return;
			var portal = Hud.Game.Portals.FirstOrDefault(p => p.ActorAvailable && EnmityPortals.Contains(p.SnoActor.Sno)); // Solo habrá uno, no es necesario usar un bucle
			if (portal != null && portal.AnnId != 0)
			{
				if (EnmityPortalsOrderDicc.TryGetValue(portal.AnnId, out var order))
				{
					string labelText = (order == -1)? LabelNextPortal : ((order == 0) ? LabelFirstPortal : string.Format(LabelNextPortalOrder,order));
					EnmityPortalLabel.Paint(WorldLayer.Ground, portal, portal.FloorCoordinate, labelText);
					EnmityPortalLabel.Paint(WorldLayer.Map, portal, portal.FloorCoordinate, labelText);
				}
			}
		}
	}
}