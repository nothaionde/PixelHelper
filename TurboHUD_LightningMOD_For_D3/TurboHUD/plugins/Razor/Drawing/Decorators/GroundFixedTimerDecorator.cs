//a combination of GroundTimerDecorator + GroundLabelDecorator
//- start tick can be specified
//- actor reference no longer required

namespace Turbo.Plugins.Razor
{
	using System;
	using System.Collections.Generic;
	using SharpDX;
	using SharpDX.Direct2D1;
	
	using Turbo.Plugins.Default;

    // this is not a plugin, just a helper class to display timers on the ground
    public class GroundFixedTimerDecorator : IWorldDecorator
    {
		public int CreatedAtInGameTick { get; set; }
		public IFont TextFont { get; set; }
		
        public bool Enabled { get; set; }
        public WorldLayer Layer { get; } = WorldLayer.Ground;
        public IController Hud { get; }

        public IBrush BackgroundBrushEmpty { get; set; }
        public IBrush BackgroundBrushFill { get; set; }
        public IBrush BorderBrush { get; set; }

        public float Radius { get; set; }
        public float CountDownFrom { get; set; }
        
        //public int StepCount { get; set; } = 5; // must be a whole number divisor of 360

        public GroundFixedTimerDecorator(IController hud)
        {
            Enabled = true;
            Hud = hud;
            BorderBrush = hud.Render.CreateBrush(128, 0, 0, 0, 1);
        }

        public void Paint(IActor actor, IWorldCoordinate coord, string text)
        {
            if (!Enabled)
                return;

            var rad = Radius / 1200.0f * Hud.Window.Size.Height;
            var max = CountDownFrom;
            var elapsed = (float)(Hud.Game.CurrentGameTick - (CreatedAtInGameTick > 0 ? CreatedAtInGameTick : (actor is object ? actor.CreatedAtInGameTick : 0))) / 60.0f;
            if (elapsed < 0)
                return;

            if (elapsed > max)
                elapsed = max;

            //var startAngle = (Convert.ToInt32(360 / max * elapsed) - 90) / StepCount * StepCount;
            //var endAngle = 360 - 90;
			
            if (BackgroundBrushEmpty != null)
				BackgroundBrushEmpty.DrawEllipse(actor.ScreenCoordinate.X, actor.ScreenCoordinate.Y, rad+2, rad+2);

            if (BackgroundBrushFill != null)
            {
                using (var pg = Hud.Render.CreateGeometry())
                {
                    using (var gs = pg.Open())
                    {
						//var AngleStart = (Convert.ToInt32(360 / max * elapsed) - 90);
						//var AngleStop = 360 - 90;
						var AngleStop = (Convert.ToInt32(360 / max * elapsed) - 90);
						var AngleStart = 360 - 90;
						var mx = rad * (float)Math.Cos(AngleStart * Math.PI / 180f);
						var my = rad * (float)Math.Sin(AngleStart * Math.PI / 180f);

						gs.BeginFigure(new Vector2(actor.ScreenCoordinate.X, actor.ScreenCoordinate.Y), FigureBegin.Filled);
						gs.AddLine(new Vector2(actor.ScreenCoordinate.X + mx, actor.ScreenCoordinate.Y + my));
						
						mx = rad * (float)Math.Cos(AngleStop * Math.PI / 180f);
						my = rad * (float)Math.Sin(AngleStop * Math.PI / 180f);
						gs.AddArc(new ArcSegment()
						{
							//Point = new Vector2(screenX, screenY), //endpoint //new Vector2(Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f)
							Point = new Vector2(actor.ScreenCoordinate.X + mx, actor.ScreenCoordinate.Y + my), //endpoint //new Vector2(Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f)
							Size = new Size2F(rad, rad),
							RotationAngle = 0, //(float)(45f * System.Math.PI / 180f),  //45, //
							//SweepDirection = SweepDirection.Clockwise, //SweepDirection.Counterclockwise, //Clockwise,
							ArcSize = AngleStart - AngleStop > 180 ? ArcSize.Large : ArcSize.Small //AngleStop - AngleStart > 180 ? ArcSize.Small : ArcSize.Large //
						});

                        gs.EndFigure(FigureEnd.Closed);
                        gs.Close();
                    }

                    BackgroundBrushFill.DrawGeometry(pg);
                }
            }

            /*if (BorderBrush != null)
            {
                using (var pg = Hud.Render.CreateGeometry())
                {
                    using (var gs = pg.Open())
                    {
                        var mx = rad * (float)Math.Cos(0 * Math.PI / 180.0f);
                        var my = rad * (float)Math.Sin(0 * Math.PI / 180.0f);

                        gs.BeginFigure(new Vector2(screenCoord.X + mx, screenCoord.Y + my), FigureBegin.Hollow);
                        for (var angle = StepCount; angle <= 360; angle += StepCount)
                        {
                            mx = rad * (float)Math.Cos(angle * Math.PI / 180.0f);
                            my = rad * (float)Math.Sin(angle * Math.PI / 180.0f);
                            var vector = new Vector2(screenCoord.X + mx, screenCoord.Y + my);
                            gs.AddLine(vector);
                        }

                        gs.EndFigure(FigureEnd.Closed);
                        gs.Close();
                    }

                    BorderBrush.DrawGeometry(pg);
                }
            }*/
			
			//label
			if (TextFont == null)
                return;

            if (CountDownFrom > 0)
            {
                var remaining = CountDownFrom - ((Hud.Game.CurrentGameTick - CreatedAtInGameTick) / 60.0f);
                if (remaining < 0)
                    remaining = 0;

                //var vf = (remaining > 1.0f) ? "F0" : "F1";
                //text = remaining.ToString(vf, CultureInfo.InvariantCulture);
				var layout = TextFont.GetTextLayout(remaining.ToString(remaining > 1.0f ? "F0" : "F1"));
				TextFont.DrawText(layout, actor.ScreenCoordinate.X - layout.Metrics.Width*0.5f, actor.ScreenCoordinate.Y - layout.Metrics.Height*0.5f);
            }
        }

        public IEnumerable<ITransparent> GetTransparents()
        {
            yield return BackgroundBrushEmpty;
            yield return BackgroundBrushFill;
            yield return BorderBrush;
            yield return TextFont;
        }
    }
}