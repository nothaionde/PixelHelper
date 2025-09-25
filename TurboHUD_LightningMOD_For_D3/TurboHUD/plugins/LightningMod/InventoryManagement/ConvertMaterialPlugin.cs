namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;
    using System.Linq;

    public class ConvertMaterialPlugin : BaseKanaiPlugin, IKeyEventHandler, IInGameTopPainter, IAfterCollectHandler
    {
        private string str_Header;
        private string str_Info;
        private string str_NoItem;
        private string str_Running;
        public ConvertMaterialPlugin() : base(7, 8, 9)
        {
        }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent) && keyEvent.IsPressed && !Hud.GetPlugin<UpgradeToAncientPlugin>().Running && !Hud.GetPlugin<UpgradeRareToLengendaryPlugin>().Running)
            {
                var pageNum = GetPageNum();
                if (!TurnedOn && !PageIndexes.Contains(pageNum)) return;
                TurnedOn = !TurnedOn;
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory) return;
            if (!Hud.Game.IsInGame || !transmuteDialog.Visible || !pageNumber.Visible || Hud.GetPlugin<UpgradeToAncientPlugin>().Running || Hud.GetPlugin<UpgradeRareToLengendaryPlugin>().Running) return;
            var pageNum = GetPageNum();
            if (!PageIndexes.Contains(pageNum)) return;
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Header = "【雷电宏-自动转换材料】";
                str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动转换材料\r\n蓝白底材装备推荐1级小号购买商店蓝白装";
                str_NoItem = "没有转换材料所需底材装备";
                str_Running = "自动转换材料中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Header = "【雷電宏-自動轉換材料】";
                str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動轉換材料\r\n藍白底材裝備推薦1級小號購買商店藍白裝";
                str_NoItem = "沒有轉換材料所需底材裝備";
                str_Running = "自動轉換材料中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Header = "【МОД-Преобразовать материал】";
                str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта";
                str_NoItem = "Нет предметов для преобразования";
                str_Running = "Преобразование...";
            }
            else
            {
                str_Header = "【Convert Material-Mod】";
                str_Info = "press " + ToggleKeyEvent.ToString() + " to start";
                str_NoItem = "no items to convert";
                str_Running = "converting...";
            }
            var y = vendorPage.Rectangle.Y + vendorPage.Rectangle.Height * 0.037f;
            var layout = HeaderFont.GetTextLayout(str_Header);
            HeaderFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
            y += layout.Metrics.Height * 1.3f;
            var qualities = GetItemQualities(pageNum);

            var items = GetItemsToConvert(qualities);
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

        private static List<ItemQuality> GetItemQualities(int pageNum)
        {
            var qualitiesInOrder = new List<ItemQuality>();
            if (pageNum == 7)
            {
                qualitiesInOrder.Add(ItemQuality.Magic4);
                qualitiesInOrder.Add(ItemQuality.Magic5);
                qualitiesInOrder.Add(ItemQuality.Magic6);
                qualitiesInOrder.Add(ItemQuality.Rare4);
                qualitiesInOrder.Add(ItemQuality.Rare5);
                qualitiesInOrder.Add(ItemQuality.Rare6);
            }
            else if (pageNum == 8)
            {
                qualitiesInOrder.Add(ItemQuality.Inferior);
                qualitiesInOrder.Add(ItemQuality.Normal);
                qualitiesInOrder.Add(ItemQuality.Superior);
                qualitiesInOrder.Add(ItemQuality.Rare4);
                qualitiesInOrder.Add(ItemQuality.Rare5);
                qualitiesInOrder.Add(ItemQuality.Rare6);
            }
            else if (pageNum == 9)
            {
                qualitiesInOrder.Add(ItemQuality.Inferior);
                qualitiesInOrder.Add(ItemQuality.Normal);
                qualitiesInOrder.Add(ItemQuality.Superior);
                qualitiesInOrder.Add(ItemQuality.Magic4);
                qualitiesInOrder.Add(ItemQuality.Magic5);
                qualitiesInOrder.Add(ItemQuality.Magic6);
            }

            return qualitiesInOrder;
        }

        private long GetRequiredMaterialAmount(int pageNum)
        {
            var qualitiesInOrder = new List<ItemQuality>();
            if (pageNum == 7) return Hud.Game.Me.Materials.ReusableParts;
            else if (pageNum == 8) return Hud.Game.Me.Materials.ArcaneDust;
            else if (pageNum == 9) return Hud.Game.Me.Materials.VeiledCrystal;

            return 0;
        }

        private List<IItem> GetItemsToConvert(List<ItemQuality> itemQualities)
        {
            var result = new List<IItem>();
            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                if (item.SnoItem.Kind != ItemKind.loot) continue;

                var canUpgrade =
                    (item.SnoItem.MainGroupCode != "gems_unique") &&
                    (item.SnoItem.MainGroupCode != "riftkeystone") &&
                    (item.SnoItem.MainGroupCode != "horadriccache") &&
                    (item.Quantity <= 1) &&
                    itemQualities.Contains(item.Quality);

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
            var pageNum = GetPageNum();
            var qualities = GetItemQualities(pageNum);
            while (ValidateTransmuteTurnedOn(false))
            {

                var enoughMaterials = (Hud.Game.Me.Materials.DeathsBreath >= 1) && (GetRequiredMaterialAmount(pageNum) >= 100);
                if (!enoughMaterials) break;

                var itemsToUpgrade = GetItemsToConvert(qualities);
                if (itemsToUpgrade.Count == 0) break;

                itemsToUpgrade.Sort((a, b) =>
                {
                    var r = a.Quality.CompareTo(b.Quality);
                    if (r == 0) r = a.InventoryX.CompareTo(b.InventoryX);
                    if (r == 0) r = a.InventoryY.CompareTo(b.InventoryY);
                    return r;
                });

                var selectedItem = itemsToUpgrade[0];
                if (!TransmuteOne(selectedItem))
                {
                    break;
                }

                if (!OpenKanaiCube(pageNum))
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