// TriunesWillPlugin
// Ground Decorators & Timers for Triune's Will circles from Season 18 buff.
// Additional Marker when you are in a circle (Thanks to JarJar & RNN !).

using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.User
{
    public class TriunesWillPlugin : BasePlugin, IInGameWorldPainter
    {
	public WorldDecoratorCollection LoveDecorator { get; set; }
	public WorldDecoratorCollection DeterminationDecorator { get; set; }
	public WorldDecoratorCollection CreationDecorator { get; set; }

	public WorldDecoratorCollection LoveInsideDecorator { get; set; }
	public WorldDecoratorCollection DeterminationInsideDecorator { get; set; }
	public WorldDecoratorCollection CreationInsideDecorator { get; set; }

	public float CircleRadius { get; set; }
	public float InsideRadius { get; set; }

	public bool EnableLove { get; set; }
	public bool EnableDetermination { get; set; }
	public bool EnableCreation { get; set; }
	public bool EnableInsideMarker { get; set; }

	public TriunesWillPlugin()
        {
	    Enabled = true;

	    EnableLove = true;		// Damage Circle
	    EnableCreation = true;	// CDR Circle
	    EnableDetermination = true;	// RCR Circle

	    EnableInsideMarker = true;	// Additional Circle Decorator when you are in it
        }

	public override void Load(IController hud)
        {
	    base.Load(hud);

	    CircleRadius = 10.0f; // Change the radius of the circle decorator
	    InsideRadius = 10.0f; // Change the radius of the additional "inside marker"

	    LoveInsideDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 150, 0, 0, -2, SharpDX.Direct2D1.DashStyle.Dash), // 220, 0, 64
                    Radius = InsideRadius,
                }
                );

	    CreationInsideDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 0, 80, 150, -2, SharpDX.Direct2D1.DashStyle.Dash),
                    Radius = InsideRadius,
                }
                );

	    DeterminationInsideDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 120, 80, 0, -2, SharpDX.Direct2D1.DashStyle.Dash), // Dark Blue 0, 64, 255 - Gold 164, 100, 32
                    Radius = InsideRadius,
                }
                );

	    LoveDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 255, 0, 128, -2),
                    Radius = CircleRadius,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 255, 96, 255, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(164, 192, 0, 0, 0),
                    Radius = 30,
                }
                );

	    CreationDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 64, 200, 144, -2),
                    Radius = CircleRadius,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 96, 230, 196, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(164, 0, 192, 192, 0),
                    Radius = 30,
                }
                );

	    DeterminationDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 164, 164, 32, -2), // Dark Blue 0, 64, 255
                    Radius = CircleRadius,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 200, 200, 96, true, false, 128, 0, 0, 0, true), // Dark Blue 96, 96, 255
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(164, 164, 164, 0, 0), // Dark Blue 0, 0, 192
                    Radius = 30,
                }
                );

        }

	public void PaintWorld(WorldLayer layer)
        {
	    if (Hud.Game.IsInTown) return;

	    bool Inside = false;
			
	if (EnableLove) {
	    var love = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 488071) == 1).OrderBy(d => d.CentralXyDistanceToMe);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(488071, 2)) { Inside = true; }
	    foreach (var actor in love)
	    {
		LoveDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { LoveInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
	    }
	}

	if (EnableCreation) {
            var creation = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_7_Visual_Effect_None, 488071) == 1).OrderBy(d => d.CentralXyDistanceToMe);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(488071, 8)) { Inside = true; }
            foreach (var actor in creation)
            {
		CreationDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { CreationInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
            }
	}

	if (EnableDetermination) {
            var determination = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_6_Visual_Effect_None, 488071) == 1).OrderBy(d => d.CentralXyDistanceToMe);
	    if (EnableInsideMarker && Hud.Game.Me.Powers.BuffIsActive(488071, 5)) { Inside = true; }
            foreach (var actor in determination)
            {
		DeterminationDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
		if (Inside) { DeterminationInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null); Inside = false; }
            }
	}


        }
    }
}