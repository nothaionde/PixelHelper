namespace Turbo.Plugins.PixelDrama.Town
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;
    using System.Threading;

    public class AutoStashItems
    {
        private readonly IController Hud;
        private const int GridW = 10;
        private const int GridH = 7;
        public string debug = "";

        public AutoStashItems(IController hud)
        {
            Hud = hud;
        }

        public bool StashItems()
        {
            var freeSpace = Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed;
            if (freeSpace != 0)
            {
                var stash = Hud.Game.Actors.FirstOrDefault(x =>
                x.SnoActor.Sno == ActorSnoEnum._player_shared_stash);
                if (stash != null)
                {
                    var stashTab = Hud.Inventory.SelectedStashTabIndex; // -1 When not active
                    var stashPage = Hud.Inventory.SelectedStashPageIndex; // 0 When not active but can be legit
                    var stashTabAbs = stashTab + (stashPage * Hud.Inventory.MaxStashTabCountPerPage); // -1 When not active
                    var stashVisible = Hud.Inventory.StashMainUiElement.Visible;
                    if (!stashVisible)
                    {
                        //TryToReachObject(stash);
                    }
                    else
                    {
                        var tabsPerPage = Hud.Inventory.MaxStashTabCountPerPage;
                        var maxPages = 2; // Use just 10 tabs
                        var maxTabs = maxPages * tabsPerPage;

                        int debugTabPage = 7;

                        for (var abs = 0; abs < maxTabs; abs++)
                        {
                            var pageIndex = abs / tabsPerPage;
                            var tabIndex = abs % tabsPerPage;

                            var used = Hud.Inventory.GetStashTabUsedSpace(pageIndex, tabIndex);
                            var free = 70 - used;

                            if (free == 1)
                            {

                            }
                            if (abs == debugTabPage)
                            {

                                var pageUI = Hud.Inventory.GetStashPageUiElement(pageIndex);
                                var tabUI = Hud.Inventory.GetStashTabUiElement(tabIndex);
                                if (pageIndex != stashPage)
                                {
                                    Hud.Interaction.ClickUiElement(MouseButtons.Left, pageUI);
                                }
                                if (stashTab != tabIndex)
                                {
                                    Hud.Interaction.ClickUiElement(MouseButtons.Left, tabUI);
                                }
                            }
                        }
                    }
                }
                else
                {
                    return true;
                    // Should never be a problem while botting. Stash should be always visible
                    // Just continue loop. Don't know how bot can be away from stash
                }
                return false;
                //foreach (var item in Hud.Inventory.ItemsInInventory)
                //{
                //    Hud.Interaction.ClickInventoryItem(MouseButtons.Right, item);
                //}
            }
            return true;
        }

        private void TryToReachObject(IActor actor)
        {
            if (actor.NormalizedXyDistanceToMe > 5)
            {
                Hud.Interaction.DoAction(ActionKey.Move);
                Hud.Interaction.MouseMove(
                    (int)actor.ScreenCoordinate.X,
                    (int)actor.ScreenCoordinate.Y
                );
                Thread.Sleep(120);
                Hud.Interaction.MouseDown(MouseButtons.Left);
                Thread.Sleep(120);
                Hud.Interaction.MouseUp(MouseButtons.Left);
            }
            else
            {
                Hud.Interaction.TalkTownActor(actor);
            }
        }
    }
}
