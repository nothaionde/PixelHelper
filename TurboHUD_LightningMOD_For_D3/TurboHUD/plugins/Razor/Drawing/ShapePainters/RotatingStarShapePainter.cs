using System;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor
{
    public class RotatingStarShapePainter : IShapePainter
    {
        public IController Hud { get; }

        public RotatingStarShapePainter(IController hud)
        {
            Hud = hud;
        }

        public void Paint(float x, float y, float radius, IBrush brush, IBrush shadowBrush)
        {
            var tickRotationAngle = ((Hud.Game.CurrentRealTimeMilliseconds / 10) % 360); // /2
            if (shadowBrush != null)
            {
                shadowBrush.StrokeWidth = brush.StrokeWidth + 1;
                shadowBrush.DrawLine(x + radius * (float)Math.Cos((0.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((0.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f));
                shadowBrush.DrawLine(x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f));
                shadowBrush.DrawLine(x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((360.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((360.0f + tickRotationAngle) * Math.PI / 180.0f));

				tickRotationAngle += 180;
                shadowBrush.DrawLine(x + radius * (float)Math.Cos((0.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((0.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f));
                shadowBrush.DrawLine(x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f));
                shadowBrush.DrawLine(x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((360.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((360.0f + tickRotationAngle) * Math.PI / 180.0f));

				tickRotationAngle -= 180;
            }

            brush.DrawLine(x + radius * (float)Math.Cos((0.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((0.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f));
            brush.DrawLine(x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f));
            brush.DrawLine(x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((360.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((360.0f + tickRotationAngle) * Math.PI / 180.0f));
			
			tickRotationAngle += 180;
            brush.DrawLine(x + radius * (float)Math.Cos((0.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((0.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f));
            brush.DrawLine(x + radius * (float)Math.Cos((120.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((120.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f));
            brush.DrawLine(x + radius * (float)Math.Cos((240.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((240.0f + tickRotationAngle) * Math.PI / 180.0f), x + radius * (float)Math.Cos((360.0f + tickRotationAngle) * Math.PI / 180.0f), y + radius * (float)Math.Sin((360.0f + tickRotationAngle) * Math.PI / 180.0f));
        }
    }
}