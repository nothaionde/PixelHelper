//using System;
using System.Collections.Generic;

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Proc
{
	public class PlayerProcInfo
	{
		//public string PlayerName { get; set; }
		public uint HeroId { get; set; }
		public HeroClass HeroClass { get; set; }
		public int LastSeenTick { get; set; }
		
		public int LivesSpent { get; set; }
		public int LivesRemaining { get; set; }
		//public int StartTick { get; set; }
		//public int FinishTick { get; set; } //gametick / 60 = seconds
		//public int SoundPlayedTick { get; set; }
		public Dictionary<uint, ProcInfo> Procs { get; set; } = new Dictionary<uint, ProcInfo>();
		public Dictionary<uint, int> ProcCount { get; set; } = new Dictionary<uint, int>(); //keep track of proc counts
		public Dictionary<uint, int> Data { get; set; } = new Dictionary<uint, int>(); //variables for keeping track of states
		//public string Debug { get; set; }
		
		public PlayerProcInfo(IPlayer player)
		{
			//PlayerName = player.BattleTagAbovePortrait;
			HeroId = player.HeroId;
			HeroClass = player.HeroClassDefinition.HeroClass;
			//Texture = GetHeroHead();
		}
	}
}