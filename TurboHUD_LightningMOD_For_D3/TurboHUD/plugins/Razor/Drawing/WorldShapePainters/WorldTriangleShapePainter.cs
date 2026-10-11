namespace Turbo.Plugins.Razor
{
	using System;
	using SharpDX;
	using SharpDX.Direct2D1;
	
	using Turbo.Plugins.Default;

    public class WorldTriangleShapePainter : IWorldShapePainter
    {
        public IController Hud { get; }
		public float Radius { get; set; } = 1f;
        public int VerticeCount { get; set; } = 3;
        public int VerticeJump { get; set; } = 1;
		public float BorderWidth { get; set; } = 0;
		public float BorderShrink { get; set; } = 0.6f;
		public float ShadowWidth { get; set; } = 0;

        public WorldTriangleShapePainter(IController hud)
        {
            Hud = hud;
        }

        public void Paint(float x, float y, float z, float radius, float rotation, IBrush brush, IBrush shadowBrush)
        {
            if (shadowBrush != null)
            {
                shadowBrush.StrokeWidth = brush.StrokeWidth >= 0 ? brush.StrokeWidth + ShadowWidth : brush.StrokeWidth - ShadowWidth;
                Paint(x, y, z, radius, rotation, shadowBrush);
            }

			if (BorderWidth > 0)
			{
				var tmp = brush.StrokeWidth;
				brush.StrokeWidth = BorderWidth;
				Paint(x, y, z, Radius, rotation, brush);
				
				brush.StrokeWidth = tmp;
				Paint(x, y, z, Radius * BorderShrink, rotation, brush);
			}
			else
				Paint(x, y, z, Radius, rotation, brush);
        }

        private void Paint(float x, float y, float z, float radius, float rotation, IBrush brush)
        {
			using (var pg = Hud.Render.CreateGeometry())
			{
				using (var gs = pg.Open())
				{
					float angleStep = 360f / VerticeCount;
					bool start = false;
					
					float screenX;
					float screenY;
					for (var i = 0; i < VerticeCount; i++)
					{
						var sx = radius * (float)Math.Cos((((i + 0) * angleStep) + rotation) * Math.PI / 180f);
						var sy = radius * (float)Math.Sin((((i + 0) * angleStep) + rotation) * Math.PI / 180f);
						var ex = radius * (float)Math.Cos((((i + VerticeJump) * angleStep) + rotation) * Math.PI / 180f);
						var ey = radius * (float)Math.Sin((((i + VerticeJump) * angleStep) + rotation) * Math.PI / 180f);
						//brush.DrawLineWorld(x + sx, y + sy, z, x + ex, y + ey, z);
						
						if (!start)
						{
							start = true;

							Hud.Window.WorldToScreenCoordinate(x + sx, y + sy, z, out screenX, out screenY);
							var vector = new Vector2(screenX, screenY);
							gs.BeginFigure(vector, FigureBegin.Filled);
						}
						
						Hud.Window.WorldToScreenCoordinate(x + ex, y + ey, z, out screenX, out screenY);
						gs.AddLine(new Vector2(screenX, screenY));
					}

					gs.EndFigure(FigureEnd.Closed);
					gs.Close();
				}
 
				brush.DrawGeometry(pg);
			}
        }
    }
}