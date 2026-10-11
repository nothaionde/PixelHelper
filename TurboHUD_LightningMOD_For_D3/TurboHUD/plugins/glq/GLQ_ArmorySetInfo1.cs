﻿using Turbo.Plugins.Default;
using System.Linq;

namespace Turbo.Plugins.glqz
{
    public class GLQ_ArmorySetInfo1 : BasePlugin, IInGameTopPainter
    {
        public TopLabelDecorator InArmorySetList { get; set; }
        private int stashPage, stashTab, stashTabAbs;
        private IWatch _watch;
        public GLQ_ArmorySetInfo1()
        {
            Enabled = true;
        }
        public override void Load(IController hud)
        {
            base.Load(hud);
            InArmorySetList = new TopLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(100, 0, 0, 0, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, false, false, true),
            };
            _watch = Hud.Time.CreateAndStartWatch();
        }
        public void PaintTopInGame(ClipState clipState)
        {
            var uiInv = Hud.Inventory.InventoryMainUiElement;
            if (!uiInv.Visible) return;
            var HoveredItem = Hud.Inventory.HoveredItem;
            if (HoveredItem != null)
            {
                var player = Hud.Game.Me;
                string text = null;
                for (int i = 0; i < player.ArmorySets.Length; ++i)
                {
                    var armorySet = player.ArmorySets[i];
                    if (armorySet != null)
                    {
                        if (armorySet.ContainsItem(HoveredItem))
                        {
                            var name = Hud.Game.Me.ArmorySets[armorySet.Index].Name;
                            text = text + name + "\n";
                        }
                    }
                }
                if (text != null)
                {
                    var fulltext = "\n" + "Оружейная：" + "\n" + "\n" + text;
                    InArmorySetList.TextFunc = () => fulltext;
                    var uiMain = Hud.Inventory.GetHoveredItemMainUiElement();
                    var layout = InArmorySetList.TextFont.GetTextLayout(fulltext);
                    
                    var x = uiMain.Rectangle.Left - (layout.Metrics.Width + Hud.Window.Size.Width * 0.01f);
                    var y = uiMain.Rectangle.Top + 5;
                    if (HoveredItem.Location == ItemLocation.Inventory)
                    {
                        x = uiMain.Rectangle.Right;
                    }
                    else if (HoveredItem.Location == ItemLocation.Stash)
                    {
                        x = uiMain.Rectangle.Left - (layout.Metrics.Width + Hud.Window.Size.Width * 0.01f);
                    }
                    else
                    {
                        x = uiMain.Rectangle.Left - (layout.Metrics.Width + Hud.Window.Size.Width * 0.01f);
                        y = uiMain.Rectangle.Top + 35;
                    }
					InArmorySetList.Paint(x, y, layout.Metrics.Width + Hud.Window.Size.Width * 0.01f, layout.Metrics.Height, HorizontalAlign.Center);
                }         
            }
            var allitem = Hud.Game.Items.Where(x => x.Location != ItemLocation.Merchant && x.Location != ItemLocation.Floor);
            stashTab = Hud.Inventory.SelectedStashTabIndex;
            stashPage = Hud.Inventory.SelectedStashPageIndex;
            stashTabAbs = stashTab + stashPage * Hud.Inventory.MaxStashTabCountPerPage;
            if (clipState != ClipState.Inventory) return;
            foreach (var item in allitem)
            {
                if (item.Location == ItemLocation.Stash)
                {
                    var tabIndex = item.InventoryY / 10;
                    if (tabIndex != stashTabAbs) continue;
                }
                if ((item.InventoryX < 0) || (item.InventoryY < 0)) continue;

                var rect = Hud.Inventory.GetItemRect(item);
                if (rect == System.Drawing.RectangleF.Empty) continue;
                DrawArmorySet(item, rect);
            }
        }
        private void DrawArmorySet(IItem item, System.Drawing.RectangleF rect)
        {
            
            bool InArmorySet = false;
            if ((item.Location != ItemLocation.Inventory) && (item.Location != ItemLocation.Stash)) return;

            for (int i = 0; i < Hud.Game.Me.ArmorySets.Length; ++i)
            {
                var armorySet = Hud.Game.Me.ArmorySets[i];
                if (armorySet != null)
                {
                    if (armorySet.ContainsItem(item))
                    {
                        InArmorySet = true;
                        break;
                    }
                }
            }
            if (InArmorySet)
            {
                var ArmorySetTexture = Hud.Texture.GetTexture(670858621);
                var h = ArmorySetTexture.Height * 0.4f / 1200.0f * Hud.Window.Size.Height;
                var rh = h;
                var mod = (_watch.ElapsedMilliseconds) % 1000;
                var ratio = 0.8f + 1.2f / 1000.0f * (mod < 500 ? mod : 1000 - mod);
                rh *= ratio;
                var x = rect.Right - h * 1.4f - ((rh - h) / 2);
                var y = rect.Top - h * 0.20f - ((rh - h) / 2);
                ArmorySetTexture.Draw(x, y, rh, rh, 1);
            }
        }
    }
}