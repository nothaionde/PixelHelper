using System;
using System.Collections.Generic;

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Proc
{
	public class ProcRule
	{
		//buff info
		public uint Sno { get; set; }
		public int IconIndex { get; set; }
		public bool ShowTimeLeft { get; set; } = true;
		public bool CheckInactive { get; set; } = false; //check for the buff being not active
		public bool IsExtraLife { get; set; } = true;

		//optional overrides
		public Func<IPlayer, bool> IsAvailable { get; set; }
		public Func<IPlayer, bool> IsProcced { get; set; }
		public uint UseCooldownTime { get; set; } //show cooldown of this skill sno instead
		public BuffRule UseBuff { get; set; } //show the duration of this buff instead
		public ISnoItem UseItemTexture { get; set; } // = (sno) => Hud.Texture.GetTexture(Hud.Sno.GetSnoPower(sno).NormalIconTextureId);
		public uint UseTextureId { get; set; }

		//notification options
		public bool Beep { get; set; } = false;
		public string SoundFileMe { get; set; } //= "ProcYou.wav";
		public string SoundFileOther { get; set; } //= "ProcOther.wav";
		public string TTSMe { get; set; } //= "You are on prock";
		public string TTSOther { get; set; } //= "<name> is on prock";
		
		public ProcRule(uint sno)
		{
			Sno = sno;
		}
	}
}