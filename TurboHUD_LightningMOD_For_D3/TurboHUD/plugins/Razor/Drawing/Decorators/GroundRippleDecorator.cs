using System;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor
{
    // this is not a plugin, just a helper class to display a circle on the ground
    public class GroundRippleDecorator : IWorldDecoratorWithRadius
    {
        public bool Enabled { get; set; }

        public IBrush Brush { get; set; }
		public IBrush ShadowBrush { get; set; }
		
		public float OpacityMinimumMultiplier { get; set; } = 0.15f;
        public float Radius { get; set; }
        public IRadiusTransformator RadiusTransformator { get; set; }

        public WorldLayer Layer { get; } = WorldLayer.Ground;
        public IController Hud { get; }

        public GroundRippleDecorator(IController hud)
        {
            Enabled = true;
            Hud = hud;
        }

        public void Paint(IActor actor, IWorldCoordinate coord, string text)
        {
            if (!Enabled)
                return;
            if (Brush == null)
                return;

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

            if (RadiusTransformator != null)
            {
				float old = radius;
                radius = RadiusTransformator.TransformRadius(radius);
			
				if (OpacityMinimumMultiplier < 1)
				{
					/*float diff = RadiusTransformator.RadiusMaximumMultiplier - RadiusTransformator.RadiusMinimumMultiplier; //intervals
					float multiplier = radius / old;
					float pct = (multiplier - RadiusTransformator.RadiusMinimumMultiplier) / diff;*/
					//diff = (1f - OpacityMinimumMultiplier) * pct;

					/*Brush.Opacity = 1f - (radius / old);
					if (Brush.Opacity < OpacityMinimumMultiplier)
						Brush.Opacity = OpacityMinimumMultiplier;*/
					Brush.Opacity = (1f - OpacityMinimumMultiplier) * (1f - (radius / old)) + OpacityMinimumMultiplier;
				}
            }

            ShadowBrush?.DrawWorldEllipse(radius, -1, coord);
            Brush.DrawWorldEllipse(radius, -1, coord);
        }

        public IEnumerable<ITransparent> GetTransparents()
        {
            yield return Brush;
            yield return ShadowBrush;
        }
    }
}