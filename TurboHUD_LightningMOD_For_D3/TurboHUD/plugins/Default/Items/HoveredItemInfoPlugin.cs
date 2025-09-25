using System.Globalization;

namespace Turbo.Plugins.Default
{
    public class HoveredItemInfoPlugin : BasePlugin, IInGameTopPainter
    {
        public IFont ItemLevelFont { get; set; }
        public TopLabelDecorator LegendaryNameDecorator { get; set; }
        public TopLabelDecorator SetNameDecorator { get; set; }
        public bool CanCubedEnabled { get; set; }
        private IWatch _watch;

        public HoveredItemInfoPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            ItemLevelFont = Hud.Render.CreateFont("tahoma", 7, 255, 154, 105, 24, true, false, 128, 0, 0, 0, true);

            LegendaryNameDecorator = new TopLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 10, 235, 235, 110, 0, false, false, true),
                TextFunc = () => Hud.Inventory.HoveredItem.FullNameLocalized,
            };

            SetNameDecorator = new TopLabelDecorator(Hud)
            {
                BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                BorderBrush = Hud.Render.CreateBrush(255, 0, 0, 0, -1),
                TextFont = Hud.Render.CreateFont("tahoma", 10, 255, 50, 220, 50, false, false, true),
                TextFunc = () => Hud.Inventory.HoveredItem.FullNameLocalized,
            };

            _watch = Hud.Time.CreateAndStartWatch();
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.AfterClip)
                return;

            var item = Hud.Inventory.HoveredItem;
            if (item == null)
                return;

            var uicMain = Hud.Inventory.GetHoveredItemMainUiElement();
            var uicTop = Hud.Inventory.GetHoveredItemTopUiElement();

            if (item.Unidentified)
            {
                var decorator = (item.SetSno != uint.MaxValue) ? SetNameDecorator : LegendaryNameDecorator;

                decorator.Paint(uicTop.Rectangle.Left, uicTop.Rectangle.Top, uicTop.Rectangle.Width, uicTop.Rectangle.Height * 0.7f, HorizontalAlign.Center);
            }

            var iLevelText = "i" + item.SnoItem.Level.ToString("D", CultureInfo.InvariantCulture);
            var iLevelLayout = ItemLevelFont.GetTextLayout(iLevelText);
            ItemLevelFont.DrawText(iLevelLayout, uicTop.Rectangle.Left - (Hud.Window.Size.Height * 0.0166f), uicTop.Rectangle.Top + (((Hud.Window.Size.Height * 0.022f) - iLevelLayout.Metrics.Height) / 2));
        }
    }
}