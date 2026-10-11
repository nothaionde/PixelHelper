namespace Turbo.Plugins.Razor
{
	using System;
	using Turbo.Plugins.Default;
	
    public class RippleRadiusTransformator : IRadiusTransformator
    {
        public IController Hud { get; }

        public int PingSpeed { get; set; }
		public int Offset { get; set; } //time delay from current in milliseconds
		public bool Outward { get; set; } //animation direction
        public float RadiusMinimumMultiplier { get; set; }
        public float RadiusMaximumMultiplier { get; set; }

        public RippleRadiusTransformator(IController hud, int pingSpeed, bool outward = true, int offset = 0, float radiusMaximumMultiplier = 1, float radiusMinimumMultiplier = 0)
        {
            Hud = hud;
            PingSpeed = pingSpeed;
			Offset = offset;
			Outward = outward;
            RadiusMinimumMultiplier = radiusMinimumMultiplier;
            RadiusMaximumMultiplier = radiusMaximumMultiplier;
        }

        public float TransformRadius(float radius)
        {
            if (PingSpeed <= 0)
                return radius;

            var msec = Hud.Game.CurrentRealTimeMilliseconds + Offset;
            
			//adapted from StandardPingRadiusTransformator
			return Outward ?
				radius * (RadiusMinimumMultiplier + ((RadiusMaximumMultiplier - RadiusMinimumMultiplier) * (msec % PingSpeed) / PingSpeed)) :
				radius * (RadiusMaximumMultiplier - ((RadiusMaximumMultiplier - RadiusMinimumMultiplier) * (msec % PingSpeed) / PingSpeed)); 
				
			//Hud.TextLog.Log("_radius", string.Format("({0} % {1}) / {1} = {2}", msec, PingSpeed, (msec % PingSpeed) / PingSpeed), false, true);
        }
    }
}