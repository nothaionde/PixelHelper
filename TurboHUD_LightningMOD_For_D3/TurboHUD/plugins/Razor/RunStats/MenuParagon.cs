namespace Turbo.Plugins.Razor.RunStats
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Log;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;
	using Turbo.Plugins.Razor.Util;

	public class MenuParagon : BasePlugin, IMenuAddon, IAfterCollectHandler//, ICustomizer /*, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public bool HideDefaultPlugin { get; set; } = true;
		
		public uint[] LevelIncrements { get; set; } = new uint[] { 1, 2, 5, 10, 20, 50, 100, 250, 500, 1000 };
		
		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public IFont TextFont { get; set; }
		public IFont TimeFont { get; set; }
		public IFont LevelFont { get; set; }
		public IFont ParagonFont { get; set; }
		public IFont GreenFont { get; set; }
		public IFont RateFont { get; set; }
		public IBrush SelectedBrush { get; set; }
		
		//private Dictionary<uint, string> ToLevel;
		private Dictionary<uint, uint> StartingParagon = new Dictionary<uint, uint>();
		private Dictionary<uint, uint> LastSeenParagon = new Dictionary<uint, uint>();
		private LabelTableDecorator TableUI;
		private string[,] Cached;
		private LabelColumnDecorator TrackerOptionsUI;
		private IStatTracker SelectedTracker;
		private string SelectedHint;
		//private long[] XPToLevel;
		//private string[] TimeToLevel;
		private uint LastLevelSeen;
		private int LastUpdateTick;
		private MenuPlugin Plugin;
		
        public MenuParagon()
        {
            Enabled = true;
			Priority = 10;
			DockId = "TopCenter";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		/*public void Customize()
		{
			
		}*/
		
		public void OnRegister(MenuPlugin plugin)
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<TopExperienceStatistics>(false);
			
			Plugin = plugin;
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 175, 175, 175, false, false, 135, 0, 0, 0, true);
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, 135, 0, 0, 0, true);
			LevelFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 87, 137, 255, false, false, 135, 0, 0, 0, true); //57, 137, 205
			ParagonFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 255, 255, 255, true, false, 135, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, 135, 0, 0, 0, true);
			RateFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 160, false, false, 135, 0, 0, 0, true);
			
			//XPToLevel = new long[LevelIncrements.Length];
			//TimeToLevel = new string[LevelIncrements.Length];
			Cached = new string[LevelIncrements.Length, 4];
			//SelectedTracker = Hud.Game.CurrentHeroToday;
			SelectedBrush = Hud.Render.CreateBrush(55, 150, 150, 150, 0); //plugin.BgBrushAlt; //
			
			//Label
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => {
					bool maxed = Hud.Game.Me.CurrentLevelNormal == Hud.Game.Me.CurrentLevelNormalCap;
					((LabelRowDecorator)Label).Labels[1].Enabled = maxed;

					if (maxed)
					{
						//if (Label.Hint is object)
						//	((LabelStringDecorator)Label.Hint).StaticText = "Paragon Level";
						return Hud.Game.Me.CurrentLevelParagonDouble > 0 ? Hud.Game.Me.CurrentLevelParagonDouble.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) : Hud.Game.Me.CurrentLevelParagon.ToString();
					}
					
					//if (Label.Hint is object)
					//	((LabelStringDecorator)Label.Hint).StaticText = "Character Level";
					return Hud.Game.Me.CurrentLevelNormal.ToString();
				}) {Font = LevelFont},
				new LabelStringDecorator(Hud, "[P]") {Font = ParagonFont, SpacingLeft = 3}
			) {/*Hint = plugin.CreateHint("Level")*/};
			LabelHint = plugin.CreateHint("Время до уровня");
			
			TableUI = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					//paragon indicator
					//new LabelStringDecorator(Hud) {Font = LevelFont, SpacingLeft = 5, SpacingRight = 5},
					//incremented level
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = LevelFont},
						new LabelStringDecorator(Hud, "[P]") {Font = ParagonFont, SpacingLeft = 2}
					) {Hint = plugin.CreateHint(string.Empty), Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 2},
					//xp to level
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Оставшийся опыт"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingTop = 2, SpacingBottom = 2, SpacingLeft = 10, SpacingRight = 10},
					//time to level
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Время до уровня"), Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingTop = 2, SpacingBottom = 2, SpacingLeft = 10, SpacingRight = 10}
				)
			) {
				SpacingBottom = 5,
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				//SpacingLeft = 10,
				//SpacingRight = 10,
				//Hint = new LabelStringDecorator(Hud, "2tooltip!") {Font = TextFont},
				//OnClick = (lbl) => Hud.Sound.Speak("2"),
				FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= LevelIncrements.Length) //TimeToLevel.Length)
						return false;
					
					//incremented level
					LabelStringDecorator label = (LabelStringDecorator)((LabelRowDecorator)row.Labels[0]).Labels[1];
					label.Enabled = Cached[index, 1] is object; //Hud.Game.Me.CurrentLevelNormal == Hud.Game.Me.CurrentLevelNormalCap;
					
					label = (LabelStringDecorator)((LabelRowDecorator)row.Labels[0]).Labels[0];
					//uint lvl = LastLevelSeen + LevelIncrements[index];
					//if (lvl < Hud.Game.Me.CurrentLevelNormalCap)
						label.StaticText = Cached[index, 0]; //lvl.ToString();
					//else
					//	label.StaticText = (lvl - Hud.Game.Me.CurrentLevelNormalCap).ToString();
					//if (label.Hint is object)
					//	((LabelStringDecorator)label.Hint).StaticText = "+" + LevelIncrements[index];
					((LabelStringDecorator)row.Labels[0].Hint).StaticText = "+" + LevelIncrements[index];
					
					//xp to level
					label = (LabelStringDecorator)row.Labels[1];
					//long xp = XPToLevel[index];
					label.StaticText = Cached[index, 2]; //ValueToString(Hud.Game.Me.ParagonTotalExp, ValueFormat.LongNumber) + " xp"; //ValueToString(xp, xp < 1000000000 ? ValueFormat.NormalNumberNoDecimal : ValueFormat.LongNumber);
					//ExpToParagonLevel(Hud.Game.Me.CurrentLevelParagon+1);
					
					//time to level
					label = (LabelStringDecorator)row.Labels[2];
					label.StaticText = Cached[index, 3]; //TimeToLevel[index];
					
					return true;
				}
			};
			
			TrackerOptionsUI = new LabelColumnDecorator(Hud, 
				CreateTrackerOption(Hud.Game.CurrentHeroToday, 0, "Текущий герой (сегодня)"),
				CreateTrackerOption(Hud.Tracker.Session, 1, "За сессию"),
				CreateTrackerOption(Hud.Tracker.CurrentAccountToday, 2, "Сегодня")
			) {BackgroundBrush = plugin.BgBrush, HoveredBrush = plugin.HighlightBrush};
			
			//select the tracker option
			if (!string.IsNullOrWhiteSpace(Config))
			{
				if (int.TryParse(Config, out var idx) && idx < TrackerOptionsUI.Labels.Count)
					TrackerOptionsUI.Labels[idx].OnClick(TrackerOptionsUI.Labels[idx]);
			}
			else
				TrackerOptionsUI.Labels[0].OnClick(TrackerOptionsUI.Labels[0]);
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ВРЕМЯ ПОВЫШАТЬ УРОВЕНЬ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						//plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelAlignedDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "🡹") {Font = GreenFont},
						new LabelStringDecorator(Hud, () => StartingParagon.ContainsKey(Hud.Game.Me.HeroId) ? (Hud.Game.Me.CurrentLevelParagon - StartingParagon[Hud.Game.Me.HeroId]).ToString() : "0") {Font = TimeFont}
					) {Hint = plugin.CreateHint("Парагона получено"), Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10},
					//new LabelStringDecorator(Hud, "XP Rate: ") {Font = TimeFont},
					new LabelExpandDecorator(Hud,
						new LabelStringDecorator(Hud, () => "@ " + SelectedTracker.GainedExperiencePerHourPlay.ToHumanReadable(4) + " xp/h ▸") {Hint = plugin.CreateHint(() => SelectedHint), Font = RateFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10},
						TrackerOptionsUI
					) {Alignment = HorizontalAlign.Right}
				) {BackgroundBrush = plugin.BgBrush, SpacingBottom = 4}, //BackgroundBrush = plugin.BgBrush, 
				TableUI
			) {
				OnBeforeRender = (label) => {
					if (Cached == null)
						return false;
					
					try {
						IPlayer me = Hud.Game.Me;
						uint current = me.CurrentLevelNormal < me.CurrentLevelNormalCap ? me.CurrentLevelNormal : Hud.Game.Me.CurrentLevelParagon; //me.CurrentLevelNormal;
						long xpTotal = 0;
						double xph = SelectedTracker.GainedExperiencePerHourPlay; //Hud.Game.CurrentHeroToday.GainedExperiencePerHourPlay;
						
						int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
						if (LastLevelSeen != current || diff < 0 || diff > 360) //5 seconds
						{
							long xpToLevel = 0;
							long xpToLevelCap = me.CurrentLevelNormal < me.CurrentLevelNormalCap ? XpInfo.LevelXpTable[me.CurrentLevelNormalCap] - XpInfo.LevelXpTable[current+1] + Hud.Sno.GetExpToNextLevel(me) : 0;
							
							for (int i = 0; i < LevelIncrements.Length; ++i)
							{
								if (me.CurrentLevelNormal < me.CurrentLevelNormalCap) //currently below 70
								{
									var next = me.CurrentLevelNormal + LevelIncrements[i];
									if (next > me.CurrentLevelNormalCap)
									{
										xpToLevel = xpToLevelCap;
										next -= me.CurrentLevelNormalCap;
										xpToLevel += Hud.Sno.TotalParagonExperienceRequired(next); //- me.ParagonTotalExp;
										Cached[i,0] = next.ToString();
										Cached[i,1] = string.Empty;
									}
									else
									{
										xpToLevel = XpInfo.LevelXpTable[next] - XpInfo.LevelXpTable[current+1] + Hud.Sno.GetExpToNextLevel(me);
										Cached[i,0] = next.ToString();
										Cached[i,1] = null;
									}
								}
								else //currently max level and gaining paragon
								{
									var next = Hud.Game.Me.CurrentLevelParagon + LevelIncrements[i];
									xpToLevel = Hud.Sno.TotalParagonExperienceRequired(next) - me.ParagonTotalExp;
									Cached[i,0] = next.ToString();
									Cached[i,1] = string.Empty;
								}
								
								Cached[i,2] = ((double)xpToLevel).ToHumanReadable(5) + " xp";
								
								if (xph < 1)
									//TimeToLevel[i] = "∞";
									Cached[i,3] = "∞";
								else
								{
									try {
										TimeSpan timeRequired = TimeSpan.FromHours(xpToLevel / xph);
										//var ticks = Convert.ToInt64(Math.Ceiling(hours * 60.0d * 60.0d * 1000.0d * TimeSpan.TicksPerMillisecond));
										//text += ValueToString(ticks, ValueFormat.LongTimeNoSeconds);
										//text += hours + "h";
										if (timeRequired.Days > 0)
											//TimeToLevel[i] = ValueToString(timeRequired.Days, ValueFormat.NormalNumberNoDecimal) + timeRequired.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
											Cached[i,3] = ValueToString(timeRequired.Days, ValueFormat.NormalNumberNoDecimal) + timeRequired.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
										else if (timeRequired.Hours > 0)
											//TimeToLevel[i] = timeRequired.ToString("h'h 'mm'm 'ss's'");
											Cached[i,3] = timeRequired.ToString("h'h 'mm'm 'ss's'");
										else if (timeRequired.Minutes > 0)
											//TimeToLevel[i] = timeRequired.ToString("m'm 'ss's'");
											Cached[i,3] = timeRequired.ToString("m'm 'ss's'");
										else 
											//TimeToLevel[i] = timeRequired.ToString("%s's'");
											Cached[i,3] = timeRequired.ToString("%s's'");
									} catch (Exception) {
										//TimeToLevel[i] = "∞";
										Cached[i,3] = "∞";
									}
									
									LastUpdateTick = Hud.Game.CurrentGameTick;
								}
							}
							
							LastLevelSeen = current;
						}
					}
					catch (Exception e) {
						Hud.TextLog.Log("_MenuParagon", "exception occurred in area: " + Hud.Game.Me.SnoArea?.NameLocalized + ", level: " + Hud.Game.Me.CurrentLevelNormal + "(" + Hud.Game.Me.CurrentLevelParagon + ")\n" + e.ToString() + "\n\n", true, true);
					}
					
					return true;
				}
			};
			
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			foreach (IPlayer player in Hud.Game.Players)
			{
				if (!StartingParagon.ContainsKey(player.HeroId))
				{
					StartingParagon[player.HeroId] = player.CurrentLevelParagon;
					LastSeenParagon[player.HeroId] = player.CurrentLevelParagon;
				}
				else if (!LastSeenParagon.ContainsKey(player.HeroId) || LastSeenParagon[player.HeroId] != player.CurrentLevelParagon)
				{
					//level up event
					//if (player.CurrentLevelParagon - LastSeenParagon[player.HeroId])
						
					LastSeenParagon[player.HeroId] = player.CurrentLevelParagon;
				}
			}

			// if (!StartingParagon.ContainsKey(Hud.Game.Me.HeroId))
				// StartingParagon[Hud.Game.Me.HeroId] = Hud.Game.Me.CurrentLevelParagon;
			
			/*IPlayer me = Hud.Game.Me;
			uint current = me.CurrentLevelNormal;
			long xpTotal = 0;
			double xph = Hud.Game.CurrentHeroToday.GainedExperiencePerHourPlay;
			
			if (me.CurrentLevelNormal < me.CurrentLevelNormalCap)
				xpTotal = (long)(XpInfo.LevelXpTable[current+1] - Hud.Sno.GetExpToNextLevel(me));
			else
			{
				current += me.CurrentLevelParagon;
				xpTotal = me.ParagonTotalExp; //Hud.Sno.TotalParagonExperienceRequired(me.CurrentLevelParagon) - xpTotal;
			}

			if (LastLevelSeen == current)
			{
				int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
				if (diff >= 0 || diff < 360) //5 seconds
					return;
			}
			
			for (int i = 0; i < LevelIncrements.Length; ++i)
			{
				long xpRemaining = 0;
				uint next = current + LevelIncrements[i];
				if (next <= me.CurrentLevelNormalCap)
				{
					xpRemaining = XpInfo.LevelXpTable[next] - xpTotal;
				}
				else
				{
					uint plvl = next - me.CurrentLevelNormalCap;
					xpRemaining = Hud.Sno.TotalParagonExperienceRequired(plvl) - xpTotal;
					
					//if (current < me.CurrentLevelNormalCap)
					//	xpRemaining += XpInfo.LevelXpTable[me.CurrentLevelNormalCap]; //- xpTotal;
				}
				
				//XPToLevel[i] = xpRemaining;
				Cached[i,0] = ValueToString(Hud.Game.Me.ParagonTotalExp, ValueFormat.LongNumber); //ValueToString(xpRemaining, xpRemaining < 1000000000 ? ValueFormat.NormalNumberNoDecimal : ValueFormat.LongNumber);
				
				if (xph < 1)
					//TimeToLevel[i] = "∞";
					Cached[i,1] = "∞";
				else
				{
					try {
						TimeSpan timeRequired = TimeSpan.FromHours(xpRemaining / xph);
						//var ticks = Convert.ToInt64(Math.Ceiling(hours * 60.0d * 60.0d * 1000.0d * TimeSpan.TicksPerMillisecond));
						//text += ValueToString(ticks, ValueFormat.LongTimeNoSeconds);
						//text += hours + "h";
						if (timeRequired.Days > 0)
							//TimeToLevel[i] = ValueToString(timeRequired.Days, ValueFormat.NormalNumberNoDecimal) + timeRequired.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
							Cached[i,1] = ValueToString(timeRequired.Days, ValueFormat.NormalNumberNoDecimal) + timeRequired.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
						else if (timeRequired.Hours > 0)
							//TimeToLevel[i] = timeRequired.ToString("h'h 'mm'm 'ss's'");
							Cached[i,1] = timeRequired.ToString("h'h 'mm'm 'ss's'");
						else if (timeRequired.Minutes > 0)
							//TimeToLevel[i] = timeRequired.ToString("m'm 'ss's'");
							Cached[i,1] = timeRequired.ToString("m'm 'ss's'");
						else 
							//TimeToLevel[i] = timeRequired.ToString("%s's'");
							Cached[i,1] = timeRequired.ToString("%s's'");
					} catch (Exception) {
						//TimeToLevel[i] = "∞";
						Cached[i,1] = "∞";
					}
					
					LastUpdateTick = Hud.Game.CurrentGameTick;
				}
			}
			
			LastLevelSeen = current;*/
		}
		
		//borrowed from TopExperienceStatistics.cs
		public string ExpToParagonLevel(uint paragonLevel)
        {
            if (paragonLevel > Hud.Game.Me.CurrentLevelParagon)
            {
                var xpRequired = Hud.Sno.TotalParagonExperienceRequired(paragonLevel);
                var xpRemaining = xpRequired - Hud.Game.Me.ParagonTotalExp;
                return ValueToString(xpRemaining, ValueFormat.LongNumber);
            }

            return null;
        }
		
		private LabelStringDecorator CreateTrackerOption(IStatTracker tracker, int index, string name)
		{
			return new LabelStringDecorator(Hud, name) {
				OnClick = (label) => {
					
					if (SelectedTracker == tracker)//Hud.Game.CurrentHeroToday)
						return;
					
					SelectedTracker = tracker; //Hud.Game.CurrentHeroToday; 
					SelectedHint = name; //((LabelStringDecorator)label).StaticText;
					label.BackgroundBrush = SelectedBrush;
					LastUpdateTick = 0; //update cache immediately
					
					string idx = index.ToString();
					if (Config != idx)
					{
						Config = idx;
						Plugin.Save();
						
						foreach (var lbl in TrackerOptionsUI.Labels)
						{
							if (lbl != label)
								lbl.BackgroundBrush = null;
						}
					}
				}, 
				Font = RateFont, 
				Alignment = HorizontalAlign.Left, 
				SpacingLeft = 10, 
				SpacingRight = 10,
				SpacingTop = 1,
				SpacingBottom = 3,
			};
		}
		
		/*public void UpdateToLevel()
		{
			IPlayer me = Hud.Game.Me;
			uint current = me.CurrentLevelNormal;
			long xpTotal = 0;
			
			if (me.CurrentLevelNormal < me.CurrentLevelNormalCap)
				xpTotal = (long)(XpInfo.LevelXpTable[current+1] - Hud.Sno.GetExpToNextLevel(me));
			else
			{
				current += me.CurrentLevelParagon;
				xpTotal = me.ParagonTotalExp; //Hud.Sno.TotalParagonExperienceRequired(me.CurrentLevelParagon) - xpTotal;
			}
			
			if (LastLevelSeen == current)
			{
				int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
				if (diff >= 0 || diff < 360) //5 seconds
					return;
			}
				
			for (int i = 0; i < LevelIncrements.Count; ++i)
			{
				long xpRemaining = 0;
				uint next = current + LevelIncrements[i];
				if (next <= me.CurrentLevelNormalCap)
				{
					xpRemaining = XpInfo.LevelXpTable[next] - xpTotal;
				}
				else
				{
					uint plvl = next - me.CurrentLevelNormalCap;
					xpRemaining = Hud.Sno.TotalParagonExperienceRequired(plvl) - xpTotal;
					
					//if (current < me.CurrentLevelNormalCap)
					//	xpRemaining += XpInfo.LevelXpTable[me.CurrentLevelNormalCap]; //- xpTotal;
				}
				
				try {
					TimeSpan timeRequired = TimeSpan.FromHours(xpRemaining / xph);
					//var ticks = Convert.ToInt64(Math.Ceiling(hours * 60.0d * 60.0d * 1000.0d * TimeSpan.TicksPerMillisecond));
					//text += ValueToString(ticks, ValueFormat.LongTimeNoSeconds);
					//text += hours + "h";
					if (timeRequired.Days > 0)
						ToLevel[i] = ValueToString(timeRequired.Days, ValueFormat.NormalNumberNoDecimal) + timeRequired.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
					if (timeRequired.Hours > 0)
						ToLevel[i] = timeRequired.ToString("h'h 'mm'm 'ss's'");
					if (timeRequired.Minutes > 0)
						ToLevel[i] = timeRequired.ToString("m'm 'ss's'");
					else 
						ToLevel[i] = timeRequired.ToString("%s's'");
				} catch (Exception) {
					ToLevel[i] = "∞";
				}
			}
			
			LastLevelSeen = current;
			LastUpdateTick = Hud.Game.CurrentGameTick;
		}*/
	}
}