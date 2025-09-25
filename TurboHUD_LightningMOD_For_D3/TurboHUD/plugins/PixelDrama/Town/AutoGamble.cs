namespace Turbo.Plugins.PixelDrama.Town
{
    using System;
    using System.Text;
    using System.Windows.Forms;
    using Turbo.Plugins.Default;

    public class AutoGamble
    {
        private readonly IController Hud;
        private readonly IUiElement KadalaDialog;
        private readonly IUiElement ShopMainPage;
        private readonly IUiElement ShopGoldText;
        private readonly IUiElement[] UiETabs;
        private readonly IUiElement[] UiEItemsRegion;
        private bool needToMoveMouse = false;

        private long msLapseAction { get; set; } = 0;
        private const long msLapseMin = 50;

        public AutoGamble(IController hud)
        {
            Hud = hud;

            KadalaDialog = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.shop_dialog_mainPage.panel",
                null,
                null
            );
            ShopMainPage = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.shop_dialog_mainPage",
                null,
                null
            );
            ShopGoldText = Hud.Render.RegisterUiElement(
                "Root.NormalLayer.shop_dialog_mainPage.gold_text",
                ShopMainPage,
                null
            );

            UiEItemsRegion = new IUiElement[]
            {
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 0 0",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 1 0",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 0 1",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 1 1",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 0 2",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 1 2",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 0 3",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 1 3",
                    ShopMainPage,
                    null
                ),
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.shop_item_region.item 0 4",
                    ShopMainPage,
                    null
                ),
            };

            UiETabs = new IUiElement[]
            {
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.tab_0",
                    ShopMainPage,
                    null
                ), // AnimState 48-49-50 (over/active/out)
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.tab_1",
                    ShopMainPage,
                    null
                ), // AnimState 39-40-41 (over/active/out)
                Hud.Render.RegisterUiElement(
                    "Root.NormalLayer.shop_dialog_mainPage.tab_2",
                    ShopMainPage,
                    null
                ), // AnimState 45-46-47 (over/active/out)
            };
        }
        public bool IsGambleTabIsActive() 
        {
            return Hud.Game?.IsInTown == true
                && Hud.Inventory.InventoryMainUiElement.Visible
                && ShopMainPage.Visible
                && ShopGoldText.Visible
                && ShopGoldText.ReadText(Encoding.UTF8, true).EndsWith("{icon:x1_shard}");
        }
        public void TryToGamble()
        {
            if (
                Hud.Game?.IsInTown == true
                && Hud.Inventory.InventoryMainUiElement.Visible
                && ShopMainPage.Visible
                && ShopGoldText.Visible
                && ShopGoldText.ReadText(Encoding.UTF8, true).EndsWith("{icon:x1_shard}")
            )
            {
                if (Hud.Game.CurrentRealTimeMilliseconds > msLapseAction)
                {
                    var item = KadalaItemType.GetByName(
                        PixelHelperSettings.Instance.AutoGambleItem
                    );
                    var itemActive = KadalaItemType
                        .GetByName(PixelHelperSettings.Instance.AutoGambleItem)
                        .Index;
                    if (
                        itemActive > 0
                        && itemActive < 18
                        && Hud.Game.Me.Materials.BloodShard >= item.ShardCost
                    )
                    {
                        if (UiETabs[item.UiTabIndex].Visible)
                        {
                            var tempX = Hud.Window.CursorX;
                            var tempY = Hud.Window.CursorY;
                            if (UiETabs[item.UiTabIndex].AnimState == item.RequiredTabAnimState)
                            {
                                needToMoveMouse = true;
                                int numShopShards =
                                    (int)Hud.Game.Me.Materials.BloodShard / item.ShardCost;
                                int numShopInv =
                                    (Hud.Game.Me.InventorySpaceTotal - Hud.Game.InventorySpaceUsed)
                                    / item.InventorySlots;
                                var numShop = Math.Min(numShopShards, numShopInv);
                                if (numShop++ > 0)
                                {
                                    while (numShop-- > 0)
                                    {
                                        var itemRegionIndex = UiEItemsRegion[item.UiRegionIndex];
                                        var x = (int)(
                                            itemRegionIndex.Rectangle.X
                                            + itemRegionIndex.Rectangle.Width * 0.5f
                                        );
                                        var y = (int)(
                                            itemRegionIndex.Rectangle.Y
                                            + itemRegionIndex.Rectangle.Height * 0.5f
                                        );
                                        Hud.Interaction.MouseMove(x, y, 1, 1);
                                        Hud.Interaction.MouseDown(MouseButtons.Right);
                                        Hud.Interaction.MouseUp(MouseButtons.Right);
                                    }
                                }
                            }
                            else
                            {
                                var tabPosition = UiETabs[item.UiTabIndex];
                                var x = (int)(
                                    tabPosition.Rectangle.X + tabPosition.Rectangle.Width * 0.5f
                                );
                                var y = (int)(
                                    tabPosition.Rectangle.Y + tabPosition.Rectangle.Height * 0.5f
                                );
                                Hud.Interaction.MouseMove(x, y, 1, 1);
                                needToMoveMouse = true;
                                Hud.Interaction.MouseDown(MouseButtons.Left);
                                Hud.Interaction.MouseUp(MouseButtons.Left);
                                msLapseAction = Hud.Game.CurrentRealTimeMilliseconds + msLapseMin;
                            }
                            if (needToMoveMouse)
                            {
                                Hud.Interaction.MouseMove(tempX, tempY, 1, 1);
                                needToMoveMouse = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
