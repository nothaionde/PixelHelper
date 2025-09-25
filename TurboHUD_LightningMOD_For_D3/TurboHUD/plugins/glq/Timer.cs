using System;

namespace Turbo.Plugins.glq
{
	/// <summary>
	/// Description of UtilClass.
	/// </summary>
	public class Timer
	{
        private const int MAX_TIMER_NUM = 10000;
		private static long[] t = new long[MAX_TIMER_NUM];
		private static uint[] Sno = new uint[MAX_TIMER_NUM];
		
		public Timer()
		{
		}
		public static void Reset()
		{
			for (int i=0; i < t.Length; i++)
				t[i] = 0;
		}
		private static bool Delay(IController Hud, int id, int delay)
		{
            var msec = Hud.Game.CurrentRealTimeMilliseconds;
			
			if (t[id] == 0)  //保证第一次使用delay必然被执行，应对那些定时比较大的情况
			{
				t[id] = msec;
				return true;
			}
			if (Math.Abs(msec - t[id]) >= delay)
			{
				t[id] = msec;
				return true;
			}
			return false;
		}
		public static bool Delay(IController Hud, uint sno, int delay)
		{
			int id = 0;
			
			for (int i=1; i<Sno.Length; i++)
			{
				if ((Sno[i] == 0) && (id == 0)) id = i;
				if (Sno[i] == sno)
				{
					id = i;
					break;
				}
			}
			Sno[id] = sno;
			return Delay(Hud, id, delay);
		}
	}
}
