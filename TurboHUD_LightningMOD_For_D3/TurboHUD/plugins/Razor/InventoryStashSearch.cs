/*

This plugin adds a chat command to highlight items in your inventory, paperdoll (equipped), and stash to make it easier to find your stuff. This also works when using the built-in stash search field.

To do:
- expand on armory profiles to include potion and skill setups
- expand on save data to include item stats
- save and load character data across TH sessions

Changelog
- August 7, 2020
	- Converted some hardcoded text prompts to variables for localization
	- Fixed parameter regex to capture more than one word of text and handle empty parameters
	- Fixed NullPointerException when a parameter search attempts to access the stat text of unidentified items
	- Changed click detection to use my Click library because it has better desync correction
	- Changed search summary to display parameters used
- August 6, 2020
	- Fixed search summary to take into account search parameters
	- Search snapshots (for cross-character searches) now record items inside sockets
- August 3, 2020
	- Added legendary gem levels to the generated search term
	- Added +<stat>, greater than<rank>, less than<rank>, parameters for a more comprehensive search for the current character only (result summary is not accurate for searches with parameters yet)
	- Small optimization: regex objects are now reused instead of recreated for every draw
- July 17, 2020 - Shifted armory tooltip text down in item tooltips if Jack Ceparou's WeaponDamageRerollCalculatorPlugin is installed
- July 7, 2020 - Bugfixes, added the ability to search stash when only the inventory is open anywhere in the game, 
- June 23, 2020 - Bugfixes, update for S21
	- Resolved the search order of chat command vs built-in stash search (and results display persistence)
	- Added armor set sno's for Season 21's new sets (both have irregular piece names)
- May 14, 2020 - Full rewrite
	- extends the built-in stash search field to mark tabs and inventory with results
	- added armory items searching, tagging, in-tooltip listing
	- experimental cross-character inventory, stash, armory lookup (if you've logged into those characters during the current TH session)
	- added search results summary display and error messages
- February 19, 2019 - Initial release

*/

using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text; //StringBuilder
using System.Text.RegularExpressions; //Regex, MatchCollection, Group
using System.Windows.Forms; //Keys

using Turbo.Plugins.Default;
using Turbo.Plugins.Razor.Click;

namespace Turbo.Plugins.Razor
{
    public class InventoryStashSearch : BasePlugin, IInGameTopPainter, ICustomizer, INewAreaHandler, IAfterCollectHandler, IItemLocationChangedHandler, IItemPickedHandler, ILeftClickHandler //, IKeyEventHandler/*, IChatLineChangedHandler*/
    {
		public bool MarkArmorySets { get; set; } = true;
		public bool ShowArmoryInTooltips { get; set; } = true;

		//text that needs to be translated for localization
		
		public string TextStash { get; set; } = "Stash";
		public string TextInventory { get; set; } = "Inventory";
		public string TextEquipped { get; set; } = "Equipped";
		public string TextFollower { get; set; } = "Follower";
		public string TextLegendary { get; set; } = "Legendary"; //specifically for "legendary gem" matches
		public string TextSeason { get; set; } = "Season";
		public string TextCritChance { get; set; } = "Critical Hit Chance Increased by [{VALUE}*100|1|]%"; //leave the [{VALUE}*100|1|] intact //crit chance text is missing from perfection attribute text (this value should be tooltip text for crit chance)

		public string TextSearchResults { get; set; } = "⌕"; //"Search results for";
		public string TextError { get; set; } = "⚠️"; //"Error:";

		//set names that don't have common naming scheme, this links them together with a common search phrase
		public Dictionary<uint, string> SetNames { get; set; } = new Dictionary<uint, string>() {
			//Barbarian Sets
			//{1376402780, "Immortal King's Call"},
			//{858522158, "Wrath of the Wastes"},
			//{3778768965, "The Legacy of Raekor"},
			//{4276705470, "Might of the Earth"},
			
			//Crusader Sets
			//{3056131212, "Armor of Akkhan"},
			//{1298357201, "Thorns of the Invoker"},
			//{3056130948, "Roland's Legacy"},
			
			//Demon Hunter Sets
			//{3709730243, "The Shadow's Mantle"},
			//{2328093147, "Embodiment of the Marauder"},
			{2328092884, "Unhallowed Essence"},
			//{1376366843, "Natalya's Vengeance"},
			{3056341713, "Gears of the Dreadlands"},
			
			//Monk Sets
			//{1376474654, "Inna's Mantra"},
			{546014795, "Raiment of a Thousand Storms"},
			//{4203567106, "Uliana's Stratagem"},
			{3199582862, "Monkey King's Garb"},
			
			//Necromancer Sets
			//{1113279417, "Grace of Inarius"},
			//{1113279418, "Pestilence Master's Shroud"},
			//{1113279415, "Bones of Rathma"},
			//{1113279416, "Trag'Oul's Avatar"},
			{858480180, "Masquerade of the Burning Carnival"},
			
			//Witch Doctor Sets
			//{1875714076, "Helltooth Harness"},
			//{1875713814, "Spirit of Arachyr"},
			//{3362577626, "Raiment of the Jade Harvester"},
			
			//Wizard Sets
			{3704768029, "Delsere's Magnum Opus"},
			//{3644785385, "Vyr's Amazing Arcana"},
			
			//Craftable sets (all of them have a naming scheme that doesn't require any search hints)
			
			//Misc Sets
			{1377373079, "Endless Walk"},
			{2328795073, "Bastions of Will"},
			{3677852057, "Istvan's Paired Blades"},
			//{1376294969, "Shenlong's Spirit"},
			{1377409016, "Legacy of Nightmares"},
			{3163460410, "Norvald's Fervor"},
			//{1232859152, "Krelm's Buff Bulwark"},
			//{1376187158, "Bul-Kathos's Oath"},
			//{1376223095, "Danetta's Hatred"},
		};

		public string Mark { get; set; } = "◣"; //◣◣
		public IFont MarkerCurrentHero { get; set; }
		public IFont MarkerOtherHero { get; set; }
		public IFont ArmoryTooltipMe { get; set; }
		public IFont ArmoryTooltipOther { get; set; }
		
		public bool FilterResultsByHeroType { get; set; } = true; //searches run on hardcore seasonal characters will only return results from hardcore seasonal characters
		
		public IBrush ShadowBrush { get; set; }
		
		public IFont SearchFont { get; set; }
		public IFont InstructionFont { get; set; }
		public IBrush HighlightBrush { get; set; }
		public IBrush MuteBrush { get; set; }
		public IBrush BgBrush { get; set; }
		public int ResultsPerTabCountOffsetX { get; set; }
		public int ResultsPerTabCountOffsetY { get; set; }
		
		
		//public string LegendaryText { get; set; }
		
		public string SearchCmd { get; set; }
		public string SearchCmdInstruction { get; set; }
		public bool ShowResultsWhileTyping { get; set; }
		public int MinimumSearchLength { get; set; }
		public float ButtonGap { get; set; } = 5f;
		
		public Dictionary<uint, SearchSnapshot> Snapshots = new Dictionary<uint, SearchSnapshot>();
		
		//public string LookingForCmd { get; set; }
		//public string LookingForCmdInstruction { get; set; }
		//public Dictionary<string, Regex> LookingFor { get; set; };
		
		//public string ValidInputIcon { get; set; } //not yet implemented
		//public string InvalidInputIcon { get; set; } //not yet implemented
		//public IFont ValidIndicatorFont { get; set; } //not yet implemented
		//public IFont InvalidIndicatorFont { get; set; } //not yet implemented
		
		private Regex RegexItemLinkStartTag = new Regex(@"\|H[^\|]+");
		private Regex RegexItemLinkEndTag = new Regex(@"\|h");
		private Regex RegexColorTag = new Regex(@"\{\/?c[^\}]*\}");
		private Regex RegexItemLinkQuality = new Regex(@"[\{\[\]\}]");
		private Regex RegexParameters = new Regex(@"([\+\>\<])([A-Za-z0-9 ]*)");
		private Regex RegexNumber = new Regex(@"\s(\d+)");
		private string SearchString;
		//private string SearchPhrase;
		private Regex Search;
		private HashSet<Tuple<string, string>> Parameters = new HashSet<Tuple<string, string>>();
		//private string Buffer = "";
		private string LastChatParams;
		private string LastSearchParams;
		private int SelectedIndex = -1;
		private uint SelectedHero = 0;
		private bool LButtonPressed = false;
		private bool LastSearchTypeIsChat = false;
		//private bool ShowFilterSearch = false;
		private float ButtonWidth;
		private float ButtonHeight;
		//private string ArmoryPromptOKed = null;
		private int ArmoryPromptOKed = 0;
		//private uint LastHeroSeen;
		private bool CheckForMissingArmoryItems = false;
		private string SalvageInstruction = null;
		private bool CheckForDeletion = false;
		private int LastSalvageCheck;
		private int LastPickupCheck;
		private bool IsRerollCalculatorEnabled;
		private float FontSize = 0;
		//private bool UpdateHero = false;
		
		//private ItemLocation lastFrom;
		//private ItemLocation lastTo;
		
		private IFont DebugFont;
		
        public InventoryStashSearch()
        {
            Enabled = true;
			
			SearchCmd = "/s"; //don't include a trailing space in the search command
			//Cmd = "/s"; //don't include a trailing space in the search command
			//ShowResultsWhileTyping = true; //when the search term is at least MinimumSearchLength characters long, automatically start showing search results
			MinimumSearchLength = 2;
			
			//ValidInputIcon = "✓"; //not yet implemented
			//InvalidInputIcon = "⚠️"; //not yet implemented
			
			//LegendaryText = "Legendary"; //translate this to whatever language your game client displays
			
			//set names that don't have common naming scheme that would otherwise link them together in a search
			SetNames = new Dictionary<uint, string>() { //translate this to whatever language your game client displays
				//Barbarian Sets
				//{1376402780, "Immortal King's Call"},
				//{858522158, "Wrath of the Wastes"},
				//{3778768965, "The Legacy of Raekor"},
				//{4276705470, "Might of the Earth"},
				
				//Crusader Sets
				//{3056131212, "Armor of Akkhan"},
				//{1298357201, "Thorns of the Invoker"},
				//{3056130948, "Roland's Legacy"},
				
				//Demon Hunter Sets
				//{3709730243, "The Shadow's Mantle"},
				//{2328093147, "Embodiment of the Marauder"},
				{2328092884, "Unhallowed Essence"},
				//{1376366843, "Natalya's Vengeance"},
				{3056341713, "Gears of the Dreadlands"},
				
				//Monk Sets
				//{1376474654, "Inna's Mantra"},
				{546014795, "Raiment of a Thousand Storms"},
				//{4203567106, "Uliana's Stratagem"},
				{3199582862, "Monkey King's Garb"},
				
				//Necromancer Sets
				//{1113279417, "Grace of Inarius"},
				//{1113279418, "Pestilence Master's Shroud"},
				//{1113279415, "Bones of Rathma"},
				//{1113279416, "Trag'Oul's Avatar"},
				{858480180, "Masquerade of the Burning Carnival"},
				
				//Witch Doctor Sets
				//{1875714076, "Helltooth Harness"},
				//{1875713814, "Spirit of Arachyr"},
				//{3362577626, "Raiment of the Jade Harvester"},
				
				//Wizard Sets
				{3704768029, "Delsere's Magnum Opus"},
				//{3644785385, "Vyr's Amazing Arcana"},
				
				//Craftable sets (all of them have a naming scheme that doesn't require any search hints)
				
				//Misc Sets
				{1377373079, "Endless Walk"},
				{2328795073, "Bastions of Will"},
				{3677852057, "Istvan's Paired Blades"},
				//{1376294969, "Shenlong's Spirit"},
				{1377409016, "Legacy of Nightmares"},
				{3163460410, "Norvald's Fervor"},
				//{1232859152, "Krelm's Buff Bulwark"},
				//{1376187158, "Bul-Kathos's Oath"},
				//{1376223095, "Danetta's Hatred"},
			};			
			
			ResultsPerTabCountOffsetX = 14;
			ResultsPerTabCountOffsetY = -15;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			SearchFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, 242, 0, 0, 0, true);
			InstructionFont = Hud.Render.CreateFont("tahoma", 7, 200, 249, 232, 169, false, false, 242, 0, 0, 0, true);
			
			HighlightBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 3);
			MuteBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 0);
			BgBrush = Hud.Render.CreateBrush(200, 0, 0, 0, 0);
			ShadowBrush = Hud.Render.CreateBrush(75, 0, 0, 0, 2);
			
			MarkerCurrentHero = Hud.Render.CreateFont("tahoma", 10f, 255, 66, 239, 245, false, false, true); //225, 225, 225 //225, 255, 225
			MarkerOtherHero = Hud.Render.CreateFont("tahoma", 10f, 255, 235, 95, 52, false, false, true);
			ArmoryTooltipMe = Hud.Render.CreateFont("Arial", 7.5f, 255, 66, 239, 245, false, false, 242, 0, 0, 0, true); //225, 225, 225
			ArmoryTooltipOther = Hud.Render.CreateFont("Arial", 7.5f, 255, 235, 95, 52, false, false, 242, 0, 0, 0, true);
			
			SearchCmdInstruction = "Type "+SearchCmd+" Item Name in your chatbox to search your stash + inventory";
			//LookingForCmdInstruction = "Type "+LookingForCmd+" Item Name in your chatbox to remember items to look for";
			
			DebugFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, false, false, 242, 0, 0, 0, true);
			
			//register the built-in stash search field for data collection when the stash is open
			Hud.Render.RegisterUiElement("Root.NormalLayer.stash_dialog_mainPage.StashFilter.FilterInput", Hud.Inventory.StashMainUiElement, null);
			
			//register for armory change detection when armory is open
			//Hud.Render.RegisterUiElement("Root.TopLayer.textInputPrompt", Hud.Render.GetUiElement("Root.NormalLayer.equipmentManager_mainPage"), null);
			Hud.Render.RegisterUiElement("Root.TopLayer.textInputPrompt.stack.footer.button_stack.ok", Hud.Render.GetUiElement("Root.NormalLayer.equipmentManager_mainPage"), null);
			Hud.Render.RegisterUiElement("Root.NormalLayer.equipmentManager_mainPage.loadout_name", Hud.Render.GetUiElement("Root.NormalLayer.equipmentManager_mainPage"), null);
			Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.instruction", Hud.Render.GetUiElement("Root.NormalLayer.vendor_dialog_mainPage.panel"), null);
			
			//LButtonPressed = Hud.Input.IsKeyDown(Keys.LButton);
        }
		
		//use this function for the one-off initialization of this variable
		public void Customize()
		{
			IsRerollCalculatorEnabled = Hud.AllPlugins.Any(p => p.GetType().Name == "WeaponDamageRerollCalculatorPlugin");
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				//force a resave
				//SnapshotCurrentArmory();
				//ArmorySets.Remove(Hud.Game.Me.HeroId);
				//UpdateHero = true;
				CheckForMissingArmoryItems = true;
			}
		}
		
		public void OnLeftMouseDown()
		{
			LButtonPressed = true;
		}
		
		public void OnLeftMouseUp()
		{
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			if (!Snapshots.ContainsKey(Hud.Game.Me.HeroId))
				SaveSnapshot();
			else if (CheckForMissingArmoryItems)
			{
				CheckForMissingArmoryItems = false;				
				
				SearchSnapshot snapshot = Snapshots[Hud.Game.Me.HeroId];
				if (snapshot is object)
				{
					foreach (ArmorySet loadout in snapshot.Loadouts)
					{
						if (loadout.Incomplete)
						{
							IPlayerArmorySet pset = Hud.Game.Me.ArmorySets.FirstOrDefault(s => s.Index == loadout.Index);
							if (pset is object)
							{
								//add missing items
								foreach (IItem item in Hud.Game.Items.Where(i => pset.ContainsItem(i) && !loadout.Items.ContainsKey(i.Seed)))
									loadout.Items.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
								
								//update the loadout completion status
								loadout.Incomplete = loadout.Items.Count != pset.ItemAnnIds.Count();
							}
						}
					}
				}
			}
			else if (Hud.Window.IsForeground && Hud.Game.CurrentGameTick - LastPickupCheck > 30)
			{
				LastPickupCheck = Hud.Game.CurrentGameTick;
				
				SearchSnapshot snapshot = Snapshots[Hud.Game.Me.HeroId];
				foreach (IItem item in Hud.Inventory.ItemsInInventory)
				{
					if (!snapshot.Inventory.ContainsKey(item.Seed))
						snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
					//snapshot.Inventory[item.Seed] = GenerateItemSearchTerms(item);
					//OnItemLocationChanged(IItem item, ItemLocation from, ItemLocation to)
				}
			}
		}
		
		//this does not cover item pickup and salvage events
		public void OnItemLocationChanged(IItem item, ItemLocation from, ItemLocation to)
		{
			/* public enum ItemLocation : int
			Floor = -1,
			Inventory = 0,
			Head = 1,
			Torso = 2,
			RightHand = 3,
			LeftHand = 4,
			Hands = 5,
			Waist = 6,
			Feet = 7,
			Shoulders = 8,
			Legs = 9,
			Bracers = 10,
			LeftRing = 11,
			RightRing = 12,
			Neck = 13,
			MerchantBuyback = 14,
			Stash = 15,
			Gold = 16,
			MerchantAvaibleItemsForPurchase = 17,
			Merchant = 18,
			PtrVendor = 19,
			InSocket = 20,
			Unknown1 = 21,
			Unknown2 = 22,
			PetRightHand = 23,
			PetLeftHand = 24,
			PetSpecial = 25,
			PetNeck = 26,
			PetRightRing = 27,
			PetLeftRing = 28,
			VendorToken = 1000
			*/
			
			//0 = inventory
			//1-13 = equipped
			//23-28 = follower, 21 and 22 are the current 1-2 slots for follower in town
			SearchSnapshot snapshot = Snapshots[Hud.Game.Me.HeroId];
			
			if (from == ItemLocation.Inventory)
				snapshot.Inventory.Remove(item.Seed);
			else if ((int)from > 0 && (int)from < 14)
				snapshot.Equipped.Remove(item.Seed);
			else if (from == ItemLocation.InSocket)
			{
				//todo: remove from data storage
			}
			else if ((int)from > 22 && (int)from < 29)
				snapshot.Follower.Remove(item.Seed);
			
			if (to == ItemLocation.Inventory)
			{
				if (!snapshot.Inventory.ContainsKey(item.Seed))
					snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
			}
			else if ((int)to > 0 && (int)to < 14)
			{
				if (!snapshot.Equipped.ContainsKey(item.Seed))
					snapshot.Equipped.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
			}
			else if ((int)to > 20 && (int)to < 29)
			{
				if (!snapshot.Follower.ContainsKey(item.Seed))
					snapshot.Follower.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
			}
			
			//lastFrom = from;
			//lastTo = to;
		}
		
		public void OnItemPicked(IItem item)
		{
			SearchSnapshot snapshot = Snapshots[Hud.Game.Me.HeroId];
			if (!snapshot.Inventory.ContainsKey(item.Seed))
				snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
			//looking for an armory save 
			var armoryprompt = Hud.Render.GetUiElement("Root.TopLayer.textInputPrompt.stack.footer.button_stack.ok"); //Hud.Render.GetUiElement("Root.TopLayer.textInputPrompt");
			if (armoryprompt.Visible)
			{
				if (Hud.Window.CursorInsideRect(armoryprompt.Rectangle.X, armoryprompt.Rectangle.Y, armoryprompt.Rectangle.Width, armoryprompt.Rectangle.Height))
				{
					ArmoryPromptOKed = Hud.Game.CurrentGameTick; //true;
					//ArmoryPromptOKed = Hud.Window.CursorInsideRect(armoryprompt.Rectangle.X, armoryprompt.Rectangle.Y, armoryprompt.Rectangle.Width, armoryprompt.Rectangle.Height);
					//TextLayout testlayout = SearchFont.GetTextLayout(ArmoryPromptOKed.ToString()); //(item.Location is object).ToString()
					//SearchFont.DrawText(testlayout, Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f);
				}
			}
			else if (ArmoryPromptOKed > 0 && Hud.Game.CurrentGameTick - ArmoryPromptOKed > 30) //!string.IsNullOrEmpty(ArmoryPromptOKed))
			{
				//Hud.Render.GetUiElement("Root.NormalLayer.equipmentManager_mainPage.loadout_name").ReadText(System.Text.Encoding.Default, false);
				
				IPlayerArmorySet aset = Hud.Game.Me.ArmorySets.FirstOrDefault(s => s.Name == Hud.Render.GetUiElement("Root.NormalLayer.equipmentManager_mainPage.loadout_name").ReadText(System.Text.Encoding.Default, false)); //ArmoryPromptOKed);
				if (aset is object)
					SaveSnapshot(aset);
				
				ArmoryPromptOKed = 0; //null;
			}
			
			bool isStashOpen = Hud.Inventory.StashMainUiElement.Visible;
			bool isInventoryOpen = Hud.Inventory.InventoryMainUiElement.Visible;
			
			//TODO - if stash is not open, show stash results in top box
			//TODO - if inventory is not open, show current char inventory results in top box
			//only process searches if stash or inventory are open
			if (!isStashOpen && !isInventoryOpen) 
			{
				SearchString = null;
				Search = null;
				SelectedIndex = -1;
				SelectedHero = 0;
				LastChatParams = null;
				LastSearchParams = null;
				return;
			}
			
			if (clipState == ClipState.AfterClip)
			{
				//draw armory stuff on top of tooltips
				if (ShowArmoryInTooltips)
				{
					//var title = Hud.Inventory.GetHoveredItemTopUiElement();
					//if (title is object)
					//{
						//TextLayout layout = SearchFont.GetTextLayout(string.IsNullOrEmpty(((int)item.Location).ToString()).ToString()); //(item.Location is object).ToString()
						//IItem test = Hud.Game.Items.FirstOrDefault(i => i.Seed == 798213911);
						//if (test is object)
						//{
							//TextLayout test = SearchFont.GetTextLayout(string.Format("lastFrom = {0}, lastTo = {1}", lastFrom, lastTo));
							//SearchFont.DrawText(test, Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f);
						//}
					//}
					
					
					IItem item = Hud.Inventory.HoveredItem;
					if (item == null)
						return;

					//var uicMain = Hud.Inventory.GetHoveredItemMainUiElement();
					//ui = Hud.Render.GetUiElement("Root.TopLayer.tooltip_dialog_background.tooltip_2");
					var tooltip = Hud.Inventory.GetHoveredItemMainUiElement(); //Hud.Inventory.GetHoveredItemTopUiElement(); //Hud.Render.GetUiElement("Root.TopLayer.tooltip_dialog_background.tooltip_2");
					if (tooltip is object)
					{
						float x = tooltip.Rectangle.Right - tooltip.Rectangle.Width*0.045f; //tooltip.Rectangle.Left + tooltip.Rectangle.Width*0.045f;
						float y = tooltip.Rectangle.Top + tooltip.Rectangle.Width*(IsRerollCalculatorEnabled && item.SnoItem.HasGroupCode("weapons") ? 0.525f : 0.325f); //Hud.Inventory.GetHoveredItemTopUiElement().Rectangle.Bottom + tooltip.Rectangle.Width*0.225f; //tooltip.Rectangle.Bottom - tooltip.Rectangle.Width*0.16f;
						
						//var hint = Hud.Render.GetUiElement("Root.TopLayer.item 2.stack.frame_instruction");
						//if (hint is object && hint.Visible)
						//	y -= hint.Rectangle.Height; //tooltip.Rectangle.Width*0.09f;
						
						foreach (SearchSnapshot snapshot in Snapshots.Values)
						{
							IFont font = snapshot.HeroId == Hud.Game.Me.HeroId ? ArmoryTooltipMe : ArmoryTooltipOther;
							IEnumerable<ArmorySet> loadouts = snapshot.Loadouts.Where(l => !string.IsNullOrEmpty(l.Name) && l.Items.ContainsKey(item.Seed));
							int count = loadouts.Count();
							
							foreach (ArmorySet loadout in loadouts)
							{
								TextLayout layout = font.GetTextLayout(string.Format("{0}: {1} ({2})", snapshot.HeroName, loadout.Name, ToRoman(loadout.Index+1)));
								font.DrawText(layout, x - layout.Metrics.Width, y); //x, y);
								y += layout.Metrics.Height; //-=
							}
						}
					}

				}
			}

			//rendering of inventory/stash markers only happens during the Inventory clipstate
			if (clipState != ClipState.Inventory) return;
			
			//defaults
			if (SelectedHero == 0)
			{
				SelectedHero = Hud.Game.Me.HeroId;
				SelectedIndex = -1;
			}
			if (ButtonWidth == 0)
			{
				TextLayout layout = InstructionFont.GetTextLayout("VVVV");
				ButtonWidth = layout.Metrics.Width;
				ButtonHeight = layout.Metrics.Height*1.25f;
			}
			
			IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
			bool isChatOpen = ui.Visible;
			if (isChatOpen)
			{
				string chatText = GetText(ui, SearchCmd, 0);
				if (chatText != LastChatParams)
				{
					LastChatParams = chatText;
					
					if (chatText is object)
					{
						//chatSearchChanged = true;
						//ShowFilterSearch = false;
						if (chatText.Length >= MinimumSearchLength)
						{
							LastSearchTypeIsChat = true;
							ProcessSearchString(chatText);
						}
						else
						{
							SearchString = null;
							Search = null;							
						}
					}
					//when the chatline closes from pressing Enter, there is a brief moment when ReadText returns an empty string before ui.Visible == false
					/*else if (LastSearchTypeIsChat)
					{
						SearchString = null;
						Search = null;
					}*/
				}				
			}
			
			//figure out which items to mark and how to render them
			Func<IItem, bool> evaluator = null;
			Action<RectangleF> highlighter = Highlight;
			Action<RectangleF> muter = DoNothing;
			int stashCount = 0;
			int inventoryCount = 0;
			int equippedCount = 0;
			
			if (isStashOpen)
			{
				//check stash filter for a search term
				//if (!chatSearchChanged)
				//{
					ui = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.StashFilter.FilterInput");
					if (ui.Visible)
					{
						string searchText = GetText(ui, null, MinimumSearchLength);
						if (searchText != LastSearchParams)
						{
							LastSearchParams = searchText;

							if (searchText is object)
							{
								//stashSearchChanged = true;
								LastSearchTypeIsChat = false;
								//ShowFilterSearch = true;
								ProcessSearchString(searchText);
							}
							else if (!LastSearchTypeIsChat)
							{
								SearchString = null;
								Search = null;
							}
						}
						
						//draw icons indicating game mode filtering
						/*float iconX = ui.Rectangle.Right; // - ui.Rectangle.Height*0.6f;
						if (Hud.Game.Me.HeroIsHardcore)
						{
							ITexture modeIcon = Hud.Texture.GetTexture(640380019);
							float iconH = ui.Rectangle.Height*0.75f;
							float iconW = modeIcon.Width*(iconH/modeIcon.Height);
							//iconX -= iconW*1.15f;
							modeIcon.Draw(iconX - iconW*1.15f, ui.Rectangle.Y + ui.Rectangle.Height*0.5f - iconH*0.5f, iconW, iconH);
							//HighlightBrush.DrawRectangle(iconX, ui.Rectangle.Y + ui.Rectangle.Height*0.5f - iconH*0.5f, iconW, iconH);
						}
						if (Hud.Game.Me.Hero.Seasonal)
						{
							ITexture modeIcon = Hud.Texture.GetTexture(214823302);
							float iconH = modeIcon.Height*0.95f;
							float iconW = modeIcon.Width*(iconH/modeIcon.Height);
							//iconX -= iconW;
							modeIcon.Draw(iconX, ui.Rectangle.Bottom - iconH*1.075f, iconW, iconH); // - iconW*0.275f
							//HighlightBrush.DrawRectangle(iconX, ui.Rectangle.Bottom - iconH*1.075f, iconW, iconH); // + iconW*0.25f
							//iconX += iconW*0.35f;
						}*/
						/* //old
						float iconX = ui.Rectangle.X;
						if (Hud.Game.Me.Hero.Seasonal)
						{
							ITexture modeIcon = Hud.Texture.GetTexture(214823302);
							float iconH = modeIcon.Height*0.95f;
							float iconW = modeIcon.Width*(iconH/modeIcon.Height);
							iconX -= iconW;
							modeIcon.Draw(iconX + iconW*0.275f, ui.Rectangle.Bottom - iconH*1.075f, iconW, iconH);
							//HighlightBrush.DrawRectangle(iconX + iconW*0.25f, ui.Rectangle.Bottom - iconH*1.075f, iconW, iconH);
							iconX += iconW*0.35f;
						}
						if (Hud.Game.Me.HeroIsHardcore)
						{
							ITexture modeIcon = Hud.Texture.GetTexture(640380019);
							float iconH = ui.Rectangle.Height*0.75f;
							float iconW = modeIcon.Width*(iconH/modeIcon.Height);
							iconX -= iconW;
							modeIcon.Draw(iconX, ui.Rectangle.Y + ui.Rectangle.Height*0.5f - iconH*0.5f, iconW, iconH);
							//HighlightBrush.DrawRectangle(iconX, ui.Rectangle.Y + ui.Rectangle.Height*0.5f - iconH*0.5f, iconW, iconH);
						}*/
					}
				//}
				
				
				if (!isChatOpen)
				{
					ui = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.button_stash");

					//draw hero heads to represent current armory sets
					int count = Snapshots.Count;
					float buttonX = ui.Rectangle.X + ui.Rectangle.Width*0.5f - (ButtonWidth*count + ButtonGap*(count-1))*0.5f;
					float buttonY = ui.Rectangle.Y + ui.Rectangle.Height + ButtonHeight*0.5f; //layout.Metrics.Height*2
					float height = ButtonHeight*1.2f;
					uint hoveredHero = 0;
					foreach (SearchSnapshot ss in Snapshots.Values)
					{
						ITexture texture = GetHeroHead(ss.HeroClass, ss.HeroIsMale);
						if (texture is object)
						{
							if (ss.HeroId == SelectedHero)
								MuteBrush.DrawRectangle(buttonX, buttonY, ButtonWidth, height);

							float width = texture.Width*(height/texture.Height);
							texture.Draw(buttonX + ButtonWidth*0.5f - width*0.5f, buttonY, width, height); //ButtonHeight
							
							if (Hud.Window.CursorInsideRect(buttonX, buttonY, ButtonWidth, height))
							{
								ShadowBrush.DrawRectangle(buttonX, buttonY, ButtonWidth, height);
								
								//draw name tooltip
								Hud.Render.SetHint(string.Format("{0} ({1} {2})", ss.HeroName, TextSeason, ss.HeroSeason));
								hoveredHero = ss.HeroId;
							}
							
							buttonX += ButtonGap + ButtonWidth;
						}
					}

					//draw armory search shortcut buttons
					SearchSnapshot snapshot = Snapshots[SelectedHero];
					count = 0;
					foreach (ArmorySet loadout in snapshot.Loadouts) //Hud.Game.Me.ArmorySets
					{
						if (!string.IsNullOrEmpty(loadout.Name))
							++count;
					}
					
					buttonX = ui.Rectangle.X + ui.Rectangle.Width*0.5f - (ButtonWidth*count + ButtonGap*(count-1))*0.5f;
					buttonY = ui.Rectangle.Y + ui.Rectangle.Height + ButtonHeight*2; //layout.Metrics.Height*2
					
					int hoveredIndex = -1;
					foreach (ArmorySet loadout in snapshot.Loadouts) //IPlayerArmorySet in Hud.Game.Me.ArmorySets
					{
						if (string.IsNullOrEmpty(loadout.Name)) continue;
						
						//outline
						ShadowBrush.DrawRectangle(buttonX, buttonY, ButtonWidth, ButtonHeight);
						
						if (loadout.Index == SelectedIndex)
							MuteBrush.DrawRectangle(buttonX, buttonY, ButtonWidth, height);
						
						TextLayout layout = InstructionFont.GetTextLayout(ToRoman(loadout.Index + 1)); //IPlayerArmorySet.Index is zero-based
						InstructionFont.DrawText(layout, buttonX + ButtonWidth*0.5f - layout.Metrics.Width*0.5f, buttonY + ButtonHeight*0.5f - layout.Metrics.Height*0.5f);
						
						if (Hud.Window.CursorInsideRect(buttonX, buttonY, ButtonWidth, ButtonHeight))
						{
							//remember that this index was hovered for click detection
							hoveredIndex = loadout.Index;
							
							//draw name tooltip
							//Hud.Render.SetHint(loadout.Name);
							//Hud.Render.SetHint(string.Format("{0} ({1}) {2}", loadout.Name, loadout.Items.Count, loadout.Incomplete.ToString()));
							Hud.Render.SetHint(string.Format("{0} ({1})", loadout.Name, loadout.Items.Count));
						}
						
						buttonX += ButtonWidth + ButtonGap;
						//count += 1;
					}
					
					//click detection
					/*if (LButtonPressed)
					{
						if (!Hud.Input.IsKeyDown(Keys.LButton))
						{
							//OnMouseUp();
							LButtonPressed = false;
						}
					}
					else if (Hud.Input.IsKeyDown(Keys.LButton))
					{
						//OnMouseDown();
						if (hoveredIndex > -1)
						{
							SelectedIndex = hoveredIndex;
							SearchString = null;
							Search = null;
						}
						else if (hoveredHero > 0)
						{
							if (hoveredHero != SelectedHero)
							{
								SelectedHero = hoveredHero;
								SelectedIndex = -1;
							}
							
						}
						
						LButtonPressed = true;
					}*/
					
					if (LButtonPressed)
					{
						LButtonPressed = false;
						
						if (hoveredIndex > -1)
						{
							SelectedIndex = hoveredIndex;
							SearchString = null;
							Search = null;
						}
						else if (hoveredHero > 0)
						{
							if (hoveredHero != SelectedHero)
							{
								SelectedHero = hoveredHero;
								SelectedIndex = -1;
							}
							
						}
					}
				}
				
				//mark selected armory loadout items
				if (SelectedIndex > -1)
				{
					ArmorySet loadout = Snapshots[SelectedHero].Loadouts[SelectedIndex]; //Hud.Game.Me.ArmorySets[SelectedIndex];
					evaluator = (item) => loadout.ContainsItem(item);
					highlighter = Highlight;
					muter = Mute;
					
					//show query title at the top
					//ui = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.button_stash");
					TextLayout layout = SearchFont.GetTextLayout(string.Format("Showing Armory Set {0}: \"{1}\"", ToRoman(loadout.Index + 1), loadout.Name));
					//SearchFont.DrawText(layout, ui.Rectangle.X + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
					SearchFont.DrawText(layout, Hud.Window.Size.Width * 0.019f + Hud.Window.Size.Width*(0.255f - 0.019f)*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
					
					//show results for armory items on different characters' inventory or paperdoll
					MuteBrush.DrawRectangle(Hud.Window.Size.Width * 0.019f, Hud.Window.Size.Height * 0.026f, Hud.Window.Size.Width * (0.255f - 0.019f), Hud.Window.Size.Height * (0.097f - 0.026f));

					float x = Hud.Window.Size.Width * 0.0225f;
					float y = Hud.Window.Size.Height * 0.03f;

					float maxWidth = 0;
					int i = 0;
					float y2 = y;
					HashSet<int> missingItems = loadout.Items.Keys.Where(k => !Hud.Game.Items.Any(item => item.Seed == k)).ToHashSet();
					//IEnumerable<SearchSnapshot> otherHeroSnapshots = Snapshots.Values.Where(s => s.HeroId != SelectedHero);
					foreach (SearchSnapshot snapshot in Snapshots.Values.Where(s => s.HeroId != SelectedHero))
					{
						List<string> results = new List<string>();
						int count = snapshot.Inventory.Keys.Count(k => missingItems.Contains(k));
						if (count > 0)
							results.Add(string.Format("{0}({1})", TextInventory, count));
						count = snapshot.Equipped.Keys.Count(k => missingItems.Contains(k));
						if (count > 0)
							results.Add(string.Format("{0}({1})", TextEquipped, count));
						count = snapshot.Follower.Keys.Count(k => missingItems.Contains(k));
						if (count > 0)
							results.Add(string.Format("{0}({1})", TextFollower, count));
						
						string result = string.Join(", ", results);
						if (!string.IsNullOrEmpty(result))
						{
							ITexture texture = GetHeroHead(snapshot.HeroClass, snapshot.HeroIsMale);
							if (texture is object)
							{
								float width = texture.Width*(ButtonHeight/texture.Height);
								
								result = snapshot.HeroName + ": " + result;
								layout = InstructionFont.GetTextLayout(result);
								if (i++ < 3)
								{
									if (maxWidth < layout.Metrics.Width)
										maxWidth = layout.Metrics.Width;
								}
								else
								{
									x += maxWidth + ButtonGap*3 + ButtonWidth;
									y2 = y;
									maxWidth = layout.Metrics.Width;
									i = 0;
								}
								texture.Draw(x, y2, width, ButtonHeight);
								InstructionFont.DrawText(layout, x + ButtonGap + width, y2 + ButtonHeight*0.5f - layout.Metrics.Height*0.5f);
								
								y2 += ButtonHeight + ButtonGap;
							}
						}
					}
				}
				else if (Search is object)
				{
					evaluator = SearchItem;
					
					if (LastSearchTypeIsChat)
					{
						//highlighter = Highlight;
						muter = Mute;
					}
					

				}
				else if (!string.IsNullOrEmpty(SearchString))
				{
					//show error message
					//ui = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.button_stash");
					TextLayout layout = SearchFont.GetTextLayout(string.Format("{0} {1}", TextError, SearchString));
					//SearchFont.DrawText(layout, ui.Rectangle.X + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
					SearchFont.DrawText(layout, Hud.Window.Size.Width * 0.019f + Hud.Window.Size.Width*(0.255f - 0.019f)*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
				}
				
				if (evaluator is object)
				{
					int[] resultsCountPerTab = new int[Hud.Inventory.MaxStashTabCountPerPage*Hud.Inventory.MaxStashPageCount];					
					var selectedPage = Hud.Inventory.SelectedStashPageIndex;
					var selectedTab = Hud.Inventory.SelectedStashTabIndex;
					
					selectedTab += selectedPage * Hud.Inventory.MaxStashTabCountPerPage;
					
					foreach (IItem item in Hud.Inventory.ItemsInStash)
					{
						var tabIndex = item.InventoryY / 10;
						
						//if this is not the currently selected tab, count up the matching results, but don't render anything
						if (tabIndex != selectedTab) 
						{
							if (evaluator(item))
							{
								resultsCountPerTab[tabIndex] += 1;
								++stashCount;
							}
							else if (item.SocketCount > 0 && item.ItemsInSocket != null) 
							{ //&& item.ItemsInSocket.Length > 0) {
								foreach (IItem socketedItem in item.ItemsInSocket)
								{
									if (evaluator(socketedItem))
									{
										resultsCountPerTab[tabIndex] += 1;
										++stashCount;
										break;
									}
								}
							}
							
							continue;
						}
						
						//this is the currently selected tab, do the search and rendering
						var rect = Hud.Inventory.GetItemRect(item);
						
						if (evaluator(item))
						{
							highlighter(rect);
							resultsCountPerTab[selectedTab] += 1;
							++stashCount;
						} else {
							bool isMatchSocketed = false;
							if (item.SocketCount > 0 && item.ItemsInSocket != null)
							{ //&& item.ItemsInSocket.Length > 0) {
								foreach (IItem socketedItem in item.ItemsInSocket)
								{
									if (evaluator(socketedItem))
									{
										highlighter(rect);
										isMatchSocketed = true;
										resultsCountPerTab[selectedTab] += 1;
										++stashCount;
										break;
									}
								}
							}
							
							//grey out the item
							if (!isMatchSocketed)
								muter(rect);
						}
					}
					
					//show search results summaries
					for (int i = 0; i < Hud.Inventory.MaxStashPageCount; ++i)
					{
						int count = 0;

						if (i == Hud.Inventory.SelectedStashPageIndex) {
							for (int j = 0; j < Hud.Inventory.MaxStashTabCountPerPage; ++j)
							{
								int k = j + i * Hud.Inventory.MaxStashTabCountPerPage;
								if (resultsCountPerTab[k] > 0)
								{
									//side tab
									count += resultsCountPerTab[k];
									ui = Hud.Inventory.GetStashTabUiElement(j);
									TextLayout layout = SearchFont.GetTextLayout(resultsCountPerTab[k].ToString()); // + (resultsCountPerTab[k] > 1 ? " results" : " result")
									float width = ui.Rectangle.Width*0.44f;
									float height = layout.Metrics.Height*1.2f;
									BgBrush.DrawRectangle(ui.Rectangle.X + ResultsPerTabCountOffsetX, ui.Rectangle.Y + ui.Rectangle.Height - height + ResultsPerTabCountOffsetY, width, height);
									SearchFont.DrawText(layout, ui.Rectangle.X + ResultsPerTabCountOffsetX + width*0.5f - layout.Metrics.Width*0.5f, ui.Rectangle.Y + ui.Rectangle.Height - height*0.5f - layout.Metrics.Height*0.5f + ResultsPerTabCountOffsetY);
								}
							}
						}
						else 
						{
							for (int j = 0; j < Hud.Inventory.MaxStashTabCountPerPage; ++j) 
							{
								int k = j + i * Hud.Inventory.MaxStashTabCountPerPage;
								if (resultsCountPerTab[k] > 0) 
									count += resultsCountPerTab[k];
							}
						}
						
						if (count > 0) 
						{
							//top tab
							ui = Hud.Inventory.GetStashPageUiElement(i);
							TextLayout layout = SearchFont.GetTextLayout(count.ToString()); // + (count > 1 ? " results" : " result")
							float width = ui.Rectangle.Width*0.7f;
							float height = layout.Metrics.Height*1.2f;
							BgBrush.DrawRectangle(ui.Rectangle.X + ui.Rectangle.Width*0.5f - width*0.5f, ui.Rectangle.Y + ui.Rectangle.Height - height, width, height);
							SearchFont.DrawText(layout, ui.Rectangle.X + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, ui.Rectangle.Y + ui.Rectangle.Height - height*0.5f - layout.Metrics.Height*0.5f);
						}
					}
				}
				
				if (MarkArmorySets)
				{
					var selectedPage = Hud.Inventory.SelectedStashPageIndex;
					var selectedTab = Hud.Inventory.SelectedStashTabIndex;
					//IUiElement chatwindow = Hud.Render.GetUiElement("Root.NormalLayer.chatoutput_dialog_backgroundScreen.chat_content.MessageListContainer.chat_messagelist");
						
					selectedTab += selectedPage * Hud.Inventory.MaxStashTabCountPerPage;
						
					foreach (IItem item in Hud.Inventory.ItemsInStash) {
						
						var tabIndex = item.InventoryY / 10;
						if (tabIndex != selectedTab) continue;
							
						//this is the currently selected tab, do rendering					
						MarkArmoryItem(item);
					}
				}
			} //end: isStashOpen
			else if (Search is object)
			{
				evaluator = SearchItem;
				
				if (LastSearchTypeIsChat)
				{
					//highlighter = Highlight;
					muter = Mute;
				}
				

			}
			else if (!string.IsNullOrEmpty(SearchString))
			{
				//show error message
				//ui = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.button_stash");
				TextLayout layout = SearchFont.GetTextLayout(string.Format("{0} {1}", TextError, SearchString));
				//SearchFont.DrawText(layout, ui.Rectangle.X + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
				SearchFont.DrawText(layout, Hud.Window.Size.Width * 0.019f + Hud.Window.Size.Width*(0.255f - 0.019f)*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
			}
			
			if (isInventoryOpen)
			{
				if (evaluator is object)
				{
					highlighter = Highlight;
					muter = Mute;
					
					foreach (IItem item in Hud.Inventory.ItemsInInventory)
					{
						var rect = Hud.Inventory.GetItemRect(item);
						
						if (evaluator(item))
						{
							highlighter(rect);
							++inventoryCount;
						}
						else
						{
							bool isMatchSocketed = false;
							if (item.SocketCount > 0 && item.ItemsInSocket != null) //&& item.ItemsInSocket.Length > 0) {
							{
								foreach (IItem socketedItem in item.ItemsInSocket)
								{
									if (evaluator(socketedItem))
									{
										highlighter(rect);
										isMatchSocketed = true;
										++inventoryCount;
										break;
									}
								}
							}
							
							//grey out the item
							if (!isMatchSocketed)
								muter(rect);
						}
					}
					
					//check equipped items
					var equippedItems = Hud.Game.Items.Where(x => (int)x.Location > 0 && (int)x.Location < 14); //casting enums is not kosher but saves the code from having to check all the different possible paperdoll slots individually
					foreach (IItem item in equippedItems)
					{
						var rect = Hud.Inventory.GetItemRect(item);

						if (evaluator(item))
						{
							highlighter(rect);
							++equippedCount;
						}
						else
						{
							bool isMatchSocketed = false;
							if (item.SocketCount > 0 && item.ItemsInSocket != null) //&& item.ItemsInSocket.Length > 0) {
							{
								foreach (IItem socketedItem in item.ItemsInSocket)
								{
									if (evaluator(socketedItem))
									{
										highlighter(rect);
										isMatchSocketed = true;
										++equippedCount;
										break;
									}
								}
							}
							
							//grey out the item
							if (!isMatchSocketed)
								muter(rect);
						}
					}
				}
				
				if (MarkArmorySets)
				{
					foreach (IItem item in Hud.Inventory.ItemsInInventory)
						MarkArmoryItem(item);
					
					//check equipped items
					var equippedItems = Hud.Game.Items.Where(x => (int)x.Location > 0 && (int)x.Location < 14); //casting enums is not kosher but saves the code from having to check all the different possible paperdoll slots individually
					foreach (IItem item in equippedItems)
						MarkArmoryItem(item);
				}
				
				//track item salvaging
				IUiElement uiSalvageText = Hud.Render.GetUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.instruction");
				if (uiSalvageText.Visible)
				{
					string txt = uiSalvageText.ReadText(System.Text.Encoding.Default, false);
					if (txt != SalvageInstruction)
					{
						if (!string.IsNullOrEmpty(SalvageInstruction))
						{
							//salvage mode, check inventory and paperdoll for deletions
							CheckForDeletion = true;
						}
						else
						{
							SalvageInstruction = txt;
						}
					}
				}
				
				//rate limited for performance
				if (CheckForDeletion && Hud.Game.CurrentGameTick - LastSalvageCheck > 30)
				{
					CheckForDeletion = false;
					LastSalvageCheck = Hud.Game.CurrentGameTick;
					
					SearchSnapshot snapshot = Snapshots[Hud.Game.Me.HeroId];
					
					foreach (int seed in snapshot.Inventory.Keys.ToArray())
					{
						if (!Hud.Game.Items.Any(i => i.Seed == seed))
							snapshot.Inventory.Remove(seed);
					}
					
					foreach (int seed in snapshot.Equipped.Keys.ToArray())
					{
						if (!Hud.Game.Items.Any(i => i.Seed == seed))
							snapshot.Equipped.Remove(seed);
					}
				}
			} //end is inventory open
			
			//search other character's inventories and show summary
			if (Search is object)
			{
				float x = Hud.Window.Size.Width * 0.0225f;
				float y = Hud.Window.Size.Height * 0.03f;

				//show search query
				//ui = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.button_stash");
				//TextLayout layout = SearchFont.GetTextLayout(string.Format("Showing Armory Set {0}: \"{1}\"", ToRoman(loadout.Index + 1), loadout.Name));
				//SearchFont.DrawText(layout, ui.Rectangle.X + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
				string query = "⌕ \"" + (SearchString is object ? SearchString : "null") + "\"";
				foreach (Tuple<string, string> pm in Parameters)
					query += string.Format(" {0}\"{1}\"", pm.Item1, pm.Item2);
					
				TextLayout layout = SearchFont.GetTextLayout(query); //"⌕ \"" + (SearchString is object ? SearchString : "null") + "\"" ); // Search results for
				if (FontSize == 0)
					FontSize = (float)layout.GetFontSize(0);
				layout.SetFontSize(FontSize*1.25f, new TextRange(0, 1));

				float bgWidth = Hud.Window.Size.Width * (0.255f - 0.019f);
				float stashPanelRight = 0;
				if (isStashOpen)
				{
					MuteBrush.DrawRectangle(Hud.Window.Size.Width * 0.019f, Hud.Window.Size.Height * 0.026f, bgWidth, Hud.Window.Size.Height * (0.097f - 0.026f));
					IUiElement stashpanel = Hud.Render.GetUiElement("Root.NormalLayer.stash_dialog_mainPage.panel");
					stashPanelRight = stashpanel.Rectangle.Right;
				}
				else
				{
					IUiElement chatwindow = Hud.Render.GetUiElement("Root.NormalLayer.chatoutput_dialog_backgroundScreen.chat_content.MessageListContainer.chat_messagelist");
					float bgY = Hud.Window.Size.Height * 0.025f - layout.Metrics.Height;
					MuteBrush.DrawRectangle(Hud.Window.Size.Width * 0.019f, bgY, bgWidth, chatwindow.Rectangle.Y - bgY);
				}
				
				//SearchFont.DrawText(layout, x, y - layout.Metrics.Height*1.2f);
				//SearchFont.DrawText(layout, ui.Rectangle.X + ui.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);
				SearchFont.DrawText(layout, Hud.Window.Size.Width * 0.019f + Hud.Window.Size.Width*(0.255f - 0.019f)*0.5f - layout.Metrics.Width*0.5f, Hud.Window.Size.Height * 0.025f - layout.Metrics.Height);

				//float maxWidth = 0;
				int i = 0;
				float y2 = y;
				float x2 = x;
				
				//show stash results count
				if (!isStashOpen)
				{
					stashCount = Hud.Inventory.ItemsInStash.Count(item => evaluator(item));
					stashCount += Hud.Inventory.ItemsInStash.Where(item => item.SocketCount > 0 && item.ItemsInSocket is object).Select(item => item.ItemsInSocket.Count(socketedItem => evaluator(socketedItem))).Sum();
				}
				
				//stash chest icon
				if (stashCount > 0)
				{
					ITexture texture = Hud.Texture.GetTexture(2996246533); //GetHeroHead(snapshot.HeroClass, snapshot.HeroIsMale);
					if (texture is object)
					{
						float width = texture.Width*(ButtonHeight/texture.Height);						
						string result = string.Format("{0}({1})", TextStash, stashCount);
						layout = InstructionFont.GetTextLayout(result);
						
						float iconW = texture.Width*0.7f;
						float iconH = texture.Height*0.7f;
						texture.Draw(x2 - iconW*0.3f, y2 - iconH*0.25f, iconW, iconH); //texture.Draw(x, y2, width, ButtonHeight);
						
						InstructionFont.DrawText(layout, x2 + ButtonGap + width, y2 + ButtonHeight*0.5f - layout.Metrics.Height*0.5f);
						
						y2 += ButtonHeight + ButtonGap;
						++i;
					}
				}
				
				//show other characters inventories
				foreach (SearchSnapshot snapshot in Snapshots.Values)
				{
					if (isStashOpen && i++ >= 3 && x == x2)
					{
						x2 = stashPanelRight + ButtonGap*3 + ButtonWidth; //maxWidth + ButtonGap*3 + ButtonWidth;
						y2 = y;
						//maxWidth = layout.Metrics.Width;
						i = 0;
						
						IUiElement chatwindow = Hud.Render.GetUiElement("Root.NormalLayer.chatoutput_dialog_backgroundScreen.chat_content.MessageListContainer.chat_messagelist");
						MuteBrush.DrawRectangle(stashPanelRight, Hud.Window.Size.Height * 0.026f, bgWidth, chatwindow.Rectangle.Y - y);
					}

					List<string> results = new List<string>();
					if (Hud.Game.Me.HeroId == snapshot.HeroId)
					{
						if (inventoryCount > 0)
							results.Add(string.Format("{0}({1})", TextInventory, inventoryCount));
							
						if (equippedCount > 0)
							results.Add(string.Format("{0}({1})", TextEquipped, equippedCount));
						
						//if (count > 0)
						//	results.Add(string.Format("{0}({1})", TextFollower, count));
						
						//continue;
					}
					//not a perfect way to test for saved data seasonality, but it's good enough for now
					else if (FilterResultsByHeroType && (snapshot.HeroIsHardcore != Hud.Game.Me.HeroIsHardcore || (Hud.Game.Me.Hero.Seasonal && snapshot.HeroSeason != Hud.Game.Me.Hero.Season)))
						continue;
					else
					{
						int count = snapshot.Inventory.Values.Count(s => MatchSearchParameters(s.Item1, s.Item2)); //Search.IsMatch(s.Item1)
						if (count > 0)
							results.Add(string.Format("{0}({1})", TextInventory, count));
							
						count = snapshot.Equipped.Values.Count(s => MatchSearchParameters(s.Item1, s.Item2)); //Search.IsMatch(s)
						if (count > 0)
							results.Add(string.Format("{0}({1})", TextEquipped, count));
						
						count = snapshot.Follower.Values.Count(s => MatchSearchParameters(s.Item1, s.Item2)); //Search.IsMatch(s)
						if (count > 0)
							results.Add(string.Format("{0}({1})", TextFollower, count));
					}
					
					if (results.Count > 0)
					{
						string result = string.Join(", ", results);
						if (!string.IsNullOrEmpty(result))
						{
							ITexture texture = GetHeroHead(snapshot.HeroClass, snapshot.HeroIsMale);
							if (texture is object)
							{
								float width = texture.Width*(ButtonHeight/texture.Height);
								//ITexture mode = //seasonal
								//mode = //hardcore
								
								result = snapshot.HeroName + ": " + result;
								layout = InstructionFont.GetTextLayout(result);
								/*if (i++ < 3)
								{
									if (maxWidth < layout.Metrics.Width)
										maxWidth = layout.Metrics.Width;
								}
								else
								{
									x += maxWidth + ButtonGap*3 + ButtonWidth;
									y2 = y;
									maxWidth = layout.Metrics.Width;
									i = 0;
								}*/
								texture.Draw(x2, y2, width, ButtonHeight);
								InstructionFont.DrawText(layout, x2 + ButtonGap + width, y2 + ButtonHeight*0.5f - layout.Metrics.Height*0.5f);
								
								//detect mouse hover over the icon for a tooltip
								if (Hud.Window.CursorInsideRect(x2, y2, width, ButtonHeight))
									Hud.Render.SetHint(string.Format("{0} ({1} {2})", snapshot.HeroName, TextSeason, snapshot.HeroSeason));
								
								y2 += ButtonHeight + ButtonGap;
							}
						}
					}
				}
			}
		}
		
		private bool SearchItem(IItem item)
		{
			bool isMatch = Search.IsMatch(GenerateItemSearchTerms(item));
			
			if (isMatch)
			{
				//now check for special parameters
				foreach (Tuple<string, string> param in Parameters)
				{
					if (param.Item1 == ">")
					{
						//is this item a legendary gem?
						if (item.SnoItem.SnoItemType.Id == 1888008307)
						{
							try {
								//isMatch = isMatch && (item.JewelRank > Int32.Parse(param.Item2));
								if (item.JewelRank <= Int32.Parse(param.Item2))
									return false;
							} catch (FormatException) {
								return false;
							}
						}
					}
					else if (param.Item1 == "<")
					{
						//is this item a legendary gem?
						if (item.SnoItem.SnoItemType.Id == 1888008307)
						{
							try {
								//isMatch = isMatch && (item.JewelRank < Int32.Parse(param.Item2));
								if (item.JewelRank >= Int32.Parse(param.Item2))
									return false;
							} catch (FormatException) {
								return false;
							}
						}
					}
					else if (param.Item1 == "+")
					{
						bool found = false;
						
						if (!string.IsNullOrEmpty(param.Item2) && item.Perfections is object) //unidentified items have item.Perfections = null
						{
							//match item stat text
							Regex r = new Regex(Regex.Replace(param.Item2, @"\s+", ".+?"), RegexOptions.IgnoreCase);

							foreach (IItemPerfection perfection in item.Perfections)
							{
								string description = perfection.Attribute.GetDescription(perfection.Modifier);
								if (string.IsNullOrEmpty(description))
								{
									//check if it is crit chance, we may have to define the text ourselves
									if (perfection.Attribute == Hud.Sno.Attributes.Crit_Percent_Bonus_Capped)
									{
										//if (Regex.IsMatch(TextCritChance, param.Item2, RegexOptions.IgnoreCase))
										if (r.IsMatch(TextCritChance))
										{
											found = true;
											break;
										}
									}
								}
								else if (r.IsMatch(description)) //Regex.IsMatch(description, param.Item2, RegexOptions.IgnoreCase))
								{
									found = true;
									break;
								}
							}
						}
						
						if (!found)
							return false;
					}
				}
			}
			
			return isMatch;
		}
		
		private bool MatchSearchParameters(string name, string stats)
		{
			bool isMatch = Search.IsMatch(name);
			
			if (isMatch)
			{
				//now check for special parameters
				foreach (Tuple<string, string> param in Parameters)
				{
					if (param.Item1 == ">")
					{
						//is this item a legendary gem?
						Match match = RegexNumber.Match(name);
						if (match.Success)
						{
							try {
								//jewel rank > param rank
								if (Int32.Parse(match.Groups[1].Value) <= Int32.Parse(match.Groups[1].Value))
									return false;
								//int jewelRank = Int32.Parse(match.Groups[1].Value);
								//isMatch = isMatch && (jewelRank > Int32.Parse(param.Item2));
							} catch (FormatException) {
								return false;
							}
						}
						else
							return false;
					}
					else if (param.Item1 == "<")
					{
						//is this item a legendary gem?
						Match match = RegexNumber.Match(name);
						if (match.Success)
						{
							try {
								//jewel rank < param rank
								if (Int32.Parse(match.Groups[1].Value) >= Int32.Parse(match.Groups[1].Value))
									return false;
								//int jewelRank = Int32.Parse(match.Groups[1].Value);
								//isMatch = isMatch && (jewelRank < Int32.Parse(param.Item2));
							} catch (FormatException) {
								return false;
							}
						}
						else
							return false;
					}
					else if (param.Item1 == "+")
					{
						//match item stat text
						Regex r = new Regex(Regex.Replace(param.Item2, @"\s+", ".+?"), RegexOptions.IgnoreCase | RegexOptions.Singleline);
						if (!string.IsNullOrEmpty(param.Item2) && !string.IsNullOrEmpty(stats) && !r.IsMatch(param.Item2)) //!Regex.IsMatch(stats, param.Item2, RegexOptions.IgnoreCase))
							return false;
					}
				}
			}
			
			return isMatch;
		}
		
		private void Highlight(RectangleF rect)
		{
			HighlightBrush.DrawRectangle(rect);
		}
		
		private void Mute(RectangleF rect)
		{
			MuteBrush.DrawRectangle(rect);
		}
		
		private void DoNothing(RectangleF rect)
		{
		}
			
		private void ProcessSearchString(string text)
		{
			//deselect armory index
			SelectedIndex = -1;
			
			//process the search string
			//SearchString = Regex.Replace(text, @"\|H[^\|]+", ""); //remove item start tag //|HItem
			//SearchString = Regex.Replace(SearchString, @"\|h", ""); //remove item end tag
			//SearchString = Regex.Replace(SearchString, @"\{\/?c[^\}]*\}", ""); //removes color tags
			//SearchString = Regex.Replace(SearchString, @"[\{\[\]\}]", ""); //removes {{[item name]}} ancient rank markers
			
			SearchString = RegexItemLinkStartTag.Replace(text, ""); //remove item start tag //|HItem
			SearchString = RegexItemLinkEndTag.Replace(SearchString, ""); //remove item start tag //|HItem
			SearchString = RegexColorTag.Replace(SearchString, ""); //removes color tags
			SearchString = RegexItemLinkQuality.Replace(SearchString, ""); //removes {{[item name]}} ancient rank markers
			
			//get the parameters
			//Regex r = new Regex(@"([\+\>\<])(\w+)");
			Parameters.Clear();
			foreach (Match match in RegexParameters.Matches(SearchString))
			{
				if (!string.IsNullOrEmpty(match.Groups[2].Value))
					Parameters.Add(Tuple.Create(match.Groups[1].Value, match.Groups[2].Value.Trim()));
			}
			
			//SearchString = Regex.Replace(SearchString, @"([\-\+\>\<])(\w+)", ""); //removes {{[item name]}} ancient rank markers
			//SearchString = r.Replace(SearchString, "").Trim();
			SearchString = RegexParameters.Replace(SearchString, "").Trim();
			
			if (!string.IsNullOrEmpty(SearchString))
			{
				//cache a regex object for searching item names instead of creating it for every string match attempted
				try
				{
					//allow the user to use regex symbols but the plugin doesn't need to work too hard to validate the input string, just don't render search results if creating a regex object from the search string throws an exception
					Search = new Regex(Regex.Replace(SearchString, @"\s+", ".+?"), RegexOptions.IgnoreCase);
				}
				catch (Exception) { //instead of throwing exceptions, render an alert indicator for (in)valid inputs
					//SearchPhrase = Regex.Replace(SearchString, @"\s+", " "); //to be displayed
					SearchString = "Invalid regex ("+text+")"; //string.Empty;
					Search = null;
				}
			}
			else
			{
				SearchString = "Parsing yielded empty result";
				Search = null;
			}
		}
		
		//recursive function adapted from https://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals
		private string ToRoman(int n)
		{
			if ((n < 1) || (n > 10)) return string.Empty;
			if (n == 10) return "X" + ToRoman(n - 10);
			if (n >= 9) return "IX" + ToRoman(n - 9);
			if (n >= 5) return "V" + ToRoman(n - 5);
			if (n >= 4) return "IV" + ToRoman(n - 4);
			if (n >= 1) return "I" + ToRoman(n - 1);
			return string.Empty;
		}
		
		private string GenerateItemSearchTerms(IItem item)
		{
			string search = item.FullNameLocalized;

			//special case of having a rare name
			if (!string.IsNullOrEmpty(item.RareName))
				search = item.RareName + ", " + search;

			//set name
			if (item.SetSno != uint.MaxValue)
			{
				if (SetNames.ContainsKey(item.SetSno))
					search += " " + SetNames[item.SetSno];
			}
			//special case of "legendary gem"
			else if (item.SnoItem.SnoItemType.Id == 1888008307)
			{
				search += string.Format(" {0} {1}", TextLegendary, item.JewelRank);
			}
			
			//item type name
			if (!string.IsNullOrEmpty(item.SnoItem.SnoItemType.NameLocalized))
				search += " " + item.SnoItem.SnoItemType.NameLocalized;
			
			return search;
		}
		
		//get item stat text
		private string GenerateStatSearchTerms(IItem item)
		{
			//unidentified item
			if (item.Perfections == null)
				return null;
			
			string stats = string.Empty;
			
			foreach (IItemPerfection perfection in item.Perfections)
				stats += Environment.NewLine + perfection.Attribute.GetDescription(perfection.Modifier);
				
			return stats.TrimStart(Environment.NewLine.ToCharArray());
		}
		
		private string GetText(IUiElement ui, string command = null, int minParamLength = 0)
        {
			//IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
			if (ui.Visible) 
			{
				//ui is visible, capture the text in its prompt
				string chatText = ui.ReadText(System.Text.Encoding.Default, false); //System.Text.Encoding.UTF8
				if (!string.IsNullOrEmpty(chatText))
				{
					chatText = chatText.Trim();
					
					if (!string.IsNullOrEmpty(command))
					{
						if (chatText.StartsWith(command, true, CultureInfo.InvariantCulture))
						{
							if (chatText.Trim().Length == command.Length)
							{
								if (minParamLength == 0)
									return string.Empty;
							}
							else if (chatText[command.Length] == ' ')
							{
								chatText = chatText.Substring(command.Length).Trim();
								if (chatText.Length >= minParamLength)
									return chatText;
							}
						}
					}
					else if (chatText.Length >= minParamLength)
						return chatText;
				}
			}
			
			return null;
		}
		
		private ITexture GetHeroHead(HeroClass cls, bool isMale)
		{
			//borrowed the texture numbers from OtherPlayersHeadsPlugin.cs
			switch(cls)
			{ 
				case HeroClass.Barbarian:
					return Hud.Texture.GetTexture(isMale ? 3921484788 : 1030273087);
				case HeroClass.Crusader:
					return Hud.Texture.GetTexture(isMale ? 3742271755 : 3435775766);
				case HeroClass.DemonHunter:
					return Hud.Texture.GetTexture(isMale ? 3785199803 : 2939779782);
				case HeroClass.Monk:
					return Hud.Texture.GetTexture(isMale ? 2227317895 : 2918463890);
				case HeroClass.Necromancer:
					return Hud.Texture.GetTexture(isMale ? 3285997023 : 473831658);
				case HeroClass.WitchDoctor:
					return Hud.Texture.GetTexture(isMale ? 3925954876 : 1603231623);
				case HeroClass.Wizard:
					return Hud.Texture.GetTexture(isMale ? (uint)44435619 : 876580014);
				default:
					break;
			}
			
			return null;
		}
		
		private void MarkArmoryItem(IItem item)
		{
			if (!MarkArmorySets)
				return;
			
			SearchSnapshot snapshot = Snapshots[Hud.Game.Me.HeroId];
			foreach (ArmorySet loadout in snapshot.Loadouts)
			{
				if (loadout.Items.ContainsKey(item.Seed))
				{
					//mark it
					var rect = Hud.Inventory.GetItemRect(item);
					TextLayout mark = MarkerCurrentHero.GetTextLayout(Mark);
					MarkerCurrentHero.DrawText(mark, rect.X, rect.Bottom - mark.Metrics.Height*0.9f);
					
					/*ITexture texture = Hud.Texture.GetTexture(670858621); //texture id found in glq's armory plugin
					float tWidth = texture.Width*0.4f;
					float tHeight = texture.Height*0.4f;
					texture.Draw(rect.X - tWidth*0.35f, rect.Bottom - tHeight*0.65f, tWidth, tHeight);*/
					return;
				}
			}			
			
			foreach (SearchSnapshot ss in Snapshots.Values)
			{
				if (ss.HeroId == Hud.Game.Me.HeroId)
					continue;
				
				foreach (ArmorySet loadout in ss.Loadouts)
				{
					if (loadout.Items.ContainsKey(item.Seed))
					{
						//mark it
						var rect = Hud.Inventory.GetItemRect(item);
						TextLayout mark = MarkerCurrentHero.GetTextLayout(Mark);
						MarkerOtherHero.DrawText(mark, rect.X, rect.Bottom - mark.Metrics.Height*0.9f);
						return;
					}
				}
			}
		}
		
		private void SaveSnapshot(IPlayerArmorySet aset = null)
		{
			IPlayer me = Hud.Game.Me;
			SearchSnapshot snapshot;

			if (aset is object)
			{
				snapshot = Snapshots[me.HeroId];
				
				//save only the specified one
				ArmorySet loadout = new ArmorySet()
				{
					Index = aset.Index,
					Name = aset.Name,						
				};

				//check all the items currently in the game
				foreach (IItem item in Hud.Game.Items.Where(i => aset.ContainsItem(i)))
				{
					//save the seed value of armory items
					//if (pset.ContainsItem(item))
					loadout.Items.Add(item.Seed, Tuple.Create(item.FullNameLocalized, GenerateStatSearchTerms(item)));
				}
				
				loadout.Incomplete = aset.ItemAnnIds.Count() != loadout.Items.Count; //aset.ItemAnnIds.Any(id => !Hud.Game.Items.Any(i => i.AnnId == id));
				
				//snapshot.Loadouts[aset.Index] = loadout;
				int index = snapshot.Loadouts.FindIndex(l => l.Index == aset.Index);
				if (index > -1)
					snapshot.Loadouts[index] = loadout;
			}
			else
			{
				snapshot = new SearchSnapshot()
				{
					HeroId = me.HeroId,
					HeroName = me.HeroName,
					HeroIsMale = me.HeroIsMale,
					HeroClass = me.HeroClassDefinition.HeroClass,
					HeroSeason = me.Hero.Season,
					HeroIsHardcore = me.HeroIsHardcore,
				};
				
				foreach (IItem item in Hud.Game.Items)
				{
					//save inventory items //0 = inventory
					if (item.Location == ItemLocation.Inventory)
					{
						if (!snapshot.Inventory.ContainsKey(item.Seed))
							snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //new ItemSnapshot() { Name = item.FullNameLocalized, ExtraSearchTerms = generateExtraSearchTerms(item), ArmoryAnnId = );
						
						if (item.ItemsInSocket is object)
						{
							foreach (IItem socketedItem in item.ItemsInSocket)
							{
								if (!snapshot.Inventory.ContainsKey(socketedItem.Seed))
									snapshot.Inventory.Add(socketedItem.Seed, Tuple.Create(GenerateItemSearchTerms(socketedItem), GenerateStatSearchTerms(socketedItem))); //snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //new ItemSnapshot() { Name = item.FullNameLocalized, ExtraSearchTerms = generateExtraSearchTerms(item), ArmoryAnnId = );
							}
						}
					}
					//save equipped items //1-13 = equipped
					else if ((int)item.Location > 0 && (int)item.Location < 14)
					{
						if (!snapshot.Equipped.ContainsKey(item.Seed))
							snapshot.Equipped.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //snapshot.Equipped.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
						
						if (item.ItemsInSocket is object)
						{
							foreach (IItem socketedItem in item.ItemsInSocket)
							{
								if (!snapshot.Equipped.ContainsKey(socketedItem.Seed))
									snapshot.Equipped.Add(socketedItem.Seed, Tuple.Create(GenerateItemSearchTerms(socketedItem), GenerateStatSearchTerms(socketedItem))); //snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //new ItemSnapshot() { Name = item.FullNameLocalized, ExtraSearchTerms = generateExtraSearchTerms(item), ArmoryAnnId = );
							}
						}
					}
					//save follower items //21-28 = follower
					else if ((int)item.Location > 20 && (int)item.Location < 29)
					{
						if (!snapshot.Follower.ContainsKey(item.Seed))
							snapshot.Follower.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //snapshot.Follower.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item)));
						
						if (item.ItemsInSocket is object)
						{
							foreach (IItem socketedItem in item.ItemsInSocket)
							{
								if (!snapshot.Follower.ContainsKey(socketedItem.Seed))
									snapshot.Follower.Add(socketedItem.Seed, Tuple.Create(GenerateItemSearchTerms(socketedItem), GenerateStatSearchTerms(socketedItem))); //snapshot.Inventory.Add(item.Seed, Tuple.Create(GenerateItemSearchTerms(item), GenerateStatSearchTerms(item))); //new ItemSnapshot() { Name = item.FullNameLocalized, ExtraSearchTerms = generateExtraSearchTerms(item), ArmoryAnnId = );
							}
						}
					}
				}
				
				//save all of them
				foreach (IPlayerArmorySet pset in me.ArmorySets)
				{
					ArmorySet loadout = new ArmorySet()
					{
						Index = pset.Index,
						Name = pset.Name,
						
					};

					//check all the items currently in the game
					foreach (IItem item in Hud.Game.Items.Where(i => pset.ContainsItem(i)))
					{
						//save the seed value of armory items
						//if (pset.ContainsItem(item))
						loadout.Items[item.Seed] = Tuple.Create(item.FullNameLocalized, GenerateStatSearchTerms(item)); //loadout.Items.Add(item.Seed, item.FullNameLocalized);
					}
					
					loadout.Incomplete = pset.ItemAnnIds.Count() != loadout.Items.Count; //pset.ItemAnnIds.Any(id => !Hud.Game.Items.Any(i => i.AnnId == id));
					
					snapshot.Loadouts.Add(loadout);
					//snapshot.Loadouts[pset.Index] = loadout;
				}
			}				
			
			Snapshots[me.HeroId] = snapshot;
		}
		
		public class SearchSnapshot
		{
			public uint HeroId { get; set; }
			public string HeroName { get; set; }
			public HeroClass HeroClass { get; set; }
			public int HeroSeason { get; set; }
			public bool HeroIsMale { get; set; }
			public bool HeroIsHardcore { get; set; }
			//public IPlayerArmorySet[] ArmorySets { get; set; }
			public List<ArmorySet> Loadouts { get; set; } = new List<ArmorySet>();
			public Dictionary<int, Tuple<string, string>> Inventory { get; set; } = new Dictionary<int, Tuple<string, string>>();
			public Dictionary<int, Tuple<string, string>> Equipped { get; set; } = new Dictionary<int, Tuple<string, string>>();
			public Dictionary<int, Tuple<string, string>> Follower { get; set; } = new Dictionary<int, Tuple<string, string>>();
			
			public SearchSnapshot() {}
		}
		
		public class ArmorySet
		{
			public int Index { get; set; }
			public string Name { get; set; }
			public Dictionary<int, Tuple<string, string>> Items { get; set; } = new Dictionary<int, Tuple<string, string>>(); //item.Seed, item.FullNameLocalized
			public bool Incomplete { get; set; } //missing items?
			//public Dictionary<int, uint> DisplacedItems { get; set; } = new Dictionary<int, uint>(); //item.Seed, IPlayer.HeroId

			public ArmorySet() {}

			public bool ContainsItem(IItem item)
			{
				//return Items.ContainsKey(item.AnnId);
				return Items.ContainsKey(item.Seed);
			}
		}
		
		/*public IPlugin GetPlugin(string pluginName)
		{
			return Hud.AllPlugins.FirstOrDefault(p => p.GetType().Name == pluginName);
		}*/
	}
}