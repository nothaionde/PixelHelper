namespace Turbo.Plugins.Razor
{
	using System;
	using SharpDX;
	using SharpDX.Direct2D1;
	
	using Turbo.Plugins.Default;

    public class WorldDownArrowShapePainter : IWorldShapePainter
    {
        public IController Hud { get; }
		public float Width { get; set; } = 1.75f; //like world-coordinate radius
		public float HeightWidthRatio { get; set; } = 6f; //the number of times higher it is than wide
		public float ArrowHeadRatio { get; set; } = 3.5f;
		private TriangleShapePainter Triangle;

        public WorldDownArrowShapePainter(IController hud)
        {
            Hud = hud;
			Triangle = new TriangleShapePainter(hud);
        }

        public void Paint(float x, float y, float z, float radius, float rotation, IBrush brush, IBrush shadowBrush)
        {
            if (shadowBrush != null)
            {
                Paint(x, y, z, radius, rotation, shadowBrush);
            }

			Paint(x, y, z, radius, rotation, brush);
        }

        private void Paint(float x, float y, float z, float radius, float rotation, IBrush brush)
        {
			Hud.Window.WorldToScreenCoordinate(x, y, z, out float screenX, out float screenY);
			Hud.Window.WorldToScreenCoordinate(x + Width, y, z, out float screenX2, out float screenY2);
			
			//calculate width
			float w = (float)Math.Abs(screenX2 - screenX);
			float h = w*HeightWidthRatio;
			
			screenY -= h*radius;
			//brush.DrawRectangle(screenX - w*0.5f, screenY - h, w, h);
			
			//Triangle.Paint(screenX, screenY, 50f, brush, null);
			//var angle = 30.0f;
			//brush.DrawLine(screenX + (h * (float)Math.Cos((0.0f + angle) * Math.PI / 180.0f)), screenY + (h * (float)Math.Sin((0.0f + angle) * Math.PI / 180.0f)), screenX + (h * (float)Math.Cos((120.0f + angle) * Math.PI / 180.0f)), screenY + (h * (float)Math.Sin((120.0f + angle) * Math.PI / 180.0f)));
			
			/*var angle = 30.0f;
			radius = 50f;
			x = screenX;
			y = screenY;

			brush.DrawLine(x + (radius * (float)Math.Cos((0.0f + angle) * Math.PI / 180.0f)), y + (radius * (float)Math.Sin((0.0f + angle) * Math.PI / 180.0f)), x + (radius * (float)Math.Cos((120.0f + angle) * Math.PI / 180.0f)), y + (radius * (float)Math.Sin((120.0f + angle) * Math.PI / 180.0f)));
            brush.DrawLine(x + (radius * (float)Math.Cos((120.0f + angle) * Math.PI / 180.0f)), y + (radius * (float)Math.Sin((120.0f + angle) * Math.PI / 180.0f)), x + (radius * (float)Math.Cos((240.0f + angle) * Math.PI / 180.0f)), y + (radius * (float)Math.Sin((240.0f + angle) * Math.PI / 180.0f)));
            brush.DrawLine(x + (radius * (float)Math.Cos((240.0f + angle) * Math.PI / 180.0f)), y + (radius * (float)Math.Sin((240.0f + angle) * Math.PI / 180.0f)), x + (radius * (float)Math.Cos((360.0f + angle) * Math.PI / 180.0f)), y + (radius * (float)Math.Sin((360.0f + angle) * Math.PI / 180.0f)));
			*/
			using (var pg = Hud.Render.CreateGeometry())
			{
				using (var gs = pg.Open())
				{
					float arrowHeadLength = w * ArrowHeadRatio;
					
					gs.BeginFigure(new Vector2(screenX, screenY), FigureBegin.Filled);
					
					screenX2 = screenX + arrowHeadLength * (float)Math.Cos(-60f * Math.PI / 180.0f);
					screenY2 = screenY + arrowHeadLength * (float)Math.Sin(-60f * Math.PI / 180.0f);
					gs.AddLine(new Vector2(screenX2, screenY2));
					
					float screenY3 = screenY + (screenY2-screenY)*0.5f;
					//gs.AddLine(new Vector2(screenX, screenY3));
					//brush.DrawRectangle(screenX - w*0.5f, screenY2 - h, w, h);
					
					//float m = (y2-y1)/(x2-x1); //slope
					float m = (screenY2 - screenY3) / (screenX2 - screenX);
					//y = m * (**x** - x1) + y1 //given x, we want to know y
					float screenX3 = screenX + w*0.5f;
					screenY3 = m * (screenX3 - screenX2) + screenY2;
					gs.AddLine(new Vector2(screenX3, screenY3));
					
					float screenY4 = screenY2 - h;
					gs.AddLine(new Vector2(screenX3, screenY4));
					gs.AddLine(new Vector2(screenX - w*0.5f, screenY4));
					gs.AddLine(new Vector2(screenX - w*0.5f, screenY3));
					
					//float screenX5 = screenX + arrowHeadLength * (float)Math.Cos(240f * Math.PI / 180.0f);
					//screenY5 = screenY + arrowHeadLength * (float)Math.Sin(240f * Math.PI / 180.0f);
					//gs.AddLine(new Vector2(screenX5, screenY2));
					gs.AddLine(new Vector2(screenX - (screenX2 - screenX), screenY2));
					
					gs.EndFigure(FigureEnd.Closed);
					gs.Close();
					
					
					/*float angleStep = 360f / VerticeCount;
					bool start = false;
					
					//float screenX;
					//float screenY;
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
					gs.Close();*/
				}
 
				brush.DrawGeometry(pg);
			}
        }
    }
}