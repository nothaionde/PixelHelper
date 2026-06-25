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
        public string debugText = "";
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
        public IKeyEvent ToggleKeyEvent { get; set; }
        private readonly bool[,] _usedSpaces = new bool[10, 6];
        private int minimumShardsToGamble = 50;
        private bool townInitialized = false;

        private enum HelperState
        {
            InTown,
            GrOrRift,
        }

        private enum TownState
        {
            JustEntered,
            Teleporting,
            Gambling,
            Salvaging,
            NoFreeSpace,
            AssigningParagons,
            ClosingRift,
            AutoOpeningRift,
            Idle,
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

            watermarkEnabled = Hud.Render.CreateFont("tahoma", 8, 255, 0, 170, 0, true, false, false);
            watermarkDisabled = Hud.Render.CreateFont("tahoma", 8, 255, 170, 0, 0, true, false, false);
            debugFont = Hud.Render.CreateFont("tahoma", 16, 255, 0, 170, 0, true, false, false);

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
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip)
                return;

            debugFont.DrawText(debugText, 4, Hud.Window.Size.Height * 0.166f);

            if (isHelperOn)
                watermarkEnabled.DrawText("Pixel Helper: On. F5 To Turn Off! ", 4, Hud.Window.Size.Height * 0.966f);
            else
                watermarkDisabled.DrawText("Pixel Helper: Off. F5 To Turn On! ", 4, Hud.Window.Size.Height * 0.966f);
        }

        public void OnNewArea(bool newGame, ISnoArea area)
        {
            LastSnoArea = area;

            if (area.IsTown)
            {
                helperState = HelperState.InTown;
                townState = TownState.Idle;
                townInitialized = false;
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
            //debugText = $"State: {townState}";

            if (!isHelperOn || !IsActive())
                return;

            switch (helperState)
            {
                case HelperState.InTown:
                    ProcessTownState();
                    break;

                case HelperState.GrOrRift:
                    ProcessOutsideTownState();
                    break;
            }
        }

        private void ProcessTownState()
        {
            if (ExecuteState())
                return;

            townState = SelectNextState();
        }

        private bool ExecuteState()
        {
            switch (townState)
            {
                case TownState.Gambling:
                    return DoGambling();
                case TownState.Salvaging:
                    return DoSalvaging();
                case TownState.ClosingRift:
                    return DoCloseRift();
                case TownState.AssigningParagons:
                    return DoAssigningParagons();
                case TownState.AutoOpeningRift:
                    return DoAutoOpeningRift();
                case TownState.Teleporting:
                    return Hud.Game.CurrentAct != 1;
                case TownState.Idle:
                    return false;
            }
            return false;
        }

        private TownState SelectNextState()
        {
            // --- Act logic ---
            if (PixelHelperSettings.Instance.AlwaysActOne)
            {
                if (Hud.Game.CurrentAct != 1)
                    return TownState.Teleporting;
            }
            else if (Hud.Game.CurrentAct != 1)
                return TownState.Idle;

            // --- ONE-TIME TOWN LOGIC ---
            if (!townInitialized)
            {
                if (NeedsSalvage())
                    return TownState.Salvaging;

                if (CanGamble())
                    return TownState.Gambling;

                townInitialized = true;
            }

            // --- REACTIVE LOGIC ---
            if (CanClosePortal())
            {;
                return TownState.ClosingRift;
            }

            if (NeedsParagons())
            {
                return TownState.AssigningParagons;
            }

            if (CanOpenRift())
            {
                return TownState.AutoOpeningRift;
            }

            return TownState.Idle;
        }

        private void ProcessOutsideTownState()
        {
            if (PixelHelperSettings.Instance.AutoPickUp)
                pickUpPlugin.tryToPickUp();

            if (PixelHelperSettings.Instance.AutoGemUp)
                autoUpgrade.TryToUpgrade();

            if (PixelHelperSettings.Instance.AutoPylons)
                openChestPlugin.TryToOpenStuff();
        }

        private bool DoGambling()
        {
            var kadala = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._x1_randomitemnpc);

            if (kadala == null)
                return false;

            if (!autoGamble.IsGambleTabIsActive())
            {
                TryToReachNPC(kadala);
                return false;
            }

            autoGamble.TryToGamble();
            return CanGamble();
        }

        private bool CanGamble()
        {
            return PixelHelperSettings.Instance.AutoGamble &&
                   Hud.Game.Me.Materials.BloodShard >= minimumShardsToGamble &&
                   IsEnoughSpaceFor1x2Item();
        }

        private bool NeedsSalvage()
        {
            return PixelHelperSettings.Instance.AutoSalvage &&
                   autoSalvagePlugin.GetItemsToSalvage().Any() &&
                   (!IsEnoughSpaceFor1x2Item() ||
                    Hud.Game.Me.Materials.BloodShard < minimumShardsToGamble);
        }

        private bool NeedsParagons()
        {
            return PixelHelperSettings.Instance.AlwaysUsePara &&
                   Hud.Game.Me.ParagonPointsAvailable[0] > 0;
        }

        private bool CanOpenRift()
        {
            autoOpenRift.TryToAcceptGr();
            var orek = Hud.Game.Actors.FirstOrDefault(x => x.SnoActor.Sno == ActorSnoEnum._x1_lr_nephalem);
            if (orek == null || !PixelHelperSettings.Instance.AutoOpenRift)
            {
                return false;
            }
            var quest = glq.PublicClassPlugin.riftQuest(Hud);
            if (quest == null)
            {
                return true;
            }
            bool iconDone = orek.GetAttributeValueAsInt(Hud.Sno.Attributes.Conversation_Icon, 0, -1) == 3;
            bool noRift = !glq.PublicClassPlugin.IsGreaterRift(Hud)
                       && !glq.PublicClassPlugin.IsNephalemRift(Hud);

            return iconDone && noRift;
        }

        private bool DoSalvaging()
        {
            var items = autoSalvagePlugin.GetItemsToSalvage();

            if (!items.Any())
                return false;

            var blacksmith = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._pt_blacksmith_repairshortcut);

            if (blacksmith == null)
                return true;

            if (!autoSalvagePlugin.IsSalvageTabIsActive())
            {
                TryToReachNPC(blacksmith);
                return true;
            }

            autoSalvagePlugin.TryToSalvage(LastSnoArea);

            return true;
        }

        private bool DoAssigningParagons()
        {
            if (!PixelHelperSettings.Instance.AlwaysUsePara)
            {
                return false;
            }

            alwaysUseYourParagon.TryToUseParagons();
            return Hud.Game.Me.ParagonPointsAvailable[0] != 0;
        }

        private bool DoAutoOpeningRift()
        {
            var obelisk = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._x1_openworld_lootrunobelisk_b).FirstOrDefault();

            if (obelisk == null)
                return false;

            TryToReachNPC(obelisk);
            autoOpenRift.TryToOpenRift();
            return true;
        }

        private bool IsEnoughSpaceFor1x2Item()
        {
            Array.Clear(_usedSpaces, 0, _usedSpaces.Length);

            foreach (var item in Hud.Inventory.ItemsInInventory)
            {
                for (var x = 0; x < item.SnoItem.ItemWidth; x++)
                    for (var y = 0; y < item.SnoItem.ItemHeight; y++)
                    {
                        var xpos = item.InventoryX + x;
                        var ypos = item.InventoryY + y;

                        if (xpos >= 0 && xpos < 10 && ypos >= 0 && ypos < 6)
                            _usedSpaces[xpos, ypos] = true;
                    }
            }

            for (var x = 0; x < 10; x++)
                for (var y = 0; y < 5; y++)
                    if (!_usedSpaces[x, y] && !_usedSpaces[x, y + 1])
                        return true;

            return false;
        }

        private bool DoCloseRift()
        {
            var orek = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._x1_lr_nephalem);

            if (orek == null)
                return false;

            if (orek.GetAttributeValueAsInt(Hud.Sno.Attributes.Conversation_Icon, 0, -1) == 1)
            {
                TryToReachNPC(orek);
                return true;
            }
            return false;

        }

        private bool CanClosePortal()
        {

            var orek = Hud.Game.Actors.FirstOrDefault(x => x.SnoActor.Sno == ActorSnoEnum._x1_lr_nephalem);

            if (orek == null)
                return false;

            return orek.GetAttributeValueAsInt(Hud.Sno.Attributes.Conversation_Icon, 0, -1) == 1;
        }

        private bool LayerVisible(string path)
        {
            var layer = Hud.Render.GetUiElement(path);
            return layer != null && layer.Visible;
        }

        private void TryToReachNPC(IActor actor)
        {
            if (actor == null)
                return;

            if (actor.NormalizedXyDistanceToMe < 5)
            {
                Hud.Interaction.TalkTownActor(actor);
            }
            else
            {
                Hud.Interaction.MouseMove((int)actor.ScreenCoordinate.X, (int)actor.ScreenCoordinate.Y);
                Hud.Interaction.DoActionAutoShift(ActionKey.Move);
                Thread.Sleep(60);
            }
        }

        private bool IsActive()
        {
            bool active =
                Hud.Game.IsInGame &&
                !Hud.Game.IsLoading &&
                !Hud.Game.Me.IsDead &&
                !Hud.Render.UiHidden &&
                Hud.Window.IsForeground &&
                Hud.Game.MapMode == MapMode.Minimap &&
                Hud.Game.Me.AnimationState != AcdAnimationState.Dead;

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
