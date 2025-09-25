namespace Turbo.Plugins.LightningMod
{
    using System.Collections.Generic;
    using System.Linq;

    public class UpgradeToAncientPlugin : BaseKanaiPlugin, IKeyEventHandler, IInGameTopPainter, IAfterCollectHandler
    {
        public int Mode { get; set; } = 0;//0=远古或太古时停止，1=仅太古时停止
        private string str_Header;
        private string str_InfoLock;
        private string str_Info;
        private string str_NoItem;
        private string str_Running;
        private List<IItem> ancient = new List<IItem>();
        public UpgradeToAncientPlugin() : base(2)
        {
        }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent) && keyEvent.IsPressed && !Hud.GetPlugin<ConvertMaterialPlugin>().Running && !Hud.GetPlugin<UpgradeRareToLengendaryPlugin>().Running)
            {
                var pageNum = GetPageNum();
                if (!TurnedOn && !PageIndexes.Contains(pageNum)) return;
                TurnedOn = !TurnedOn;
                if(TurnedOn)
                {
                    foreach (var item in Hud.Inventory.ItemsInInventory)
                    {
                        if (item.SnoItem.Kind != ItemKind.loot) continue;
                        var canUpgrade = item.IsLegendary && !item.Unidentified &&
                            (!InventoryLockForUpgradeToAncient || !item.IsInventoryLocked) &&
                            (item.SnoItem.MainGroupCode != "gems_unique") &&
                            (item.SnoItem.MainGroupCode != "riftkeystone") &&
                            (item.SnoItem.MainGroupCode != "horadriccache") &&
                            (item.SnoItem.MainGroupCode != "-") &&
                            (item.SnoItem.MainGroupCode != "pony") &&
                            (!item.SnoItem.Code.StartsWith("P71_Ethereal")) &&
							(!item.SnoItem.Code.StartsWith("P72_Soulshard")) &&
							(item.SnoItem.NameEnglish != "Hellforge Ember") &&
                            item.AncientRank > 0;
                        if(canUpgrade) ancient.Add(item);
                    }
                }
                else
                {
                    ancient.Clear();
                }
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory) return;
            if (!Hud.Game.IsInGame || !transmuteDialog.Visible || !pageNumber.Visible || Hud.GetPlugin<ConvertMaterialPlugin>().Running || Hud.GetPlugin<UpgradeRareToLengendaryPlugin>().Running) return;
            var pageNum = GetPageNum();
            if (!PageIndexes.Contains(pageNum)) return;
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Header = "【雷电宏-自动重铸" + (Mode == 0 ? "远古或太古":"太古") + "】";
                str_InfoLock = "没有开启物品锁，无法使用该插件\r\n请在雷电宏相关中设置物品锁";
                str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动重铸普通传奇为" + (Mode == 0 ? "远古或太古" : "太古");
                if (InventoryLockForUpgradeToAncient) str_Info = str_Info + "\r\n注意：请把不需要升级的物品\r\n放在物品锁区域内（蓝色框）";
                str_NoItem = "没有普通传奇可以被重铸";
                str_Running = "自动重铸中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Header = "【雷電宏-自動重鑄" + (Mode == 0 ? "遠古或洪荒" : "洪荒") + "】";
                str_InfoLock = "沒有開啟物品鎖，無法使用該插件\r\n請在雷電宏相關中設置物品鎖";
                str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動重鑄普通傳奇為" + (Mode == 0 ? "遠古或洪荒" : "洪荒");
                if (InventoryLockForUpgradeToAncient) str_Info = str_Info + "\r\n注意：請把不需要重鑄的物品\r\n放在物品鎖區域內（藍色框）";
                str_NoItem = "沒有物品可被分解";
                str_Running = "自動重鑄中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Header = "【МОД - Перековывать в " + (Mode == 0 ? "древний" : "первозданный") + "】";
                str_InfoLock = "Нет блокировки в инвентаре\r\nЗадайте её в закладке Макрос";
                str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта";
                str_NoItem = "Нет предметов для перековки";
                str_Running = "Перековка...";
            }
            else
            {
                str_Header = "【Reforge to " + (Mode == 0 ? "Ancient or Primal" : "Primal") + "-Mod】";
                str_InfoLock = "inventory lock is missing\r\nYou need to set it in Macros";
                str_Info = "press " + ToggleKeyEvent.ToString() + " to start reforge to " + (Mode == 0 ? "Ancient or Primal" : "Primal");
                str_NoItem = "no items to reforge";
                str_Running = "reforging...";
            }
            var y = vendorPage.Rectangle.Y + vendorPage.Rectangle.Height * 0.037f;
            var layout = HeaderFont.GetTextLayout(str_Header);
            HeaderFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
            y += layout.Metrics.Height * 1.3f;

            if ((((Hud.Inventory.InventoryLockArea.Width <= 0) || (Hud.Inventory.InventoryLockArea.Height <= 0)) && InventoryLockForUpgradeToAncient))
            {
                layout = InfoFont.GetTextLayout(str_InfoLock);
                InfoFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
                return;
            }

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

                var canUpgrade = item.IsLegendary && !item.Unidentified &&
                    (Running ? (item.AncientRank >= 0 && item.AncientRank <= (Mode == 0 ? 0 : 1)): (item.AncientRank >= 0)) &&
                    (!InventoryLockForUpgradeToAncient || !item.IsInventoryLocked) &&
                    (item.SnoItem.MainGroupCode != "gems_unique") &&
                    (item.SnoItem.MainGroupCode != "riftkeystone") &&
                    (item.SnoItem.MainGroupCode != "horadriccache") &&
                    (item.SnoItem.MainGroupCode != "-") &&
                    (item.SnoItem.MainGroupCode != "pony") &&
                    (!item.SnoItem.Code.StartsWith("P71_Ethereal")) &&
                    (!item.SnoItem.Code.StartsWith("P72_Soulshard")) &&
                    (item.SnoItem.NameEnglish != "Hellforge Ember") &&
                    (item.Quantity <= 1);
                if (canUpgrade) result.Add(item);
            }
            foreach(var item in ancient)
            {
                if(item != null)
                {
                    result.Add(item);
                }
                
            }
            return result;
        }

        public void AfterCollect()
        {
            
            if (!ValidateTransmuteTurnedOn(InventoryLockForUpgradeToAncient ? true : false)) return;
            if (Running) return;
            Running = true;
            var tempX = Hud.Window.CursorX;
            var tempY = Hud.Window.CursorY;
            while (ValidateTransmuteTurnedOn(InventoryLockForUpgradeToAncient ? true : false))
            {
                var enoughMaterials = (Hud.Game.Me.Materials.ForgottenSoul >= 50) && (Hud.Game.Me.Materials.KhanduranRune >= 5) && (Hud.Game.Me.Materials.CaldeumNightShade >= 5) && (Hud.Game.Me.Materials.ArreatWarTapestry >= 5) && (Hud.Game.Me.Materials.CorruptedAngelFlesh >= 5) && (Hud.Game.Me.Materials.WestmarchHolyWater >= 5);
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

                if (!OpenKanaiCube(2))
                {
                    break;
                }
                itemsToUpgrade.Clear();
                ancient.Remove(selectedItem);
            }
            ancient.Clear();
            Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
            TurnedOn = false;
            Running = false;
        }
    }
}