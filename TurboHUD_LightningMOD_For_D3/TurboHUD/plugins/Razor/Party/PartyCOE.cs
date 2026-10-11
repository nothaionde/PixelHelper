/*

This plugin is an enhanced version of TurboHUD's default ConventionOfElementsBuffListPlugin.
Movable, resizable, can select more than one element for countdown (highest element bonus matching equipped skills is selected by default), toggleable audio countdown cues, multiple display modes, countdown borders and boxy countdown clock face visual style :)

Changelog
- February 5, 2024
	- attempt to fix COE snapping to the top left corner of the screen (edge case when TH hasn't fully populated player data)
- January 31, 2024
	- fixed coe still displaying after player took off coe within detection range
- January 27, 2024
	- disable autoselect element if Stone of Jordan is equipped
- January 26, 2024
	- now limits auto element selection to yourself (TH doesn't consistenly provide elemental damage values for other players)
	- fixed manual element selections resetting frequently
- January 22, 2024
	- fix custom selection getting reset after every rift/randomly
- January 20, 2024
	- added option to disable auto selection of highest element (AutoSelectHighestElement = true)
	- changed element auto selection algorithm to prioritize the highest elemental damage bonus
- February 20, 2022
	- fixed scaling of COE icons for non-monk classes (reverted an experimental change)
	- adjusted the best-element selection algorithm to not automatically select an element when there is no elemental damage bonus and there is a tie on skill element tallies
- February 17, 2022
	- converted my mouse click detection and blocking implementation to use the new TurboHUD functions
- February 15, 2022
	- attempt to fix bugged positioning behavior when starting a new game
- February 3, 2022
	- fixed NPE in the new COE data checking logic in AfterCollect
- February 2, 2022
	- attempt to fix the party desync that happens sometimes, especially when party members change
- December 13, 2021
	- changed the PlaySounds property from a bool to a function that returns a bool to make it possible to customize the conditions under which sounds are played
	- removed the PlaySoundsInTown property
- July 30, 2021
	- optimized clock face drawing
	- smoother corners for the countdown border rendering
- July 14, 2021
	- implemented mouse click supression for selecting elements to announce
	- fixed disposal of data and MovableArea for players who leave the game
- May 6, 2021
	- fixed COE buff displaying before it becomes active after getting equipped
	- fixed toggle voice while hovering over the movable area conflicting with toggle voice off while not hovering over any player portraits or panels
- May 5, 2021
	- fixed COE buff still showing after being deactivated
	- added the ability to toggle voice off even when not hovering over any player portraits or movable CoE panels
- April 28, 2021
	- added ScaleFontOnResize option
	- added UI obstruction checking to the tooltip hint and Voice tracking toggle hotkey when hovering over a player portrait
	- fixed Voice toggle when hovering over a player portrait
	- moved starting position to approximately where the default plugin would draw CoE
	- fixed speaker icon positioning formula
- April 21, 2021
	- added DisplayMode.ActiveSelectedOnly option
	- restructured the execution logic so the plugin doesn't have to repeat code for each display mode
- April 18, 2021
	- fixed visible flickering at element transitions (switched from checking IBuff.TimeElapsedSeconds to IBuff.TimeLeftSeconds)
	- added two display modes (DisplayMode.All and DisplayMode.SelectedOnly) that can be toggled with a hotkey
- April 15, 2021
	- initial version

*/

namespace Turbo.Plugins.Razor.Party
{
	using SharpDX;
	using SharpDX.Direct2D1; //FigureBegin, FigureEnd
	using SharpDX.DirectInput; //Key
	using SharpDX.DirectWrite; //TextLayout, TextRange
	using System; //Math
	using System.Collections.Generic;
	using System.Linq;
	using System.Media;
	using System.Threading;

	using Turbo.Plugins.Default;
	//using Turbo.Plugins.Razor.Click;
	using Turbo.Plugins.Razor.Hotkey;
	using Turbo.Plugins.Razor.Movable;
 
    public class PartyCOE : BasePlugin, IAfterCollectHandler/*, INewAreaHandler*/, IHotkeyEventHandler, IMovable, IMovableKeyEventHandler, IMovableLeftClickHandler, ICustomizer, IMouseClickBlocker //, ILeftBlockHandler
    {
		public bool AutoAnnounceSelf { get; set; } = true; //if no player is selected, announce your own coe by default (this is modified by the script whenever you select or deselect yourself to be announced, so setting it manually only affects its initial behavior)
		public bool AutoSelectHighestElement { get; set; } = false;

		public enum DisplayMode { All, SelectedOnly, ActiveSelectedOnly }
		public DisplayMode Mode { get; set; } = DisplayMode.All;

		public bool DisableDefaultPlugin { get; set; } = true;
		public IKeyEvent ToggleAnnounceHotkey { get; set; } //voice tracking toggle
		public IKeyEvent ToggleDisplayModeHotkey { get; set; }
		public bool ScaleFontOnResize { get; set; } = true;

		public Func<IController, IPlayer, bool> PlaySounds { get; set; } = (Hud, AnnouncedPlayer) => !Hud.Game.IsInTown && (AnnouncedPlayer.IsMe || !AnnouncedPlayer.IsInTown); //a function that returns true or false on whether or not to play sounds //to announce only in a greater rift: Hud.Game.SpecialArea == SpecialArea.GreaterRift
		public bool UseTextToSpeech { get; set; } = false; //this can get desynced - it is more reliable to invoke sound files

		public string[] TTSCount { get; set; } = new string[]
		{
			"1",
			"2",
			"3",
		};
		public string[] TTSElement = new string[]
		{
			null, //offset
			"Arcane",
			"Cold",
			"Fire",
			"Holy",
			"Lightning",
			"Physical",
			"Poison"
		};
		public IBrush[] ElementBrushes { get; set; }
		public SoundPlayer[] SoundCount { get; set; }
		public SoundPlayer[] SoundElement { get; set; }
		public float[] SoundTriggerTime { get; set; } = new float[] //seconds before element active to trigger sound alert
		{
			0.1f, //PlayedElement
			1.3f, //Played1
			2.4f, //Played2
			3.4f, //Played3
		};
		public uint AnnouncePlayer { 
			get { return _announceHeroId; }
			set {
				_announceHeroId = value;
				Played1 = false;
				Played2 = false;
				Played3 = false;
				PlayedElement = false;
			}
		}// = 0; //this is the currently selected player to be announced
		
		public float IconSizeMultiplier { get; set; } = 1f; //0.603f; //starting value (0.55f is the SizeMultiplier value in Default\BuffLists\ConventionOfElementsBuffListPlugin.cs)
		public float NotSelectedSizeMultiplier { get; set; } = 0.7f; //multiplier to whatever the icon size is currently set as
		public float DarkenNotActiveOpacity { get; set; } = 0.4f; //set to 0 if you don't want any icons to be darkened
		public float IconOpacity { get; set; } = 0.6f; //starting value (0.55f is the SizeMultiplier value in Default\BuffLists\ConventionOfElementsBuffListPlugin.cs)
		public float IconSpacing { get; set; } = 0f; //starting value (0.55f is the SizeMultiplier value in Default\BuffLists\ConventionOfElementsBuffListPlugin.cs)
		public Dictionary<uint, int> ConventionCycleStartTick { get; private set; } = new Dictionary<uint, int>();
		public Dictionary<HeroClass, int[]> ConventionCycle = new Dictionary<HeroClass, int[]>() 
		{
			{ HeroClass.Barbarian, new int[] { 2, 3, 5, 6 } }, //: if (i == 1 || i == 4 || i == 7) continue; break;
			{ HeroClass.Crusader, new int[] { 3, 4, 5, 6 } }, //: if (i == 1 || i == 2 || i == 7) continue; break;
			{ HeroClass.DemonHunter, new int[] { 2, 3, 5, 6 } }, // if (i == 1 || i == 4 || i == 7) continue; break;
			{ HeroClass.Monk, new int[] { 2, 3, 4, 5, 6 } }, //: if (i == 1 || i == 7) continue; break;
			{ HeroClass.Necromancer, new int[] { 2, 6, 7 } }, //: if (i == 1 || i == 3 || i == 4 || i == 5) continue; break;
			{ HeroClass.WitchDoctor, new int[] { 2, 3, 6, 7 } }, //: if (i == 1 || i == 4 || i == 5) continue; break;
			{ HeroClass.Wizard, new int[] { 1, 2, 3, 5 } }, //: if (i == 4 || i == 6 || i == 7) continue; break;				
		};

		public IBrush TimeLeftClockBrush { get; set; }
		public float ClockBorderWidth { get; set; } = 3f;
		//public IBrush InactiveBrush { get; set; }
		//public IBrush ActiveBrush { get; set; }
		public IFont TimeLeftFont { get; set; }
		
		public IFont IconFont { get; set; }
		public string IconSymbol { get; set; } = "🔊";
		public string TextHintIcon { get; set; }
		
		public class COEInfo
		{
			public bool Active { get; set; } = true;
			public uint HeroId { get; set; }
			public HeroClass HeroClass { get; set; }
			public int ConventionCycleStartTick { get; set; } = -1;
			public int HighestElement { get; set; }
			public List<int> CustomElements { get; set; } = new List<int>();
			public MovableArea Display { get; set; }
			public int LastActiveTick { get; set; }
			public string Debug { get; set; }
		}
		private Dictionary<uint, COEInfo> Data = new Dictionary<uint, COEInfo>();
		
		//ILeftBlockHandler
		//public Func<bool> LeftClickBlockCondition { get; set; }
		
		//private Dictionary<uint, MovableArea> Displays = new Dictionary<uint, MovableArea>();
		//private Dictionary<uint, int> HighestElement = new Dictionary<uint, List<int>>();
		//private Dictionary<uint, int> HighestElement = new Dictionary<uint, List<int>>();
		private int AllowedInactiveTicks = 20; //number of ticks (60 ticks = 1 second) before deleting inactive coe entry
		//private System.Drawing.RectangleF SnapToPortraitArea;
		private MovableController Mover;
		//private UIOverlapHelper Overlay;
		private float IconWidth;
		private float IconHeight;
		private uint HoveredPlayer;
		private int HoveredElement;
		private uint _announceHeroId = 0;
		private bool Played3;
		private bool Played2;
		private bool Played1;
		private bool PlayedElement;
		private float FontRatio; //calculated dynamically
		private int[] TranslateIndexToOffense = new int[] {
			//0 - physical
			//1 - fire
			//2 - lightning
			//3 - cold
			//4 - poison
			//5 - arcane
			//6 - holy
			-1, //nothing
			5, //coe 1 (arcane) = IPlayer.Offense.ElementalDamageBonus 5
			3, //coe 2 (cold) = IPlayer.Offense.ElementalDamageBonus 3
			1, //coe 3 (fire) = IPlayer.Offense.ElementalDamageBonus 1
			6, //coe 4 (holy) = IPlayer.Offense.ElementalDamageBonus 6
			2, //coe 5 (lightning) = IPlayer.Offense.ElementalDamageBonus 2
			0, //coe 6 (physical) = IPlayer.Offense.ElementalDamageBonus 0
			4, //coe 7 (poison) = IPlayer.Offense.ElementalDamageBonus 4
		};
		
        public PartyCOE()
        {
            Enabled = true;
        }
 
        public override void Load(IController hud)
        {
            base.Load(hud);
			
			ToggleAnnounceHotkey = Hud.Input.CreateKeyEvent(true, Key.V, true, false, false); //Ctrl + V
			ToggleDisplayModeHotkey = Hud.Input.CreateKeyEvent(true, Key.Grave, false, false, false); //V
			
			//ShadowBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0);
			TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);
			TimeLeftFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
			//InactiveBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 3);
			//ActiveBrush = Hud.Render.CreateBrush(255, 0, 255, 0, 3);
			
			IconFont = Hud.Render.CreateFont("tahoma", 9, 255, 0, 255, 25, false, false, 175, 0, 0, 0, true);
			
			ElementBrushes = new IBrush[]
			{
				Hud.Render.CreateBrush(255, 255, 0, 0, 3), //not selected
				Hud.Render.CreateBrush(255, 249, 0, 161, 3), //Arcane
				Hud.Render.CreateBrush(255, 150, 199, 246, 3), // Cold
				Hud.Render.CreateBrush(255, 255, 90, 8, 3), // Fire
				Hud.Render.CreateBrush(255, 255, 239, 90, 3), // Holy
				Hud.Render.CreateBrush(255, 81, 40, 255, 3), //51, 51, 255, 3), //247, 247, 247, 3), // Lightning
				Hud.Render.CreateBrush(255, 155, 146, 113, 3), // Physical
				Hud.Render.CreateBrush(255, 64, 142, 16, 3), // Poison
			};
			
			if (PlaySounds is object)
			{
				//Played4 = false;
				Played3 = false;
				Played2 = false;
				Played1 = false;
				PlayedElement = false;

				if (!UseTextToSpeech)
				{
					SoundCount = new SoundPlayer[] {
						Hud.Sound.LoadSoundPlayer("1.wav"),
						Hud.Sound.LoadSoundPlayer("2.wav"),
						Hud.Sound.LoadSoundPlayer("3.wav")
					};
					
					SoundElement = new SoundPlayer[] {
						null,
						Hud.Sound.LoadSoundPlayer("Arcane.wav"),
						Hud.Sound.LoadSoundPlayer("Cold.wav"),
						Hud.Sound.LoadSoundPlayer("Fire.wav"),
						Hud.Sound.LoadSoundPlayer("Holy.wav"),
						Hud.Sound.LoadSoundPlayer("Lightning.wav"),
						Hud.Sound.LoadSoundPlayer("Physical.wav"),
						Hud.Sound.LoadSoundPlayer("Poison.wav")
					};
				}
				
				//Hud.Sound.VolumeMultiplier = 2d;				
//				TextHintIcon = ToggleAnnounceHotkey.ToString() + " to toggle sound alerts";
				TextHintIcon = ToggleAnnounceHotkey.ToString() + " для переключения звуковых оповещений";				
			}
			
			//LeftClickBlockCondition = () => HoveredElement > 0;
        }
		
		public void Customize()
		{
			if (DisableDefaultPlugin)
				Hud.TogglePlugin<ConventionOfElementsBuffListPlugin>(false);
		}
		
		/*public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				if (Mover is object)
				{
					foreach (KeyValuePair<uint, COEInfo> pair in Data)
					{
						var area = pair.Display;
						if (area is object)
							Mover.DeleteArea(area); //
					}
					
					Data.Clear();
				}
			}
		}*/
		
		public void AfterCollect()
		{
			if (!Hud.Window.IsForeground || !Hud.Game.IsInGame || Hud.Game.IsLoading || Mover == null)
				return;
			
			//update data if players are in range
			foreach (IPlayer player in Hud.Game.Players.Where(p => p.HasValidActor && p.CoordinateKnown && Hud.Game.Me.SnoArea.Sno == p.SnoArea.Sno)) //.Where(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno)) // && Hud.Game.Me.SnoArea.Sno == p.SnoArea.Sno
			{
				if (Data.ContainsKey(player.HeroId))
				{
					if (!player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 0)) //if you don't specify index 0, it will still return true for a little while after unequipping COE
					{
						//test
						//Hud.Sound.Speak("Deleting COE Data");
						var info = Data[player.HeroId];
						if (info.LastActiveTick < Hud.Game.CurrentGameTick && Hud.Game.CurrentGameTick - info.LastActiveTick > 60)
						{
							var area = info.Display;
							
							//remove nonexistant player info from the list
							Data.Remove(player.HeroId);
							
							//request deletion of the area
							if (area is object)
								Mover.DeleteArea(area);
							
							continue;
						}
					}
				}
				else if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.ConventionOfElements.Sno, 0) && !string.IsNullOrEmpty(player.BattleTagAbovePortrait)) //if (!Data.ContainsKey(player.HeroId))
				{
					//Hud.Sound.Speak("Add");
					//create new MovableArea for this
					var rect = player.PortraitUiElement.Rectangle;
					var pCycle = ConventionCycle[player.HeroClassDefinition.HeroClass];

					Data.Add(player.HeroId, new COEInfo()
					{
						HeroId = player.HeroId,
						HeroClass = player.HeroClassDefinition.HeroClass,
						//CustomElements = new List<int>(),
						Display = Mover.CreateArea(
							this,
							player.BattleTagAbovePortrait + "_" + player.HeroId, //"Position" + (i+1), //area name
							new System.Drawing.RectangleF(rect.Right, rect.Y, IconWidth*pCycle.Length + IconSpacing*(pCycle.Length-1), IconHeight),
							//new System.Drawing.RectangleF(rect.Right, rect.Y + rect.Height*0.48f, IconWidth*5 + IconSpacing*4, IconHeight), 
							true, //enabled at start?
							true, //true, //save to config file?
							ResizeMode.FixedRatio //resizable?
						),
					});
				}
				else
					continue;
				
				//if (player.HasValidActor && player.CoordinateKnown)
				{
					IBuff coe = player.Powers.GetBuff(Hud.Sno.SnoPowers.ConventionOfElements.Sno);
					//if (coe.IconCounts[0] == 1) //coe is object && 
					//{
						//how far from the beginning of the current coe cycle are we?
						double timeElapsed = 0;
						bool isAnyActive = false;
						//string debug = string.Empty;
						foreach (int index in ConventionCycle[player.HeroClassDefinition.HeroClass])
						{
							//debug += "[" + index + ":" + coe.TimeElapsedSeconds[index] + ":" + coe.TimeLeftSeconds[index] + "]";
							if (coe.TimeLeftSeconds[index] != 0)
							{
								double elapsed = 4 - Math.Min(4, coe.TimeLeftSeconds[index]);
								if (elapsed > 0)
									timeElapsed += elapsed;
								
								isAnyActive = true;
								
								break;
							}
							
							timeElapsed += 4;
						}
					//}
					
					//ignore weird data
					if (!isAnyActive || timeElapsed >= 16)
						continue;
					
					//update info
					COEInfo info = Data[player.HeroId];
					if (!info.Active)
					{
						//Hud.Sound.Speak("On");
						info.Active = true;
						info.Display.Enabled = true;
					}
					info.LastActiveTick = Hud.Game.CurrentGameTick;
					info.ConventionCycleStartTick = Hud.Game.CurrentGameTick - (int)(timeElapsed * 60); //LastConventionCycleSeen
					
					//Hud.TextLog.Log("_coe", string.Format("{0} = {1} - ({2} * 60)", info.ConventionCycleStartTick, Hud.Game.CurrentGameTick, timeElapsed), false, true);
					//info.Debug = debug + " - " + timeElapsed + "s"; //Hud.Game.CurrentGameTick.ToString() + " - " + timeElapsed + "s";
					//if (!info.Display.Enabled)
					//	info.Display.Enabled = true;
					
					//auto select the highest bonuse(s) to highlight + some additional conflict resolution
					if (info.CustomElements.Count > 0) //Any()
						info.HighestElement = -1;
					else if (player.IsMe && AutoSelectHighestElement && !player.Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneOfJordan.Sno))
					{
						int[] cycle = ConventionCycle[info.HeroClass];
						//Dictionary<int, int> highest = new Dictionary<int, int>();
						//int maybeAnnounced = -1;
						//int highestElement = 0;
						info.HighestElement = -1;
						
						if (player.Offense.HighestElementalDamageBonus == 0)
						{
							var highestWpnDmgSkill = player.Powers.UsedSkills.Where(s => s.WeaponDamageMultiplier > 0).OrderByDescending(s => s.WeaponDamageMultiplier).FirstOrDefault();
							if (highestWpnDmgSkill is object)
								info.HighestElement = Array.IndexOf(TranslateIndexToOffense, highestWpnDmgSkill.ElementalType);
						}
						else
						{
							int highestElement = 0;
							
							foreach (IPlayerSkill skill in player.Powers.UsedSkills.Where(s => s.WeaponDamageMultiplier > 0))
							{
								if (skill.ElementalDamageBonus > 0)
								{
									if (highestElement == 0)
									{
										highestElement = Array.IndexOf(TranslateIndexToOffense, skill.ElementalType);
									}
									
									var element = Array.IndexOf(TranslateIndexToOffense, skill.ElementalType);
									
									if (element != highestElement)
									{
										if (highestElement == 0 || player.Offense.ElementalDamageBonus[element] > player.Offense.ElementalDamageBonus[highestElement])
											highestElement = element;
										
										if (skill.ElementalDamageBonus == player.Offense.HighestElementalDamageBonus)
											break;
									}
								}
								
							}
							
							if (highestElement > 0)
								info.HighestElement = highestElement;
						}
					}
				}
				
				if (AnnouncePlayer == 0 && player.IsMe && AutoAnnounceSelf)
					AnnouncePlayer = player.HeroId;
			}
		}
		
		public void OnHotkeyEvent(IKeyEvent keyEvent)
		{
			//check for the defined hotkey
			//if (keyEvent.IsPressed)
			//{
			//	if (ToggleAnnounceHotkey is object && ToggleAnnounceHotkey.Matches(keyEvent))
			//	{
			//		//check if the cursor is supposed to be blocked
			//		if (Mover == null || Mover.Overlay.GetUiObstructingCursor() is object)
			//			return;
            //
			//		foreach (IPlayer player in Hud.Game.Players)
			//		{
			//			//check if the player's portrait is being hovered over
			//			var rect = player.PortraitUiElement.Rectangle;
			//			if (Hud.Window.CursorInsideRect(rect.X, rect.Y, rect.Width, rect.Height))
			//			{
			//				if (AnnouncePlayer == player.HeroId)
			//				{
			//					if (AnnouncePlayer == Hud.Game.Me.HeroId && AutoAnnounceSelf)
			//						AutoAnnounceSelf = false;
			//					
			//					AnnouncePlayer = 0;
			//				}
			//				else if (Data.ContainsKey(player.HeroId))
			//				{
			//					AnnouncePlayer = player.HeroId;
			//					
			//					if (AnnouncePlayer == Hud.Game.Me.HeroId && !AutoAnnounceSelf)
			//						AutoAnnounceSelf = true;
			//				}
            //
			//				//break;
			//				return;
			//			}
			//		}
			//		
			//		//not hovering over portrait or any of the coe panels
			//		if (AnnouncePlayer > 0 && (Mover.HoveredPluginArea == null || !Data.Values.Any(info => info.Display == Mover.HoveredPluginArea)))
			//		{
			//			if (AnnouncePlayer == Hud.Game.Me.HeroId && AutoAnnounceSelf)
			//				AutoAnnounceSelf = false;
			//			
			//			AnnouncePlayer = 0;
			//		}
			//	}
			//	else if (ToggleDisplayModeHotkey is object && ToggleDisplayModeHotkey.Matches(keyEvent))
			//	{
			//		//ShowSelectedOnly = !ShowSelectedOnly;
			//		if (Mode == DisplayMode.All)
			//			Mode = DisplayMode.SelectedOnly;
			//		else if (Mode == DisplayMode.SelectedOnly)
			//			Mode = DisplayMode.ActiveSelectedOnly;
			//		else
			//			Mode = DisplayMode.All;
			//	}
			//}
		}
		
		public void OnKeyEvent(MovableController mover, IKeyEvent keyEvent, MovableArea area)
		{
			if (keyEvent.IsPressed)
			{
				if (ToggleAnnounceHotkey is object && ToggleAnnounceHotkey.Matches(keyEvent))
				{
					foreach (KeyValuePair<uint, COEInfo> pair in Data)
					{
						if (pair.Value.Display == area)
						{
							if (AnnouncePlayer == pair.Key)
							{
								if (pair.Key == Hud.Game.Me.HeroId && AutoAnnounceSelf)
									AutoAnnounceSelf = false;

								AnnouncePlayer = 0;
							}
							else
							{
								AnnouncePlayer = pair.Key;
								
								if (pair.Key == Hud.Game.Me.HeroId && !AutoAnnounceSelf)
									AutoAnnounceSelf = true;
							}
							
							break;
						}
					}
				}
			}
		}
		
		public bool MouseClickShouldBeBlocked(System.Windows.Forms.MouseButtons button, int x, int y)
		{
			return button == System.Windows.Forms.MouseButtons.Left && Mover is object && !Mover.EditMode && HoveredElement > 0;
		}
 
		public void OnRegister(MovableController mover)
		{
			Mover = mover;
			//Overlay = Hud.GetPlugin<UIOverlapHelper>();
			
			ITexture texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.ConventionOfElements.Icons[1].TextureId);
			IconWidth = texture.Width * IconSizeMultiplier;
			IconHeight = texture.Height * IconSizeMultiplier;
			
			//var rect = Hud.Game.Me.PortraitUiElement.Rectangle;
			//SnapToPortraitArea = new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width + IconWidth*3, rect.Height*4);
			
			if (TimeLeftFont is object)
			{
				//var pCycle = ConventionCycle[Hud.Game.Me.HeroClassDefinition.HeroClass];
				//var layout = TimeLeftFont.GetTextLayout("x");
				var fontSize = (float)TimeLeftFont.GetTextLayout("x").GetFontSize(0);
				FontRatio = IconWidth / fontSize;
			}
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			////if (Hud.Render.UiHidden)
			////	return;
			////KeyValuePair<uint, COEInfo> pair = Data.FirstOrDefault(kvp => kvp.Value.Display == area);
			//uint heroId = 0;
			//COEInfo info = null;
			//foreach (KeyValuePair<uint, COEInfo> pair in Data)
			//{
			//	if (pair.Value.Display == area)
			//	{
			//		//verify that the player is in the game
			//		//heroId = pair.Key;
			//		//info = pair.Value;
			//		IPlayer player = Hud.Game.Players.FirstOrDefault(p => p.HeroId == pair.Key);
			//		if (player is object) //Hud.Game.Players.Any(p => p.HeroId == pair.Key)
			//		{
			//			info = pair.Value;
            //
			//			//if there is no coe data
			//			if (info.ConventionCycleStartTick < 0)
			//				break;
            //
			//			float x = area.Rectangle.X + deltaX;
			//			float y = area.Rectangle.Y + deltaY;
			//			
			//			//snap to default position if in another player's portrait area
			//			var rect = area.Rectangle;
			//			if (!mover.EditMode) //&& SnapToPortraitArea.IntersectsWith(rect))
			//			{
			//				var portrait = player.PortraitUiElement.Rectangle;
			//				if (x >= portrait.Right && x <= portrait.Right+rect.Width && y != portrait.Y+portrait.Height*0.48f) //if (y < portrait.Y || y > portrait.Bottom - rect.Height)
			//				{
			//					//move it to the correct player's portrait
			//					area.Rectangle = new System.Drawing.RectangleF(portrait.Right, portrait.Y + portrait.Height*0.48f, rect.Width, rect.Height); //portrait.Right, portrait.Y, rect.Width, rect.Height); //rect.Right, rect.Y + rect.Height*0.51f
			//				}
			//			}
			//			
			//			//get the coe cycle subset for this class
			//			int[] cycle = ConventionCycle[player.HeroClassDefinition.HeroClass];
			//			
			//			//debug draw the list of selected elements
			//			//var test = TimeLeftFont.GetTextLayout(string.Join(", ", SelectedElements[player.HeroId]) + "\n" + string.Join(", ", cycle));
			//			//TimeLeftFont.DrawText(test, x, y + area.Rectangle.Height);
			//			
			//			//int lastCycleStartTick = ConventionCycleStartTick[player.HeroId]; //last seen
			//			//if (lastCycleStartTick > Hud.Game.CurrentGameTick) //wait for the next collection update
			//			//	break;
			//			
			//			//COEInfo info = Data[player.HeroId];
			//			int cycleStartTick = info.ConventionCycleStartTick; //ConventionCycleStartTick[player.HeroId]; //lastCycleStartTick;
			//			if (cycleStartTick > Hud.Game.CurrentGameTick) //wait for the next collection update
			//				return; //break;
			//			
			//			//calculate the current icon width
			//			float ratio = rect.Width / (IconWidth*cycle.Length + IconSpacing*(cycle.Length - 1)); //originalWidth;
			//			float iWidth = IconWidth*ratio;
			//			float iSpacing = IconSpacing*ratio;
			//			//float width = (rect.Width - IconSpacing*(cycle.Length - 1)) / cycle.Length; //IconWidth*cycle.Length + IconSpacing*(cycle.Length-1)
			//			
			//			//how far from the beginning of the current coe cycle are we?
			//			int elementTicks = 4 * 60;
			//			int cycleDurationInTicks = cycle.Length * elementTicks;
			//			int cycleTicksElapsed = Hud.Game.CurrentGameTick - cycleStartTick;
			//			while (Hud.Game.CurrentGameTick - cycleStartTick > cycleDurationInTicks)
			//				cycleStartTick += cycleDurationInTicks;
			//			
			//			//int quotient = (int)Math.Floor((double)cycleTicksElapsed / (double)cycleDurationInTicks);
			//			//if (quotient > 0)
			//			//	cycleStartTick += quotient * cycleDurationInTicks;
			//			
			//			//int shift = 0;
			//			//cycleStartTick + elementTicks*(i+1) >= Hud.Game.CurrentGameTick
			//			//elementTicks*(i+1) >= Hud.Game.CurrentGameTick - cycleStartTick
			//			//i + 1 >= (Hud.Game.CurrentGameTick - cycleStartTick) / elementTicks
			//			//i >= ((Hud.Game.CurrentGameTick - cycleStartTick) / elementTicks) - 1
			//			//int shift = (int)(((float)(Hud.Game.CurrentGameTick - cycleStartTick) / (float)elementTicks) - 1f);
			//			int[] shifted = new int[cycle.Length];
			//			for (int i = 0, j = 0; i < cycle.Length; ++i)
			//			{
			//				if (cycleStartTick + elementTicks*(i+1) >= Hud.Game.CurrentGameTick)
			//				{
			//					int shift = i;
			//					
			//					while (i < cycle.Length)
			//						shifted[j++] = cycle[i++];
			//					
			//					for (int k = 0; k < shift; ++k)
			//						shifted[j++] = cycle[k];
			//					
			//					break;
			//				}
			//			}
			//			
			//			//draw
			//			string debug = string.Empty;
			//			HoveredPlayer = 0;
			//			HoveredElement = 0;
			//			float activeX = 0;
			//			float activeY = 0;
			//			float activeW = 0;
			//			float activeH = 0;
			//			//int activeIndex = -1;
			//			int activeElement = 0;
			//			bool activeSelected = false;
			//			for (int i = 0; i < shifted.Length; ++i)
			//			{
			//				int element = shifted[i];
			//				bool selected = info.CustomElements.Contains(element) || info.HighestElement == element; //(info.HighestElement == element && !info.CustomElements.Contains(element * -1)) || info.CustomElements.Contains(element);
			//				float w = iWidth;
			//				float h = rect.Height;
			//				float y2 = y;
			//				
			//				if (!selected)
			//				{
			//					if (Mode != DisplayMode.All)
			//						continue;
            //
			//					y2 = y + h*0.5f - h*NotSelectedSizeMultiplier*0.5f;
			//					w *= NotSelectedSizeMultiplier;
			//					h *= NotSelectedSizeMultiplier;
			//					
			//					DrawElement(element, x, y2, w, h, false);
			//					
			//					//hover
			//					if (Mode == DisplayMode.All && Hud.Window.CursorInsideRect(x, y2, w, h))
			//					{
			//						HoveredPlayer = player.HeroId;
			//						HoveredElement = element;
			//					}
			//				}
			//				else if (Mode != DisplayMode.ActiveSelectedOnly || i == 0)
			//				{
			//					DrawElement(element, x, y2, w, h, true, i == 0);
			//					
			//					//hover
			//					if (Mode == DisplayMode.All && Hud.Window.CursorInsideRect(x, y2, w, h))
			//					{
			//						HoveredPlayer = player.HeroId;
			//						HoveredElement = element;
			//					}
			//				}
			//				
			//				//overlay
			//				if (i == 0)
			//				{
			//					if (selected || Mode != DisplayMode.ActiveSelectedOnly)
			//					{
			//						debug += i.ToString() + " ";
			//						//int index = Array.IndexOf(cycle, element);
			//						//var timeElapsed = (double)(Hud.Game.CurrentGameTick - (cycleStartTick + elementTicks*index))/60f;
			//						//activeIndex = Array.IndexOf(cycle, element);
			//						activeElement = element;
			//						//activeRect = new RectangleF(x, y2, w, h);
			//						activeX = x;
			//						activeY = y2;
			//						activeW = w;
			//						activeH = h;
			//						activeSelected = selected;
			//						//DrawTimeLeftClock(activeRect, timeElapsed, 4 - timeElapsed, selected ? ActiveBrush : InactiveBrush);
			//					}
			//				}
			//				else
			//				{
			//					//darken
			//					if (Mode != DisplayMode.ActiveSelectedOnly && (HoveredPlayer != player.HeroId || HoveredElement != element))
			//					{
			//						TimeLeftClockBrush.Opacity = DarkenNotActiveOpacity;
			//						TimeLeftClockBrush.DrawRectangle(x, y2, w, h);
			//					}
			//					
			//					//countdown
			//					if (selected)
			//					{
			//						int index = Array.IndexOf(cycle, element);
			//						float ticksLeft = cycleDurationInTicks - (Hud.Game.CurrentGameTick - (cycleStartTick + elementTicks*index));
			//						while (ticksLeft > cycleDurationInTicks)
			//							ticksLeft -= cycleDurationInTicks;
			//						
			//						float timeLeft = (float)ticksLeft / 60f; //(cycleDurationInTicks - (Hud.Game.CurrentGameTick - (cycleStartTick + elementTicks*index))) / 60f;
			//						//float timeLeft = (float)(cycleStartTick + elementTicks*index - Hud.Game.CurrentGameTick) / 60f;
			//						
			//						if (Mode != DisplayMode.ActiveSelectedOnly)
			//						{
			//							//figure out if font size needs to be adjusted
			//							//var fontSize = layout.GetFontSize(0);
			//							//fontSize * ratio = originalSize
			//							//var fontSize = originalSize / ratio;
			//							string text = timeLeft.ToString(timeLeft >= 1 ? "F0" : "F1");
			//							var layout = TimeLeftFont.GetTextLayout(text);
			//							
			//							if (ScaleFontOnResize)
			//								layout.SetFontSize(iWidth / FontRatio, new TextRange(0, text.Length));
			//							
			//							TimeLeftFont.DrawText(layout, x + w*0.5f - layout.Metrics.Width*0.5f, y2 + h*0.5f - layout.Metrics.Height*0.5f);
			//						}
			//						
			//						//sounds
			//						if (PlaySounds is object && AnnouncePlayer == player.HeroId && PlaySounds(Hud, player)) //(PlaySoundsInTown || !Hud.Game.IsInTown))
			//						{
			//							if (!Played3 && timeLeft <= SoundTriggerTime[3] && timeLeft > SoundTriggerTime[2]) //Played3
			//							{
			//								Played3 = true;
			//								Played2 = false;
			//								Played1 = false;
			//								PlayedElement = false;
            //
			//								ThreadPool.QueueUserWorkItem(state => {
			//									try {
			//										if (UseTextToSpeech)
			//											Hud.Sound.Speak(TTSCount[2]);
			//										else
			//											SoundCount[2]?.Play();
			//									} catch (Exception) {}
			//								});
            //
			//							}
			//							else if (!Played2 && timeLeft <= SoundTriggerTime[2] && timeLeft > SoundTriggerTime[1]) //Played2
			//							{
			//								Played2 = true; //Played2
			//								Played3 = false;
			//								Played1 = false;
			//								PlayedElement = false;
			//								
			//								ThreadPool.QueueUserWorkItem(state => {
			//									try {
			//										if (UseTextToSpeech)
			//											Hud.Sound.Speak(TTSCount[1]);
			//										else
			//											SoundCount[1]?.Play();
			//									} catch (Exception) {}
			//								});
			//							}
			//							else if (!Played1 && timeLeft <= SoundTriggerTime[1] && timeLeft > SoundTriggerTime[0])
			//							{
			//								Played1 = true;
			//								Played3 = false;
			//								Played2 = false;
			//								PlayedElement = false;
            //
			//								ThreadPool.QueueUserWorkItem(state => {
			//									try {
			//										if (UseTextToSpeech)
			//											Hud.Sound.Speak(TTSCount[0]);
			//										else
			//											SoundCount[0]?.Play();
			//									} catch (Exception) {}
			//								});
			//							}
			//							else if (!PlayedElement && timeLeft < SoundTriggerTime[0] && timeLeft > 0) //PlayedElement
			//							{
			//								PlayedElement = true;
			//								Played3 = false;
			//								Played2 = false;
			//								Played1 = false;
            //
			//								ThreadPool.QueueUserWorkItem(state => {
			//									try {
			//										if (UseTextToSpeech)
			//											Hud.Sound.Speak(TTSElement[element]);
			//										else
			//											SoundElement[element]?.Play();
			//									} catch (Exception) {}
			//								});
			//							}
			//						}
			//					}
			//				}
			//				
			//				//draw speaker icon
			//				if (AnnouncePlayer == player.HeroId)
			//				{
			//					var r = player.PortraitUiElement.Rectangle;
			//					var icon = IconFont.GetTextLayout(IconSymbol);
			//					var sx = r.Right - icon.Metrics.Width - 2;
			//					var sy = r.Y + r.Height*0.48f + iWidth*0.5f - icon.Metrics.Height*0.5f;
			//					IconFont.DrawText(icon, sx, sy);
			//					
			//					if (mover.Overlay.GetUiObstructingCursor() == null && !string.IsNullOrEmpty(TextHintIcon) && Hud.Window.CursorInsideRect(sx, sy, icon.Metrics.Width, icon.Metrics.Height))
			//						Hud.Render.SetHint(TextHintIcon);
			//				}
			//				
			//				x += w + iSpacing;
			//			}
			//			
			//			if (activeElement > 0) //!EqualityComparer<RectangleF>.Default.Equals(activeRect, default(RectangleF)))
			//			{
			//				var activeIndex = Array.IndexOf(cycle, activeElement);
			//				var timeElapsed = (double)(Hud.Game.CurrentGameTick - (cycleStartTick + elementTicks*activeIndex))/60f;
			//				DrawTimeLeftClock(new RectangleF(activeX, activeY, activeW, activeH), timeElapsed, 4 - timeElapsed, activeSelected ? ElementBrushes[activeElement] : ElementBrushes[0]);
			//			}
			//		}
			//		/*else if (Math.Abs(Hud.Game.CurrentGameTick - Data[pair.Key].LastActiveTick) > 120) //wait 2 seconds before removing missing player
			//		{
			//			//remove nonexistant player info from the list
			//			Data.Remove(pair.Key);
			//			
			//			//request deletion of the area
			//			mover.DeleteArea(area);
			//		}*/
			//		
			//		break;
			//	}
			//}
        }
		
		public void OnLeftMouseDown(MovableArea area)
		{
			if (HoveredElement > -1 && HoveredPlayer > 0 && Data.ContainsKey(HoveredPlayer))
			{
				COEInfo info = Data[HoveredPlayer];
				if (info.HighestElement == HoveredElement)
				{
					if (info.CustomElements.Contains(HoveredElement * -1))
						info.CustomElements.Remove(HoveredElement * -1);
					else
						info.CustomElements.Add(HoveredElement * -1);
				}
				else if (info.CustomElements.Contains(HoveredElement))
					info.CustomElements.Remove(HoveredElement);
				else
					info.CustomElements.Add(HoveredElement);
			}
		}
		
		public void OnLeftMouseUp(MovableArea area)
		{
			
		}
		
		/*public void OnRightMouseDown()
		{
		}
		
		public void OnRightMouseUp()
		{
		}*/
		
		private void DrawTimeLeftClock(RectangleF rect, double elapsed, double timeLeft, IBrush border = null)
		{
			if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null))
			{
				//var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
				//var startAngle = 0;
				TimeLeftClockBrush.Opacity = 1 - (float)(0.3f / (timeLeft + elapsed) * elapsed);
				
				var angle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
				using (var pg = Hud.Render.CreateGeometry())
				{
					using (var gs = pg.Open())
					{
						gs.BeginFigure(rect.Center, FigureBegin.Filled);
						gs.AddLine(new Vector2(rect.Center.X, rect.Y));
						
						if (angle >= 45)
						{
							gs.AddLine(new Vector2(rect.Right, rect.Y));
							
							if (angle >= 135)
							{
								gs.AddLine(new Vector2(rect.Right, rect.Bottom));
								
								if (angle >= 225)
								{
									gs.AddLine(new Vector2(rect.Left, rect.Bottom));
									
									if (angle >= 315)
									{
										gs.AddLine(new Vector2(rect.X, rect.Y));
										gs.AddLine(new Vector2(rect.X + ((rect.Width*0.5f)/45f)*(angle - 315), rect.Y));
									}
									else
										gs.AddLine(new Vector2(rect.X, rect.Bottom - (rect.Height/90f)*(angle - 225)));
								}
								else
									gs.AddLine(new Vector2(rect.Right - (rect.Width/90f)*(angle - 135), rect.Bottom));
							}
							else
								gs.AddLine(new Vector2(rect.Right, rect.Y + (rect.Height/90f)*(angle - 45)));
						}
						else
							gs.AddLine(new Vector2(rect.Center.X + ((rect.Width*0.5f)/45f)*angle, rect.Y));
							
						gs.EndFigure(FigureEnd.Closed);
						gs.Close();
					}

					TimeLeftClockBrush.DrawGeometry(pg);
				}

				if (border is object)
				{
					border.StrokeWidth = ClockBorderWidth;

					using (var pg = Hud.Render.CreateGeometry())
					{
						using (var gs = pg.Open())
						{
							gs.BeginFigure(new Vector2(rect.Center.X, rect.Y), FigureBegin.Filled);
							if (angle <= 315) //left
							{
								gs.AddLine(new Vector2(rect.X, rect.Y));
								
								if (angle <= 225)
								{
									gs.AddLine(new Vector2(rect.X, rect.Bottom));
									
									if (angle <= 135)
									{
										gs.AddLine(new Vector2(rect.Right, rect.Bottom));
										
										if (angle <= 45)
										{
											gs.AddLine(new Vector2(rect.Right, rect.Y));
											gs.AddLine(new Vector2(rect.Center.X + ((rect.Width*0.5f)/45f)*angle, rect.Y));
										}
										else
											gs.AddLine(new Vector2(rect.Right, rect.Y + (rect.Height/90f)*(float)Math.Max(0, angle - 45)));
									}
									else
										gs.AddLine(new Vector2(rect.Right - (rect.Width/90f)*(float)Math.Max(0, angle - 135), rect.Bottom));
								}
								else
									gs.AddLine(new Vector2(rect.X, rect.Bottom - (rect.Height/90f)*(float)Math.Max(0, angle - 225)));
							}
							else
								gs.AddLine(new Vector2(rect.X + ((rect.Width*0.5f)/45f)*(float)Math.Max(0, angle - 315), rect.Y));
							
							gs.EndFigure(FigureEnd.Open);
							gs.Close();
						}
						
						border.DrawGeometry(pg);
					}
				}
			}
		}
		
		private void DrawElement(int element, float x, float y, float w, float h, bool isSelected = true, bool isActive = false)
		{
			if (IconOpacity < 1f)
			{
				IBrush brush = ElementBrushes[element];
				brush.StrokeWidth = 0;
				brush.DrawRectangle(x+1, y+1, w-2, h-2);
			}

			ITexture texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.ConventionOfElements.Icons[element].TextureId);
			texture.Draw(x, y, w, h, isActive ? 1f : IconOpacity);
			(isSelected ? Hud.Texture.BuffFrameTexture : Hud.Texture.DebuffFrameTexture).Draw(x, y, w, h);
			
			//test
			//var test = TimeLeftFont.GetTextLayout(Hud.Game.Me.Offense.ElementalDamageBonus[TranslateIndexToOffense[element]].ToString());
			//TimeLeftFont.DrawText(test, x, y - test.Metrics.Height);
		}
    }
}