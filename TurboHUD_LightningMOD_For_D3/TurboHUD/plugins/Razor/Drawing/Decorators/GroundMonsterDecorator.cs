using System;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor
{
    // this is not a plugin, just a helper class to display a circle on the ground
    public class GroundMonsterDecorator : IWorldDecoratorWithRadius
    {
        public bool Enabled { get; set; }
        public WorldLayer Layer { get; } = WorldLayer.Ground;
        public IController Hud { get; }

		public bool SeeThrough { get; set; } = false;

        public IBrush Brush { get; set; }
		public IBrush BackgroundBrush { get; set; }
        public IBrush ShadowBrush { get; set; }

        public float Radius { get; set; }
        public IRadiusTransformator RadiusTransformator { get; set; }

        public GroundMonsterDecorator(IController hud)
        {
            Enabled = true;
            Hud = hud;
        }

        public void Paint(IActor actor, IWorldCoordinate coord, string text)
        {
            if (!Enabled) return;
            if (Brush == null) return;

            var radius = Radius;
            if (radius == -1)
            {
                if (actor != null)
                {
                    radius = Math.Min(actor.RadiusBottom, 20);
                }
                else return;
            }

            if (RadiusTransformator != null)
            {
                radius = RadiusTransformator.TransformRadius(radius);
            }

			//test
			Brush.StrokeWidth = 3;
			
            if (ShadowBrush != null)
            {
                if (Brush.StrokeStyle.DashStyle == SharpDX.Direct2D1.DashStyle.Solid)
                {
                    ShadowBrush.StrokeWidth = Brush.StrokeWidth >= 0 ? Brush.StrokeWidth + 1 : Brush.StrokeWidth - 1;
                    ShadowBrush.DrawWorldEllipse(radius+Brush.StrokeWidth*0.05f, -1, coord);	
					ShadowBrush.DrawWorldEllipse(radius-Brush.StrokeWidth*0.15f, -1, coord);
					ShadowBrush.DrawWorldEllipse(radius-Brush.StrokeWidth*0.35f, -1, coord);
                }
            }

			if (!SeeThrough) {
				if (BackgroundBrush != null)
					BackgroundBrush.DrawWorldEllipse(radius, -1, coord);
				else {
					float stroke_width = Brush.StrokeWidth;
					Brush.StrokeWidth = 0;
					Brush.Opacity = 65f/255f;
					Brush.DrawWorldEllipse(radius, -1, coord);
					Brush.StrokeWidth = stroke_width;
				}
				
				Brush.Opacity = 1f;
			} else
				Brush.Opacity = 0.35f;

            Brush.DrawWorldEllipse(radius+Brush.StrokeWidth*0.05f, -1, coord);
			Brush.DrawWorldEllipse(radius-Brush.StrokeWidth*0.15f, -1, coord);
			Brush.DrawWorldEllipse(radius-Brush.StrokeWidth*0.35f, -1, coord);
        }

        public IEnumerable<ITransparent> GetTransparents()
        {
            yield return Brush;
            yield return ShadowBrush;
        }
    }
}