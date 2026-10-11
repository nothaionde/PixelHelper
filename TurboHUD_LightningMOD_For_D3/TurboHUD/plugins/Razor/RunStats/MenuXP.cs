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

	public class MenuXP : BasePlugin, IMenuAddon/*, IAfterCollectHandler, IBeforeRenderHandler , IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public bool HideDefaultPlugin { get; set; } = true;
		
		//public new Dictionary<string, IStatTracker> Trackers { get; set; }
		//public List<IStatTracker> Trackers { get; set; }
		//public IStatTracker DefaultTracker { get; set; }
		
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
		public IFont TrackerFont { get; set; }
		
		public IBrush PlayedBrush { get; set; }
		public IBrush FullBrush { get; set; }
		public IBrush SelectedBrush { get; set; }

		private LabelRowDecorator RowUI;
		private object[,] SessionTrackers;
		private object[,] HeroTrackers;
		private object[,] AccountTrackers;
		private List<object[,]> Sections;
		private List<LabelTableDecorator> Tables;
		private int LastUpdateTick;
		private int SelectedSection = 0;
		private int SelectedRow = 0;
		private int SelectedCol = 3;
		
		private MenuPlugin Plugin;
		
        public MenuXP()
        {
            Enabled = true;
			Priority = 20;
			DockId = "TopCenter";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//DefaultTracker = Hud.Tracker.Session;
			
			SelectedBrush = Hud.Render.CreateBrush(200, 255, 255, 255, 2);
        }
		
		/*public void Customize()
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<TopExperienceStatistics>(false);
		}*/
		
		public void OnRegister(MenuPlugin plugin)
		{
			//LabelSize = Hud.Window.Size.Height * 0.11f;
			if (HideDefaultPlugin)
				Hud.TogglePlugin<TopExperienceStatistics>(false);
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 160, false, false, 135, 0, 0, 0, true);
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, 135, 0, 0, 0, true);
			LevelFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 175, 175, 175, false, false, 135, 0, 0, 0, true); //57, 137, 205
			TrackerFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 87, 137, 255, false, false, 135, 0, 0, 0, true);
			
			PlayedBrush = Hud.Render.CreateBrush(35, 255, 245, 59, 0);
			FullBrush = Hud.Render.CreateBrush(35, 150, 150, 150, 0);
			
			//ToLevel = new Dictionary<uint, string>();
			SessionTrackers = new object[,] {{Hud.Tracker.Session, "За сессию", null, null, null, null, null, null}};
			AccountTrackers = new object[,] {
				{Hud.Tracker.CurrentAccountToday, "Аккаунт сегодня (любая сложность)", null, null, null, null, null, null},
				{Hud.Game.CurrentAccountTodayOnCurrentDifficulty, "Аккаунт сегодня (текущая сложность)", null, null, null, null, null, null},
				{Hud.Tracker.CurrentAccountYesterday, "Аккаунт вчера", null, null, null, null, null, null},
				{Hud.Tracker.CurrentAccountTotal, "Аккаунт всего", null, null, null, null, null, null}
			};
			HeroTrackers = new object[,] {
				{Hud.Game.CurrentHeroToday, "Герой сегодня (любая сложность)", null, null, null, null, null, null},
				{Hud.Game.CurrentHeroTodayOnCurrentDifficulty, "Герой сегодня (текущая сложность)", null, null, null, null, null, null},
				{Hud.Game.CurrentHeroYesterday, "Герой вчера", null, null, null, null, null, null},
				{Hud.Game.CurrentHeroTotal, "Герой всего", null, null, null, null, null, null}
			};
			Sections = new List<object[,]>() {
				SessionTrackers,
				AccountTrackers,
				HeroTrackers
			};
			//Cached = new string[Trackers.Length, 4] {{}};
			
			
			//Label
			Label = new LabelStringDecorator(Hud, () => {
				//if (SelectedSection < 0)
				//	return DefaultTracker.GainedExperiencePerHourPlay.ToHumanReadable(4) + " xp/h";
				
				IStatTracker tracker = (IStatTracker)Sections[SelectedSection][SelectedRow, 0];
				
				switch (SelectedCol) {
					case 1: return ((double)tracker.GainedExperience).ToHumanReadable(5) + " xp";
					case 2: return plugin.MillisecondsToString(tracker.PlayElapsedMilliseconds);
					case 3: return tracker.GainedExperiencePerHourPlay.ToHumanReadable(5) + " xp/h"; //ValueToString(tracker.GainedExperiencePerHourPlay, ValueFormat.ShortNumber) + " xp/h";
					case 4: return plugin.MillisecondsToString(tracker.ElapsedMilliseconds);
					case 5: return tracker.GainedExperiencePerHourFull.ToHumanReadable(5) + " xp/h"; //ValueToString(tracker.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h";
					case 6: return plugin.MillisecondsToString(tracker.TownElapsedMilliseconds);
				}
				
				return "--";
			}) {Font = TextFont, FuncCacheDuration = 0.25f};
			/*new LabelRowDecorator(Hud,
				//new LabelStringDecorator(Hud, "[P]") {Font = TrackerFont, SpacingRight = 2},
				new LabelStringDecorator(Hud, () => DefaultTracker.GainedExperiencePerHourPlay.ToHumanReadable(4) + " xp/h") {Font = TextFont, FuncCacheDuration = 2}
			);*/ //{Hint = null}; //plugin.CreateHint("Session xp gain rate (Played)")};
			LabelHint = plugin.CreateHint("Experience Gain");
			
			//Row
			RowUI = new LabelRowDecorator(Hud,
					//paragon indicator
					//new LabelStringDecorator(Hud) {Font = LevelFont, SpacingLeft = 5, SpacingRight = 5},
					//tracker name
					new LabelStringDecorator(Hud) {Font = TrackerFont, Alignment = HorizontalAlign.Right, SpacingRight = 5},
					//experience gained
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Полученный опыт"), Font = LevelFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 2},
					//play time
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Игровое время"), Font = TimeFont, BackgroundBrush = PlayedBrush, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 2},
					//xp rate play
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Опыт (без учета города)"), Font = TextFont, BackgroundBrush = PlayedBrush, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 2},
					//full time
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Время игры"), Font = TimeFont, Alignment = HorizontalAlign.Left, BackgroundBrush = FullBrush, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 2},
					//xp rate full
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Опыт (с учетом города)"), Font = TextFont, BackgroundBrush = FullBrush, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 2},
					//town time
 					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Время в городе"), Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2}
				) {Alignment = HorizontalAlign.Left};
			
			//generate all of the section displays
			Tables = new List<LabelTableDecorator>();
			for (int i = 0; i < Sections.Count; ++i)
			{
				Tables.Add(new LabelTableDecorator(Hud, RowUI) {
					Alignment = HorizontalAlign.Left,
					BackgroundBrush = plugin.BgBrush, 
					HoveredBrush = plugin.HighlightBrush,
					SpacingLeft = 10,
					SpacingRight = 10,
					FillWidth = false, //true,
					OnClick = GenerateClickFunc(i), //onclickFunc, //selectLabelDisplay((LabelTableDecorator)label, i), //Array.IndexOf(Sections, SessionTrackers)),
					OnFillRow = GenerateFillFunc(i) //onfillFunc//(row, index) => rowFunc(i, row, index) //SessionTrackers
				});
			}
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ПОЛУЧЕНЫЙ ОПЫТ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						//plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				//new LabelStringDecorator(Hud, "Current Session") {Font = plugin.TitleFont},
				Tables[0], //TableUI,
				new LabelStringDecorator(Hud, "ТЕКУЩИЙ ГЕРОЙ") {Font = plugin.TitleFont, BackgroundBrush = plugin.BgBrushAlt, SpacingTop = 5, SpacingBottom = 3},
				Tables[1], //TableUI2,
				new LabelStringDecorator(Hud, "ТЕКУЩИЙ АККАУНТ") {Font = plugin.TitleFont, BackgroundBrush = plugin.BgBrushAlt, SpacingTop = 5, SpacingBottom = 3},
				Tables[2] //TableUI3
			) {
				OnBeforeRender = (label) => {
					//update the tracker cache at most once a second
					int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
					if (diff < 0 || diff > 60) //1 second
					{
						foreach (var section in Sections)
						{
							for (int i = 0; i < section.GetLength(0); ++i) //object[] cached in SessionTrackers)
							{
								IStatTracker tracker = (IStatTracker)section[i, 0];
								section[i, 2] = ((double)tracker.GainedExperience).ToHumanReadable(5) + " xp"; //ValueToString(tracker.GainedExperience, ValueFormat.LongNumber) + " xp";
								section[i, 3] = plugin.MillisecondsToString(tracker.PlayElapsedMilliseconds);
								section[i, 4] = tracker.GainedExperiencePerHourPlay.ToHumanReadable(5) + " xp/h"; //ValueToString(tracker.GainedExperiencePerHourPlay, ValueFormat.ShortNumber) + " xp/h";
								section[i, 5] = plugin.MillisecondsToString(tracker.ElapsedMilliseconds);
								section[i, 6] = tracker.GainedExperiencePerHourFull.ToHumanReadable(5) + " xp/h"; //ValueToString(tracker.GainedExperiencePerHourFull, ValueFormat.ShortNumber) + " xp/h";
								section[i, 7] = plugin.MillisecondsToString(tracker.TownElapsedMilliseconds);
							}
						}
						
						LastUpdateTick = Hud.Game.CurrentGameTick;
					}
					
					//sync table widths
					if (Tables.Count > 0 && Tables[0].RowWidths is object)
					{
						float[] widths = new float[RowUI.Labels.Count];
						foreach (var table in Tables)
						{
							for (int i = 0; i < RowUI.Labels.Count; ++i)
							{
								if (widths[i] < table.RowWidths[i])
									widths[i] = table.RowWidths[i];
							}
						}
						
						foreach (var table in Tables)
						{
							for (int i = 0; i < RowUI.Labels.Count; ++i)
								table.RowWidths[i] = widths[i];
						}
					}					
					
					return true;
				}
			};
			
			Plugin = plugin;
			
			//Load config
			if (!string.IsNullOrWhiteSpace(Config))
			{
				string[] infos = Config.Split(',');
				if (infos.Length > 2)
				{
					if (!int.TryParse(infos[0], out int section))
						return;
					if (section < 0 || section >= Sections.Count)
						return;

					if (!int.TryParse(infos[1], out int row))
						return;
					if (row >= RowUI.Labels.Count)
						return;
					
					if (!int.TryParse(infos[2], out int col))
						return;
					if (col < 1 || col >= RowUI.Labels.Count)
						return;
					
					SelectedSection = section;
					SelectedRow = row;
					SelectedCol = col;
					
					((LabelStringDecorator)Label).Font = ((LabelStringDecorator)RowUI.Labels[SelectedCol]).Font;
				}
			}
		}
		
		private Action<ILabelDecorator> GenerateClickFunc(int section)
		{
			return (label) => {//object[,] trackers) {
				LabelTableDecorator table = (LabelTableDecorator)label;
				
				if (table.HoveredCol > 0)
				{
					SelectedSection = section; //Array.IndexOf(Sections, trackers);
					SelectedRow = table.HoveredRow;
					SelectedCol = table.HoveredCol;
					
					((LabelStringDecorator)Label).Font = ((LabelStringDecorator)RowUI.Labels[SelectedCol]).Font;
					
					string config = string.Format("{0},{1},{2}", SelectedSection, SelectedRow, SelectedCol);
					if (config != Config)
					{
						Config = config;
						Plugin.Save();
					}
				}
			};
		}
		
		private Func<LabelRowDecorator, int, bool> GenerateFillFunc(int section)
		{
			return (row, index) => {
				object[,] trackers = Sections[section];
				if (index >= trackers.GetLength(0))
					return false;
				
				IStatTracker tracker = (IStatTracker)trackers[index, 0];
				
				//name
				LabelStringDecorator label = (LabelStringDecorator)row.Labels[0];
				label.StaticText = (string)trackers[index, 1];
				
				label = (LabelStringDecorator)row.Labels[1];
				label.StaticText = (string)trackers[index, 2]; //ValueToString(tracker.GainedExperience, ValueFormat.LongNumber) + "xp";
				label.BorderBrush = null;
				
				label = (LabelStringDecorator)row.Labels[2];
				label.StaticText = (string)trackers[index, 3];
				label.BorderBrush = null;
				
				label = (LabelStringDecorator)row.Labels[3];
				label.StaticText = (string)trackers[index, 4];
				label.BorderBrush = null;

				label = (LabelStringDecorator)row.Labels[4];
				label.StaticText = (string)trackers[index, 5];
				label.BorderBrush = null;

				label = (LabelStringDecorator)row.Labels[5];
				label.StaticText = (string)trackers[index, 6];
				label.BorderBrush = null;
				
				label = (LabelStringDecorator)row.Labels[6];
				label.StaticText = (string)trackers[index, 7];
				label.BorderBrush = null;
				
				if (section == SelectedSection && index == SelectedRow)
				{
					label = (LabelStringDecorator)row.Labels[SelectedCol];
					label.BorderBrush = SelectedBrush;
				}

				return true;
			};
		}
	}
}