using System;
using System.Drawing;
using System.Linq;
using Turbo.Plugins.Default;
//by Brodis
namespace Turbo.Plugins.glq
{

    public class GLQ_GemsInventoryCountPlugin1 : BasePlugin, IInGameTopPainter, IKeyEventHandler
    {
        private ISnoItem[,] _gems { get; set; }
        private int[] _location { get; set; }
        public RectangleF GemInvRect { get; set; }
        public RectangleF GemBackgroundRect { get; set; }
        public ITexture GemInvTexture { get; set; }
        public ITexture GemBackgroundTexture { get; set; }
        public IFont GemQuantityFont { get; set; }
        public float GemSpacing { get; set; }
        public float GemSize { get; set; }
        public bool Barring { get; private set; }

        public GLQ_GemsInventoryCountPlugin1()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            _location = new int[] { 0, 15, 19 };

            _gems = new ISnoItem[5, 5];

            uint[] gemsIdx = { 2838965543, 3446938396, 4267641563, 3256663689, 1019190639,
            2838965570, 3446938423, 4267641590, 3256663716, 1019190666};

            for (uint y = 0; y < 5; y++)
            {
                for (uint x = 0; x < 4; x++)
                {
                    _gems[y, x] = Hud.Inventory.GetSnoItem(gemsIdx[y] + x);
                }
                _gems[y, 4] = Hud.Inventory.GetSnoItem(gemsIdx[5 + y]);
            }

            GemSpacing = 5f;
            GemSize = 0.75f;

            GemInvTexture = Hud.Texture.GetItemTexture(_gems[0, 4]);
            GemBackgroundTexture = Hud.Texture.InventorySlotTexture;

            GemQuantityFont = Hud.Render.CreateFont("tahoma", 10, 255, 255, 255, 255, true, false, 128, 0, 0, 0, true);

            GemBackgroundRect = new RectangleF((Hud.Window.Size.Width - GemInvTexture.Width * GemSize * 5f) * -4.5f, (Hud.Window.Size.Height - GemInvTexture.Height * GemSize * 5f) * -2.5f,
            (GemInvTexture.Width * GemSize + GemSpacing) * 6f, (GemInvTexture.Height * GemSize + GemSpacing) * 6f);

        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory) return;
            if (Hud.Game.Me.CurrentLevelNormalCap != 70) return;
            if (!Hud.Inventory.InventoryMainUiElement.Visible) return;
            var uiInvRect = Hud.Render.GetUiElement("Root.NormalLayer.inventory_dialog_mainPage");
            GemInvRect = new RectangleF(uiInvRect.Rectangle.Left + (uiInvRect.Rectangle.Width * 0.460f), uiInvRect.Rectangle.Top + (uiInvRect.Rectangle.Height * 0.04f), GemInvTexture.Width * GemSize, GemInvTexture.Height * GemSize);
            GemInvTexture.Draw(GemInvRect.X, GemInvRect.Y, GemInvRect.Width, GemInvRect.Height);
            if (Hud.Window.CursorInsideRect(GemInvRect.X, GemInvRect.Y, GemInvRect.Width, GemInvRect.Height))
            {
                Hud.Render.SetHint("Показывает наличие самоцветов. Последняя колонка показывает количество безупречных королевских самоцветов, если обновить все самоцветы.\nПри нажатии ALT будут показаны все самоцветы, кроме тех, что уже находятся в сокете.");
                ITexture texture;
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        texture = Hud.Texture.GetItemTexture(_gems[y, x]);
                        if (texture != null)
                        {
                            GemBackgroundTexture.Draw(GemBackgroundRect.Left + ((GemInvRect.Width + GemSpacing) * x) - GemSpacing * 0.5f,
                                GemBackgroundRect.Top + ((GemInvRect.Height + GemSpacing) * y) - GemSpacing * 0.5f,
                                GemInvRect.Width + GemSpacing, GemInvRect.Height + GemSpacing);

                            texture.Draw(GemBackgroundRect.Left + ((GemInvRect.Width + GemSpacing) * x), GemBackgroundRect.Top + ((GemInvRect.Height + GemSpacing) * y),
                                GemInvRect.Width, GemInvRect.Height);

                            var layout = GemQuantityFont.GetTextLayout(ValueToString(CountGems(_gems[y, x]), ValueFormat.NormalNumberNoDecimal));
                            GemQuantityFont.DrawText(layout, GemBackgroundRect.Left + ((GemInvRect.Width + GemSpacing) * x) + (GemInvRect.Width - layout.Metrics.Width) * 0.5f,
                                GemBackgroundRect.Top + ((GemInvRect.Height + GemSpacing) * y) + layout.Metrics.Height * 0.5f);
                        }
                    }

                    texture = Hud.Texture.GetItemTexture(_gems[y, 4]);
                    if (texture != null)
                    {
                        var x = 5;
                        var total = CountGems(_gems[y, 4]) +
                            Math.Floor(
                                (Math.Floor(
                                    (Math.Floor(
                                        (Math.Floor(CountGems(_gems[y, 0]) / 3f) + CountGems(_gems[y, 1])) / 3f) + CountGems(_gems[y, 2])) / 3f) + CountGems(_gems[y, 3])) / 3f);

                        GemBackgroundTexture.Draw(GemBackgroundRect.Left + ((GemInvRect.Width + GemSpacing) * x) - GemSpacing * 0.5f,
                            GemBackgroundRect.Top + ((GemInvRect.Height + GemSpacing) * y) - GemSpacing * 0.5f,
                            GemInvRect.Width + GemSpacing, GemInvRect.Height + GemSpacing);

                        texture.Draw(GemBackgroundRect.Left + ((GemInvRect.Width + GemSpacing) * x), GemBackgroundRect.Top + ((GemInvRect.Height + GemSpacing) * y),
                            GemInvRect.Width, GemInvRect.Height);

                        var layout = GemQuantityFont.GetTextLayout("=" + total.ToString());
                        GemQuantityFont.DrawText(layout, GemBackgroundRect.Left + ((GemInvRect.Width + GemSpacing) * x) + (GemInvRect.Width - layout.Metrics.Width) * 0.5f,
                                GemBackgroundRect.Top + ((GemInvRect.Height + GemSpacing) * y) + layout.Metrics.Height * 0.5f);
                    }

                }
            }

        }

        private long CountGems(ISnoItem snoItem)
        {
            var count = 0;
            
            if (Barring)
            {
                foreach (var item in Hud.Inventory.ItemsInStash)
                {
                    if (item.SnoItem == snoItem) count += (int)item.Quantity;
                }

                foreach (var item in Hud.Inventory.ItemsInInventory)
                {
                    if (item.SnoItem == snoItem) count += (int)item.Quantity;
                }
            }
            else
            {
                var Items = Hud.Game.Items.Where(i => _location.Contains((int)i.Location));
                foreach (var item in Items)
                {
                    if (item.SnoItem == snoItem) count += item.Quantity > 0 ? (int)item.Quantity : 1;
                }
            }

            return count;
        }

        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            Barring = keyEvent.AltPressed;
        }
    }

}