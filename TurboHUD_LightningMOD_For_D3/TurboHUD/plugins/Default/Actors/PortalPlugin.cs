using System.Linq;

namespace Turbo.Plugins.Default
{
    public class PortalPlugin : BasePlugin, IInGameWorldPainter
    {
        public WorldDecoratorCollection Decorator { get; set; }

        public PortalPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            Decorator = new WorldDecoratorCollection(
                new MapTextureDecorator(Hud)
                {
                    Texture = Hud.Texture.GetTexture(448992, 0),
                    Radius = 1f,
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 200, 255, 255, 0, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 6.0f,
                }
                );
        }

        public void PaintWorld(WorldLayer layer)
        {
            var portals = Hud.Game.Portals.Where(x => x.Scene?.SnoArea?.IsTown != true);

            foreach (var portal in portals)
            {
                Decorator.Paint(layer, null, portal.FloorCoordinate, portal.TargetArea.NameLocalized ?? "unknown portal");
            }
        }
    }
}