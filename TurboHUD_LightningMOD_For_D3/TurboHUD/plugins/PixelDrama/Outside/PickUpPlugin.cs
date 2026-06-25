namespace Turbo.Plugins.PixelDrama.Outside
{
    using Turbo.Plugins.glq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using SharpDX.DirectInput;
    using Turbo.Plugins.Default;

    public class PickUpPlugin
    {
        public bool PickGem { get; set; } = true;
        public bool PickCraft { get; set; } = true;
        public bool PickLegend { get; set; } = true;
        public bool PickAncient { get; set; } = true;
        public bool PickPrimal { get; set; } = true;
        public bool PickWhite { get; set; } = true;
        public bool PickBlue { get; set; } = true;
        public bool PickYellow { get; set; } = true;
        public bool PickDB { get; set; } = true;
        public bool AutoPickupOutTown { get; set; } = true;
        public bool ShowTips { get; set; } = false;
        public bool WhenForceMoveInvalid { get; set; } = true;
        public IKeyEvent ToggleKeyEvent { get; set; }
        public bool Running { get; private set; }
        public IFont TipsFont { get; private set; }
        private IItem selectedItem { get; set; }
        private IItem clickedItem { get; set; }
        private IUiElement uiInv { get; set; }
        private readonly bool[,] _usedSpaces = new bool[10, 6];
        public int IntervalMilliseconds { get; set; } = 1;
        private IWatch _timer;
        private string str_Info;
        private string str_Running;
        private int itmesCount;
        private int inventoryFreeSpace;
        public bool isItemsAround = false;
        public bool Clicking = false;
        public bool WaitingforClick = false;
        public double PickupRadius = 10;
        private readonly HashSet<uint> _clickedAnnIds = new HashSet<uint>();
        private readonly HashSet<string> Whitelist = new HashSet<string>() { };
        private readonly HashSet<string> Blacklist = new HashSet<string>() { };
        private IController Hud;

        public PickUpPlugin(IController hud)
        {
            Hud = hud;
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F4, false, false, false);
            TipsFont = Hud.Render.CreateFont("tahoma", 8, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            uiInv = Hud.Inventory.InventoryMainUiElement;
            _timer = Hud.Time.CreateAndStartWatch();
            PickGem = PixelHelperSettings.Instance.PickMaterials;
            PickCraft = PixelHelperSettings.Instance.PickMaterials;
            PickLegend = PixelHelperSettings.Instance.PickLegendaries;
            PickAncient = PixelHelperSettings.Instance.PickLegendaries;
            PickPrimal = PixelHelperSettings.Instance.PickLegendaries;
            PickWhite = PixelHelperSettings.Instance.PickWhite;
            PickBlue = PixelHelperSettings.Instance.PickBlue;
            PickYellow = PixelHelperSettings.Instance.PickYellow;
            PickDB = PixelHelperSettings.Instance.PickDeathBreath;
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
        private bool CalculateStackItemfreeSpace(IItem item, int max)
        {
            bool stackable = Hud.Game.Items.Any(i => {
                return i.Location == ItemLocation.Inventory && i.SnoItem.Sno == item.SnoItem.Sno && item.Quantity + i.Quantity <= max;
            });
            bool freeSpace = inventoryFreeSpace >= item.SnoItem.ItemWidth * item.SnoItem.ItemHeight;
            return stackable || freeSpace;
        }
        private bool Calculate1x2freeSpace()
        {
            Array.Clear(_usedSpaces, 0, _usedSpaces.Length);
            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                for (var x = 0; x < item.SnoItem.ItemWidth; x++)
                {
                    for (var y = 0; y < item.SnoItem.ItemHeight; y++)
                    {
                        var xpos = item.InventoryX + x;
                        var ypos = item.InventoryY + y;
                        if (xpos >= 0 && xpos < 10 && ypos >= 0 && ypos < 6)
                        {
                            _usedSpaces[xpos, ypos] = true;
                        }
                    }
                }
            }

            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 6 - 1; y++)
                {
                    if (!_usedSpaces[x, y] && !_usedSpaces[x, y + 1])
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public void OnNewArea(bool newGame, ISnoArea area)
        {
            isItemsAround = false;
            _clickedAnnIds.Clear();
            selectedItem = null;
        }

        private IQuest ENQuest
        {
            get
            {
                return Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 486773);//梦魇回响
            }
        }
        private bool IsInEchoingNightmareCanbePickup
        {
            get
            {
                return ENQuest != null &&
                       (ENQuest.QuestStepId == 2 || ENQuest.QuestStepId == 9 || ENQuest.State == QuestState.completed);//2为打开宝箱,9为完成进度或死亡结束
            }
        }

        public void tryToPickUp()
        {
            PickGem = PixelHelperSettings.Instance.PickMaterials;
            PickCraft = PixelHelperSettings.Instance.PickMaterials;
            PickLegend = PixelHelperSettings.Instance.PickLegendaries;
            PickAncient = PixelHelperSettings.Instance.PickLegendaries;
            PickPrimal = PixelHelperSettings.Instance.PickLegendaries;
            PickWhite = PixelHelperSettings.Instance.PickWhite;
            PickBlue = PixelHelperSettings.Instance.PickBlue;
            PickYellow = PixelHelperSettings.Instance.PickYellow;

            if (!Hud.Game.IsInGame)
                return;
            if (Hud.Game.IsPaused)
                return;
            if (Hud.Game.IsLoading)
                return;
            if (Hud.Game.Me.IsDead)
                return;
            if (Hud.Game.Me.Powers.CantMove)
                return;
            if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorGhostedBuff.Sno))
                return;
            if (PublicClassPlugin.isCasting(Hud))
                return;
            if (!Hud.Window.IsForeground)
                return;
            if (!Hud.Render.MinimapUiElement.Visible)
                return;
            if (Hud.Render.IsAnyBlockingUiElementVisible)
                return;
            if (!_timer.TimerTest(IntervalMilliseconds))
                return;
            if (Hud.Game.Me.SnoArea.NameEnglish == "Echoing Nightmare" && !IsInEchoingNightmareCanbePickup)//梦魇回响中未完成前不拾取
                return;
            if (Clicking)
            {
                return;
            }
            if (uiInv.Visible)
            {
                isItemsAround = false;
                Running = false;
                return;
            }
            inventoryFreeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
            bool? has1x2free = null;
            var DiabolicalChest = Hud.Game.Actors.Where(x => x.IsOnScreen && !x.IsDisabled && !x.IsOperated && x.SnoActor.NameEnglish == "Diabolical Chest").FirstOrDefault();
            var Items = Hud.Game.Items.Where(item =>
            {
                if (WhenForceMoveInvalid && Hud.Interaction.IsHotKeySet(ActionKey.Move) && Hud.Interaction.IsContinuousActionStarted(ActionKey.Move))
                    return false;
                if (item.Location != ItemLocation.Floor || !item.IsOnScreen || item.NormalizedXyDistanceToMe > PickupRadius || (item.AccountBound && !item.BoundToMyAccount))
                    return false;
                if (_clickedAnnIds.Contains(item.AnnId))
                    return false;
                if (DiabolicalChest != null && item.FloorCoordinate.XYDistanceTo(DiabolicalChest.FloorCoordinate) < 10)
                    return false;
                if (Whitelist.Contains(item.SnoItem.NameLocalized.ToLower()))//白名单物品（比如“团结”）
                    return false;
                if (Whitelist.Contains(item.FullNameLocalized.ToLower()))//白名单物品（比如“太古传奇 团结”）
                    return false;
                if (Blacklist.Contains(item.SnoItem.NameLocalized.ToLower()))//黑名单物品（比如“团结”）
                    return false;
                if (Blacklist.Contains(item.FullNameLocalized.ToLower()))//黑名单物品（比如“太古传奇 团结”）
                    return false;
                if (PickCraft && item.SnoItem.Kind == ItemKind.craft && item.SnoItem.NameEnglish != "Ramaladni's Gift")//材料但不为打孔器时可直接拾取
                    return true;

                if (PickCraft && item.SnoItem.HasGroupCode("uber") && item.SnoItem.Sno != 2788723894 && item.SnoItem.Sno != 1054965529 && item.SnoItem.Sno != 2622355732 && item.SnoItem.Sno != 1458185494)//红门材料
                    return true;
                if (PickCraft && item.SnoItem.HasGroupCode("uber") && (item.SnoItem.Sno == 2788723894 || item.SnoItem.Sno == 1054965529 || item.SnoItem.Sno == 2622355732 || item.SnoItem.Sno == 1458185494) && CalculateStackItemfreeSpace(item, 100))//炼狱装置
                    return true;

                if (PickCraft && item.SnoItem.HasGroupCode("riftkeystone"))
                    return true;

                if (PickCraft && item.SnoItem.Kind == ItemKind.book)
                    return true;

                if (item.SnoItem.Sno == 2087837753)//死亡之息
                    return PixelHelperSettings.Instance.PickDeathBreath;
                if (PickCraft && item.SnoItem.NameEnglish == "Ramaladni's Gift" && CalculateStackItemfreeSpace(item, 100))//打孔器
                    return false;

                if (PickCraft && item.SnoItem.Code.StartsWith("P72_Soulshard"))//灵魂碎片
                    return true;
                if (PickCraft && item.SnoItem.NameEnglish == "Hellforge Ember")//地狱熔炉余烬
                    return true;
                if (PickCraft && item.SnoItem.NameEnglish == "Whisper of Atonement")//贖罪之語，赎罪低语，梦魇回响里的宝石
                    return true;
                if (PickCraft && item.SnoItem.NameEnglish == "Holiday Gift")//节日礼物
                    return true;
                if (item.SnoItem.Kind != ItemKind.gem && item.SnoItem.Kind != ItemKind.craft && inventoryFreeSpace < item.SnoItem.ItemWidth * item.SnoItem.ItemHeight)//除宝石和材料外至少要1个物品格子
                    return false;

                if (item.SnoItem.ItemWidth == 1 && item.SnoItem.ItemHeight == 2)
                {
                    if (has1x2free == null)
                    {
                        has1x2free = Calculate1x2freeSpace();
                    }

                    if (has1x2free == false)
                        return false;
                }

                if (PickGem && item.SnoItem.Kind == ItemKind.gem && CalculateStackItemfreeSpace(item, 5000))
                    return true;

                if (PickLegend && item.IsLegendary && (item.SnoItem.Kind == ItemKind.loot || item.SnoItem.Kind == ItemKind.potion) && item.AncientRank == 0)
                    return true;

                if (PickAncient && item.IsLegendary && item.SnoItem.Kind == ItemKind.loot && item.AncientRank == 1)
                    return true;

                if (PickPrimal && item.IsLegendary && item.SnoItem.Kind == ItemKind.loot && item.AncientRank == 2)
                    return true;

                if (PickYellow && item.IsRare && item.SnoItem.Kind == ItemKind.loot)
                    return true;

                if (PickBlue && item.IsMagic && item.SnoItem.Kind == ItemKind.loot)
                    return true;

                if (PickWhite && item.IsNormal && item.SnoItem.Kind == ItemKind.loot && !item.SnoItem.HasGroupCode("devilshand") && !item.SnoItem.HasGroupCode("riftkeystone") && item.SnoItem.NameEnglish != "Holiday Gift")
                    return true;
                return false;
            }).OrderBy(item => item.NormalizedXyDistanceToMe);
            itmesCount = Items.Count();
            selectedItem = Items.FirstOrDefault();
            if (itmesCount > 1 && inventoryFreeSpace < 2)
            {
                Running = false;
                isItemsAround = false;
                return;
            }
            isItemsAround = (itmesCount > 0 && selectedItem != null && selectedItem.Location == ItemLocation.Floor) ? true : false;
            if (isItemsAround == false)
            {
                Running = false;
                return;
            }
            clickedItem = Hud.Game.Items.Where(item =>
            {
                if (item.Location != ItemLocation.Floor || !item.IsOnScreen || item.NormalizedXyDistanceToMe > PickupRadius || (item.AccountBound && !item.BoundToMyAccount))
                    return false;
                if (_clickedAnnIds.Contains(item.AnnId))
                    return true;
                return false;
            }).OrderBy(item => item.NormalizedXyDistanceToMe).FirstOrDefault();
            if (selectedItem == null || uiInv.Visible)
            {
                isItemsAround = false;
                Running = false;
                return;
            }

            if ((!Hud.Game.Me.IsInTown && AutoPickupOutTown) || Running)
            {
                var tempX = Hud.Window.CursorX;
                var tempY = Hud.Window.CursorY;
                PickUpItem();
                Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
                Hud.Interaction.MouseUp(MouseButtons.Left);
            }
        }

        private void PickUpItem()
        {
            Clicking = true;
            try
            {

                for (var i = 0; i <= 50; i++)
                {
                    Hud.Interaction.MouseMove(selectedItem.ScreenCoordinate.X, selectedItem.ScreenCoordinate.Y, 1, 1);
                    Hud.Interaction.MouseDown(MouseButtons.Left);
                    Hud.Interaction.MouseUp(MouseButtons.Left);
                    if (!Hud.Game.Items.Any(x => x.AnnId == selectedItem.AnnId && x.Location == ItemLocation.Floor))
                    {
                        break;
                    }
                    if (uiInv.Visible || (itmesCount > 1 && inventoryFreeSpace < 2) || (WhenForceMoveInvalid && Hud.Interaction.IsHotKeySet(ActionKey.Move) && Hud.Interaction.IsContinuousActionStarted(ActionKey.Move)))
                    {
                        isItemsAround = false;
                        Running = false;
                        break;
                    }
                    if (i == 50)
                        _clickedAnnIds.Add(selectedItem.AnnId);
                    Hud.ReCollect();
                    Hud.Wait(5);
                }
                _timer.Restart();
            }
            catch (NullReferenceException) { }
            finally
            {
                Clicking = false;
            }
        }
    }
}