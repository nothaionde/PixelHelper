/*

Draws the specified ShapePainter along the circumference of the world circle of the specified radius, at the specified angle interval

*/

namespace Turbo.Plugins.Razor
{
	using SharpDX;
	using SharpDX.Direct2D1;
	using System; //Math
	using System.Collections.Generic;
	using Turbo.Plugins.Default;
	
	// this is not a plugin, just a helper class to display labels on the ground
	public class GroundOrbitDecorator : IWorldDecoratorWithRadius
	{
		public bool Enabled { get; set; }
		public WorldLayer Layer { get; } = WorldLayer.Ground;
		public IController Hud { get; }

		public IBrush Brush { get; set; }
		public IBrush ShadowBrush { get; set; }
		public IWorldShapePainter ShapePainter { get; set; }

		public float Radius { get; set; } = 1.0f;
		public float Rotation { get; set; } = -45f;
		public int Interval { get; set; } = 90;
		public Func<float, float, float> Direction = (x, y) => (x == 0 && y == 0 ? 0 : (float)(Math.Atan2(y, x) / (Math.PI / 180f))) + 180f; //point towards coord //without the added 180f at the end, it points away from coord

		public IRadiusTransformator RadiusTransformator { get; set; }
		public IRotationTransformator RotationTransformator { get; set; }

		public GroundOrbitDecorator(IController hud)
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
			if (Radius <= 0)
				return;
			if (ShapePainter == null)
				return;
			
			var radius = RadiusTransformator != null ? RadiusTransformator.TransformRadius(Radius) : Radius;
			var rotation = RotationTransformator != null ? RotationTransformator.TransformRotation(Rotation) : Rotation;
			float[] offsets = new float[] {-75f, -15f, -45f, 45f}; //left, bottom, right, top
			int count = 360 / Interval;
			
			for (int i = 0; i < count; i++)
			{
				float mx = radius * (float)Math.Cos((i * Interval + rotation) * Math.PI / 180f);
				float my = radius * (float)Math.Sin((i * Interval + rotation) * Math.PI / 180f);

				ShapePainter.Paint(coord.X + mx, coord.Y + my, coord.Z, 1f, Direction(mx, my), Brush, ShadowBrush);

			}
		}

		public IEnumerable<ITransparent> GetTransparents()
		{
			yield return Brush;
			yield return ShadowBrush;
		}
	}
}