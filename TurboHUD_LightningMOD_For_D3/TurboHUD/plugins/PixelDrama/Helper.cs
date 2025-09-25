namespace Turbo.Plugins.PixelDrama
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;
    using SharpDX.DirectInput;
    using Turbo.Plugins.Default;
    using Turbo.Plugins.PixelDrama.Town;
    using Turbo.Plugins.PixelDrama.Outside;

    using System.IO;

    public class Helper
        : BasePlugin,
          IAfterCollectHandler,
          INewAreaHandler,
          IInGameTopPainter,
          IKeyEventHandler
    {
        public static Helper _helper = null;
        public string statusText = "N/A";
        public string debugText = "N/A";
        public bool isHelperOn;
        private IController Hud;
        private PixelHelper pixelHelper;
        private Zoom zoom;
        private AutoGamble autoGamble;
        private AutoSalvage autoSalvagePlugin;
        private AlwaysUseYourParagon alwaysUseYourParagon;
        private PickUpPlugin pickUpPlugin;
        private AutoUpgrade autoUpgrade;
        private OpenChestPlugin openChestPlugin;
        private AutoOpenRift autoOpenRift;
        private AutoStashItems autoStashItems;
        private ISnoArea LastSnoArea { get; set; } = null;
        private Thread helperThread;
        private IFont watermarkEnabled;
        private IFont watermarkDisabled;
        private IFont debugFont;
        private HelperState helperState;
        private TownState townState;
        private GrOrRiftState grOrRiftState;
        public IKeyEvent ToggleKeyEvent { get; set; }
        protected IUiElement uiJoinPatryAcceptButton;
        protected IUiElement uiGRmainPage;
        private readonly bool[,] _usedSpaces = new bool[10, 6];
        private int minimumShardsToGamble = 100;
        public static bool shouldDisconnect = false;
        private bool isBotting = true;
        private int keysToKeep = 1000;
        private int grLevel = 110;

        private enum HelperState
        {
            InTown,
            GrOrRift,
        }

        private enum TownState
        {
            JustEntered,
            Gambling,
            Salvaging,
            NoFreeSpace, // No free space in inv with lootfilter. Need to stash items.
            AssigningParagons,
            ClosingRift,
            AutoOpeningRift,
            Idle, // Idle means if GR or ANY other activity will start - you will accept it. Should not be reached if brother is playing. If it was reached smth happend or stash is full
            
        }

        private enum GrOrRiftState
        {
            PathFinding,
            FoundPackOrBigDensity,
            FoundPylon,
            BossSpawned,
            FoundClicableObjectOrNextFloor,
            FoundLoot,
            GrOrRiftCompleted, // Make sure that after gr all gems was up if not tp back and up them. Plugin can fail and miss one upgrade
        }

        private enum VendorType
        {
            Kadala,
            Blacksmith,
            Orek
        }

        public Helper()
        {
            Enabled = true;
            if (_helper == null)
            {
                _helper = this;
            }
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (ToggleKeyEvent.Matches(keyEvent))
            {
                if (keyEvent.IsPressed)
                {
                    isHelperOn = !isHelperOn;
                }
            }
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            Hud = hud;

            isHelperOn = false;
            pixelHelper = new PixelHelper();
            helperThread = new Thread(() => pixelHelper.ShowDialog());
            helperThread.SetApartmentState(ApartmentState.STA);
            helperThread.Start();

            zoom = new Zoom("Diablo III64", 0x5AACF8);

            watermarkEnabled = Hud.Render.CreateFont(
                "tahoma",
                8,
                255,
                0,
                170,
                0,
                true,
                false,
                false
            );
            watermarkDisabled = Hud.Render.CreateFont(
                "tahoma",
                8,
                255,
                170,
                0,
                0,
                true,
                false,
                false
            );
            debugFont = Hud.Render.CreateFont(
                "tahoma",
                16,
                255,
                0,
                170,
                0,
                true,
                false,
                false
            );
            pixelHelper.SetLegendaries(Utils.GetAllSalvageableLegendaryAndSetItems(hud));
            pixelHelper.SetHelper(this);
            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.F5, false, false, false);
            autoGamble = new AutoGamble(hud);
            alwaysUseYourParagon = new AlwaysUseYourParagon(hud);
            autoSalvagePlugin = new AutoSalvage(hud);
            pickUpPlugin = new PickUpPlugin(hud);
            autoUpgrade = new AutoUpgrade(hud);
            openChestPlugin = new OpenChestPlugin(hud);
            autoOpenRift = new AutoOpenRift(hud);
            autoStashItems = new AutoStashItems(hud);
            uiJoinPatryAcceptButton = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.rift_join_party_main.LayoutRoot.Background.buttons.accept",
                null,
                null
            );
            uiGRmainPage = Hud.Render.RegisterUiElement("Root.NormalLayer.rift_dialog_mainPage", null, null);

            //var items = PixelHelperSettings.Instance.GetItemsToSave();
            //foreach ( var item in items )
            //{
            //    Hud.Debug(item.ItemName);
            //    Hud.Debug(item.IsAncient? "True" : "False");
            //    Hud.Debug(item.MinAffix);
            //}
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip)
                return;
            //debugFont.DrawText($"state: {townState}. NewGame: {ForceDisconnect.Globals.create_new_game}", 4, Hud.Window.Size.Height * 0.966f);
            debugFont.DrawText(debugText, 4, Hud.Window.Size.Height * 0.966f);
            if (isHelperOn)
                watermarkEnabled.DrawText("Pixel Helper: On. F5 To Turn Off! ", 4, Hud.Window.Size.Height * 0.966f);
            else
                watermarkDisabled.DrawText("Pixel Helper: Off. F5 To Turn On! ", 4, Hud.Window.Size.Height * 0.966f);
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            LastSnoArea = area;
            if (newGame)
            {
                if (area.IsTown && Hud.Game.CurrentAct != 1)
                {
                    townState = TownState.JustEntered;
                }
                else
                {
                    townState = TownState.Gambling;
                }
                helperState = HelperState.InTown;
            }
            if (area.IsTown)
            {
                helperState = HelperState.InTown;
                townState = TownState.JustEntered;
            }
            else
            {
                helperState = HelperState.GrOrRift;
            }
            alwaysUseYourParagon.OnNewArea(newGame, area);
            pickUpPlugin.OnNewArea(newGame, area);
        }

        public void AfterCollect()
        {
            //var obelisk = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._x1_openworld_lootrunobelisk_b).FirstOrDefault();
            //debugText = $"IsClickable: {obelisk.IsClickable}";
            //var freeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
            //if (freeSpace != 60)
            //{
            //    autoStashItems.StashItems();
            //}
            //debugText = $"Free Space: {freeSpace}. Stash: {autoStashItems.debug}";
            debugText = $"State: {townState}";
            if (!isHelperOn || !IsActive())
                return;
            switch (helperState)
            {
                case HelperState.InTown:
                    ProcessTownState();
                    break;
                case HelperState.GrOrRift:
                    ProcessOutsideTownState();
                    // TODO: Implement general rift logic
                    break;
            }
        }

        private void ProcessTownState()
        {
            switch (townState)
            {
                case TownState.JustEntered:
                    HandleJustEntered();
                    break;
                case TownState.Gambling:
                    if (DoGambling())
                    {
                        townState = TownState.Salvaging;
                    }
                    break;
                case TownState.Salvaging:
                    if (DoSalvaging())
                    {
                        if (Hud.Game.Me.Materials.BloodShard > minimumShardsToGamble)
                        {
                            townState = TownState.Gambling;
                        }
                        else
                        {
                            townState = TownState.AssigningParagons;
                        }
                    }
                    break;
                case TownState.NoFreeSpace:
                    //if (StashItems())
                    //{
                    //    townState = TownState.AssigningParagons;
                    //}
                    //else
                    //{
                    //    townState = TownState.NoFreeSpace;
                    //}
                    townState = TownState.NoFreeSpace;
                    break;
                case TownState.AssigningParagons:
                    if (DoAssigningParagons())
                    {
                        townState = TownState.ClosingRift;
                    }
                    break;
                case TownState.ClosingRift:
                    if (DoCloseRift())
                    {
                        townState = TownState.AutoOpeningRift;
                    }
                    break;
                case TownState.AutoOpeningRift:
                    if (DoAutoOpeningRift())
                    {
                        townState = TownState.Idle;
                    }
                    break;
                case TownState.Idle:
                    DoIdleStuff();
                    break;
            }
        }

        private void ProcessOutsideTownState()
        {
            if (PixelHelperSettings.Instance.AutoPickUp)
            {
                pickUpPlugin.tryToPickUp();
            }
            if (PixelHelperSettings.Instance.AutoGemUp)
            {
                autoUpgrade.TryToUpgrade();
            }
            if (PixelHelperSettings.Instance.AutoPylons)
            {
                openChestPlugin.TryToOpenStuff();
            }
        }

        private void HandleJustEntered()
        {
            if (PixelHelperSettings.Instance.AlwaysActOne)
            {
                if (Hud.Game.CurrentAct != 1)
                {
                    // Always Act One Should TP Just Waiting
                    return;
                }
            }
            townState = TownState.Gambling;
        }

        private bool DoGambling()
        {
            if (!PixelHelperSettings.Instance.AutoGamble)
            {
                return true;
            }

            var shards = Hud.Game.Me.Materials.BloodShard;
            if (shards < minimumShardsToGamble || !IsEnoughSpaceFor1x2Item())
            {
                return true;
            }

            var kadala = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._x1_randomitemnpc
            );
            if (kadala == null)
            {
                return false;
            }

            if (!autoGamble.IsGambleTabIsActive())
            {
                TryToReachNPC(kadala);
                return false;
            }
            else
            {
                autoGamble.TryToGamble();
                return !IsEnoughSpaceFor1x2Item() || shards < minimumShardsToGamble;
            }
        }

        private bool DoSalvaging()
        {
            if (!PixelHelperSettings.Instance.AutoSalvage)
            { 
                return true; 
            }
            if (!autoSalvagePlugin.GetItemsToSalvage().Any())
            {
                return true;
            }
            var blacksmith = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._pt_blacksmith_repairshortcut
            );
            if (blacksmith == null)
            {
                return false;
            }
            if (!autoSalvagePlugin.IsSalvageTabIsActive())
            {
                TryToReachNPC(blacksmith);
                return false;
            }
            else
            {
                autoSalvagePlugin.TryToSalvage(LastSnoArea);
                return !autoSalvagePlugin.GetItemsToSalvage().Any();
            }
        }

        private bool StashItems()
        {
            var freeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
            if (isBotting && freeSpace < 40)
            {
                return autoStashItems.StashItems();
            }
            else if (freeSpace < 20)
            {
                return autoStashItems.StashItems();
            }
            else
            {
                return true;
            }
        }

        private bool DoAssigningParagons()
        {
            if (PixelHelperSettings.Instance.AlwaysUsePara)
            {

                alwaysUseYourParagon.TryToUseParagons();
                if (Hud.Game.Me.ParagonPointsAvailable[0] == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool DoAutoOpeningRift()
        {
            if (PixelHelperSettings.Instance.AutoOpenRift)
            {
                var obelisk = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._x1_openworld_lootrunobelisk_b).FirstOrDefault();
                if (obelisk == null)
                {
                    return false;
                }
                if (glq.PublicClassPlugin.riftQuest(Hud) == null)
                {
                    TryToReachNPC(obelisk);
                    autoOpenRift.TryToOpenRift();
                }
                return glq.PublicClassPlugin.riftQuest(Hud) != null;
            }
            else
            {
                townState = TownState.Idle;
            }
            return true;
        }

        private bool IsEnoughSpaceFor1x2Item()
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
                            _usedSpaces[xpos, ypos] = true;
                    }
                }
            }

            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 5; y++)
                {
                    if (!_usedSpaces[x, y] && !_usedSpaces[x, y + 1])
                        return true;
                }
            }
            return false;
        }

        private void DoIdleStuff()
        {
            if (PixelHelperSettings.Instance.AlwaysUsePara)
            {
                if (Hud.Game.Me.ParagonPointsAvailable[0] != 0)
                {
                    townState = TownState.AssigningParagons;
                }
            }
            var OpenPortal = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._x1_openworld_tiered_rifts_portal).FirstOrDefault();
            if (OpenPortal != null)
            {
                if (CanClosePortal())
                {
                    townState = TownState.ClosingRift;
                }
            }
            if (PixelHelperSettings.Instance.AutoAcceptGR)
            {
                if (LayerVisible("Root.NormalLayer.rift_join_party_main.LayoutRoot"))
                {
                    Hud.Interaction.ClickUiElement(MouseButtons.Left, uiJoinPatryAcceptButton);
                }
            }
        }

        private bool DoCloseRift()
        {
            var orek = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._x1_lr_nephalem
            );
            if (orek == null)
            {
                return false;
            }
            if (CanClosePortal())
            {
                TryToReachNPC(orek);
                return false;
            }
            return true;
        }

        private bool CanClosePortal()
        {
            bool isGR = glq.PublicClassPlugin.IsGreaterRift(Hud);
            bool isNR = glq.PublicClassPlugin.IsNephalemRift(Hud);
            bool isBossIsDead = glq.PublicClassPlugin.IsGuardianDead(Hud);

            bool isAllGemsWasUp = true;
            foreach (var player in Hud.Game.Players)
            {
                var reminder = player.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Bonus, 2147483647, 0) +
                              player.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Max, 2147483647, 0) -
                              player.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Used, 2147483647, 0);
                if (reminder != 0)
                {
                    isAllGemsWasUp = false;
                    break;
                }
            }

            var orek = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._x1_lr_nephalem
            );
            if (orek == null)
            {
                return false;
            }
            if ((isGR || isNR) && isBossIsDead && isAllGemsWasUp)
            {
                return true;
            }
            return false;
        }

        private bool LayerVisible(string path)
        {
            var layer = Hud.Render.GetUiElement(path);
            return layer != null && layer.Visible;
        }

        private void TryToReachNPC(IActor actor)
        {
            if (actor.NormalizedXyDistanceToMe > 5)
            {
                Hud.Interaction.MouseMove(
                    (int)actor.ScreenCoordinate.X,
                    (int)actor.ScreenCoordinate.Y
                );
                Thread.Sleep(60);
                Hud.Interaction.MouseDown(MouseButtons.Left);
                Thread.Sleep(60);
                Hud.Interaction.MouseUp(MouseButtons.Left);
            }
            else
            {
                Hud.Interaction.TalkTownActor(actor);
            }
        }

        private bool IsActive()
        {
            bool active =
                Hud.Game.IsInGame
                && !Hud.Game.IsLoading
                && !Hud.Game.Me.IsDead
                && !Hud.Render.UiHidden
                && Hud.Window.IsForeground
                && Hud.Game.MapMode == MapMode.Minimap
                && Hud.Game.Me.AnimationState != AcdAnimationState.Dead;

            string[] blockedPaths =
            {
                "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline",
                "Root.NormalLayer.BattleNetFriendsList_main.LayoutRoot.OverlayContainer.FriendsListContent",
                "Root.NormalLayer.SkillPane_main.LayoutRoot.SkillsList",
                "Root.NormalLayer.gamemenu_dialog.gamemenu_bkgrnd",
                "Root.NormalLayer.BattleNetLeaderboard_main.LayoutRoot.OverlayContainer",
                "Root.NormalLayer.BattleNetAchievements_main.LayoutRoot.OverlayContainer",
                "Root.TopLayer.ContextMenus.PlayerContextMenu",
            };

            foreach (var path in blockedPaths)
            {
                var elem = Hud.Render.GetUiElement(path);
                if (elem?.Visible == true)
                    return false;
            }

            return active;
        }

        public void ToggleZoom(bool enabled)
        {
            if (enabled)
            {
                zoom.Attach();
                zoom.Enable(-1.5f);
            }
            else
            {
                zoom.Disable();
                zoom.Enable(0.0f);
            }
        }
    }
}
