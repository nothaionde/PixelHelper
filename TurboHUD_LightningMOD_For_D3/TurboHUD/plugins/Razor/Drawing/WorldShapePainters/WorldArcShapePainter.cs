namespace Turbo.Plugins.Razor
{
	using System;
	using SharpDX;
	using SharpDX.Direct2D1;
	
	using Turbo.Plugins.Default;

	public class WorldArcShapePainter : IWorldShapePainter
	{
		public IController Hud { get; }
		public float AngleStart { get; set; } = 0;
		public float AngleStop { get; set; } = 135f;
		public float Rotation { get; set; } = 0;
		public float RadiusDecay { get; set; } = 0;
		public float Step { get; set; } = 8f; //optimization level (how many degrees to skip inbetween each vector that makes up the arc)
		//public float LineLength { get; set; } = 0.7f; //percent in decimal format
		//public IBrush Brush2 { get; set; }
		
		//private IBrush debug;

		public WorldArcShapePainter(IController hud)
		{
			Hud = hud;
			//debug = Hud.Render.CreateBrush(255, 255, 0, 0, 3);
		}

		public void Paint(float x, float y, float z, float radius, float rotation, IBrush brush, IBrush shadowBrush)
		{
			/*if (shadowBrush != null)
			{
				shadowBrush.StrokeWidth = brush.StrokeWidth >= 0 ? brush.StrokeWidth + 1 : brush.StrokeWidth - 1;
				Paint(x, y, z, radius, rotation, shadowBrush);
			}

			Paint(x, y, z, radius, rotation, brush);*/
			
			Arc(x, y, z, radius, rotation, brush);
		}

		private void Arc(float x, float y, float z, float radius, float rotation, IBrush brush)
		{
			/*using (var pg = Hud.Render.CreateGeometry())
			{
				using (var gs = pg.Open())
				{
					var mx = radius * (float)Math.Cos((AngleStart  + rotation) * Math.PI / 180f);
					var my = radius * (float)Math.Sin((AngleStart + rotation) * Math.PI / 180f);
					//var screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, z).ToScreenCoordinate(true);
					//var vector = new Vector2(screenCoord.X, screenCoord.Y);
					Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out float screenX, out float screenY);
					var vector = new Vector2(screenX, screenY);

 
					gs.BeginFigure(vector, FigureBegin.Filled); // FigureBegin.Filled , FigureBegin.Hollow

					var start = AngleStart;
					var stop = AngleStop;
					if (AngleStart > AngleStop)
						stop = 359;

					for (int angle = start + 1; angle <= stop; angle++)
					{
						mx = radius * (float)Math.Cos((angle + rotation) * Math.PI / 180f);
						my = radius * (float)Math.Sin((angle + rotation) * Math.PI / 180f);

						radius -= RadiusDecay;
						if (radius < 0)
							radius = 0;

						//screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, z).ToScreenCoordinate(true);
						//vector = new Vector2(screenCoord.X, screenCoord.Y);
						Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out screenX, out screenY);
						vector = new Vector2(screenX, screenY);
 
						gs.AddLine(vector);
					}
					
					if (AngleStart > AngleStop)
					{
						start = 0;
						stop = AngleStop;

						for (int angle = start; angle <= stop; angle++)
						{
							mx = radius * (float)Math.Cos((angle + rotation) * Math.PI / 180f);
							my = radius * (float)Math.Sin((angle + rotation) * Math.PI / 180f);

							radius -= RadiusDecay;
							if (radius < 0)
								radius = 0;

							//screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, 0).ToScreenCoordinate(true);
							//vector = new Vector2(screenCoord.X, screenCoord.Y);
							Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out screenX, out screenY);
							vector = new Vector2(screenX, screenY);
	 
							gs.AddLine(vector);
						}
					}
 
					gs.EndFigure(FigureEnd.Open);
					gs.Close();
				}
 
				brush.DrawGeometry(pg);
			}*/
			
			using (var pg = Hud.Render.CreateGeometry())
			{
				using (var gs = pg.Open())
				{
					var mx = radius * (float)Math.Cos((AngleStart  + rotation) * Math.PI / 180f);
					var my = radius * (float)Math.Sin((AngleStart + rotation) * Math.PI / 180f);
					//var screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, z).ToScreenCoordinate(true);
					//var vector = new Vector2(screenCoord.X, screenCoord.Y);
					Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out float screenX, out float screenY);
					var vector = new Vector2(screenX, screenY);

 
					gs.BeginFigure(vector, FigureBegin.Filled); // FigureBegin.Filled , FigureBegin.Hollow

					var start = AngleStart;
					var stop = AngleStop;
					if (AngleStart > AngleStop)
						//stop = 359;
						stop = 360;
					
					for (float angle = IncrementAngle(start, stop); angle < stop; ) //start + Step //angle++
					{
						mx = radius * (float)Math.Cos((angle + rotation) * Math.PI / 180f);
						my = radius * (float)Math.Sin((angle + rotation) * Math.PI / 180f);

						radius -= RadiusDecay;
						if (radius < 0)
							radius = 0;

						//screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, z).ToScreenCoordinate(true);
						//vector = new Vector2(screenCoord.X, screenCoord.Y);
						Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out screenX, out screenY);
						vector = new Vector2(screenX, screenY);
 
						gs.AddLine(vector);
						
						angle = IncrementAngle(angle, stop);
					}
					
					gs.EndFigure(FigureEnd.Open);
					gs.Close();
				}
 
				brush.DrawGeometry(pg);
			}
			
			/*
			
					if (AngleStart > AngleStop)
					{
						start = 0;
						stop = AngleStop;
						//var step = 20f;
						//find the angle diff
						

						for (float angle = start; angle <= stop; )
						{
							mx = radius * (float)Math.Cos((angle + rotation) * Math.PI / 180f);
							my = radius * (float)Math.Sin((angle + rotation) * Math.PI / 180f);

							radius -= RadiusDecay;
							if (radius < 0)
								radius = 0;

							//screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, 0).ToScreenCoordinate(true);
							//vector = new Vector2(screenCoord.X, screenCoord.Y);
							Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out screenX, out screenY);
							vector = new Vector2(screenX, screenY);
	 
							gs.AddLine(vector);
							
							//angle = IncrementAngle(angle, stop);
							if (angle != stop - 1)
							{
								if (angle + Step >= stop)
									angle = stop - 1;
								else
									angle += Step;
							}
						}
					}
			
			*/
			
			/*using (var pg = Hud.Render.CreateGeometry())
			{
				using (var gs = pg.Open())
				{
					var mx = radius * (float)Math.Cos((AngleStart + rotation) * Math.PI / 180f);
					var my = radius * (float)Math.Sin((AngleStart + rotation) * Math.PI / 180f);
					//var screenCoord = Hud.Window.CreateWorldCoordinate(x + mx, y + my, z).ToScreenCoordinate(true);
					//var vector = new Vector2(screenCoord.X, screenCoord.Y);
					Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out float screenX, out float screenY);
					var start = new Vector2(screenX, screenY);
					gs.BeginFigure(start, FigureBegin.Filled);
					
					mx = radius * (float)Math.Cos((AngleStop + rotation) * Math.PI / 180f);
					my = radius * (float)Math.Sin((AngleStop + rotation) * Math.PI / 180f);
					Hud.Window.WorldToScreenCoordinate(x + mx, y + my, z, out screenX, out screenY);
					gs.AddArc(new ArcSegment()
					{
						Point = new Vector2(screenX, screenY), //endpoint //new Vector2(Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f)
						Size = new Size2F(30, 25),
						RotationAngle = 0, //(float)(45f * System.Math.PI / 180f),  //45, //
						SweepDirection = (SweepDirection)0, //SweepDirection.Counterclockwise, //Clockwise,
						ArcSize = ArcSize.Large //ArcSize.Small //
					});
					
					gs.EndFigure(FigureEnd.Open);
					gs.Close();
				}
				
				debug.DrawGeometry(pg);
			}*/
			
			/*ArcSegment arc = new D2D1.ArcSegment()
			{
				Point = EndPoint,
				Size = Size,
				RotationAngle = RotationAngle,
				SweepDirection = SweepDirection,
				ArcSize = ArcSize
			};
			sink.AddArc(arc);*/
		}
		
		private float IncrementAngle(float angle, float stop)
		{
			if (angle != stop - 1)
			{
				if (angle + Step > stop - 1)
					angle = stop - 1;
				else
					angle += Step;
			}
			else
				angle = stop;
			
			return angle;
		}
		
		/*private int AngleDiff(float a, float b)
		{
			int d = Math.Abs(a - b) % 360; 
			int r = d > 180 ? 360 - d : d;
			//calculate sign 
			int sign = (a - b >= 0 && a - b <= 180) || (a - b <=-180 && a- b>= -360) ? 1 : -1; 
			r *= sign;
			
			return r;
		}*/
	}
}