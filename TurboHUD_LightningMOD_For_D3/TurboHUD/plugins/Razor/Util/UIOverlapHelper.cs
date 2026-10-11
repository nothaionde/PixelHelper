/*

This plugin consolidates UI obstruction and area overlap checking. This way, mouse obstruction checks are done once, not once per plugin.

Changelog
July 14, 2021
	- added ability to specify custom blocking areas as RectangleFs under UIGroup.Custom
July 13, 2021
	- added PlayerContextMenu to UIGroup.Clip
July 8, 2021
	- added the add-friend prompt window path to UIGroup.Prompts
	- added new utility function IsUiVisible for checking UI visibility of UIGroups
July 6, 2021
	- fixed bug with GetUiObstructingCursor caching that caused incorrect results sometimes
	- rewritten to query Render with uipaths as needed instead of caching the iuielement at the start
April 7, 2021
	- Initial release

*/

namespace Turbo.Plugins.Razor.Util
{
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic; //Dictionary

	using Turbo.Plugins.Default;
	
    public class UIOverlapHelper : BasePlugin//, IAfterCollectHandler
    {
		public enum UIGroup { Prompt, Left, LeftShort, Right, RightShort, Center, Clip, Tooltip, Mail, Custom }
		
		private UIGroup[] CursorCheckGroups = new UIGroup[] {UIGroup.LeftShort, UIGroup.RightShort, UIGroup.Left, UIGroup.Right, UIGroup.Center, UIGroup.Clip}; //UIGroup.Prompt, 
		private UIGroup[] AreaCheckGroups = new UIGroup[] {UIGroup.Prompt, UIGroup.Clip, UIGroup.Mail, UIGroup.Custom}; //UIGroup.Prompt, 
		private UIGroup[] ShortGroups = new UIGroup[] {UIGroup.LeftShort, UIGroup.RightShort};
		
		public Dictionary<UIGroup, string[]> UIPaths { get; private set; }
		public List<RectangleF> CustomBlockingAreas { get; private set; } = new List<RectangleF>();
		//public Dictionary<string, Func<RectangleF, RectangleF>> ClipAreaCorrection { get; private set; }
		
		private IUiElement _uiObstructingCursor; //cache
		//private Dictionary<RectangleF, bool> _isAreaObstructed = new Dictionary<RectangleF, bool>(); //cache
		private int LastCursorCheckTick;
		private int LastAreaCheckTick;
		
		public UIOverlapHelper() : base()
        {
            Enabled = true;
			Order = -10002; //run this before the other floater stuff
        }
		
        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//registering some UI elements that aren't collected by default
			Hud.Render.RegisterUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot", Hud.Render.GetUiElement("Root.NormalLayer.vendor_dialog_mainPage.panel"), null);
			Hud.Render.RegisterUiElement("Root.TopLayer.textInputPrompt.stack", Hud.Render.InGameBottomHudUiElement, null);
			Hud.Render.RegisterUiElement("Root.NormalLayer.Kanais_Collection.LayoutRoot", Hud.Render.GetUiElement("Root.NormalLayer.Kanais_Collection"), null); //credit to s4000's Useful Cube Items plugin for this uipath
			Hud.Render.RegisterUiElement("Root.TopLayer.BattleNetModalNotifications_main.ModalNotification", Hud.Render.InGameBottomHudUiElement, null);
			Hud.Render.RegisterUiElement("Root.NormalLayer.Mail_main.LayoutRoot", Hud.Render.InGameBottomHudUiElement, null);
			Hud.Render.RegisterUiElement("Root.NormalLayer.questlore_dialog", Hud.Render.InGameBottomHudUiElement, null);
			
			UIPaths = new Dictionary<UIGroup, string[]>() {
				{UIGroup.Prompt, new string[] {
					"Root.TopLayer.textInputPrompt.stack", //save armory prompt
					"Root.TopLayer.BattleNetModalNotifications_main.ModalNotification", //network disconnect popup
					"Root.TopLayer.BattleNetSocialDialogs_main.LayoutRoot.DialogSendRealIdRequest", //add friend prompt
					"Root.NormalLayer.equipmentManager_mainPage.details" //armory details page
				}},
				{UIGroup.Left, new string[] {
					"Root.NormalLayer.rift_dialog_mainPage",
					"Root.NormalLayer.equipmentManager_mainPage", //armory
					"Root.NormalLayer.questlore_dialog" //journal
				}},
				{UIGroup.LeftShort, new string[] {
					Hud.Inventory.StashMainUiElement.Path,
					"Root.NormalLayer.shop_dialog_mainPage.panel", //merchant panels
					"Root.NormalLayer.vendor_dialog_mainPage.panel", //artisan, gem upgrade, and kanai cube panels
					"Root.NormalLayer.hireling_dialog_mainPage",
					"Root.NormalLayer.Mail_main.LayoutRoot" //MailUI //Root.NormalLayer.Mail_main.LayoutRoot.ListDialogContent
				}},
				{UIGroup.Center, new string[] {
					"Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList",
					"Root.NormalLayer.Paragon_main.LayoutRoot.ParagonPointSelect",
					"Root.NormalLayer.BattleNetLeaderboard_main.LayoutRoot.OverlayContainer",
					"Root.NormalLayer.Guild_main.LayoutRoot.OverlayContainer",
					"Root.NormalLayer.BattleNetProfileBannerCustomization_main.LayoutRoot.OverlayContainer"
				}},
				{UIGroup.Clip, new string[] {
					"Root.NormalLayer.Kanais_Recipes_main.LayoutRoot", //secondary panel
					"Root.NormalLayer.Kanais_Collection.LayoutRoot", //secondary panel
					"Root.TopLayer.ContextMenus.PlayerContextMenu",
					"Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline" //chat box
				}},
				{UIGroup.Right, new string[] {
					"Root.NormalLayer.BattleNetFriendsList_main.LayoutRoot.OverlayContainer.FriendsListContent", //friends
					"Root.NormalLayer.GroupList_main.LayoutRoot.OverlayContainer.GroupsListContent", //community
					"Root.NormalLayer.inventory_side_pane_container" //paperdoll details pane
				}},
				{UIGroup.RightShort, new string[] {
					Hud.Inventory.InventoryMainUiElement.Path
				}},
					
				{UIGroup.Tooltip, new string[] {
					"Root.TopLayer.tooltip_dialog_background.tooltip_2"
				}},
				{UIGroup.Mail, new string[] {
					"Root.NormalLayer.Mail_main.LayoutRoot"
				}},
				{UIGroup.Custom, new string[] {
					
				}},
			};
        }
		
		public IUiElement GetUiObstructingCursor()
		{
			var diff = Hud.Game.CurrentGameTick - LastCursorCheckTick;
			if (diff < 0 || diff > 10)
			{
				LastCursorCheckTick = Hud.Game.CurrentGameTick;
				_uiObstructingCursor = _findUiObstructingCursor();
			}
			
			return _uiObstructingCursor;
		}
		
		public bool IsUiVisible(params UIGroup[] groups)
		{
			foreach (var grp in groups)
			{
				foreach (var path in UIPaths[grp])
				{
					var ui = Hud.Render.GetUiElement(path);
					if (ui is object && ui.Visible)
						return true;
				}
			}
			
			return false;
		}
		
		public bool IsUiObstructingArea(RectangleF rect, params UIGroup[] groups)
		{
			/*var diff = Hud.Game.CurrentGameTick - LastAreaCheckTick;
			if (diff < 0 || diff > 10)
			{
				LastAreaCheckTick = Hud.Game.CurrentGameTick;
				_isAreaObstructed.Clear();
			}
			else if (diff != 0)
			{
				var key = _isAreaObstructed.Keys.FirstOrDefault(k => k.Equals(rect));
				if (key is object)
					return _isAreaObstructed[key];
			}*/
			
			if (groups.Length == 0)
			{
				foreach (var grp in AreaCheckGroups)
				{
					foreach (var path in UIPaths[grp])
					{
						var ui = Hud.Render.GetUiElement(path);
						if (ui is object && ui.Visible)
						{
							if (ShortGroups.Contains(grp))
							{
								var truncated = new RectangleF(ui.Rectangle.X, ui.Rectangle.Y, ui.Rectangle.Width, ui.Rectangle.Height * 0.825f);
								if (truncated.IntersectsWith(rect))
								{
									if (ui.Visible)
									{
										//_isAreaObstructed[rect] = true;
										return true;
									}
								}
							}
							else if (ui.Rectangle.IntersectsWith(rect))
							{
								//_isAreaObstructed[rect] = true;
								return true;
							}
						}
					}
				}
				
				//_isAreaObstructed[rect] = false;
			}
			else
			{
				foreach (var grp in groups)
				{
					if (grp == UIGroup.Custom)
					{
						if (CustomBlockingAreas.Count > 0)
						{
							foreach (var area in CustomBlockingAreas)
							{
								if (area.IntersectsWith(rect))
									return true;
							}
						}
					}
					else
					{
						foreach (var path in UIPaths[grp])
						{
							var ui = Hud.Render.GetUiElement(path);
							if (ui is object && ui.Visible)
							{
								if (ShortGroups.Contains(grp))
								{
									var truncated = new RectangleF(ui.Rectangle.X, ui.Rectangle.Y, ui.Rectangle.Width, ui.Rectangle.Height * 0.825f);
									if (truncated.IntersectsWith(rect))
									{
										if (ui.Visible)
										{
											//_isAreaObstructed[rect] = true;
											return true;
										}
									}
								}
								else if (ui.Rectangle.IntersectsWith(rect))
								{
									//_isAreaObstructed[rect] = true;
									return true;
								}
							}
						}
					}
				}
				
				//_isAreaObstructed[rect] = false;
			}
			
			return false;
		}

		public IUiElement _findUiObstructingCursor()
		{
			foreach (UIGroup grp in CursorCheckGroups)
			{
				foreach (string path in UIPaths[grp])
				{
					var ui = Hud.Render.GetUiElement(path);
					if (ui is object && ui.Visible && Hud.Window.CursorInsideRect(ui.Rectangle.X, ui.Rectangle.Y, ui.Rectangle.Width, (ShortGroups.Contains(grp) ? ui.Rectangle.Height * 0.825f : ui.Rectangle.Height)))
						return ui;
				}
			}
			
			return null;
		}
    }
}