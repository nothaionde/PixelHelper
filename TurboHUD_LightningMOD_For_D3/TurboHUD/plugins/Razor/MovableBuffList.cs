/*

This plugin converts the default BuffList plugins into movable and resizable areas.

Changelog
August 10, 2021
	- added checks for invalid BuffRule definitions that have nonexistent ISnoPower snos
August 9, 2021
	- added Default\BuffLists\TopLeftBuffListPlugin and Default\BuffLists\TopRightBuffListPlugin
	- added null checks when handling data from the default plugins
August 5, 2021
	- countdown border brush now has two colors to distinguish between buffs and debuffs (green and red respectively by default)
August 1, 2021
	- added preview icons when in Edit Mode (can be turned off by setting PreviewIconsInEditMode = false)
	- added background texture for legendary item icons (can be turned off by setting ShowLegendaryBackground = false)
July 30, 2021
	- shifted the left and right buff lists up closer to the player
	- added optional custom Opacity, StackFontSize, TimeFontSize properties
	- shortened the buff list area names for readability
	- added cleanup for orphaned movable areas from the area names change
July 29, 2021
	- optimized countdown clock face rendering and now using a boxy fill style with countdown borders adapted from my PartyCOE plugin
November 27, 2020
	- each list's Movable Area is now the size of 1 buff icon, upon which it is centered
August 2, 2020
	- Initial release

*/

namespace Turbo.Plugins.Razor
{
	using SharpDX;
	using SharpDX.Direct2D1;
	using SharpDX.DirectWrite;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Globalization;
	
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Movable;
	
	public class MovableBuffList : BasePlugin, IMovable
	{
		public bool PreviewIconsInEditMode { get; set; } = true;
		public bool ShowLegendaryBackground { get; set; } = true;
		public float? StackFontSize { get; set; } //leave empty for default
		public float? TimeFontSize { get; set; } //leave empty for default
		public float? Opacity { get; set; } //= 0.5f; //leave empty for default (0 - 1f)
		
		public IBrush BorderBrush { get; set; }
		public IBrush DebuffBorderBrush { get; set; }
		
		public BuffPainter BuffPainter { get; set; }
		public BuffRuleCalculator RuleCalculator { get; private set; }
		public float PositionOffsetX { get; set; } = 0.14f; //-0.14f;
		public float PositionOffsetH { get; set; } = 0.875f;
		
		public Dictionary<string, BuffListInfo> BuffLists = new Dictionary<string, BuffListInfo>();
		
		//private IFont DebugFont;
		private IFont StackFont;
		private IFont TimeLeftFont;
		private MovableController Mover;

		public MovableBuffList()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			
			BorderBrush = Hud.Render.CreateBrush(255, 0, 255, 0, 6f);
			DebuffBorderBrush = Hud.Render.CreateBrush(255, 0, 186, 14, 6f);
			
			if (StackFontSize.HasValue)
				StackFont = Hud.Render.CreateFont("tahoma", StackFontSize.Value, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			if (TimeFontSize.HasValue)
				TimeLeftFont = Hud.Render.CreateFont("tahoma", TimeFontSize.Value, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			//font size ratio 6:10.8, 10:18
			//DebugFont = Hud.Render.CreateFont("tahoma", 6, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
			
		}
		
		
		public void OnRegister(MovableController mover)
		{
/*			// Добавляем Баф под сезон 28
			
				Hud.RunOnPlugin<BuffSezonIcoTop>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 3); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(Hud.Window.Size.Width*0.5f - plugin.RuleCalculator.StandardIconSize*0.5f, Hud.Window.Size.Height*0.5f + Hud.Window.Size.Height*plugin.PositionOffset, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = true,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			// Конец сезонный Баф */
			
			Hud.RunOnPlugin<PlayerBottomBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(Hud.Window.Size.Width*0.5f - plugin.RuleCalculator.StandardIconSize*0.5f, Hud.Window.Size.Height*0.5f + Hud.Window.Size.Height*plugin.PositionOffset, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = true,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});

			Hud.RunOnPlugin<PlayerTopBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(Hud.Window.Size.Width*0.5f - plugin.RuleCalculator.StandardIconSize*0.5f, Hud.Window.Size.Height*0.5f + Hud.Window.Size.Height*plugin.PositionOffset, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = true,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			Hud.RunOnPlugin<PlayerLeftBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(Hud.Window.Size.Width*0.5f + Hud.Window.Size.Height*plugin.PositionOffsetX - plugin.RuleCalculator.StandardIconSize/**0.5f*/, Hud.Window.Size.Height * plugin.PositionOffsetH * 0.475f, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = false,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			Hud.RunOnPlugin<PlayerRightBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled, //Hud.Window.Size.Width*0.5f + Hud.Window.Size.Height*plugin.PositionOffsetX - plugin.RuleCalculator.StandardIconSize*0.5f
						Rectangle = new System.Drawing.RectangleF(Hud.Window.Size.Width*0.5f + Hud.Window.Size.Height*plugin.PositionOffsetX /*+ plugin.RuleCalculator.StandardIconSize*0.65f*/, Hud.Window.Size.Height * plugin.PositionOffsetH *0.475f, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = false,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			
			var uiMinimapRect = Hud.Render.MinimapUiElement.Rectangle;
			Hud.RunOnPlugin<MiniMapLeftBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(uiMinimapRect.Left - plugin.RuleCalculator.StandardIconSize*0.5f, uiMinimapRect.Top + uiMinimapRect.Height*0.5f - plugin.RuleCalculator.StandardIconSize*0.5f, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = false,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			Hud.RunOnPlugin<MiniMapRightBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(uiMinimapRect.Right - plugin.RuleCalculator.StandardIconSize*0.5f, uiMinimapRect.Top + uiMinimapRect.Height*0.5f - plugin.RuleCalculator.StandardIconSize*0.5f, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = false,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			Hud.RunOnPlugin<TopLeftBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
	            var x = (Hud.Window.Size.Width * 0.25f) - (Hud.Window.Size.Width * 0.5f / 2);
				var y = Hud.Window.Size.Height * 0.001f;
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(x, y, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = true,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			Hud.RunOnPlugin<TopRightBuffListPlugin>(plugin => {
				string name = plugin.GetType().Name;
				name = name.Remove(name.Length - 14); //"BuffListPlugin"
				
				if (Opacity.HasValue)
					plugin.BuffPainter.Opacity = Opacity.Value;
				if (StackFont is object)
					plugin.BuffPainter.StackFont = StackFont;
				if (TimeLeftFont is object)
					plugin.BuffPainter.TimeLeftFont = TimeLeftFont;
				
				float tRatio = 0;
				if (plugin.BuffPainter.TimeLeftFont is object)
				{
					var layout = plugin.BuffPainter.TimeLeftFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					tRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
				float sRatio = 0;
				if (plugin.BuffPainter.StackFont is object)
				{
					var layout = plugin.BuffPainter.StackFont.GetTextLayout(name);
					var fontSize = (float)layout.GetFontSize(0);
					sRatio = plugin.RuleCalculator.StandardIconSize / fontSize;
				}
				
	            var x = (Hud.Window.Size.Width * 0.75f) - (Hud.Window.Size.Width * 0.5f / 2);
				var y = Hud.Window.Size.Height * 0.001f;
				
				BuffLists.Add(
					name, 
					new BuffListInfo()
					{
						Name = name,
						Painter = plugin.BuffPainter, 
						RuleCalculator = plugin.RuleCalculator, 
						EnabledAtStart = plugin.Enabled,
						Rectangle = new System.Drawing.RectangleF(x, y, plugin.RuleCalculator.StandardIconSize, plugin.RuleCalculator.StandardIconSize),
						Horizontal = true,
						TimeFontRatio = tRatio,
						StackFontRatio = sRatio,
					}
				); 
				
				if (plugin.Enabled)
					plugin.Enabled = false;
			});
			
			//initialize position and dimension elements
			foreach (BuffListInfo info in BuffLists.Values)
			{
				mover.CreateArea(
					this,
					info.Name, //area name
					info.Rectangle, //position + dimensions
					info.EnabledAtStart, //enabled at start?
					true, //save to config file?
					ResizeMode.FixedRatio //resizable
				);
			}
			
			Mover = mover;
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			if (BuffLists.ContainsKey(area.Name))
			{
				var x = area.Rectangle.X + deltaX;
				var y = area.Rectangle.Y + deltaY;
				BuffListInfo info = BuffLists[area.Name];

				float iconSize = (info.Horizontal ? area.Rectangle.Height : area.Rectangle.Width);
				float ratio = info.RuleCalculator.StandardIconSize / info.RuleCalculator.SizeMultiplier;
				float multiplier = iconSize / ratio;
				
				//change the multiplier before painting so that spacing scales accordingly too
				//float tmp = info.RuleCalculator.SizeMultiplier;
				if (info.RuleCalculator.SizeMultiplier != multiplier)
					info.RuleCalculator.SizeMultiplier = multiplier;
				
				if (PreviewIconsInEditMode && mover.EditMode)
				{
					if (info.Horizontal)
						PaintRulesHorizontal(info.Painter, info.RuleCalculator.Rules, x, y, iconSize, iconSize, info.RuleCalculator.StandardIconSpacing*3, info.TimeFontRatio, info.StackFontRatio);
					else
						PaintRulesVertical(info.Painter, info.RuleCalculator.Rules, x, y, iconSize, iconSize, info.RuleCalculator.StandardIconSpacing*3, info.TimeFontRatio, info.StackFontRatio);
				}
				else
				{
					//calculate paint values
					info.RuleCalculator.CalculatePaintInfo(Hud.Game.Me);
					if (info.RuleCalculator.PaintInfoList.Count == 0)
						return;
					
					//modify font sizes
					
					//draw it
					if (info.Horizontal)
						PaintHorizontalCenter(info.Painter, info.RuleCalculator.PaintInfoList, x, y, iconSize, iconSize, info.RuleCalculator.StandardIconSpacing*3, info.TimeFontRatio, info.StackFontRatio);
					else
						PaintVerticalCenter(info.Painter, info.RuleCalculator.PaintInfoList, x, y, iconSize, iconSize, info.RuleCalculator.StandardIconSpacing*3, info.TimeFontRatio, info.StackFontRatio);
				}				
			}
			else
				mover.DeleteArea(area);
		}
		
		//for Edit Mode rendering
		public void PaintRulesHorizontal(BuffPainter painter, List<BuffRule> rules, float x, float y, float width, float size, float spacing, float timeFontRatio, float stackFontRatio)
		{
			if (painter == null || rules == null)
				return;
			
			float totalwidth = size*rules.Count + spacing*(rules.Count-1);
			x += width*0.5f - totalwidth*0.5f;
			
			foreach (BuffRule rule in rules)
			{
				ITexture texture = null;
				if (rule.UseLegendaryItemTexture is object)
				{
					texture = Hud.Texture.GetItemTexture(rule.UseLegendaryItemTexture);
					
					if (ShowLegendaryBackground)
						(rule.UseLegendaryItemTexture.SetItemBonusesSno == uint.MaxValue ? Hud.Texture.InventoryLegendaryBackgroundSmall : Hud.Texture.InventorySetBackgroundSmall).Draw(x, y, size, size, painter.Opacity);
				}
				//else if (!rule.UsePowersTexture && info.Icons[0].Exists && (info.Icons[0].TextureId != 0))
				//	textureId = info.Icons[0].TextureId;
				else
				{
					//ISnoPower power = Hud.Sno.GetSnoPower(rule.PowerSno);
					if (rule.IconIndex.HasValue) // is object
					{
						var power = Hud.Sno.GetSnoPower(rule.PowerSno);
						if (power is object && power.Icons.Length > rule.IconIndex.Value)
							texture = Hud.Texture.GetTexture(power.Icons[rule.IconIndex.Value].TextureId);
						//texture = Hud.Texture.GetTexture(Hud.Sno.GetSnoPower(rule.PowerSno)?.Icons[rule.IconIndex.Value].TextureId);
					}
					else
					{
						var t = Hud.Sno.GetSnoPower(rule.PowerSno).Icons.FirstOrDefault(i => i.TextureId != 0);
						if (t is object)
							texture = Hud.Texture.GetTexture(t.TextureId);
					}
				}
				
				if (texture is object)
				{
					texture.Draw(x, y, size, size, painter.Opacity);
					
					if (rule.ShowStacks)
					{
						var layout = painter.StackFont.GetTextLayout("0");
						layout.SetFontSize(size / stackFontRatio, new TextRange(0, 1));
						//painter.StackFont.DrawText(layout, (x + size) - (size / 8.0f) - (float)Math.Ceiling(layout.Metrics.Width), (y + size) - layout.Metrics.Height - (size / 15.0f));
						painter.StackFont.DrawText(layout, x + size*0.9f - layout.Metrics.Width, y + size*0.9f - layout.Metrics.Height);
					}
					
					

					x += size + spacing;
				}
				
			}
		}
		
		public void PaintRulesVertical(BuffPainter painter, List<BuffRule> rules, float x, float y, float height, float size, float spacing, float timeFontRatio, float stackFontRatio)
		{
			if (painter == null || rules == null)
				return;
			
			float totalheight = size*rules.Count + spacing*(rules.Count-1);
			y += height*0.5f - totalheight*0.5f;
			
			foreach (BuffRule rule in rules)
			{
				ITexture texture = null;
				if (rule.UseLegendaryItemTexture is object)
				{
					texture = Hud.Texture.GetItemTexture(rule.UseLegendaryItemTexture);
					
					if (ShowLegendaryBackground)
						(rule.UseLegendaryItemTexture.SetItemBonusesSno == uint.MaxValue ? Hud.Texture.InventoryLegendaryBackgroundSmall : Hud.Texture.InventorySetBackgroundSmall).Draw(x, y, size, size, painter.Opacity);
				}
				//else if (!rule.UsePowersTexture && info.Icons[0].Exists && (info.Icons[0].TextureId != 0))
				//	textureId = info.Icons[0].TextureId;
				else
				{
					//ISnoPower power = Hud.Sno.GetSnoPower(rule.PowerSno);
					//if (rule.IconIndex is object)
					//	texture = Hud.Texture.GetTexture(Hud.Sno.GetSnoPower(rule.PowerSno).Icons[rule.IconIndex.Value].TextureId);
					if (rule.IconIndex.HasValue) // is object
					{
						var power = Hud.Sno.GetSnoPower(rule.PowerSno);
						if (power is object && power.Icons.Length > rule.IconIndex.Value)
							texture = Hud.Texture.GetTexture(power.Icons[rule.IconIndex.Value].TextureId);
						//texture = Hud.Texture.GetTexture(Hud.Sno.GetSnoPower(rule.PowerSno)?.Icons[rule.IconIndex.Value].TextureId);
					}
					else
					{
						var t = Hud.Sno.GetSnoPower(rule.PowerSno).Icons.FirstOrDefault(i => i.TextureId != 0);
						if (t is object)
							texture = Hud.Texture.GetTexture(t.TextureId);
					}
				}
				
				if (texture is object)
				{
					texture.Draw(x, y, size, size, painter.Opacity);
					
					if (rule.ShowStacks && painter.StackFont is object)
					{
						var layout = painter.StackFont.GetTextLayout("0");
						layout.SetFontSize(size / stackFontRatio, new TextRange(0, 1));
						//painter.StackFont.DrawText(layout, (x + size) - (size / 8.0f) - (float)Math.Ceiling(layout.Metrics.Width), (y + size) - layout.Metrics.Height - (size / 15.0f));
						painter.StackFont.DrawText(layout, x + size*0.9f - layout.Metrics.Width, y + size*0.9f - layout.Metrics.Height);
					}
					
					/*if (rule.ShowTimeLeft && painter.TimeLeftFont is object)
					{
						
					}*/

					y += size + spacing;
				}
				
			}
		}
		
		public void PaintHorizontalCenter(BuffPainter painter, List<BuffPaintInfo> infoList, float x, float y, float width, float size, float spacing, float timeFontRatio, float stackFontRatio)
        {
			float totalwidth = size*infoList.Count + spacing*(infoList.Count-1);
			x += width*0.5f - totalwidth*0.5f;
			
			foreach (var info in infoList)
            {
				var firstIcon = info.Icons[0];
				var isDebuff = firstIcon.Harmful;

				//info.BackgroundTexture?.Draw(rect.X, rect.Y, rect.Width, rect.Height, Opacity);
				if (ShowLegendaryBackground)
					info.BackgroundTexture?.Draw(x, y, size, size, painter.Opacity);
				info.Texture.Draw(x, y, size, size, painter.Opacity);

				if ((info.TimeLeft > 0) && (info.Elapsed >= 0))
				{
					//DrawTimeLeftClock(x, y, size, size, info.Elapsed, info.TimeLeft);
					DrawTimeLeftClock(new RectangleF(x, y, size, size), info.Elapsed, info.TimeLeft, painter.TimeLeftClockBrush, isDebuff ? BorderBrush : DebuffBorderBrush);
					
					//DrawTimeLeftNumbers(x, y, size, size, info);
					if (info.Rule.ShowTimeLeft && painter.TimeLeftFont is object) //painter.ShowTimeLeftNumbers
					{
						string text;
						if (info.TimeLeft > 1.0f)
						{
							var mins = Convert.ToInt32(Math.Floor(info.TimeLeft / 60.0d));
							var secs = Math.Floor(info.TimeLeft - (mins * 60.0d));
							text = info.TimeLeft >= 60
								? mins.ToString("F0", CultureInfo.InvariantCulture) + ":" + (secs < 10 ? "0" : "") + secs.ToString("F0", CultureInfo.InvariantCulture)
								: info.TimeLeft.ToString("F0", CultureInfo.InvariantCulture);
						}
						else
							text = info.TimeLeft.ToString("F1", CultureInfo.InvariantCulture);

						//figure out if font size needs to be adjusted
						//var fontSize = layout.GetFontSize(0);
						//fontSize * ratio = originalSize
						//var fontSize = originalSize / ratio;
						var layout = painter.TimeLeftFont.GetTextLayout(text);
						layout.SetFontSize(size / timeFontRatio, new TextRange(0, text.Length));
						painter.TimeLeftFont.DrawText(layout, x + size*0.5f - layout.Metrics.Width*0.5f, y + size*0.5f - layout.Metrics.Height*0.5f);
					}
				}
				
				if (painter.HasIconBorder)
					(isDebuff ? painter.Hud.Texture.DebuffFrameTexture : painter.Hud.Texture.BuffFrameTexture).Draw(x, y, size, size, painter.Opacity);

				if (info.Stacks > -1 && painter.StackFont is object)
				{
					//DrawStacks(x, y, size, size, info.Stacks);
					var stacks = info.Stacks.ToString();
					var layout = painter.StackFont.GetTextLayout(stacks);
					layout.SetFontSize(size / stackFontRatio, new TextRange(0, stacks.Length));
					painter.StackFont.DrawText(layout, x + size*0.9f - layout.Metrics.Width, y + size*0.9f - layout.Metrics.Height);
				}
				
				x += size + spacing;				
			}
			
			//TextLayout test = painter.StackFont.GetTextLayout("test: " + (size / stackFontRatio));
			//test.SetFontSize(18, new TextRange(1, 3)); //change font of all but the first letter from 6f to 10f
			//test = DebugFont.GetTextLayout("test: " + test.GetFontWeight(0).GetType().Name); //GetFontFamilyName(0) //GetFontSize(0)  //test.Metrics.Width + " x " + test.Metrics.Height
			//painter.StackFont.DrawText(test, painter.Hud.Window.CursorX, painter.Hud.Window.CursorY - test.Metrics.Height);

        }
		
		public void PaintVerticalCenter(BuffPainter painter, List<BuffPaintInfo> infoList, float x, float y, float height, float size, float spacing, float timeFontRatio, float stackFontRatio)
        {
			float totalheight = size*infoList.Count + spacing*(infoList.Count-1);
			y += height*0.5f - totalheight*0.5f;
			
			foreach (var info in infoList)
            {
				var firstIcon = info.Icons[0];
				var isDebuff = firstIcon.Harmful;

				//info.BackgroundTexture?.Draw(rect.X, rect.Y, rect.Width, rect.Height, Opacity);
				if (ShowLegendaryBackground)
					info.BackgroundTexture?.Draw(x, y, size, size, painter.Opacity);
				info.Texture.Draw(x, y, size, size, painter.Opacity);

				if ((info.TimeLeft > 0) && (info.Elapsed >= 0))
				{
					DrawTimeLeftClock(new RectangleF(x, y, size, size), info.Elapsed, info.TimeLeft, painter.TimeLeftClockBrush, isDebuff ? BorderBrush : DebuffBorderBrush);
					
					//DrawTimeLeftNumbers(x, y, size, size, info);
					if (info.Rule.ShowTimeLeft && painter.TimeLeftFont is object) //painter.ShowTimeLeftNumbers
					{
						string text;
						if (info.TimeLeft > 1.0f)
						{
							var mins = Convert.ToInt32(Math.Floor(info.TimeLeft / 60.0d));
							var secs = Math.Floor(info.TimeLeft - (mins * 60.0d));
							text = info.TimeLeft >= 60
								? mins.ToString("F0", CultureInfo.InvariantCulture) + ":" + (secs < 10 ? "0" : "") + secs.ToString("F0", CultureInfo.InvariantCulture)
								: info.TimeLeft.ToString("F0", CultureInfo.InvariantCulture);
						}
						else
							text = info.TimeLeft.ToString("F1", CultureInfo.InvariantCulture);

						var layout = painter.TimeLeftFont.GetTextLayout(text);
						layout.SetFontSize(size / timeFontRatio, new TextRange(0, text.Length));
						painter.TimeLeftFont.DrawText(layout, x + size*0.5f - layout.Metrics.Width*0.5f, y + size*0.5f - layout.Metrics.Height*0.5f);
					}
				}
				
				if (painter.HasIconBorder)
					(isDebuff ? painter.Hud.Texture.DebuffFrameTexture : painter.Hud.Texture.BuffFrameTexture).Draw(x, y, size, size, painter.Opacity);

				if (info.Stacks > -1 && painter.StackFont is object)
				{
					//DrawStacks(x, y, size, size, info.Stacks);
					var stacks = info.Stacks.ToString();
					var layout = painter.StackFont.GetTextLayout(stacks);
					layout.SetFontSize(size / stackFontRatio, new TextRange(0, stacks.Length));
					painter.StackFont.DrawText(layout, x + size*0.9f - layout.Metrics.Width, y + size*0.9f - layout.Metrics.Height);
				}
				
				y += size + spacing;
			}
        }
		
		private void DrawTimeLeftClock(RectangleF rect, double elapsed, double timeLeft, IBrush clock, IBrush timeLeftBorder = null, IBrush timeElapsedBorder = null)
		{
			if ((timeLeft > 0) && (elapsed >= 0) && (clock != null))
			{
				//var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
				//var startAngle = 0;
				clock.Opacity = 1 - (float)(0.3f / (timeLeft + elapsed) * elapsed);
				//var rad = rect.Width * 0.45f;
				
				var angle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
				using (var pg = Hud.Render.CreateGeometry())
				//using (var pg2 = Hud.Render.CreateGeometry())
				{
					using (var gs = pg.Open())
					//using (var gs2 = pg2.Open())
					{
						var vec = new Vector2(rect.Center.X, rect.Y);
						gs.BeginFigure(rect.Center, FigureBegin.Filled);
						gs.AddLine(vec);
						
						//gs2.BeginFigure(vec, FigureBegin.Filled);
						
						if (angle >= 45)
						{
							vec = new Vector2(rect.Right, rect.Y);
							gs.AddLine(vec);
							//gs2.AddLine(vec);
							
							if (angle >= 135)
							{
								vec = new Vector2(rect.Right, rect.Bottom);
								gs.AddLine(vec);
								//gs2.AddLine(vec);
								
								if (angle >= 225)
								{
									vec = new Vector2(rect.Left, rect.Bottom);
									gs.AddLine(vec);
									//gs2.AddLine(vec);
									
									if (angle >= 315)
									{
										vec = new Vector2(rect.X, rect.Y);
										gs.AddLine(vec);
										//gs2.AddLine(vec);
										
										vec = new Vector2(rect.X + ((rect.Width*0.5f)/45f)*(angle - 315), rect.Y);
										gs.AddLine(vec);
										//gs2.AddLine(vec);
									}
									else
									{
										vec = new Vector2(rect.X, rect.Bottom - (rect.Height/90f)*(angle - 225));
										gs.AddLine(vec);
										//gs2.AddLine(vec);
									}
								}
								else
								{
									vec = new Vector2(rect.Right - (rect.Width/90f)*(angle - 135), rect.Bottom);
									gs.AddLine(vec);
									//gs2.AddLine(vec);
								}
							}
							else
							{
								vec = new Vector2(rect.Right, rect.Y + (rect.Height/90f)*(angle - 45));
								gs.AddLine(vec);
								//gs2.AddLine(vec);
							}
						}
						else
						{
							vec = new Vector2(rect.Center.X + ((rect.Width*0.5f)/45f)*angle, rect.Y);
							gs.AddLine(vec);
							//gs2.AddLine(vec);
						}
						
						gs.EndFigure(FigureEnd.Closed);
						gs.Close();
						
						//gs2.EndFigure(FigureEnd.Open);
						//gs2.Close();
					}

					clock.DrawGeometry(pg);
					
					//if (timeElapsedBorder is object)
					//	timeElapsedBorder.DrawGeometry(pg2);
				}
					
				if (timeLeftBorder is object)
				{
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
						
						timeLeftBorder.DrawGeometry(pg);
						
						/*	
							if (angle < 360) //left top half
								
								border.DrawLine(rect.X + ((rect.Width*0.5f)/45f)*(float)Math.Max(0, angle - 315), rect.Y, rect.Center.X, rect.Y);
							if (angle < 315) //left
								border.DrawLine(rect.X, rect.Y, rect.X, rect.Bottom - (rect.Height/90f)*(float)Math.Max(0, angle - 225));
							if (angle < 225) //bottom
								border.DrawLine(rect.X, rect.Bottom, rect.Right - (rect.Width/90f)*(float)Math.Max(0, angle - 135), rect.Bottom);
							if (angle < 135) //right
								border.DrawLine(rect.Right, rect.Y + (rect.Height/90f)*(float)Math.Max(0, angle - 45), rect.Right, rect.Bottom);
							if (angle < 45) //right top half
								border.DrawLine(rect.Center.X + ((rect.Width*0.5f)/45f)*angle, rect.Y, rect.Right, rect.Y);*/
					}
				}
			}
		}
	}
	
	public class BuffListInfo
	{
		public string Name { get; set; }
		public BuffPainter Painter { get; set; }
		public BuffRuleCalculator RuleCalculator { get; set; }
		public bool EnabledAtStart { get; set; }
		public System.Drawing.RectangleF Rectangle { get; set; }
		public bool Horizontal { get; set; } = true;
		
		public float TimeFontRatio { get; set; } //calculated dynamically
		public float StackFontRatio { get; set; } //calculated dynamically
	}
}