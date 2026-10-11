using System;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor
{
    // this is not a plugin, just a helper class to display a circle on the ground
    public class GroundSwirlDecorator : IWorldDecoratorWithRadius
    {
        public bool Enabled { get; set; }

        public IBrush Brush { get; set; }
		public IBrush ShadowBrush { get; set; }
		
        public float Radius { get; set; }
		public float RadiusStep { get; set; } = 0.5f;
		public float RadiusMin { get; set; } = 2f;
		
		public float Angle { get; set; } = 135f; //angle
		public float AngleStep { get; set; } = 45f; //angle
		
		public float Rotation { get; set; } = 0.0f;
		
		public List<IBrush> Brushes { get; set; }
        
		public IRadiusTransformator RadiusTransformator { get; set; }
		public IRotationTransformator RotationTransformator { get; set; }

        public WorldLayer Layer { get; } = WorldLayer.Ground;
        public IController Hud { get; }
		
		private WorldArcShapePainter ArcPainter;

        public GroundSwirlDecorator(IController hud)
        {
            Enabled = true;
            Hud = hud;
			ArcPainter = new WorldArcShapePainter(Hud) {AngleStart = 0, AngleStop = 180, RadiusDecay = 0f};
        }

        public void Paint(IActor actor, IWorldCoordinate coord, string text)
        {
            if (!Enabled)
                return;
            if (Brushes == null)
                return;

			var rotation = (RotationTransformator is object ? RotationTransformator.TransformRotation(Rotation) : Rotation);
            var radius = Radius;
            if (radius == -1)
            {
                if (actor != null)
                {
                    radius = Math.Min(actor.RadiusBottom, 20);
                }
                else
                {
                    return;
                }
            }
			
			if (RadiusTransformator is object)
                radius = RadiusTransformator.TransformRadius(radius);
			
			int brushIndex = 0;
			for (float r = radius, angle = 0; r >= RadiusMin; )
			{
				ArcPainter.AngleStart = angle;
				ArcPainter.AngleStop = angle + Angle;
				ArcPainter.Paint(coord.X, coord.Y, coord.Z, r, rotation, Brushes[brushIndex], ShadowBrush);
				
				r -= RadiusStep; //DecrementRadius(r, RadiusStep);
				brushIndex = (brushIndex == Brushes.Count - 1 ? 0 : brushIndex + 1);
				angle += AngleStep;
				if (angle > 359)
					angle -= 360;
			}
        }

        public IEnumerable<ITransparent> GetTransparents()
        {
            yield return Brush;
            yield return ShadowBrush;
        }
		
		private float DecrementRadius(float radius, float min)
		{
			if (radius > min)
			{
				if (radius - RadiusStep > min)
					radius = min;
				else
					radius -= RadiusStep;
			}
			else
				radius -= RadiusStep;
			
			return radius;
		}
    }
}