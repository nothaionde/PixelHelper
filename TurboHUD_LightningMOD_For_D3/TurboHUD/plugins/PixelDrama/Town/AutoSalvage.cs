namespace Turbo.Plugins.PixelDrama.Town
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.Miqui;

    public class AutoSalvage
    {
        protected IUiElement VENDOR_MAIN;
        protected IUiElement VENDOR_REPAIRALL;

        protected IUiElement VENDOR_TAB_0;
        protected IUiElement VENDOR_TAB_1;
        protected IUiElement VENDOR_TAB_2;
        protected IUiElement VENDOR_TAB_3;

        private readonly IController Hud;
        private bool doRepair { get; set; } = false;
        private long msLapseAction { get; set; } = 0;
        private IUiElement TabActive { get; set; } = null;
        public ISnoArea LastSnoArea { get; set; } = null;

        public long msLapseMin { get; set; } = 10;
        public bool AfterRepairGoToSalvageTab { get; set; }
        public bool TurnedOn { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public bool Running { get; private set; }
        public int SalvageSoulshard { get; set; }
        public int SalvagePotion { get; set; }
        public bool SalvageWhisperOfAtonement { get; set; }
        public int SalvageEthereal { get; set; }
        public int SalvagePrimal { get; set; }
        public int SalvageAncient { get; set; }
        private readonly HashSet<string> Whitelist = new HashSet<string>() { };
        private readonly HashSet<string> Blacklist = new HashSet<string>() { };
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

        public AutoSalvage(IController hud)
        {
            Hud = hud;

            AfterRepairGoToSalvageTab = false;

            VENDOR_MAIN = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage",
                null,
                null
            );
            VENDOR_REPAIRALL = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.repair_dialog.RepairAll",
                null,
                null
            );

            VENDOR_TAB_0 = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.tab_0",
                null,
                null
            );
            VENDOR_TAB_1 = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.tab_1",
                null,
                null
            );
            VENDOR_TAB_2 = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.tab_2",
                null,
                null
            );
            VENDOR_TAB_3 = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.tab_3",
                null,
                null
            );

            TurnedOn = false;
            Running = false;
            ExtremeSpeedMode = true;
            SalvageAncient = 1;
            SalvagePrimal = 1;
            SalvageEthereal = 0;
            SalvagePotion = 0;
            SalvageSoulshard = 0;
            SalvageWhisperOfAtonement = false;
            vendorPage = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage",
                Hud.Inventory.InventoryMainUiElement,
                null
            );
            salvageDialog = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog",
                vendorPage,
                null
            );
            salvageNormalButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_normal_button",
                salvageDialog,
                null
            );
            salvageMagicButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_magic_button",
                salvageDialog,
                null
            );
            salvageRareButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_rare_button",
                salvageDialog,
                null
            );
            salvageSelectedButton1 = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_all_wrapper.salvage_button",
                salvageDialog,
                null
            );
            salvageSelectedButton2 = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.vendor_dialog_mainPage.salvage_dialog.salvage_button",
                salvageDialog,
                null
            );
            okButton = Hud.Render.RegisterUiElement(
                "Root.TopLayer.confirmation.subdlg.stack.wrap.button_ok",
                salvageDialog,
                null
            );
        }

        public void TryToSalvage(ISnoArea area)
        {
            SalvageAncient = PixelHelperSettings.Instance.KeepAncients ? 1 : 0;
            SalvagePrimal = PixelHelperSettings.Instance.KeepPrimals ? 1 : 0;
            if (LastSnoArea != area)
            {
                if (area.IsTown)
                {
                    msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
                    TabActive = null;
                    doRepair = true;
                }
                LastSnoArea = area;
            }

            if (!Hud.Game.IsInGame || !Hud.Game.IsInTown || Hud.Game.IsPaused)
            {
                return;
            }

            if (doRepair)
            {
                if (VENDOR_MAIN.Visible && VENDOR_TAB_3.Visible)
                {
                    if (Hud.Game.CurrentRealTimeMilliseconds - msLapseAction > msLapseMin)
                    {
                        msLapseAction = Hud.Game.CurrentRealTimeMilliseconds;
                        if (Hud.Game.Me.AnimationState == AcdAnimationState.Idle)
                        {
                            if (VENDOR_TAB_3.AnimState == 34)
                            {
                                if (VENDOR_REPAIRALL.Visible)
                                {
                                    var match = Regex.Match(
                                        VENDOR_REPAIRALL.ReadText(Encoding.UTF8, true),
                                        @"([0-9]+)"
                                    );
                                    if (match.Success && match.Groups[1].Value != "0")
                                    {
                                        mouseLClickUiE(VENDOR_REPAIRALL);
                                    }
                                    else
                                    {
                                        if (AfterRepairGoToSalvageTab)
                                            TabActive = VENDOR_TAB_2;
                                        if (TabActive != null)
                                        {
                                            mouseLClickUiE(TabActive);
                                            TabActive = null;
                                        }
                                        doRepair = false;
                                    }
                                }
                            }
                            else
                            {
                                if (VENDOR_TAB_0.AnimState == 16)
                                    TabActive = VENDOR_TAB_0;
                                else if (VENDOR_TAB_1.AnimState == 13)
                                    TabActive = VENDOR_TAB_1;
                                else if (VENDOR_TAB_2.AnimState == 37)
                                    TabActive = VENDOR_TAB_2;
                                if (TabActive != null)
                                    mouseLClickUiE(VENDOR_TAB_3);
                            }
                        }
                    }
                }
            }

            if (!TurnedOn)
            {
                TurnedOn = true;
            }

            if (!TestTurnedOn())
                return;
            if (Running)
                return;
            if (!GetItemsToSalvage().Any())
            {
                return;
            }
            if (ExtremeSpeedMode)
            {
                ExtremeSpeedModeMethod();
            }
            else
            {
                NormalModeMethod();
            }
        }

        private void mouseLClickUiE(IUiElement uie)
        {
            int tempX = Hud.Window.CursorX;
            int tempY = Hud.Window.CursorY;
            Hud.Interaction.ClickUiElement(MouseButtons.Left, uie);
            Hud.Interaction.MouseMove(tempX, tempY);
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

        private bool isBetterItem(IItem item, int rank) //1远古，2太古，100无形
        {
            var equippedItem = Hud.Game.Items.FirstOrDefault(i =>
                i.Location >= ItemLocation.Head
                && i.Location <= ItemLocation.Neck
                && i.SnoItem.NameEnglish == item.SnoItem.NameEnglish
            );
            if (equippedItem != null)
            {
                var batterItem = rank == 100 ? item.SnoItem.IsEthereal : item.AncientRank == rank;
                return batterItem;
            }
            return false;
        }

        private bool isBetterPotion(IItem Potion)
        {
            bool newPotion = !Hud.Game.Items.Any(i =>
                i.SnoItem.Code.StartsWith("HealthPotionLegendary")
                && (
                    i.Location == ItemLocation.Stash
                    || i.Location == ItemLocation.Inventory
                    || i.Location == ItemLocation.MerchantAvaibleItemsForPurchase
                )
                && i.SnoItem.NameEnglish == Potion.SnoItem.NameEnglish
                && i != Potion
            );
            if (newPotion)
            {
                return true;
            }
            else
            {
                var potionPerfection = PotionPerfection(Potion);
                var potions = Hud.Game.Items.Where(i =>
                    i.SnoItem.Code.StartsWith("HealthPotionLegendary")
                    && (
                        i.Location == ItemLocation.Stash
                        || i.Location == ItemLocation.Inventory
                        || i.Location == ItemLocation.MerchantAvaibleItemsForPurchase
                    )
                    && i.SnoItem.NameEnglish == Potion.SnoItem.NameEnglish
                    && i != Potion
                );
                if (potions != null)
                {
                    var potion = potions
                        .OrderByDescending(i => PotionPerfection(i))
                        .FirstOrDefault();
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

        private bool ShouldProtectByPixelFilter(IItem item)
        {
            var protectItems = PixelHelperSettings.Instance.GetItemsToSave();
            var matchingFilter = protectItems.FirstOrDefault(f =>
                f.ItemName.Equals(item.SnoItem.NameEnglish)
            );

            if (matchingFilter == null)
                return false;

            if (matchingFilter.IsAncient && item.AncientRank == 0)
                return false; 

            if (!string.IsNullOrEmpty(matchingFilter.MinAffix))
            {
                if (!double.TryParse(matchingFilter.MinAffix, out double minAffixValue))
                    return true;
                var itemAffixValue = LegendaryItemAffixPlugin.Instance?.GetAffixValue(item) ?? 0.0;

                if (itemAffixValue < minAffixValue)
                    return false;
            }
            
            return true;
        }

        private bool CanSalvage(IItem item)
        {
            if (ShouldProtectByPixelFilter(item))
            {
                return false;
            }
            return (!UseInventoryLock || !item.IsInventoryLocked)
                && item.ItemsInSocket == null
                && item.EnchantedAffixCounter == 0
                && (item.SnoItem.MainGroupCode != "riftkeystone")
                && (item.SnoItem.MainGroupCode != "horadriccache")
                && (item.SnoItem.MainGroupCode != "-")
                && (item.SnoItem.MainGroupCode != "pony")
                && (item.SnoItem.MainGroupCode != "plans")
                && !item.SnoItem.MainGroupCode.Contains("cosmetic")
                && (item.Quantity <= 1)
                && !InArmorySet(item)
                && (item.SnoItem.NameEnglish != "Staff of Herding")
                && (item.SnoItem.NameEnglish != "Hellforge Ember")
                && (
                    (
                        (
                            item.SnoItem.MainGroupCode != "gems_unique"
                            || (
                                SalvageWhisperOfAtonement
                                && item.FullNameEnglish == "Whisper of Atonement"
                                && item.JewelRank < 125
                            )
                        )
                        && (
                            item.AncientRank != 1
                            || (
                                item.AncientRank == 1
                                && (
                                    (SalvageAncient == 0 && !isBetterItem(item, 1))
                                    || SalvageAncient == 2
                                )
                            )
                        )
                        && (
                            item.AncientRank != 2
                            || (
                                item.AncientRank == 2
                                && (
                                    (SalvagePrimal == 0 && !isBetterItem(item, 2))
                                    || SalvagePrimal == 2
                                )
                            )
                        )
                        && (
                            !item.SnoItem.IsEthereal
                            || (
                                item.SnoItem.IsEthereal
                                && (
                                    (SalvageEthereal == 0 && !isBetterItem(item, 100))
                                    || SalvageEthereal == 2
                                )
                            )
                        )
                        && (
                            !item.SnoItem.Code.StartsWith("HealthPotionLegendary")
                            || (
                                item.SnoItem.Code.StartsWith("HealthPotionLegendary")
                                && (
                                    SalvagePotion == 0 && !isBetterPotion(item)
                                    || SalvagePotion == 1
                                )
                            )
                        )
                        && (
                            !item.SnoItem.Code.StartsWith("P72_Soulshard")
                            || (
                                item.SnoItem.Code.StartsWith("P72_Soulshard")
                                && (
                                    (SalvageSoulshard == 0 && !isUpgraded(item) && isOwned(item))
                                    || SalvageSoulshard == 1
                                )
                                && item.JewelRank <= 0
                            )
                        )
                        && (
                            !Whitelist.Contains(item.SnoItem.NameLocalized.ToLower())
                            && !Whitelist.Contains(item.FullNameLocalized.ToLower())
                        )
                    )
                    || (
                        (
                            !Whitelist.Contains(item.SnoItem.NameLocalized.ToLower())
                            && !Whitelist.Contains(item.FullNameLocalized.ToLower())
                        )
                        && (
                            Blacklist.Contains(item.SnoItem.NameLocalized.ToLower())
                            || Blacklist.Contains(item.FullNameLocalized.ToLower())
                        )
                    )
                );
        }

        public bool IsSalvageTabIsActive()
        {
            return VENDOR_MAIN.Visible && VENDOR_TAB_3.Visible;
        }

        public List<IItem> GetItemsToSalvage()
        {
            var result = new List<IItem>();
            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                if (item.SnoItem.Kind != ItemKind.loot && item.SnoItem.Kind != ItemKind.potion)
                    continue;
                if (item.VendorBought)
                    continue;

                var canSalvage = CanSalvage(item);
                if (canSalvage)
                    result.Add(item);
            }
            result.Sort(
                (a, b) =>
                {
                    var r = a.InventoryX.CompareTo(b.InventoryX);
                    if (r == 0)
                        r = a.InventoryY.CompareTo(b.InventoryY);
                    return r;
                }
            );
            return result;
        }

        private bool isOwned(IItem item)
        {
            return Hud.Game.Items.Any(i =>
                i.Location != ItemLocation.Floor
                && i.Location != ItemLocation.Inventory
                && i.SnoItem.NameEnglish == item.SnoItem.NameEnglish
            );
        }

        private bool isUpgraded(IItem item)
        {
            return Hud.Game.Items.Any(i =>
                i != item
                && i.Location != ItemLocation.Floor
                && i.SnoItem.NameEnglish == item.SnoItem.NameEnglish
                && i.JewelRank > 0
            );
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
            if (TurnedOn == false)
                return;
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
                    if (item.SnoItem.Kind != ItemKind.loot && item.SnoItem.Kind != ItemKind.potion)
                        continue;
                    if (item.VendorBought)
                        continue;
                    if (itemsSalvaged.Contains(item.ItemUniqueId))
                        continue;

                    var canSalvage = CanSalvage(item);
                    if (canSalvage)
                        itemsToSalvage.Add(item);

                    if (item.IsNormal && canSalvage && salvageNormalButton.Visible)
                        salvageNormal = true;
                    else if (item.IsNormal && !canSalvage)
                        salvageAllNormal = false;
                    if (item.IsMagic && canSalvage && salvageMagicButton.Visible)
                        salvageMagic = true;
                    else if (item.IsMagic && !canSalvage)
                        salvageAllMagic = false;
                    if (item.IsRare && canSalvage && salvageRareButton.Visible)
                        salvageRare = true;
                    else if (item.IsRare && !canSalvage)
                        salvageAllRare = false;
                }
                if (itemsToSalvage.Count == 0)
                    break;

                itemsToSalvage.Sort(
                    (a, b) =>
                    {
                        var r = a.InventoryX.CompareTo(b.InventoryX);
                        if (r == 0)
                            r = a.InventoryY.CompareTo(b.InventoryY);
                        return r;
                    }
                );

                if (salvageNormal && salvageAllNormal)
                    SalvageAllOfType(salvageNormalButton);
                if (salvageMagic && salvageAllMagic)
                    SalvageAllOfType(salvageMagicButton);
                if (salvageRare && salvageAllRare)
                    SalvageAllOfType(salvageRareButton);
                if (
                    !(salvageNormal && salvageAllNormal)
                    && !(salvageMagic && salvageAllMagic)
                    && !(salvageRare && salvageAllRare)
                )
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
            if (!TestTurnedOn())
                return;
            if (Running)
                return;
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
                var isEnabled =
                    salvageSelectedButton1.AnimState == 19
                    || salvageSelectedButton1.AnimState == 20;
                if (enabled == isEnabled)
                    return;
                Hud.Interaction.ClickUiElement(MouseButtons.Left, salvageSelectedButton1);
                Hud.Wait(100);
            }
            else
            {
                var isEnabled =
                    salvageSelectedButton2.AnimState == 19
                    || salvageSelectedButton2.AnimState == 20;
                if (enabled == isEnabled)
                    return;
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

            if (
                !Hud.WaitFor(500, 10, 10, () => Hud.Inventory.ItemsInInventory.Count() < startCount)
            )
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
                if (item.IsLegendary)
                {
                    if (Hud.WaitFor(1000, 10, 10, () => okButton.Visible))
                    {
                        Hud.Interaction.PressEnter();
                    }
                    break;
                }
                if (
                    Hud.WaitFor(
                        1000,
                        10,
                        10,
                        () =>
                            Hud.Inventory.ItemsInInventory.Any(x =>
                                x.ItemUniqueId == item.ItemUniqueId
                            )
                    )
                )
                {
                    break;
                }
                if (i == 10)
                    return false;
            }
            //Hud.ReCollect();
            return true;
        }

        private void SalvageAll(List<IItem> itemsToSalvage)
        {
            SetAnvil(true);
            foreach (var item in itemsToSalvage)
            {
                if (!TestTurnedOn() || !TurnedOn || !Running)
                    break;
                Hud.Interaction.MoveMouseOverInventoryItem(item);
                Hud.Interaction.MouseDown(MouseButtons.Left);
                Hud.Wait(1);
                Hud.Interaction.MouseUp(MouseButtons.Left);
                if (item.IsLegendary)
                    Hud.Interaction.PressEnter();
            }
            IUiElement ui = Hud.Render.GetUiElement(
                "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline"
            );
            if (Hud.WaitFor(100, 10, 10, () => ui.Visible))
            {
                Hud.Interaction.PressEnter(); // correction chat
            }
            SetAnvil(false);
            //Hud.ReCollect();
        }

        private bool TestTurnedOn()
        {
            if (
                !Hud.Game.IsInGame
                || !salvageDialog.Visible
                || !Hud.Inventory.InventoryMainUiElement.Visible
                || (
                    (
                        (Hud.Inventory.InventoryLockArea.Width <= 0)
                        || (Hud.Inventory.InventoryLockArea.Height <= 0)
                    ) && UseInventoryLock
                )
                || !Hud.Window.IsForeground
            )
            {
                TurnedOn = false;
                Running = false;
            }
            return TurnedOn;
        }
    }
}
