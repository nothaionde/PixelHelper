namespace Turbo.Plugins.LightningMod
{
    using SharpDX.DirectInput;
    using System.Linq;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.glq;
    public class ThrowItemPlugin : BasePlugin, IKeyEventHandler, IAfterCollectHandler, IInGameTopPainter
    {
        public int Delay { get; set; } = 10;
        public bool TurnedOn { get; set; }
        public bool Running { get; private set; }
        public bool ThrowLegend { get; set; } = true;
        public bool ThrowAncient { get; set; } = true;
        public bool ThrowPrimal { get; set; } = false;
        public bool ThrowWhite { get; set; } = true;
        public bool ThrowBlue { get; set; } = true;
        public bool ThrowYellow { get; set; } = true;
        public bool UseInventoryLock { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        private IUiElement salvageDialog { get; set; }
        private IUiElement kanaiDialog { get; set; }
        private IUiElement KadalaDialog { get; set; }
        private IUiElement uiInv { get; set; }
        private IUiElement enchantDialog;
        private IUiElement uiGRmainPage;
        private IUiElement gemUpgradePane { get; set; }
        private IItem selectedItem { get; set; }
        private IItem[] items { get; set; }
        private int ItemsCount { get; set; }
        private int tempX;
        private int tempY;
        public IFont HeaderFont { get; private set; }
        public IFont InfoFont { get; private set; }
        private bool UIvisible;
        private string str_Header;
        private string str_InfoLock;
        private string str_Info;
        private string str_NoItem;
        private string str_Running;
        public bool ExtremeSpeedMode { get; set; }
        public ThrowItemPlugin()
        {
            Enabled = true;
            Running = false;
            TurnedOn = false;
            ExtremeSpeedMode = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F4, false, false, false);
            HeaderFont = Hud.Render.CreateFont("tahoma", 10, 255, 200, 200, 100, true, false, 255, 0, 0, 0, true);
            InfoFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            salvageDialog = Hud.Render.GetUiElement("Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog");
            kanaiDialog = Hud.Render.GetUiElement("Root.NormalLayer.Kanais_Recipes_main");
            KadalaDialog = Hud.Render.GetUiElement("Root.NormalLayer.shop_dialog_mainPage.panel");
            enchantDialog = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.enchant_dialog", null, null);
            uiGRmainPage = Hud.Render.RegisterUiElement("Root.NormalLayer.rift_dialog_mainPage", null, null);
            gemUpgradePane = Hud.Render.RegisterUiElement("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane", null, null);
            uiInv = Hud.Inventory.InventoryMainUiElement;
        }
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent))
            {
                if (keyEvent.IsPressed && UIvisible && (!PublicClassPlugin.IsGreaterRiftProgress(Hud) && !PublicClassPlugin.IsGuardianAlive(Hud)))
                {
                    TurnedOn = !TurnedOn;
                    if(TurnedOn)
                    {
                        tempX = Hud.Window.CursorX;
                        tempY = Hud.Window.CursorY;
                    }
                    else
                    {
                        Running = false;
                    }
                }
            }
        }

        private bool InArmorySet(IItem item)
        {
            for (int i = 0; i < Hud.Game.Me.ArmorySets.Length; ++i)
            {
                if (Hud.Game.Me.ArmorySets[i]?.ContainsItem(item) == true)
                {
                    return true;
                }
            }

            return false;
        }
        
        private void ExtremeSpeedModeMethod()
        {
            int x = Hud.Window.Size.Width / 2;
            int y = Hud.Window.Size.Height / 2;
            var Items = Hud.Inventory.ItemsInInventory.Where(item => (!UseInventoryLock || !item.IsInventoryLocked) && item.ItemsInSocket == null && item.EnchantedAffixCounter == 0 && !InArmorySet(item) && (!item.SnoItem.Code.StartsWith("P72_Soulshard")) && (item.SnoItem.NameEnglish != "Hellforge Ember") && (item.SnoItem.NameEnglish != "Angelic Crucible") && (item.SnoItem.NameEnglish != "Petrified Scream") && ((item.IsLegendary && item.SnoItem.SnoItemType.ToString() != "UpgradeableJewel" && ThrowLegend && item.AncientRank == 0 || ThrowAncient && item.AncientRank == 1 || ThrowPrimal && item.AncientRank == 2) || (ThrowYellow && item.IsRare && item.SnoItem.Kind == ItemKind.loot) || (ThrowBlue && item.IsMagic && item.SnoItem.Kind == ItemKind.loot) || (ThrowWhite && item.IsNormal && item.SnoItem.Kind == ItemKind.loot && !item.SnoItem.HasGroupCode("devilshand") && !item.SnoItem.HasGroupCode("riftkeystone"))));
            ItemsCount = Items.Count();
            
            if (TurnedOn)
            {
                if (Items == null || Items.Count() == 0 || !Hud.Window.IsForeground || !UIvisible)
                {
                    TurnedOn = false;
                    return;
                }
                foreach(var item in Items)
                {
                    Hud.Interaction.MoveMouseOverInventoryItem(item);
                    Hud.Interaction.MouseDown(MouseButtons.Left);
                    Hud.Interaction.MouseMove(x, y);
                    Hud.Interaction.MouseUp(MouseButtons.Left);
                }
                Hud.Interaction.MouseMove(tempX, tempY);
            }
        }
        private void NormalModeMethod()
        {
            int x = Hud.Window.Size.Width / 2;
            int y = Hud.Window.Size.Height / 2;
            selectedItem = Hud.Inventory.ItemsInInventory.Where(item => (!UseInventoryLock || !item.IsInventoryLocked) && item.ItemsInSocket == null && item.EnchantedAffixCounter == 0 && !InArmorySet(item) && (!item.SnoItem.Code.StartsWith("P72_Soulshard")) && (item.SnoItem.NameEnglish != "Hellforge Ember") && (item.SnoItem.NameEnglish != "Angelic Crucible") && (item.SnoItem.NameEnglish != "Petrified Scream") && ((item.IsLegendary && item.SnoItem.SnoItemType.ToString() != "UpgradeableJewel" && ThrowLegend && item.AncientRank == 0 || ThrowAncient && item.AncientRank == 1 || ThrowPrimal && item.AncientRank == 2) || (ThrowYellow && item.IsRare && item.SnoItem.Kind == ItemKind.loot) || (ThrowBlue && item.IsMagic && item.SnoItem.Kind == ItemKind.loot) || (ThrowWhite && item.IsNormal && item.SnoItem.Kind == ItemKind.loot && !item.SnoItem.HasGroupCode("devilshand") && !item.SnoItem.HasGroupCode("riftkeystone")))).OrderBy(item => item.InventoryY).ThenBy(item => item.InventoryX).FirstOrDefault();
            
            if (TurnedOn)
            {
                Running = true;
                if (selectedItem == null || !Hud.Window.IsForeground || !UIvisible)
                {
                    Hud.Interaction.MouseMove(tempX, tempY);
                    TurnedOn = false;
                    Running = false;
                    return;
                }
                Hud.Interaction.MoveMouseOverInventoryItem(selectedItem);
                Hud.Interaction.MouseDown(MouseButtons.Left);
                Hud.Interaction.MouseMove(x, y);
                Hud.Wait(Delay);
                Hud.Interaction.MouseUp(MouseButtons.Left);
                Running = false;
            }
        }
        public void AfterCollect()
        {
            if (!uiInv.Visible || salvageDialog.Visible || kanaiDialog.Visible || KadalaDialog.Visible || uiGRmainPage.Visible || enchantDialog.Visible || gemUpgradePane.Visible)
            {
                TurnedOn = false;
                UIvisible = false;
                return;
            }
            else
            {
                UIvisible = true;
            }
            if (!UIvisible || (((Hud.Inventory.InventoryLockArea.Width <= 0) || (Hud.Inventory.InventoryLockArea.Height <= 0)) && UseInventoryLock))
            {
                TurnedOn = false;
                Running = false;
                return;
            }
            if (ExtremeSpeedMode)
            {
                ExtremeSpeedModeMethod();
            }
            else
            {
                if (Running) return;
                NormalModeMethod();
            }
        }
        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory) return;
			if (!UIvisible) return;
            var y = uiInv.Rectangle.Y + uiInv.Rectangle.Height * 0.02f;
            
            if (Hud.CurrentLanguage == Language.zhCN)
            {
                str_Header = "【雷电宏-自动丢物品】";
                str_InfoLock = "没有开启物品锁，无法使用该插件\r\n请在雷电宏相关中设置物品锁";
                str_Info = "单击 " + ToggleKeyEvent.ToString() + " 开始自动丢物品";
                if (UseInventoryLock) str_Info = str_Info + "\r\n注意：请把不想被丢弃的物品\r\n放在物品锁区域内（蓝色框）";
                str_NoItem = "没有物品可被丢弃";
                str_Running = "自动丢弃中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.zhTW)
            {
                str_Header = "【雷電宏-自動丟物品】";
                str_InfoLock = "沒有開啟物品鎖，無法使用該插件\r\n請在雷電宏相關中設置物品鎖";
                str_Info = "單擊 " + ToggleKeyEvent.ToString() + " 開始自動丟物品";
                if (UseInventoryLock) str_Info = str_Info + "\r\n注意：請把不想被丟棄的物品\r\n放在物品鎖區域內（藍色框）";
                str_NoItem = "沒有物品可被丟棄";
                str_Running = "自動丟棄中...\r\n按住 " + ToggleKeyEvent.ToString() + " 停止";
            }
            else if (Hud.CurrentLanguage == Language.ruRU)
            {
                str_Header = "【МОД - Сброс на Землю】";
                str_InfoLock = "Нет блокировки в инвентаре\r\nЗадайте её в закладке Макрос";
                str_Info = "Нажать " + ToggleKeyEvent.ToString() + " для Старта";
                str_NoItem = "Нет предметов для Сброса";
                str_Running = "Сброс...";
            }
            else
            {
                str_Header = "【ThrowItem-Mod】";
                str_InfoLock = "inventory lock is missing\r\nYou need to set it in Macros";
                str_Info = "press " + ToggleKeyEvent.ToString() + " to start";
                str_NoItem = "no items to Throw";
                str_Running = "throwing...";
            }
            var layout = HeaderFont.GetTextLayout(str_Header);
            if (PublicClassPlugin.IsGreaterRiftProgress(Hud) || PublicClassPlugin.IsGuardianAlive(Hud)) return;
            if (TurnedOn)
            {
                HeaderFont.DrawText(layout, uiInv.Rectangle.X + (uiInv.Rectangle.Width - layout.Metrics.Width) / 2, y);
                y += layout.Metrics.Height * 1.3f;
                layout = InfoFont.GetTextLayout(str_Running);
                InfoFont.DrawText(layout, uiInv.Rectangle.X + (uiInv.Rectangle.Width - layout.Metrics.Width) / 2, y);
            }
            else if (uiInv?.Visible != false && salvageDialog?.Visible != true && kanaiDialog?.Visible != true && KadalaDialog?.Visible != true && uiGRmainPage?.Visible != true && enchantDialog?.Visible != true && gemUpgradePane?.Visible != true)
            {
                HeaderFont.DrawText(layout, uiInv.Rectangle.X + (uiInv.Rectangle.Width - layout.Metrics.Width) / 2, y);
                y += layout.Metrics.Height * 1.3f;
                if (((Hud.Inventory.InventoryLockArea.Width <= 0) || (Hud.Inventory.InventoryLockArea.Height <= 0)) && UseInventoryLock)
                {
                    layout = InfoFont.GetTextLayout(str_InfoLock);
                    InfoFont.DrawText(layout, uiInv.Rectangle.X + (uiInv.Rectangle.Width - layout.Metrics.Width) / 2, y);
                    return;
                }
                if (selectedItem != null || ItemsCount > 0)
                {
                    layout = InfoFont.GetTextLayout(str_Info);
                    InfoFont.DrawText(layout, uiInv.Rectangle.X + (uiInv.Rectangle.Width - layout.Metrics.Width) / 2, y);
                }
                else
                {
                    layout = InfoFont.GetTextLayout(str_NoItem);
                    InfoFont.DrawText(layout, uiInv.Rectangle.X + (uiInv.Rectangle.Width - layout.Metrics.Width) / 2, y);
                }
            }

        }
    }
}