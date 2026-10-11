namespace Turbo.Plugins.Razor
{
	using System;
	using Turbo.Plugins.Default;

    public class WorldVortexShapePainter : IWorldShapePainter
    {
        public IController Hud { get; }
        public int VerticeCount { get; set; } = 3;
        public int VerticeJump { get; set; } = 1;
		public float Rotation { get; set; } = 0;
		public float LineLength { get; set; } = 0.7f; //percent in decimal format
		public IBrush Brush2 { get; set; }

        public WorldVortexShapePainter(IController hud)
        {
            Hud = hud;
        }

        public void Paint(float x, float y, float z, float radius, float rotation, IBrush brush, IBrush shadowBrush)
        {
            if (shadowBrush != null)
            {
                shadowBrush.StrokeWidth = brush.StrokeWidth >= 0 ? brush.StrokeWidth + 1 : brush.StrokeWidth - 1;
                Paint(x, y, z, radius, rotation, shadowBrush);
            }

            Paint(x, y, z, radius, rotation, brush);
        }

        private void Paint(float x, float y, float z, float radius, float rotation, IBrush brush)
        {
			if (Brush2 == null)
				Brush2 = brush;
			
            var angleStep = 360f / VerticeCount;
            for (var i = 0; i < VerticeCount; ++i)
            {
                var sx = radius*(1f - LineLength) * (float)Math.Cos((((i + 0) * angleStep) + Rotation + rotation) * Math.PI / 180f);
                var sy = radius*(1f - LineLength) * (float)Math.Sin((((i + 0) * angleStep) + Rotation + rotation) * Math.PI / 180f);
                var ex = radius * (float)Math.Cos((((i + VerticeJump) * angleStep) + Rotation + rotation) * Math.PI / 180f);
                var ey = radius * (float)Math.Sin((((i + VerticeJump) * angleStep) + Rotation + rotation) * Math.PI / 180f);
                (i % 2 == 0 ? Brush2 : brush).DrawLineWorld(x + sx, y + sy, z, x + ex, y + ey, z);
            }
        }
    }
}