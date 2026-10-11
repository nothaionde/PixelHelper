using System; //Exception
using System.Globalization; //CultureInfo
using System.Collections.Generic;

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Proc
{
	public class PartyProcScreenshots : BasePlugin, IProcHandler, IAfterCollectHandler
    {
		public bool TakeScreenshotOnProc { get; set; } = true;
		public bool TakeScreenshotOnDeath { get; set; } = true;
		public bool TakeSelfiesOnly { get; set; } = false;
		public string SubFolderName { get; set; } = "capture_proc";
		
		private HashSet<uint> PotentiallyDead = new HashSet<uint>();
		
		public PartyProcScreenshots()
        {
            Enabled = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
        }
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			if (!TakeScreenshotOnDeath)
				return;
			
			foreach (IPlayer player in Hud.Game.Players)
			{
				if (TakeSelfiesOnly && !player.IsMe)
					continue;
				
				if (player.IsDeadSafeCheck)
				{
					if (!PotentiallyDead.Contains(player.HeroId))
					{
						try
						{
							var fileName = string.Format("death_{0}_{1}_{2}.jpg", Hud.Time.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture), player.BattleTagAbovePortrait, player.HeroId.ToString("D", CultureInfo.InvariantCulture));
							Hud.Render.CaptureScreenToFile(SubFolderName, fileName);
						}
						catch (Exception) {}

						PotentiallyDead.Add(player.HeroId);
					}
				}
				else
				{
					if (PotentiallyDead.Contains(player.HeroId))
						PotentiallyDead.Remove(player.HeroId);
				}
			}
		}
		
		public void OnProcStart(ProcInfo proc, IPlayer player)
		{
			if (!TakeScreenshotOnProc)
				return;
			
			if (TakeSelfiesOnly && !player.IsMe)
				return;
			
			try
			{
				//var fileName = string.Format("proc_{0}_{1}_{2}_{3}.jpg", proc.Rule.Sno, info.PlayerName, info.HeroId.ToString("D", CultureInfo.InvariantCulture), Hud.Time.Now.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture));
				var fileName = string.Format("proc_{0}_{1}_{2}_{3}.jpg", Hud.Time.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture), player.BattleTagAbovePortrait, player.HeroId.ToString("D", CultureInfo.InvariantCulture), proc.Rule.Sno);
				Hud.Render.CaptureScreenToFile(SubFolderName, fileName);
			}
			catch (Exception) {}
		}
		
		public void OnProcFinish(ProcInfo proc, IPlayer player) {}
    }
	

}