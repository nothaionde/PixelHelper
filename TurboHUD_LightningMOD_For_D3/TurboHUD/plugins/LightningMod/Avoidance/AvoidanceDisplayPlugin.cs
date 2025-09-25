namespace Turbo.Plugins.LightningMod
{
    using Turbo.Plugins.Default;

    public class AvoidanceDisplayPlugin : BasePlugin, IInGameTopPainter
    {
        public TopLabelDecorator Decorator { get; set; }

        public AvoidanceDisplayPlugin()
        {
            Enabled = false;
            Order = int.MaxValue;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            Decorator = new TopLabelDecorator(Hud)
            {
                BorderBrush = Hud.Render.CreateBrush(255, 180, 47, 9, -1),
                BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 0, 0),
                TextFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 110, 50, true, false, false),
                TextFunc = () => "DANGER",
            };
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.AfterClip)
                return;

            var value = Hud.Avoidance.CurrentValue;
            if (!value)
                return;

            var w = Hud.Window.Size.Height * 0.20f;
            var h = Hud.Window.Size.Height * 0.035f;
            Decorator.Paint((Hud.Window.Size.Width * 0.5f) - (w / 2), Hud.Window.Size.Height * 0.001f, w, h, HorizontalAlign.Center);
        }
    }
}