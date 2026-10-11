using Turbo.Plugins.Default;
using System.Linq;

namespace Turbo.Plugins.RNN
{
    public class Oculus_Inside : BasePlugin, ICustomizer, IInGameWorldPainter
    {
		public WorldDecoratorCollection OculusDecorator { get; set; }
		public WorldDecoratorCollection OculusInsideDecorator { get; set; }

        private float _CircleRadius { get; set; } = 10f; 

		public float CircleRadius 
        {
            get { return _CircleRadius; }  
            set
            {
                if (value == _CircleRadius || value < 10f)   // We will not do anything if the user assigns an invalid value, or if this value matches the current one
                    return;
 
                _CircleRadius = value;      // We update the value
                InitDecorators();     // Redefine Decorators
            }
        }		
		
		public Oculus_Inside()
		{
			Enabled = true;			
		}
		
		public override void Load(IController hud)
		{
			base.Load(hud);

			CircleRadius = 10f;
		}

        public void Customize()
        {
			Hud.TogglePlugin<OculusPlugin>(false);
			InitDecorators();			
		}
		
        private void InitDecorators()
        {
			OculusDecorator = new WorldDecoratorCollection(
                new GroundCircleDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(255, 128, 255, 0, -2),
                    Radius = _CircleRadius,
                },
                new GroundLabelDecorator(Hud)
                {
                    CountDownFrom = 7,
                    TextFont = Hud.Render.CreateFont("tahoma", 11, 255, 96, 255, 96, true, false, 128, 0, 0, 0, true),
                },
                new GroundTimerDecorator(Hud)
                {
                    CountDownFrom = 7,
                    BackgroundBrushEmpty = Hud.Render.CreateBrush(128, 0, 0, 0, 0),
                    BackgroundBrushFill = Hud.Render.CreateBrush(200, 0, 192, 0, 0),
                    Radius = 20,
                },
				new MapShapeDecorator(Hud)
				{
					ShapePainter = new CircleShapePainter(Hud),
					Brush = Hud.Render.CreateBrush(255, 128, 255, 0, -1f),
					ShadowBrush = Hud.Render.CreateBrush(35, 255, 255, 255, 1),			
					Radius = _CircleRadius,
				}
			);
			
			OculusInsideDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(255, 0, 140, 0, -2f, SharpDX.Direct2D1.DashStyle.Dash),
					Radius = _CircleRadius,
				},
				new MapShapeDecorator(Hud)
				{
					ShapePainter = new CircleShapePainter(Hud),
					Brush = Hud.Render.CreateBrush(255, 0, 140, 0, -1f, SharpDX.Direct2D1.DashStyle.Dash),
					Radius = _CircleRadius,
				}
			);			
		}	


		
		public void PaintWorld(WorldLayer layer)
		{
			if (Hud.Game.IsInTown) return;
				
			var actors = Hud.Game.Actors.Where(x => x.SnoActor.Sno == ActorSnoEnum._generic_proxy && x.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, 402461) == 1);
			if (actors.Any())
			{
				foreach (var actor in actors)
				{
					OculusDecorator.Paint(layer, actor, actor.FloorCoordinate, null);
				}
				
				if (Hud.Game.Me.Powers.BuffIsActive(402461, 2))
				{
					var actor = actors.OrderBy(d => d.FloorCoordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate)).FirstOrDefault(); // Al menos hay uno, no hace falta comprobar si es != null
					OculusInsideDecorator.Paint(layer, actor, actor.FloorCoordinate, null);	
				}									
			}
		}
	}
}