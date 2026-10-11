/*

Inspired by s4000's DAV_ExtendTextLog, this implements a delayed callback system for text logging to write data to files

Tier 2:
Since TH version 20.7.12.0, ITextLogController (Hud.TextLog.Log) doesn't work in special areas and games in Torment difficulty greater than (or equal to?) Torment 1. This allows plugins (that implement the ITextLogger interface) to register themselves to be notified when logging is possible.

Tier 3:
Logging limitations removed, delay is now time-based

*/

namespace Turbo.Plugins.Razor.Log
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	
	using Turbo.Plugins.Default;

	public class TextLogger : BasePlugin, IAfterCollectHandler
	{
		private bool once = false;
		
		public TextLogger()
		{
			Enabled = true;
		}
		
		public override void Load(IController hud)
        {
            base.Load(hud);
		}
		
		public void AfterCollect()
		{
			//if (!Hud.Game.IsInGame || (Hud.Game.SpecialArea == SpecialArea.None && (int)Hud.Game.GameDifficulty < 4))
			//{
				Hud.TextLog.NotifyLogQueue();
			//}
			
			
			/*
			//debug
			if (!once)
			{
				if (DelayedLogQueue.RelativePath is object)
				{
					once = true;
					Hud.Sound.Speak(DelayedLogQueue.RelativePath == string.Empty ? "not found" : DelayedLogQueue.RelativePath);
				}
			}*/
		}
	}
	
	public static class DelayedLogQueue
	{
		//private static List<ITextLogger> NotifyQueue { get; set; } = new List<ITextLogger>();
		private static List<Tuple<ITextLogger, DateTime>> NotifyQueue = new List<Tuple<ITextLogger, DateTime>>();
		private static string RelativePath;
		//private static Dictionary<ITextLogger, DateTime> NotifyDelay { get; set; } = new Dictionary<ITextLogger, DateTime>();
		
		//put off writing for 'delay' seconds
		public static void Queue(this ITextLogController TextLog, ITextLogger plugin, float delay = 0) //delay in seconds
		{
			//try to write it immediately
			if (delay == 0)
			{
				plugin.Log(TextLog.GetRelativePath()); //RelativePath);
				return;
			}
			
			//delayed writing
			if (!NotifyQueue.Any(t => t.Item1 == plugin)) //!NotifyQueue.Contains(plugin))
				NotifyQueue.Add(new Tuple<ITextLogger, DateTime>(plugin, DateTime.Now.AddSeconds((double)delay)));
		}
		
		//put off writing until there have been no new queue requests for at least 'delay' seconds
		public static void QueueLatest(this ITextLogController TextLog, ITextLogger plugin, float delay = 0) //delay in seconds
		{
			//try to write it immediately
			if (delay == 0)
			{
				plugin.Log(TextLog.GetRelativePath()); //RelativePath);
				return;
			}
			
			//delayed writing
			var tup = NotifyQueue.FirstOrDefault(t => t.Item1 == plugin);
			if (tup is object) //!NotifyQueue.Contains(plugin))
				NotifyQueue.Remove(tup);
			
			NotifyQueue.Add(new Tuple<ITextLogger, DateTime>(plugin, DateTime.Now.AddSeconds((double)delay)));
		}
		
		public static void NotifyLogQueue(this ITextLogController TextLog)
		{
			if (NotifyQueue.Count > 0)
			{
				//TextLog.GetRelativePath(); //sets RelativePath and returns it
				
				//foreach (ITextLogger plugin in NotifyQueue)
				List<int> removal = new List<int>();
				for (int i = 0; i < NotifyQueue.Count; ++i)
				{
					Tuple<ITextLogger, DateTime> pair = NotifyQueue[i];
					if (pair.Item2 <= DateTime.Now)
					{
						removal.Add(i);
						pair.Item1.Log(TextLog.GetRelativePath()); //RelativePath);
					}
				}

				//NotifyQueue.Clear();
				for (int i = 0; i < removal.Count; ++i)
					NotifyQueue.RemoveAt(i);
			}
		}
		
		public static string GetRelativePath(this ITextLogController TextLog)
		{
			if (RelativePath == null) //string.Empty means that path resolution was attempted and failed
			{
				RelativePath = string.Empty;
				
				string current = Directory.GetCurrentDirectory();
				foreach (string filepath in Directory.GetFiles(current, "TextLogger.cs", SearchOption.AllDirectories))
				{
					string path = Path.GetDirectoryName(filepath).Substring(current.Length);
					if (path.Contains(@"Razor\Log"))
					{
						RelativePath = path.Substring(1, path.Length - 11); //path.Remove(current.Length);
						break;
					}
				}
			}
			
			return RelativePath;
		}
	}
}