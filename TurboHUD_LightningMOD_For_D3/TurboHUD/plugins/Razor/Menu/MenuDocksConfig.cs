/*
	This file contains menu dock definitions.
*/

namespace Turbo.Plugins.Razor.Menu
{
	using System.Drawing;
	
	using Turbo.Plugins.Default; //RectangleF
	using Turbo.Plugins.Razor.Plugin;

	public class MenuDocksConfig : BasePlugin, ICustomizer, IPluginManifest
	{
		//IPluginManifest
		public string Name { get; set; } //optional display name
        public string Description { get; set; } = "Definitions for all of the docks in the Menu Plugin System";
		public string Version { get; set; } = "24-Sep-2021";
		public System.Collections.Generic.List<string> Dependencies { get; set; } = new System.Collections.Generic.List<string>() {"Turbo.Plugins.Razor.Menu.MenuPlugin"};

		public MenuDocksConfig() { Enabled = true; }

		//public override void Load(IController hud) { base.Load(hud); }

		public void Customize()
		{
			Hud.RunOnPlugin<MenuPlugin>(plugin =>
			{
				plugin.Docks = new System.Collections.Generic.Dictionary<string, IMenuDock>()
				{
					{ "BottomRight", new HorizontalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelHoveredBrush = plugin.HighlightBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => {
								var potionUI = Hud.Render.GetPlayerSkillUiElement(ActionKey.Heal);
								return new RectangleF(potionUI.Rectangle.Right, potionUI.Rectangle.Bottom, Hud.Window.Size.Width - potionUI.Rectangle.Right, plugin.MenuHeight);
							},
							Alignment = HorizontalAlign.Right,
							Expand = MenuExpand.Up,
						}
					},
					{ "BottomLeft", new HorizontalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelHoveredBrush = plugin.HighlightBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => {
								var potionUI = Hud.Render.GetPlayerSkillUiElement(ActionKey.Heal);
								var healthBall = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_healthBall");
								float x = Hud.Window.Size.Width * (Hud.Game.Me.HeroIsHardcore ? 0.087f : 0.043f);
								return new RectangleF(x, potionUI.Rectangle.Bottom + 1, healthBall.Rectangle.Right - x, plugin.MenuHeight);
							}, 
							Alignment = HorizontalAlign.Right,
							Expand = MenuExpand.Up,
						}
					}, //bottomUI.Left + (bottomUI.Width * 0.09f) - lx
					{ "BottomCenter", new HorizontalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelHoveredBrush = plugin.HighlightBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => {
								var healthBall = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_healthBall");
								var manaBall = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_manaBall");
								var bottomUI = Hud.Render.InGameBottomHudUiElement;
								var bottomCenterWidth = manaBall.Rectangle.Left - healthBall.Rectangle.Right;
								return new RectangleF(healthBall.Rectangle.Right, bottomUI.Rectangle.Top + (bottomUI.Rectangle.Height * 0.335f), bottomCenterWidth, plugin.MenuHeight);
							}, //manaBall.Rectangle.Left - healthBall.Rectangle.Right, plugin.MenuHeight), 
							Alignment = HorizontalAlign.Center, 
							Expand = MenuExpand.Up, 
						}
					},
					{ "MinimapTop", new HorizontalMenuDock(Hud) { 
							//LabelBrush = plugin.BgBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => new RectangleF(Hud.Render.MinimapUiElement.Rectangle.X, Hud.Render.MinimapUiElement.Rectangle.Y, Hud.Render.MinimapUiElement.Rectangle.Width*0.85F, plugin.MenuHeight), 
							Alignment = HorizontalAlign.Left,
							Expand = MenuExpand.Down, 
						} 
					},
					{ "MinimapBottom", new HorizontalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => new RectangleF(Hud.Render.MinimapUiElement.Rectangle.X, Hud.Render.MinimapUiElement.Rectangle.Y + Hud.Render.MinimapUiElement.Rectangle.Height - plugin.MenuHeight, Hud.Render.MinimapUiElement.Rectangle.Width*0.85F, plugin.MenuHeight), 
							Alignment = HorizontalAlign.Left,
							Expand = MenuExpand.Up, 
						}
					},
					{ "TopCenter", new HorizontalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush, 
							LabelHoveredBrush = plugin.HighlightBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => {
								var healthBall = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_healthBall");
								var manaBall = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_manaBall");
								var bottomCenterWidth = manaBall.Rectangle.Left - healthBall.Rectangle.Right;
								return new RectangleF(Hud.Window.Size.Width*0.5f - bottomCenterWidth*0.5f, 0, bottomCenterWidth, plugin.MenuHeight);
							}, //new RectangleF(0, 0, Hud.Window.Size.Width, plugin.MenuHeight), 
							Alignment = HorizontalAlign.Center, 
							Expand = MenuExpand.Down, 
						}
					},
					{ "TopRight", new VerticalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelHoveredBrush = plugin.HighlightBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => new RectangleF(Hud.Window.Size.Width - plugin.MenuHeight, 0, plugin.MenuHeight, plugin.MenuHeight*6), //new RectangleF(Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f, plugin.MenuHeight, plugin.MenuHeight*6), //
							Alignment = HorizontalAlign.Left,
							Expand = MenuExpand.Left, 
						}
					},
					{ "CenterRight", new VerticalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelHoveredBrush = plugin.HighlightBrush,							
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => new RectangleF(Hud.Window.Size.Width - plugin.MenuHeight, Hud.Window.Size.Height*0.5f - Hud.Render.MinimapUiElement.Rectangle.Height*0.5f, plugin.MenuHeight, Hud.Render.MinimapUiElement.Rectangle.Height), 
							Alignment = HorizontalAlign.Center,
							Expand = MenuExpand.Left, 
						}
					},
					{ "CenterLeft", new VerticalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush,
							LabelHoveredBrush = plugin.HighlightBrush,
							LabelPinnedBrush = plugin.PinnedBrush,
							Anchor = () => new RectangleF(0, Hud.Window.Size.Height*0.5f - Hud.Render.MinimapUiElement.Rectangle.Height*0.5f, plugin.MenuHeight, Hud.Render.MinimapUiElement.Rectangle.Height), 
							Alignment = HorizontalAlign.Center,
							Expand = MenuExpand.Right,
						}
					},
					/*{ "Objectives", new HorizontalMenuDock(Hud) { 
							LabelBrush = plugin.BgBrush, 
							Anchor = () => new RectangleF(objectivesUI.Rectangle.X, objectivesUI.Rectangle.Y, objectivesUI.Rectangle.Width - plugin.MenuHeight, plugin.MenuHeight), 
							Alignment = HorizontalAlign.Right, 
							Expand = MenuExpand.Down,
						}
					},*/
				};
			});
		}
	}
}