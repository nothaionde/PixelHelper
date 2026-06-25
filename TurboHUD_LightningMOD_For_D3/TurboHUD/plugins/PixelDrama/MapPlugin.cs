namespace Turbo.Plugins.PixelDrama
{
    using System;
    using System.Linq;
    using Turbo.Plugins.Default;
    using System.Drawing;

    public class MapPlugin
    {
        private readonly IController Hud;

        public MapPlugin(IController hud)
        {
            Hud = hud;
        }

        public void PaintTopLayer()
        {
        }
    }
}