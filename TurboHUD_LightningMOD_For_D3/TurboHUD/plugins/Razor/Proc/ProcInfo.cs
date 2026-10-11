//using System;
//using System.Collections.Generic;

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Proc
{
	public class ProcInfo
	{
		public ProcRule Rule { get; set; }
		public int StartTick { get; set; }
		public int FinishTick { get; set; }
		public int LastSeenTick { get; set; }
		public ITexture Texture { get; set; }
		//public bool Notified { get; set; } = false; //end state notification
		//public int InterruptTick { get; set; }
		//public int SoundPlayedTick { get; set; }
		
		public ProcInfo(ProcRule rule)
		{
			Rule = rule;
		}
	}
}