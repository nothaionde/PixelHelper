using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor
{
    public class ClockwiseRotationTransformator : IRotationTransformator
    {
        public IController Hud { get; }
        public int Speed { get; set; }

        public ClockwiseRotationTransformator(IController hud, int speed)
        {
            Hud = hud;
            Speed = speed;
        }

        public float TransformRotation(float angle)
        {
            if (Speed <= 0) return angle;

            var msec = Hud.Game.CurrentRealTimeMilliseconds;

            return 360 - ((msec / Speed) % 360);
        }
    }
}