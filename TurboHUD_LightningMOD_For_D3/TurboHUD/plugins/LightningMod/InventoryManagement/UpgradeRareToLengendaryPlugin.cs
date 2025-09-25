namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;
    using System.Linq;

    public class UpgradeRareToLengendaryPlugin : BaseKanaiPlugin, IKeyEventHandler, IInGameTopPainter, IAfterCollectHandler
    {
        private string str_Header;
        private string str_Info;
        private string str_NoItem;
        private string str_Running;
        public UpgradeRareToLengendaryPlugin() : base(3)
        {
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent) && keyEvent.IsPressed &&  !Hud.GetPlugin<ConvertMaterialPlugin>().Running && !Hud.GetPlugin<UpgradeToAncientPlugin>().Running)
            {
                var pageNum = GetPageNum();
                if (!TurnedOn && !PageIndexes.Contains(pageNum)) return;
                TurnedOn = !TurnedOn;
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory) return;
            if (!Hud.Game.IsInGame || !transmuteDialog.Visible || !pageNumber.Visible || Hud.GetPlugin<ConvertMaterialPlugin>().Running || Hud.GetPlugin<UpgradeToAncientPlugin>().Running) return;
            var pageNum = GetPageNum();
            if (!PageIndexes.Contains(pageNum)) return;
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Header = "【雷电宏-自动升级黄装】";
                str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动升级黄装到传奇";
                str_NoItem = "没有黄装可以被升级";
                str_Running = "自动升级中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Header = "【雷電宏-自動升級黃裝】";
                str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動升級黃裝到傳奇";
                str_NoItem = "沒有黃裝可以被升級";
                str_Running = "自動升級中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Header = "【МОД - Улучшить Редкий Предмет】";
                str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта";
                str_NoItem = "Нет предметов для улучшения";
                str_Running = "Улучшение...";
            }
            else
            {
                str_Header = "【Upgrade Rare Item-Mod】";
                str_Info = "press " + ToggleKeyEvent.ToString() + " to start";
                str_NoItem = "no items to upgrade";
                str_Running = "upgrading...";
            }
            var y = vendorPage.Rectangle.Y + vendorPage.Rectangle.Height * 0.037f;
            var layout = HeaderFont.GetTextLayout(str_Header);
            HeaderFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
            y += layout.Metrics.Height * 1.3f;

            var items = GetItemsToUpgrade();
            foreach (var item in items)
            {
                var itemRect = Hud.Inventory.GetItemRect(item);
                ItemHighlighBrush.DrawRectangle(itemRect);
            }

            if (!Running)
            {
                if (items.Count > 0)
                {
                    layout = InfoFont.GetTextLayout(str_Info);
                    InfoFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
                }
                else
                {
                    layout = InfoFont.GetTextLayout(str_NoItem);
                    InfoFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
                }
            }
            else
            {
                layout = InfoFont.GetTextLayout(str_Running);
                InfoFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
            }
        }

        private List<IItem> GetItemsToUpgrade()
        {
            var result = new List<IItem>();
            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                if (item.SnoItem.Kind != ItemKind.loot) continue;

                var canUpgrade = item.IsRare &&
                    (item.SnoItem.MainGroupCode != "gems_unique") &&
                    (item.SnoItem.MainGroupCode != "riftkeystone") &&
                    (item.SnoItem.MainGroupCode != "horadriccache") &&
                    (item.Quantity <= 1) &&
                    (item.SnoItem.Level >= 65);
                if (canUpgrade) result.Add(item);
            }
            return result;
        }

        public void AfterCollect()
        {
            if (!ValidateTransmuteTurnedOn(false)) return;
            if (Running) return;
            Running = true;
            var tempX = Hud.Window.CursorX;
            var tempY = Hud.Window.CursorY;
            while (ValidateTransmuteTurnedOn(false))
            {
                
                var enoughMaterials = (Hud.Game.Me.Materials.DeathsBreath >= 25) && (Hud.Game.Me.Materials.ReusableParts >= 50) && (Hud.Game.Me.Materials.ArcaneDust >= 50) && (Hud.Game.Me.Materials.VeiledCrystal >= 50);
                if (!enoughMaterials) break;

                var itemsToUpgrade = GetItemsToUpgrade();
                if (itemsToUpgrade.Count == 0) break;

                itemsToUpgrade.Sort((a, b) =>
                {
                    var r = a.InventoryX.CompareTo(b.InventoryX);
                    if (r == 0) r = a.InventoryY.CompareTo(b.InventoryY);
                    return r;
                });

                var selectedItem = itemsToUpgrade[0];
                if (!TransmuteOne(selectedItem))
                {
                    break;
                }

                if (!OpenKanaiCube(3))
                {
                    break;
                }

                itemsToUpgrade.Clear();
            }
            Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
            TurnedOn = false;
            Running = false;
        }
    }
}