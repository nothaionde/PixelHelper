namespace Turbo.Plugins.LightningMod
{
    using SharpDX.DirectInput;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;

    public class SalvagePlugin : BasePlugin, IKeyEventHandler, IInGameTopPainter, IAfterCollectHandler
    {
        public bool TurnedOn { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public bool Running { get; private set; }
        public int SalvageSoulshard { get; set; }
        public int SalvagePotion { get; set; }
        public bool SalvageWhisperOfAtonement { get; set; }
        public int SalvageEthereal { get; set; }
        public int SalvagePrimal { get; set; }
        public int SalvageAncient { get; set; }
        private readonly HashSet<string> Whitelist = new HashSet<string>() { };//白名单内物品不被分解
        private readonly HashSet<string> Blacklist = new HashSet<string>() { };//黑名单内物品强制分解，不包含符合基础规则（如物品锁，附魔，镶嵌，军械库）的物品，物品同时在白名单内时优先白名单生效
        private IUiElement vendorPage;
        private IUiElement salvageDialog;
        private IUiElement salvageNormalButton;
        private IUiElement salvageMagicButton;
        private IUiElement salvageRareButton;
        private IUiElement salvageSelectedButton1;
        private IUiElement salvageSelectedButton2;
        private IUiElement okButton;
        public bool ExtremeSpeedMode { get; set; }
        public bool UseInventoryLock { get; set; }
        public IFont HeaderFont { get; private set; }
        public IFont InfoFont { get; private set; }
        public IBrush ItemHighlighBrush { get; set; }
        private string str_Header;
        private string str_InfoLock;
        private string str_Info;
        private string str_NoItem;
        private string str_Running;
        private int tempX;
        private int tempY;
        public SalvagePlugin()
        {
            Enabled = true;
            TurnedOn = false;
            Running = false;
            ExtremeSpeedMode = false;
            SalvageAncient = 0;//0智能分解远古，1不分解远古，2分解全部远古
            SalvagePrimal = 0;//0智能分解太古，1不分解太古，2分解全部太古
            SalvageEthereal = 0;//0智能分解无形，1不分解无形，2分解全部无形
            SalvagePotion = 0;// 0智能分解传奇药水，1分解全部传奇药水
            SalvageSoulshard = 0;//0智能分解灵魂碎片，1分解全部0级灵魂碎片
            SalvageWhisperOfAtonement = false;//分解125级以下赎罪低语宝石
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.R, false, false, false);

            vendorPage = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage", Hud.Inventory.InventoryMainUiElement, null);
            salvageDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog", vendorPage, null);
            salvageNormalButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_normal_button", salvageDialog, null);
            salvageMagicButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_magic_button", salvageDialog, null);
            salvageRareButton = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_rare_button", salvageDialog, null);
            salvageSelectedButton1 = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_button", salvageDialog, null);
            salvageSelectedButton2 = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_button", salvageDialog, null);
            okButton = Hud.Render.RegisterUiElement("Root.TopLayer.confirmation.subdlg.stack.wrap.button_ok", salvageDialog, null);

            HeaderFont = Hud.Render.CreateFont("tahoma", 12, 255, 200, 200, 100, true, false, 255, 0, 0, 0, true);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            ItemHighlighBrush = Hud.Render.CreateBrush(255, 200, 200, 100, -1.6f);

        }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent))
            {
                if (keyEvent.IsPressed)
                {
                    if (!TurnedOn)
                    {
                        TurnedOn = true;
                    }
                    else
                    {
                        TurnedOn = false;
                        Running = false;
                    }
                    TestTurnedOn();
                }
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Header = "【雷电宏-自动分解物品】";
                str_InfoLock = "没有开启物品锁，无法使用该插件\r\n请在雷电宏相关中设置物品锁";
                str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动分解";
                if (UseInventoryLock) str_Info = str_Info + "\r\n注意：请把不需要分解的物品\r\n放在物品锁区域内（蓝色框）";
                str_NoItem = "没有物品可被分解";
                str_Running = "自动分解中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Header = "【雷電宏-自動分解物品】";
                str_InfoLock = "沒有開啟物品鎖，無法使用該插件\r\n請在雷電宏相關中設置物品鎖";
                str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動分解";
                if (UseInventoryLock) str_Info = str_Info + "\r\n注意：請把不需要分解的物品\r\n放在物品鎖區域內（藍色框）";
                str_NoItem = "沒有物品可被分解";
                str_Running = "自動分解中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Header = "【МОД - Распыл】";
                str_InfoLock = "Нет блокировки в инвентаре\r\nЗадайте её в закладке Макрос";
                str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта";
                str_NoItem = "Нет предметов для распыла";
                str_Running = "Распыление...";
            }
            else
            {
                str_Header = "【Salvage-Mod】";
                str_InfoLock = "inventory lock is missing\r\nYou need to set it in Macros";
                str_Info = "press " + ToggleKeyEvent.ToString() + " to start";
                str_NoItem = "no items to salvage";
                str_Running = "salvaging...";
            }
            if (clipState != ClipState.Inventory) return;
            if (!Hud.Game.IsInGame || !salvageDialog.Visible || !Hud.Inventory.InventoryMainUiElement.Visible) return;

            var y = vendorPage.Rectangle.Y + vendorPage.Rectangle.Height * 0.037f;
            var layout = HeaderFont.GetTextLayout(str_Header);
            HeaderFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
            y += layout.Metrics.Height * 1.3f;

            if (((Hud.Inventory.InventoryLockArea.Width <= 0) || (Hud.Inventory.InventoryLockArea.Height <= 0)) && UseInventoryLock)
            {
                layout = InfoFont.GetTextLayout(str_InfoLock);
                InfoFont.DrawText(layout, vendorPage.Rectangle.X + (vendorPage.Rectangle.Width * 0.04f), y);
                return;
            }

            var items = GetItemsToSalvage();
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
        public void AddWhitelist(params string[] names)
        {
            foreach (var name in names)
            {
                Whitelist.Add(name.ToLower());
            }
        }
        public void AddBlacklist(params string[] names)
        {
            foreach (var name in names)
            {
                Blacklist.Add(name.ToLower());
            }
        }
        private bool isBetterItem(IItem item, int rank)//1远古，2太古，100无形
        {
            var equippedItem = Hud.Game.Items.FirstOrDefault(i => i.Location >= ItemLocation.Head && i.Location <= ItemLocation.Neck && i.SnoItem.NameEnglish == item.SnoItem.NameEnglish);
            if(equippedItem != null)
            {
                var batterItem = rank == 100 ? item.SnoItem.IsEthereal : item.AncientRank == rank;
                return batterItem;
            }
            return false;
        }
        private bool isBetterPotion(IItem Potion)
        {
            bool newPotion = !Hud.Game.Items.Any(i => i.SnoItem.Code.StartsWith("HealthPotionLegendary") && (i.Location == ItemLocation.Stash || i.Location == ItemLocation.Inventory || i.Location == ItemLocation.MerchantAvaibleItemsForPurchase) && i.SnoItem.NameEnglish == Potion.SnoItem.NameEnglish && i != Potion);
            if (newPotion)
            {
                return true;
            }
            else
            {
                var potionPerfection = PotionPerfection(Potion);
                var potions = Hud.Game.Items.Where(i => i.SnoItem.Code.StartsWith("HealthPotionLegendary") && (i.Location == ItemLocation.Stash || i.Location == ItemLocation.Inventory || i.Location == ItemLocation.MerchantAvaibleItemsForPurchase) && i.SnoItem.NameEnglish == Potion.SnoItem.NameEnglish && i != Potion);
                if(potions != null)
                {
                   var potion = potions.OrderByDescending(i => PotionPerfection(i)).FirstOrDefault();
                    if (potion != null)
                    {
                        if (potionPerfection > PotionPerfection(potion))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            
        }
        private double PotionPerfection(IItem Potion)
        {
            foreach (var perfection in Potion.Perfections)
            {
                var CurStat = perfection.Cur;
                var MaxStat = perfection.Max;
                var Percentage = Math.Truncate(((CurStat / MaxStat) * 100) * 10) / 10;
                if (Percentage != 100)
                {
                    return Percentage;
                }
            }
            return 0;
        }
        private bool CanSalvage(IItem item)
        {
            return (!UseInventoryLock || !item.IsInventoryLocked) && 
                item.ItemsInSocket == null && 
                item.EnchantedAffixCounter == 0 && 
                (item.SnoItem.MainGroupCode != "riftkeystone") && 
                (item.SnoItem.MainGroupCode != "horadriccache") && 
                (item.SnoItem.MainGroupCode != "-") && 
                (item.SnoItem.MainGroupCode != "pony") && 
                (item.SnoItem.MainGroupCode != "plans") && 
                !item.SnoItem.MainGroupCode.Contains("cosmetic") && 
                (item.Quantity <= 1) && 
                !InArmorySet(item) && 
                (item.SnoItem.NameEnglish != "Staff of Herding") && 
                (item.SnoItem.NameEnglish != "Hellforge Ember") &&
                (
                ((item.SnoItem.MainGroupCode != "gems_unique" || (SalvageWhisperOfAtonement && item.FullNameEnglish == "Whisper of Atonement" && item.JewelRank < 125)) &&
                (item.AncientRank != 1 || (item.AncientRank == 1 && ((SalvageAncient == 0 && !isBetterItem(item, 1)) || SalvageAncient == 2))) &&
                (item.AncientRank != 2 || (item.AncientRank == 2 && ((SalvagePrimal == 0 && !isBetterItem(item, 2)) || SalvagePrimal == 2))) &&
                (!item.SnoItem.IsEthereal || (item.SnoItem.IsEthereal && ((SalvageEthereal == 0 && !isBetterItem(item, 100)) || SalvageEthereal == 2))) &&
                (!item.SnoItem.Code.StartsWith("HealthPotionLegendary") || (item.SnoItem.Code.StartsWith("HealthPotionLegendary") && (SalvagePotion == 0 && !isBetterPotion(item) || SalvagePotion == 1))) &&
                (!item.SnoItem.Code.StartsWith("P72_Soulshard") || (item.SnoItem.Code.StartsWith("P72_Soulshard") && ((SalvageSoulshard == 0 && !isUpgraded(item) && isOwned(item)) || SalvageSoulshard == 1) && item.JewelRank <= 0)) &&
                (!Whitelist.Contains(item.SnoItem.NameLocalized.ToLower()) && !Whitelist.Contains(item.FullNameLocalized.ToLower()))) || 
                ((!Whitelist.Contains(item.SnoItem.NameLocalized.ToLower()) && !Whitelist.Contains(item.FullNameLocalized.ToLower())) && (Blacklist.Contains(item.SnoItem.NameLocalized.ToLower()) || Blacklist.Contains(item.FullNameLocalized.ToLower())))
                )
                ;
        }

        private List<IItem> GetItemsToSalvage()
        {
            var result = new List<IItem>();
            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                if (item.SnoItem.Kind != ItemKind.loot && item.SnoItem.Kind != ItemKind.potion) continue;
                if (item.VendorBought) continue;

                var canSalvage = CanSalvage(item);
                if (canSalvage) result.Add(item);
            }
            result.Sort((a, b) =>
            {
                var r = a.InventoryX.CompareTo(b.InventoryX);
                if (r == 0) r = a.InventoryY.CompareTo(b.InventoryY);
                return r;
            });
            return result;
        }
        private bool isOwned(IItem item)
        {
            return Hud.Game.Items.Any(i => i.Location != ItemLocation.Floor && i.Location != ItemLocation.Inventory && i.SnoItem.NameEnglish == item.SnoItem.NameEnglish);
        }
        private bool isUpgraded(IItem item)
        {
            return Hud.Game.Items.Any(i => i != item && i.Location != ItemLocation.Floor && i.SnoItem.NameEnglish == item.SnoItem.NameEnglish && i.JewelRank > 0);
        }
        private bool InArmorySet(IItem item)
        {
            bool In = false;
            for (int i = 0; i < Hud.Game.Me.ArmorySets.Length; ++i)
            {
                var armorySet = Hud.Game.Me.ArmorySets[i];
                if (armorySet != null)
                {
                    if (armorySet.ContainsItem(item))
                    {
                        In = true;
                        break;
                    }
                }
            }
            return In;
        }
        private void ExtremeSpeedModeMethod()
        {
            if (TurnedOn == false) return;
            Running = true;
            tempX = Hud.Window.CursorX;
            tempY = Hud.Window.CursorY;
            while (TestTurnedOn() && TurnedOn)
            {
                var itemsToSalvage = GetItemsToSalvage();
                if (itemsToSalvage == null || itemsToSalvage.Count() == 0)
                {
                    TurnedOn = false;
                    Running = false;
                    break;
                }
                SalvageAll(itemsToSalvage);
            }
            Hud.Interaction.MouseMove(tempX, tempY, 1, 1);

        }
        private void NormalModeMethod()
        {
            Running = true;
            var itemsToSalvage = new List<IItem>();
            var itemsSalvaged = new HashSet<string>();
            tempX = Hud.Window.CursorX;
            tempY = Hud.Window.CursorY;
            while (TestTurnedOn())
            {
                var salvageNormal = false;
                var salvageMagic = false;
                var salvageRare = false;
                var salvageAllNormal = true;
                var salvageAllMagic = true;
                var salvageAllRare = true;

                foreach (var item in Hud.Inventory.ItemsInInventory)
                {
                    if (item.SnoItem.Kind != ItemKind.loot && item.SnoItem.Kind != ItemKind.potion) continue;
                    if (item.VendorBought) continue;
                    if (itemsSalvaged.Contains(item.ItemUniqueId)) continue;

                    var canSalvage = CanSalvage(item);
                    if (canSalvage) itemsToSalvage.Add(item);

                    if (item.IsNormal && canSalvage && salvageNormalButton.Visible) salvageNormal = true;
                    else if (item.IsNormal && !canSalvage) salvageAllNormal = false;
                    if (item.IsMagic && canSalvage && salvageMagicButton.Visible) salvageMagic = true;
                    else if (item.IsMagic && !canSalvage) salvageAllMagic = false;
                    if (item.IsRare && canSalvage && salvageRareButton.Visible) salvageRare = true;
                    else if (item.IsRare && !canSalvage) salvageAllRare = false;
                }
                if (itemsToSalvage.Count == 0) break;

                itemsToSalvage.Sort((a, b) =>
                {
                    var r = a.InventoryX.CompareTo(b.InventoryX);
                    if (r == 0) r = a.InventoryY.CompareTo(b.InventoryY);
                    return r;
                });

                if (salvageNormal && salvageAllNormal) SalvageAllOfType(salvageNormalButton);
                if (salvageMagic && salvageAllMagic) SalvageAllOfType(salvageMagicButton);
                if (salvageRare && salvageAllRare) SalvageAllOfType(salvageRareButton);
                if (!(salvageNormal && salvageAllNormal) && !(salvageMagic && salvageAllMagic) && !(salvageRare && salvageAllRare))
                {
                    var selectedItem = itemsToSalvage.First();
                    itemsSalvaged.Add(selectedItem.ItemUniqueId);

                    if (!SalvageOne(selectedItem))
                    {
                        break;
                    }
                }
                itemsToSalvage.Clear();
            }

            SetAnvil(false);
            Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
            TurnedOn = false;
            Running = false;
        }
        public void AfterCollect()
        {
            if (!TestTurnedOn()) return;
            if (Running) return;
            if (ExtremeSpeedMode)
            {
                ExtremeSpeedModeMethod();
            }
            else
            {
                NormalModeMethod();
            }
        }

        private void SetAnvil(bool enabled)
        {
            if (salvageSelectedButton1.Visible)
            {
                var isEnabled = salvageSelectedButton1.AnimState == 19 || salvageSelectedButton1.AnimState == 20;
                if (enabled == isEnabled) return;
                Hud.Interaction.ClickUiElement(MouseButtons.Left, salvageSelectedButton1);
                Hud.Wait(100);
            }
            else
            {
                var isEnabled = salvageSelectedButton2.AnimState == 19 || salvageSelectedButton2.AnimState == 20;
                if (enabled == isEnabled) return;
                Hud.Interaction.ClickUiElement(MouseButtons.Left, salvageSelectedButton1);
                Hud.Wait(100);
            }
        }

        private bool SalvageAllOfType(IUiElement button)
        {
            var startCount = Hud.Inventory.ItemsInInventory.Count();

            SetAnvil(false);

            Hud.Interaction.ClickUiElement(MouseButtons.Left, button);

            if (!Hud.WaitFor(500, 10, 10, () => okButton.Visible))
            {
                return false;
            }

            Hud.Interaction.PressEnter();

            if (!Hud.WaitFor(500, 10, 10, () => !okButton.Visible))
            {
                return false;
            }

            if (!Hud.WaitFor(500, 10, 10, () => Hud.Inventory.ItemsInInventory.Count() < startCount))
            {
                return false;
            }

            Hud.ReCollect();
            return true;
        }

        private bool SalvageOne(IItem item)
        {
            SetAnvil(true);
            for (var i = 0; i < 10; i++)
            {
                Hud.Interaction.ClickInventoryItem(MouseButtons.Left, item);
                if(item.IsLegendary)
                {
                    if(Hud.WaitFor(1000, 10, 10, () => okButton.Visible))
                    {
                        Hud.Interaction.PressEnter();
                    }
                    break;
                }
                if (Hud.WaitFor(1000, 10, 10, () => Hud.Inventory.ItemsInInventory.Any(x => x.ItemUniqueId == item.ItemUniqueId)))
                {
                    break;
                }
                if (i == 10) return false;
            }
            //Hud.ReCollect();
            return true;
        }

        private void SalvageAll(List<IItem>itemsToSalvage)
        {
            //if (salvageSelectedButton1.AnimState != 19 && salvageSelectedButton1.AnimState != 20)
            //{
            //    Hud.Interaction.ClickUiElement(MouseButtons.Left, salvageSelectedButton1);
            //}
            SetAnvil(true);
            foreach (var item in itemsToSalvage)
            {
                if (!TestTurnedOn() || !TurnedOn || !Running) break;
                //Hud.Interaction.ClickInventoryItem(MouseButtons.Left,item);
                Hud.Interaction.MoveMouseOverInventoryItem(item);
                Hud.Interaction.MouseDown(MouseButtons.Left);
                Hud.Wait(1);
                Hud.Interaction.MouseUp(MouseButtons.Left);
                if (item.IsLegendary) Hud.Interaction.PressEnter();
            }
            IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
            if (Hud.WaitFor(100, 10, 10, () => ui.Visible))
            {
                Hud.Interaction.PressEnter();// correction chat
            }
            SetAnvil(false);
            //Hud.ReCollect();
        }

        private bool TestTurnedOn()
        {
            if (!Hud.Game.IsInGame || !salvageDialog.Visible || !Hud.Inventory.InventoryMainUiElement.Visible || (((Hud.Inventory.InventoryLockArea.Width <= 0) || (Hud.Inventory.InventoryLockArea.Height <= 0)) && UseInventoryLock) || !Hud.Window.IsForeground)
            {
                TurnedOn = false;
                Running = false;
            }
            return TurnedOn;
        }
    }
}