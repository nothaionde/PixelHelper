using System.Linq;
using Turbo.Plugins.Default;
using System.Collections.Generic;

namespace Turbo.Plugins.RNN
{
	public class GlobesPlugin1 : BasePlugin, IInGameWorldPainter, ICustomizer, IInGameTopPainter
	{
		private WorldDecoratorCollection HealthGlobeDecorator { get; set; }
		private WorldDecoratorCollection PowerGlobeDecorator { get; set; }
		private WorldDecoratorCollection NRiftOrbDecorator { get; set; }
		private WorldDecoratorCollection GRiftOrbDecorator { get; set; }
		private IBrush BrushNR { get; set; }			
		private IBrush BrushGR { get; set; }
		private IBrush BrushFillCR { get; set; }
		private IBrush BrushBorderCR { get; set; }
		private IBrush BrushCRBorder { get; set; }
		private IBrush BrushHealth { get; set; }
		private IBrush BrushPower { get; set; }
		private IFont Texto { get; set; }		
		private float Width1  { get; set; }
		private float Width2  { get; set; }
		public bool MiniGlobes { get; set; }
		public bool CirclesHealth { get; set; }
		public bool CirclesRift { get; set; }
		public bool CirclesPower { get; set; }
		public bool LabelHealth { get; set; }
		public bool LabelRift { get; set; }
		public bool LabelPower { get; set; }
		public bool MapHealth { get; set; }
		public bool MapRift { get; set; }
		public bool MapPower { get; set; }
		public bool SameRiftMapGlobes = true;		
		public string TextHealth { get; set; }
		public string TextRift { get; set; }
		public string TextPower { get; set; }
		public float SizeFontMiniGlobes { get; set; }
		public float CirclesStrokeWidth { get; set; }
		
		public GlobesPlugin1()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 30001;
			
			MiniGlobes = true;			// Show Mini globes on minimap corner
			SizeFontMiniGlobes = 7.0f;	// Size for Miniglobes
			
			CirclesHealth = false;		// Circle ground
			CirclesRift = true;
			CirclesPower = true;
			CirclesStrokeWidth = 3f;	// stroke width of circles (0f for a filled circle)
			
			LabelHealth = false;		// Label ground (!IsOnScreen)
			LabelRift = true;
			LabelPower = true;
			
			MapHealth = true;			// Show Health Globes on the Minimap
			MapRift = true;				// Show Rift (NR/GR) Globes on the Minimap
			MapPower = true;			// Show Power Globes on the Minimap
			
			SameRiftMapGlobes = false;
			
			TextHealth = "health globe";	// "health globe"
			TextRift = "rift globe";		// "rift globe"
			TextPower = "power globe";		// "power globe"						
			
			BrushHealth = Hud.Render.CreateBrush(255, 255, 50 , 50, 0);		
			BrushPower = Hud.Render.CreateBrush(255, 240, 240 , 120, 0);
			BrushNR = Hud.Render.CreateBrush(255, 250 , 128, 0, 0);
			BrushGR = Hud.Render.CreateBrush(255, 150 , 50 , 250, 0);
			BrushFillCR = Hud.Render.CreateBrush(255, 35, 200, 200, 0);
			BrushBorderCR = Hud.Render.CreateBrush(255, 35, 200, 200, 3);			
		}

        public void Customize()
        {
			Hud.TogglePlugin<GlobePlugin>(false);
			CreateDecorators();
			Texto = Hud.Render.CreateFont("tahoma", SizeFontMiniGlobes, 255,255,255,255, true, false, 128, 0, 0, 0, true);
			Width1 = Texto.GetTextLayout("100").Metrics.Width; // Размер сферы на миникарте
			Width2 = 1 + Texto.GetTextLayout("88").Metrics.Width / 2;
		}
		
        public void CreateDecorators() 
		{
			HealthGlobeDecorator = new WorldDecoratorCollection();			
			PowerGlobeDecorator = new WorldDecoratorCollection();
			GRiftOrbDecorator = new WorldDecoratorCollection();
			NRiftOrbDecorator = new WorldDecoratorCollection();

			if (MapHealth)
			{
				HealthGlobeDecorator.Decorators.Add(
					new MapShapeDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
						ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
						Radius = 5.0f,
						ShapePainter = new CircleShapePainter(Hud),
						RadiusTransformator = new StandardPingRadiusTransformator(Hud, 650)  {	RadiusMinimumMultiplier = 0.85f  }
					}									
				);
			}
			
			if (LabelHealth)
			{
				HealthGlobeDecorator.Decorators.Add(
					new GroundLabelDecorator(Hud)
					{
						BackgroundBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 0),
						TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
					}
				);					
			}
			
			if (CirclesHealth)	
			{
				HealthGlobeDecorator.Decorators.Add(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 255, 0, 0, CirclesStrokeWidth),
						Radius = 1f,
					}				
				);
			}
			
			if (MapPower)
			{
				PowerGlobeDecorator.Decorators.Add(	
					new MapShapeDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 240, 240, 120, 0),
						ShadowBrush = Hud.Render.CreateBrush(200, 0, 0, 0, 1),
						Radius = 6.0f,
						ShapePainter = new CircleShapePainter(Hud),
					}				
				);
			}
			
			if (LabelPower)
			{
				PowerGlobeDecorator.Decorators.Add(				
					new GroundLabelDecorator(Hud)
					{
						BackgroundBrush = Hud.Render.CreateBrush(255, 240, 240, 120, 0),
						TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
					}
				);				
			}
			
			if (CirclesPower)	
			{
				PowerGlobeDecorator.Decorators.Add(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 240, 240, 120, CirclesStrokeWidth),
						Radius = 1f,
					}				
				);
			}

			if (MapRift)
			{
				NRiftOrbDecorator.Decorators.Add(
					new MapShapeDecorator(Hud)
					{
						Brush = (SameRiftMapGlobes)?Hud.Render.CreateBrush(255, 240, 120, 240, 0):Hud.Render.CreateBrush(255, 250, 128, 0, 0),
						ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
						Radius = 7.0f,
						ShapePainter = new CircleShapePainter(Hud),
						RadiusTransformator = new StandardPingRadiusTransformator(Hud, 650)  {	RadiusMinimumMultiplier = 0.80f  }
					}				
				);
				
				GRiftOrbDecorator.Decorators.Add(
					new MapShapeDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 240, 120, 240, 0),
						ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
						Radius = 7.0f,
						ShapePainter = new CircleShapePainter(Hud),
						RadiusTransformator = new StandardPingRadiusTransformator(Hud, 650)  {	RadiusMinimumMultiplier = 0.80f  }
					}				
				);				
			}
			
			if (LabelRift)
			{
				NRiftOrbDecorator.Decorators.Add(
					new GroundLabelDecorator(Hud)
					{
						BackgroundBrush = Hud.Render.CreateBrush(255, 250, 128, 0, 0),
						TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
					}				
				);
				GRiftOrbDecorator.Decorators.Add(
					new GroundLabelDecorator(Hud)
					{
						BackgroundBrush = Hud.Render.CreateBrush(255, 240, 120, 240, 0),
						TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
					}								
				);				
			}
			
			if (CirclesRift)	
			{
				NRiftOrbDecorator.Decorators.Add(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 250, 128, 0, CirclesStrokeWidth),
						Radius = 1f,
					}				
				);
				GRiftOrbDecorator.Decorators.Add(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(255, 240, 120, 240, CirclesStrokeWidth),
						Radius = 1f,
					}				
				);					
			}				
		}
		
		public void DrawGlobe(ref float x, float y, int count, IBrush brush, int ext = 0)
		{
			var layout = Texto.GetTextLayout(count.ToString());	
			var w =  ((count < 10)? Width1:Width2) + (float) ext;
			x -= w; y -= w ;
			brush.DrawEllipse(x, y, w, w);
			Texto.DrawText(layout, x - layout.Metrics.Width/2, y - layout.Metrics.Height/2);
			x -= w + 2;
		}
		
		public void PaintWorld(WorldLayer layer)
		{					
			var actors = Hud.Game.Actors.Where(a => a.SnoActor.Kind == ActorKind.HealthGlobe || a.SnoActor.Kind == ActorKind.RiftOrb || a.SnoActor.Kind == ActorKind.PowerGlobe).OrderBy(n => n.SnoActor.Kind);
			foreach (var actor in actors)
			{
				if (actor.SnoActor.Kind == ActorKind.HealthGlobe)
				{
					HealthGlobeDecorator.ToggleDecorators<GroundLabelDecorator>(!actor.IsOnScreen);
					HealthGlobeDecorator.Paint(layer, actor, actor.FloorCoordinate, TextHealth);
				}
				else if (actor.SnoActor.Kind == ActorKind.PowerGlobe) 
				{
					PowerGlobeDecorator.ToggleDecorators<GroundLabelDecorator>(!actor.IsOnScreen);
					PowerGlobeDecorator.Paint(layer, actor, actor.FloorCoordinate, TextPower);	
				}
				else 
				{
					if (Hud.Game.Me.InGreaterRift)
					{
						GRiftOrbDecorator.ToggleDecorators<GroundLabelDecorator>(!actor.IsOnScreen);
						GRiftOrbDecorator.Paint(layer, actor, actor.FloorCoordinate, TextRift);
					}
					else
					{
						NRiftOrbDecorator.ToggleDecorators<GroundLabelDecorator>(!actor.IsOnScreen);
						NRiftOrbDecorator.Paint(layer, actor, actor.FloorCoordinate, TextRift);	
					}	
				}
			}
		}
		public void PaintTopInGame(ClipState clipState)
        {
			if (clipState != ClipState.BeforeClip) return;
			
			if (MiniGlobes)
			{
				var x = Hud.Render.MinimapUiElement.Rectangle.Right;
				var y = Hud.Render.MinimapUiElement.Rectangle.Bottom;	
				
				var actors = Hud.Game.Actors.Where(a => (a.SnoActor.Kind == ActorKind.HealthGlobe));
				if (actors.Any())
				{
					DrawGlobe(ref x, y, actors.Count(), BrushHealth);					
				}
				
				actors = Hud.Game.Actors.Where(a => (a.SnoActor.Kind == ActorKind.PowerGlobe));
				if (actors.Any())
				{
					DrawGlobe(ref x, y, actors.Count(), BrushPower);					
				}
				actors = Hud.Game.Actors.Where(a => (a.SnoActor.Kind == ActorKind.RiftOrb));
				
				if (actors.Any())
				{
					DrawGlobe(ref x, y, actors.Count(),Hud.Game.Me.InGreaterRift?BrushGR:BrushNR,0);					
				}				
				else
				{
					actors = Hud.Game.Items.Where(a => a.SnoItem == Hud.Sno.SnoItems.Challenge_Rifts_Orb);
					if (actors.Any())
					{
						DrawGlobe(ref x, y, actors.Count(), BrushFillCR);
						foreach (var actor in actors)
						{
							BrushBorderCR.DrawWorldEllipse(1, -1, actor.FloorCoordinate);									
						}				
					}					
				}
			}			
		}
	}
}