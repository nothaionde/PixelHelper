using System.Linq;

namespace Turbo.Plugins.Default
{
    public class MarkerPlugin : BasePlugin, IInGameWorldPainter
    {
        public WorldDecoratorCollection GenericMarkerDecorator { get; set; }

        public WorldDecoratorCollection ShrineDecorator { get; set; }
        public WorldDecoratorCollection PossibleRiftPylonDecorator { get; set; }

        public WorldDecoratorCollection PylonDecorator { get; set; }
        public WorldDecoratorCollection HealingWellDecorator { get; set; }
        public WorldDecoratorCollection PoolOfReflectionDecorator { get; set; }
        public WorldDecoratorCollection UsedMarkerDecorator { get; set; }

        public int? TooFarMarkerRange { get; set; } // ground label is hidden above this distance

        public MarkerPlugin()
        {
            Enabled = true;
            TooFarMarkerRange = 1000;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            var shadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1);

            GenericMarkerDecorator = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 255, 55, -1),
                    ShadowBrush = shadowBrush,
                    Radius = 10.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapTextureDecorator(Hud)
                {
                    Radius = 1f,
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 10,
                    Up = true,
                }
                );

            UsedMarkerDecorator = new WorldDecoratorCollection(
                new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(192, 200, 200, 200, -1),
                    ShadowBrush = shadowBrush,
                    Radius = 10.0f,
                    ShapePainter = new CircleShapePainter(Hud),
                },
                new MapTextureDecorator(Hud)
                {
                    Radius = 1f,
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 200, 200, 200, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 10,
                    Up = true,
                }
                );

            ShrineDecorator = new WorldDecoratorCollection(
                new MapTextureDecorator(Hud)
                {
                    Texture = Hud.Texture.GetTexture(218235, 0),
                    Radius = 1f,
                },
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 255, 255, 64, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 12.0f,
                }
                );

            PossibleRiftPylonDecorator = new WorldDecoratorCollection(
                new MapTextureDecorator(Hud)
                {
                    Radius = 1f,
                    Texture = Hud.Texture.GetTexture(218235, 0),
                },
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", 6f, 192, 255, 255, 55, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = 5.0f,
                }
                );

            PylonDecorator = new WorldDecoratorCollection(
                new MapTextureDecorator(Hud)
                {
                    Radius = 1f,
                },
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 255, 255, 64, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
                }
                );

            HealingWellDecorator = new WorldDecoratorCollection(
                new MapTextureDecorator(Hud)
                {
                    Texture = Hud.Texture.GetTexture(218234, 0),
                    Radius = 1f,
                },
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 255, 255, 64, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
                }
                );

            PoolOfReflectionDecorator = new WorldDecoratorCollection(
                new MapTextureDecorator(Hud)
                {
                    Texture = Hud.Texture.GetTexture(376779, 0),
                    Radius = 1f,
                },
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 255, 255, 64, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
                }
                );
        }

        public void PaintWorld(WorldLayer layer)
        {
            foreach (var marker in Hud.Game.Markers)
            {
                if (Hud.Game.Shrines.Any(x => x.IsShrine && x.FloorCoordinate.XYDistanceTo(marker.FloorCoordinate) <= 2))
                    continue;

                var decorator = marker.IsUsed
                    ? UsedMarkerDecorator
                    : GenericMarkerDecorator;

                if (!marker.IsUsed)
                {
                    if (marker.IsPylon)
                        decorator = PylonDecorator;
                    else if (marker.IsPoolOfReflection)
                        decorator = PoolOfReflectionDecorator;
                    else if (marker.IsHealingWell)
                        decorator = HealingWellDecorator;
                }

                var texture = Hud.Texture.GetTexture(marker.TextureSno, marker.TextureFrameIndex);

                foreach (var deco in decorator.GetDecorators<MapTextureDecorator>())
                    deco.Texture = texture;

                foreach (var deco in decorator.GetDecorators<MapLabelDecorator>())
                {
                    deco.RadiusOffset = texture != null ? 20 : 10;
                }

                decorator.ToggleDecorators<MapShapeDecorator>(texture == null);

                var groundLabelVisible = true;
                if (TooFarMarkerRange != null && marker.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) > TooFarMarkerRange.Value)
                    groundLabelVisible = false;

                if (marker.FloorCoordinate.IsOnScreen())
                    groundLabelVisible = false;

                decorator.ToggleDecorators<GroundLabelDecorator>(groundLabelVisible); // do not display ground labels when the actor is on the screen

                var mapLabelVisible = !marker.IsHealingWell && !marker.IsShrine && !marker.IsPoolOfReflection && !marker.IsPylon;
                decorator.ToggleDecorators<MapLabelDecorator>(mapLabelVisible);

                decorator.Paint(layer, null, marker.FloorCoordinate, marker.Name);
            }

            var shrines = Hud.Game.Shrines.Where(x => !x.IsDisabled && !x.IsOperated && x.IsShrine);
            foreach (var actor in shrines)
            {
                ShrineDecorator.ToggleDecorators<GroundLabelDecorator>(!actor.IsOnScreen); // do not display ground labels when the actor is on the screen
                ShrineDecorator.Paint(layer, actor, actor.FloorCoordinate, actor.SnoActor.NameLocalized);
            }

            if (Hud.Game.SpecialArea == SpecialArea.GreaterRift)
            {
                var riftPylonSpawnPoints = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._markerlocation_tieredriftpylon);
                foreach (var actor in riftPylonSpawnPoints)
                {
                    PossibleRiftPylonDecorator.Paint(layer, actor, actor.FloorCoordinate, "pylon?");
                }
            }
        }
    }
}