/*

Todo:
- add display for average close time (includes abandoned rifts)

*/

namespace Turbo.Plugins.Razor.RunStats
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;

	public class MenuNephalemRift : BasePlugin, IMenuAddon, IAfterCollectHandler/*, INewAreaHandler, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public bool HideWhileGreaterRiftOpen { get; set; } = true;
		
		public List<RiftInfo> History { get; private set; } = new List<RiftInfo>(); //RiftInfo
		public List<RiftInfo> Completed { get; private set; } = new List<RiftInfo>();
		public List<RiftInfo> Abandoned { get; private set; } = new List<RiftInfo>();
		public int HistoryShown { get; set; } = 15;
		private int CountAbandoned = 0;
		private int CountCompleted = 0;
		//private int CountGelatinousGoblins = 0;
		//private int CountGoblinPacks = 0;
		//private int CountRainbowGoblins = 0;
		private int AvgCompletionTicks = 0;
		private float AvgClosedPerHour = 0;

		public string TextUnknown { get; set; } = "??";
		public string TextIncomplete { get; set; } = "--";
		//public int Count { get; private set; } = 0;
		
		public ILabelDecorator HintShowCompleted;
		public ILabelDecorator HintShowAbandoned;
		public ILabelDecorator HintShowAll;
		
		public string Id { get; set; } //will be set by MenuPlugin
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; } //which dock does this plugin start in?
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }
		
		public IFont LegendaryFont { get; set; }
		public IFont AncientFont { get; set; }
		public IFont RedFont { get; set; }
		public IFont SetFont { get; set; }
		public IFont GreenFont { get; set; }
		public IFont DefaultFont { get; set; }
		public IFont XPFont { get; set; }
		public IFont DifficultyFont { get; set; }
		//public IFont HintFont { get; set; }
		public IFont CountFont { get; set; }
		public IFont TimeFont { get; set; }
		public IFont TitleFont { get; set; }
		public IBrush RedBrush { get; set; }
		public IBrush GreenBrush { get; set; }
		public IBrush ProgressBrush { get; set; }
		
		public RiftInfo RiftOpen { get; private set; } = null;
		private RiftInfo RiftClosed = null;
		private uint[] GreaterRiftQuestIds = new uint[] {13, 16, 34, 46};
		private double LastXPSeen;
		//private int AvgTurnaroundTicks = 0;
		//private MenuAlignedDecorator SummaryUI;
		//private MenuTableDecorator TableUI;
		private bool ShowCompleted = false;
		private bool ShowAbandoned = false;
		private bool GreaterRiftOpen = false;
		private LabelTableDecorator TableUI;
		private ILabelDecoratorCollection SummaryUI;
		private List<RiftInfo> View; //the currently displayed data set
		private IWatch Timer;
		private long LastUpdate = 0; //milliseconds
		private int UpdateInterval = 5000; //milliseconds
		
		private MenuPlugin Plugin;
		
		//borrowed from RNN's OtherShrinePlugin
		private Dictionary<ActorSnoEnum, ShrineType> ActorSnoEnumToShrineType = new Dictionary<ActorSnoEnum,ShrineType>
		{																						
			// _shrine_global = 135384, // _shrine_global_glow /*156680*/
			{ ActorSnoEnum._poolofreflection /*373463*/, ShrineType.PoolOfReflection },
			{ ActorSnoEnum._healthwell_global /*138989*/, ShrineType.HealingWell },
			
			{ ActorSnoEnum._x1_lr_shrine_damage /*330695*/, ShrineType.PowerPylon },
			{ ActorSnoEnum._x1_lr_shrine_electrified /*330696*/, ShrineType.ConduitPylon },
			{ ActorSnoEnum._x1_lr_shrine_infinite_casting /*330697*/, ShrineType.ChannelingPylon },
			{ ActorSnoEnum._x1_lr_shrine_invulnerable /*330698*/, ShrineType.ShieldPylon },
			{ ActorSnoEnum._x1_lr_shrine_run_speed /*330699*/, ShrineType.SpeedPylon },

			{ ActorSnoEnum._shrine_global_blessed /*176074*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._shrine_global_enlightened /*176075*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._shrine_global_fortune /*176076*/, ShrineType.FortuneShrine },
			{ ActorSnoEnum._shrine_global_frenzied /*176077*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._shrine_global_hoarder /*260346*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._shrine_global_reloaded /*260347*/, ShrineType.EmpoweredShrine },
			
			{ ActorSnoEnum._a4_heaven_shrine_global_blessed /*225025*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_enlightened /*225030*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_fortune /*225027*/, ShrineType.FortuneShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_frenzied /*225028*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_hoarder /*260344*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_reloaded /*260345*/, ShrineType.EmpoweredShrine },
			
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_blessed /*225261*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_enlightened /*225262*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_fortune /*225263*/, ShrineType.FortuneShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_frenzied /*225266*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_hoarder /*260342*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._a4_heaven_shrine_global_demoncorrupted_reloaded /*260343*/, ShrineType.EmpoweredShrine },

			{ ActorSnoEnum._p43_ad_shrine_global_blessed /*455251*/, ShrineType.BlessedShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_enlightened /*455252*/, ShrineType.EnlightenedShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_frenzied /*455253*/, ShrineType.FrenziedShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_hoarder /*455254*/, ShrineType.FleetingShrine },
			{ ActorSnoEnum._p43_ad_shrine_global_reloaded /*455256*/, ShrineType.EmpoweredShrine },

			{ ActorSnoEnum._shrine_treasuregoblin /*269349*/, ShrineType.BanditShrine },
			{ ActorSnoEnum._a4_heaven_shrine_treasuregoblin /*434409*/, ShrineType.BanditShrine },
			{ ActorSnoEnum._p43_ad_shrine_treasuregoblin /*455256*/, ShrineType.BanditShrine }
		};
		
		public class RiftInfo {
			public int StartedAt { get; set; } //rift opened
			public int CompletedAt { get; set; } //boss killed
			public int EndedAt { get; set; } //rift closed
			public int LastSeenAt { get; set; }
			public uint LastSeenStep { get; set; }
			public double Progress { get; set; }
			public int GuardianSpawnedAt { get; set; }
			public int GuardianKilledAt { get; set; }
			public string GuardianName { get; set; }
			public string GuardianLocationName { get; set; }
			//public double ExperienceAtStart { get; set; }
			public double ExperienceGained { get; set; }
			public DateTime Timestamp { get; set; }
			public GameDifficulty Difficulty { get; set; }
			public Dictionary<string, RiftShrineInfo> Shrines { get; set; }
			public bool KillTimeInaccurate { get; set; } = false;
			public int Keys { get; set; }
			
			public RiftInfo()
			{
				Shrines = new Dictionary<string, RiftShrineInfo>();
			}
		}
		
		public class RiftShrineInfo {
			public string Id { get; set; } //IMarker.Id
			public ShrineType? Type { get; set; }
			public int SeenTick { get; set; }
			public double RiftPercentage { get; set; }
			public string TypeName { get; set; }
			public string LocationName { get; set; }
			
			public RiftShrineInfo(IMarker marker, ShrineType? type, string name, int seen, double progress, string loc)
			{
				Id = marker.Id;
				SeenTick = seen;
				TypeName = name;
				LocationName = loc;
				RiftPercentage = progress;
				Type = type; //ActorSnoEnumToShrineType[marker.SnoActor.Sno];
			}
		}
		
        public MenuNephalemRift()
        {
            Enabled = true;
			Priority = 60;
			DockId = "BottomRight";
        }

        /*public override void Load(IController hud)
        {
            base.Load(hud);
        }*/
		
		/*public void Customize()
		{
			Hud.TogglePlugin<TopExperienceStatistics>(false);
		}*/
		
		public void OnRegister(MenuPlugin plugin)
		{
			Plugin = plugin;
			View = History;
			
			LegendaryFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 191, 100, 47, false, false, true); //191, 100, 47
			AncientFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 120, 0, false, false, true);
			RedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true);
			SetFont = Hud.Render.CreateFont("arial", plugin.FontSize, 190, 0, 255, 0, false, false, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 190, 255, 255, 255, false, false, true);
			DefaultFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 140, 0, false, false, true);
			XPFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 191, 255, false, false, true); //245, 218, 66
			DifficultyFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 235, 211, 52, false, false, true); //255, 30, 30
			//HintFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 255, 30, true, false, true);
			CountFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 245, 206, 66, false, false, true);
			TitleFont = Hud.Render.CreateFont("tahoma", 7f, 200, 211, 228, 255, false, false, true);
			
			RedBrush = Hud.Render.CreateBrush(100, 94, 5, 5, 0); //122, 11, 11
			GreenBrush = Hud.Render.CreateBrush(100, 0, 155, 5, 0);
			ProgressBrush = Hud.Render.CreateBrush(100, 235, 211, 52, 0);
			
			Timer = Hud.Time.CreateWatch();
			
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => AvgClosedPerHour.ToString("0.##") + "/ч"/*{
					if (RiftOpen is object && RiftOpen.StartedAt > 0) //show time elapsed
					{
						var time = TimeSpan.FromSeconds((double)(RiftOpen.LastSeenAt - RiftOpen.StartedAt) / 60d);
						if (time.TotalSeconds < 60) 
							return time.TotalSeconds.ToString("F0") + "с";
						if (time.TotalMinutes < 60)
							return time.ToString(@"mm\:ss");
						if (time.TotalHours < 24)
							return time.ToString(@"h\:mm\:ss");

						return time.ToString(@"d\:hh\:mm\:ss");
					}

					//show rifts per hour
					return ValueToString(CountCompleted, ValueFormat.NormalNumberNoDecimal);
				}*/) {Font = DifficultyFont},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture("Marker_Portal_Gold")) {TextureHeight = 32, ContentHeight = plugin.MenuHeight, ContentWidth = 16}//,
				//new MenuStringDecorator("100%") {TextFont = GreenFont, SpacingLeft = -5, SpacingRight = 10}
			); //{SpacingLeft = 15, SpacingRight = 5}; //icon spacing offset
			LabelHint = plugin.CreateHint("История Нефалемских Порталов");
			
			TableUI = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					//timestamp
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("First Seen"), Font = TimeFont, SpacingLeft = 6, SpacingRight = 6},
					//game difficulty
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Сложность"), Font = DifficultyFont, SpacingLeft = 6, SpacingRight = 6},
					//time spent obtaining 100% progress
					new LabelProgressBarDecorator(Hud, 
						new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Прогресс"), Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 6, SpacingRight = 6}
					) {BarBrush = ProgressBrush, BackgroundBrush = RedBrush},
					//shrines
					new LabelRowDecorator(Hud) {Alignment = HorizontalAlign.Left, SpacingLeft = 6, SpacingRight = 6},
					//boss icon
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture("WaypointMap_MarkerBoss")) {Hint = plugin.CreateHint("Босс"), TextureWidth = 30, TextureHeight = 30, ContentHeight = plugin.MenuHeight, SpacingLeft = 2, SpacingRight = -4},
					//boss name
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Имя Босса"), Font = AncientFont, Alignment = HorizontalAlign.Left, SpacingLeft = 6, SpacingRight = 6},
					//boss kill time
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Время Убийства Босса"), Font = AncientFont, Alignment = HorizontalAlign.Left, SpacingRight = 6},
					//completion time
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Время Завершения"), Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 6, SpacingRight = 6},
					//close time is always fixed at 30 seconds for nephalem rifts
					//new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Close Time"), Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 6, SpacingRight = 6},
					//xp gained
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Опыта Получено"), Font = XPFont, Alignment = HorizontalAlign.Left, SpacingLeft = 6, SpacingRight = 6}
				) {SpacingTop = 2, SpacingBottom = 2}
			) {
				BackgroundBrush = plugin.BgBrush, 
				SpacingLeft = 10, 
				SpacingRight = 10,
				HoveredBrush = plugin.HighlightBrush,
				//Hint = new LabelStringDecorator(Hud, "2tooltip!") {Font = TextFont},
				//OnClick = (lbl) => Hud.Sound.Speak("2"),
				FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= View.Count)
						return false;
							
					RiftInfo rift = View[index];
					/*if (RiftOpen is object && rift == RiftOpen)
						row.BackgroundBrush = GreenBrush;
					else
						row.BackgroundBrush = (rift.EndedAt == -1 ? RedBrush : null);*/
					if (RiftOpen is object && rift != RiftOpen && RiftOpen.CompletedAt < 1)
					{
						row.Enabled = false;
						return true;
					}
					
					row.Enabled = true;

					//highlight abandoned rift entries
					row.BackgroundBrush = (rift.EndedAt == -1 ? RedBrush : null);
					
					//timestamp
					TimeSpan elapsed = Hud.Time.Now - rift.Timestamp;
					string time;
					if (elapsed.TotalSeconds < 60)
						time = elapsed.TotalSeconds.ToString("F0") + "с назад";
					else if (elapsed.TotalMinutes < 10)
						time = elapsed.TotalMinutes.ToString("F0") + "м назад";
					else
						time = rift.Timestamp.ToString("hh:mm tt");
					((LabelStringDecorator)row.Labels[0]).StaticText = time;

					//game difficulty
					((LabelStringDecorator)row.Labels[1]).StaticText = rift.Difficulty.ToString().ToUpper();
					
					//progress time
					var progressBar = (LabelProgressBarDecorator)row.Labels[2]; //(LabelStringDecorator)row.Labels[2];
					var progressUI = (LabelStringDecorator)progressBar.Labels[0]; //(LabelStringDecorator)row.Labels[2];
					((LabelStringDecorator)progressUI.Hint).StaticText = (rift.Progress*100).ToString("0.##") + "% Завершено"; //update hint to reflect progress value
					if (rift.GuardianSpawnedAt == 0)
					{
						progressBar.Progress = (float)rift.Progress;
						progressBar.BackgroundBrush = ProgressBrush;
						if (rift.EndedAt == -1)
						{
							progressUI.Font = RedFont;
							progressUI.StaticText = ValueToString((long)(rift.LastSeenAt - rift.StartedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
						}
						else
						{
							//in progress
							progressUI.Font = TimeFont;
							progressUI.StaticText = ValueToString((long)(rift.LastSeenAt - rift.StartedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
						}
					}
					else
					{
						progressBar.Progress = 1f;
						progressBar.BackgroundBrush = GreenBrush;
						progressUI.Font = TimeFont;
						progressUI.StaticText = ValueToString((long)(rift.GuardianSpawnedAt - rift.StartedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
					}
					
					//shrines
					var shrinesUI = ((LabelRowDecorator)row.Labels[3]).Labels;
					//shrinesUI.Clear();
					int i = 0;
					foreach (RiftShrineInfo shrine in rift.Shrines.Values)
					{
						if (i < shrinesUI.Count)
						{
							var icon = (LabelTextureDecorator)shrinesUI[i];
							icon.Enabled = true;
							
							if (!shrine.Type.HasValue) //placeholder
							{
								icon.Texture = Hud.Texture.GetTexture(218235, 0);
								icon.TextureWidth = 38;
								icon.TextureHeight = 36;
								icon.SpacingTop = 0;
								icon.ContentHeight = plugin.MenuHeight; //*0.8f; //progressUI.Height + 2;
								icon.ContentWidth = 22; //plugin.MenuHeight; // + icon.SpacingLeft + icon.SpacingRight; //22;
							}
							else
							{
								icon.Texture = GetBuffTexture(shrine.Type.Value);
								icon.TextureWidth = plugin.MenuHeight - 2;
								icon.TextureHeight = plugin.MenuHeight - 2; //*0.8f; //progressUI.Height + 2; //plugin.MenuHeight;
								icon.SpacingTop = 0;
								icon.ContentHeight = plugin.MenuHeight; //progressUI.Height + 2;
								icon.ContentWidth = plugin.MenuHeight; // + icon.SpacingLeft + icon.SpacingRight; //plugin.MenuHeight; //22;
							}
							
							((LabelStringDecorator)shrinesUI[i].Hint).StaticText = string.Format("{0}\n{1} ({2:f2}%)", shrine.TypeName, shrine.LocationName, shrine.RiftPercentage);
						}
						else
						{
							if (!shrine.Type.HasValue) //placeholder
								shrinesUI.Add(new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(218235, 0)) {Hint = plugin.CreateHint(string.Format("{0}\n{1} ({2:f2}%)", shrine.TypeName, shrine.LocationName, shrine.RiftPercentage)), TextureWidth = 37, TextureHeight = 35, ContentHeight = plugin.MenuHeight, ContentWidth = 22, SpacingLeft = 2, SpacingRight = 2/*, Alignment = HorizontalAlign.Left, SpacingTop = -5, SpacingLeft = -8, SpacingRight = -8*/});
							else
								shrinesUI.Add(new LabelTextureDecorator(Hud, GetBuffTexture(shrine.Type.Value)) {Hint = plugin.CreateHint(string.Format("{0}\n{1} ({2:f2}%)", shrine.TypeName, shrine.LocationName, shrine.RiftPercentage)), TextureWidth = plugin.MenuHeight*0.8f, TextureHeight = plugin.MenuHeight*0.8f, ContentHeight = plugin.MenuHeight, ContentWidth = plugin.MenuHeight*0.8f + 2 + 2, SpacingLeft = 2, SpacingRight = 2});
						}

						++i;
					}
					while (i < shrinesUI.Count)
					{
						shrinesUI[i].Enabled = false;
						++i;
					}
						
					
					//boss icon
					//view.Decorators[5].Height = progressUI.Height;

					//boss name
					var bossUI = (LabelStringDecorator)row.Labels[5];
					if (!string.IsNullOrEmpty(rift.GuardianName))
					{
						bossUI.StaticText = rift.GuardianName;
						((LabelStringDecorator)bossUI.Hint).StaticText = "Босс Появился в " + rift.GuardianLocationName;
					}
					else
					{
						bossUI.StaticText = (rift.EndedAt == -1 ? TextIncomplete : TextUnknown);
						((LabelStringDecorator)bossUI.Hint).StaticText = "Имя Босса";
					}
					
					//boss kill time
					progressUI = (LabelStringDecorator)row.Labels[6];
					if (rift.GuardianKilledAt == 0)
					{
						if (rift.EndedAt == -1)
						{
							progressUI.Font = RedFont;
							progressUI.StaticText = (rift.GuardianSpawnedAt == 0 ? TextIncomplete : ValueToString((long)(rift.LastSeenAt - rift.GuardianSpawnedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime));
							//progressUI.StaticText = ValueToString((long)(rift.LastSeenAt - rift.GuardianSpawnedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
						}
						else
						{
							progressUI.Font = AncientFont;
							progressUI.StaticText = (rift.GuardianSpawnedAt == 0 ? TextUnknown : ValueToString((long)(rift.LastSeenAt - rift.GuardianSpawnedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime));
						}
					}
					else
					{
						if (rift.GuardianSpawnedAt == 0)
						{
							progressUI.Font = RedFont;
							progressUI.StaticText = TextIncomplete;
						}
						else
						{
							progressUI.Font = AncientFont;
							progressUI.StaticText = ValueToString((long)(rift.GuardianKilledAt - rift.GuardianSpawnedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
						}
					}
					//progressUI.StaticText = rift.StartedAt + " -> " + rift.GuardianKilledAt;
					
					//completion time
					var completionUI = (LabelStringDecorator)row.Labels[7];
					if (rift.CompletedAt == 0)
					{
						if (rift.EndedAt == -1)
						{
							completionUI.Font = RedFont;
							completionUI.StaticText = TextIncomplete;
							/*if (rift.StartedAt == 0)
								completionUI.StaticText = TextIncomplete;
							else
								completionUI.StaticText = ValueToString((long)(rift.LastSeenAt - rift.StartedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
							*/
						}
						else
						{
							//in progress
							completionUI.Font = TimeFont;
							completionUI.StaticText = TextUnknown;
						}
					}
					else
					{
						if (rift.StartedAt == 0)
						{
							completionUI.Font = RedFont;
							completionUI.StaticText = TextIncomplete;
						}
						else
						{
							completionUI.Font = GreenFont;
							completionUI.StaticText = ValueToString((long)(rift.CompletedAt - rift.StartedAt) * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime);
						}
					}
					
					//xp gained
					((LabelStringDecorator)row.Labels[8]).StaticText = ValueToString(rift.ExperienceGained, ValueFormat.ShortNumber) + " опыта";
					
					return true;
				}
			};
			
			SummaryUI = new LabelAlignedDecorator(Hud,
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => CountCompleted.ToString()) {Font = GreenFont},
					new LabelStringDecorator(Hud, "✔️") {Font = GreenFont}
				) {Hint = plugin.CreateHint("Завершено"), OnClick = ToggleCompleted, Alignment = HorizontalAlign.Left, SpacingLeft = 5},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => CountAbandoned.ToString()) {Font = RedFont},
					new LabelStringDecorator(Hud, "❌") {Font = RedFont}
				) {Hint = plugin.CreateHint("Незавершённые"), OnClick = ToggleAbandoned, Alignment = HorizontalAlign.Left, SpacingLeft = 5},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => AvgCompletionTicks == 0 ? TextIncomplete : ValueToString((long)AvgCompletionTicks * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime)) {Font = TimeFont}, //Hint = plugin.CreateHint(() => Completed.Count == 0 ? "Average Completion Time" : "Average Completion Time ("+Completed.Count+" Rifts)")
					new LabelStringDecorator(Hud, "🕓") {Font = TimeFont}
				) {Hint = plugin.CreateHint(() => Completed.Count == 0 ? "Среднее Время Завершения" : "Среднее Время Завершения ("+Completed.Count+" Порталов)"), Alignment = HorizontalAlign.Right, SpacingLeft = 5}
			) {
				BackgroundBrush = plugin.BgBrush, 
				SpacingLeft = 10, 
				SpacingRight = 10,
				SpacingTop = 2, 
				SpacingBottom = 2,
				OnBeforeRender = (label) => RiftOpen == null || RiftOpen.CompletedAt > 0,
			};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ИСТОРИЯ НЕФАЛЕМСКИХ ПОРТАЛОВ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				(ILabelDecorator)SummaryUI,
				TableUI
			);
			
			HintShowCompleted = plugin.CreateHint("Нажмите, чтобы посмотреть Завершенные Порталы");
			HintShowAbandoned = plugin.CreateHint("Нажмите, чтобы посмотреть Незавершённые Порталы");
			HintShowAll = plugin.CreateHint("Нажмите, чтобы увидеть все Порталы");
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame) //this can be true when loading between areas
				return;
			
			UpdateCloseRate();
			
			//private List<uint> IdGR = new List<uint> {13,16,34,46};
			//private List<uint> IdNR = new List<uint> {1,3,10,5}; 
			IQuest riftQuest = Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492); // && q.State == QuestState.started //|| q.State == QuestState.completed) && GreaterRiftQuestIds.Contains(q.QuestStepId)) ;
			if (riftQuest is object)
			{
				if (GreaterRiftQuestIds.Contains(riftQuest.QuestStepId))
				{
					GreaterRiftOpen = true;
					
					if (RiftOpen is object)
						CloseRiftInfo();
					
					return;
				}
				
				if (riftQuest.State == QuestState.none)
				{
					if (RiftOpen is object)
						CloseRiftInfo();
					
					return;
				}
				
				GreaterRiftOpen = false;
				
				if (RiftOpen == null)
				{
					//don't create a new info object if the rift quest is already completed before we start
					if (riftQuest.State != QuestState.completed)
					//if (RiftClosed == null || riftQuest.CreatedOn != RiftClosed.StartedAt)
					{
						//Hud.Sound.Speak("start");
						//Hud.Sound.Speak("start " + riftQuest.State + " " + riftQuest.QuestStepId);
						
						//open new rift info object
						RiftOpen = new RiftInfo()
						{
							//StartedAt = riftQuest.CreatedOn,
							LastSeenAt = Hud.Game.CurrentGameTick,
							//LastSeenStep = riftQuest.QuestStepId,
							Timestamp = Hud.Time.Now, //todo: could calculate this more accurately
							ExperienceGained = GetExperienceGained(true),
							Difficulty = Hud.Game.GameDifficulty,
							//KillTimeInaccurate = riftQuest.QuestStepId == 3,
						};
						
						History.Add(RiftOpen);
						
						//Panel.Resize();
						
						if (History.Count > HistoryShown)
							History.RemoveAt(0);
							
						if (!Timer.IsRunning)
							Timer.Start();
					}
				}
				else
				{
					//case check: for open rift game transfer to another open rift game

					
					//debug
					//RiftOpen.GuardianKilledAt = riftQuest.CreatedOn;
					
					//find all the shrines and pylons
					if (Hud.Game.SpecialArea == SpecialArea.Rift)
					{
						foreach (var marker in Hud.Game.Markers)
						{
							if (marker.IsPylon || marker.IsShrine) //&& !RiftOpen.Shrines.ContainsKey(marker.Id) && ActorSnoEnumToShrineType.ContainsKey(marker.SnoActor.Sno))
							{
								if (!RiftOpen.Shrines.ContainsKey(marker.Id))
								{
									//Hud.Sound.Speak(Hud.Game.Me.SnoArea?.NameLocalized);
									
									if (marker.SnoActor.Sno == ActorSnoEnum._shrine_global) //placeholder
										RiftOpen.Shrines.Add(marker.Id, new RiftShrineInfo(marker, null, marker.SnoActor.NameLocalized, Hud.Game.CurrentGameTick, Hud.Game.RiftPercentage, Hud.Game.Me.SnoArea?.NameLocalized));
									else
										RiftOpen.Shrines.Add(marker.Id, new RiftShrineInfo(marker, ActorSnoEnumToShrineType[marker.SnoActor.Sno], marker.SnoActor.NameLocalized, Hud.Game.CurrentGameTick, Hud.Game.RiftPercentage, Hud.Game.Me.SnoArea?.NameLocalized));
								}
								else if (!RiftOpen.Shrines[marker.Id].Type.HasValue && ActorSnoEnumToShrineType.ContainsKey(marker.SnoActor.Sno))
								{
									//Hud.Sound.Speak(Hud.Game.Me.SnoArea?.NameLocalized);
									//update the type now that we know what it is
									RiftOpen.Shrines[marker.Id].Type = ActorSnoEnumToShrineType[marker.SnoActor.Sno];
									RiftOpen.Shrines[marker.Id].TypeName = marker.SnoActor.NameLocalized;
								}
								else if (RiftOpen.Shrines[marker.Id].LocationName != Hud.Game.Me.SnoArea?.NameLocalized)
								{
									//update the location name (location name desync occurs when markers are checked before the player's location data is updated after an area transition)
									RiftOpen.Shrines[marker.Id].LocationName = Hud.Game.Me.SnoArea?.NameLocalized;
								}
							}
						}
					}
					
					switch (riftQuest.QuestStepId)
					{
						case 1:
							if (RiftOpen.StartedAt == 0)
							{
								RiftOpen.StartedAt = riftQuest.CreatedOn;
								RiftOpen.LastSeenStep = riftQuest.QuestStepId;
							}
							
							RiftOpen.Progress = Hud.Game.RiftPercentage / 100d;

							break;
						case 3: //Nephalem Rift Boss Appears
							if (RiftOpen.GuardianSpawnedAt == 0)
							{
								RiftOpen.GuardianSpawnedAt = riftQuest.CreatedOn; //Hud.Game.CurrentGameTick;
								RiftOpen.LastSeenStep = riftQuest.QuestStepId;
							}
							
							RiftOpen.Progress = 1f;
							
							if (Hud.Game.SpecialArea == SpecialArea.Rift && string.IsNullOrEmpty(RiftOpen.GuardianName))
							{
								RiftOpen.GuardianName = FindBossName();
								if (!string.IsNullOrEmpty(RiftOpen.GuardianName))
									RiftOpen.GuardianLocationName = Hud.Game.Me.SnoArea?.NameLocalized;
							}
							
							break;
						case 10: //Nephalem Rift Boss Killed
							if (RiftOpen.GuardianKilledAt == 0)
							{
								RiftOpen.GuardianKilledAt = riftQuest.CreatedOn; //Hud.Game.CurrentGameTick;
								RiftOpen.LastSeenStep = riftQuest.QuestStepId;
							}
							
							RiftOpen.Progress = 1f;
							
							if (Hud.Game.SpecialArea == SpecialArea.Rift)
							{
								//look for boss name
								if (string.IsNullOrEmpty(RiftOpen.GuardianName))
								{
									RiftOpen.GuardianName = FindBossName();
									if (!string.IsNullOrEmpty(RiftOpen.GuardianName))
										RiftOpen.GuardianLocationName = Hud.Game.Me.SnoArea?.NameLocalized;
								}
								
								//look for keystone drops
								if (RiftOpen.Keys == 0)
									RiftOpen.Keys = Hud.Game.Items.Count(i => i.Location == ItemLocation.Floor && i.SnoItem.HasGroupCode("riftkeystone"));
							}

							break;
						case 5: //Nephalem Rift Closing
							//if (RiftOpen.EndedAt == 0)
							//	RiftOpen.EndedAt = Hud.Game.CurrentGameTick - (int)(((float)riftQuest.CompletedOn.ElapsedMilliseconds / 1000f) * 60);
							if (RiftOpen.CompletedAt == 0)
							{
								RiftOpen.CompletedAt = riftQuest.CreatedOn; //Hud.Game.CurrentGameTick - (int)(((float)riftQuest.CompletedOn.ElapsedMilliseconds / 1000f) * 60);
								RiftOpen.LastSeenStep = riftQuest.QuestStepId;
								
								UpdateCompletionAverage();
							}
							
							RiftOpen.Progress = 1f;
							
							if (Hud.Game.SpecialArea == SpecialArea.Rift && string.IsNullOrEmpty(RiftOpen.GuardianName))
							{
								RiftOpen.GuardianName = FindBossName();
								if (!string.IsNullOrEmpty(RiftOpen.GuardianName))
									RiftOpen.GuardianLocationName = Hud.Game.Me.SnoArea?.NameLocalized;
							}

							break;
						/*case uint.MaxValue:
							CloseRiftInfo();
							break;*/
					}
					
					//pick the earliest known time as the starting value
					if (RiftOpen.StartedAt == 0)
					{
						RiftOpen.StartedAt = RiftOpen.LastSeenAt;
						if (RiftOpen.GuardianSpawnedAt > 0 && RiftOpen.GuardianSpawnedAt < RiftOpen.StartedAt)
							RiftOpen.StartedAt = RiftOpen.GuardianSpawnedAt;
						else if (RiftOpen.GuardianKilledAt > 0 && RiftOpen.GuardianKilledAt < RiftOpen.StartedAt)
							RiftOpen.StartedAt = RiftOpen.GuardianKilledAt;
						else if (RiftOpen.CompletedAt > 0 && RiftOpen.CompletedAt < RiftOpen.StartedAt)
							RiftOpen.StartedAt = RiftOpen.CompletedAt;
					}
					
					//bookkeeping
					RiftOpen.ExperienceGained += GetExperienceGained();
					RiftOpen.LastSeenAt = Hud.Game.CurrentGameTick;
				}
				
			}
			else if (RiftOpen is object)
			{
				CloseRiftInfo();
			}
		}
		
		public void Reset(ILabelDecorator label)
		{
			Timer.Stop();
			Timer.Reset();
			CountCompleted = 0;
			CountAbandoned = 0;
			AvgCompletionTicks = 0;
			AvgClosedPerHour = 0;
			LastUpdate = 0;
			Completed.Clear();
			Abandoned.Clear();
			History.Clear();
			RiftClosed = null;
			
			if (RiftOpen is object)
				History.Add(RiftOpen);
		}
		
		private double GetExperienceGained(bool startingNow = false)
		{
			if (startingNow)
			{
				LastXPSeen = Hud.Tracker.Session.GainedExperience;
				return 0;
			}
			
			double xp = Hud.Tracker.Session.GainedExperience - LastXPSeen;
			LastXPSeen = Hud.Tracker.Session.GainedExperience;
			return xp;
		}
		
		private string FindBossName()
		{
			if (Hud.Game.SpecialArea == SpecialArea.Rift)
			{
				var marker = Hud.Game.Markers.FirstOrDefault(m => m.SnoActor != null && m.SnoActor.Code.Contains("_Boss_"));
				if (marker is object)
					return marker.Name;
					
				var boss = Hud.Game.Monsters.FirstOrDefault(m => m.Rarity == ActorRarity.Boss && m.SummonerAcdDynamicId == 0);
				if (boss is object)
					return boss.SnoMonster.NameLocalized;
			}
			
			return null;
		}
		
		private void Resize()
		{
			//Hud.Sound.Speak("resize");
			
			/*if (Menu is object)
				Menu.Width = 0;
			
			if (TableUI is object)
				TableUI.Resize();*/
		}
		
		private void CloseRiftInfo()
		{
			//Hud.Sound.Speak("end");
			
			if (RiftOpen.CompletedAt == 0)
			{
				//flag as incomplete
				RiftOpen.EndedAt = -1;
				
				//add to abandoned history
				Abandoned.Add(RiftOpen);
				if (Abandoned.Count > HistoryShown)
					Abandoned.RemoveAt(0);
					
				++CountAbandoned;
			}
			else
			{
				RiftOpen.EndedAt = RiftOpen.LastSeenAt;
				
				//add to completed history
				Completed.Add(RiftOpen);
				if (Completed.Count > HistoryShown)
					Completed.RemoveAt(0);
					
				++CountCompleted;
				
				//calculate average clear time
				UpdateCompletionAverage();
				UpdateCloseRate(true);
			}
			
			//close current rift info object
			RiftClosed = RiftOpen;
			RiftOpen = null;
			
			//Resize();
		}
		
		private void UpdateCloseRate(bool forceUpdate = false)
		{
			if (Timer == null)
				return;
			
			if (forceUpdate || (Timer.IsRunning && (Timer.ElapsedMilliseconds - LastUpdate) > UpdateInterval))
			{
				//Hud.Sound.Speak("1");
				AvgClosedPerHour = (Timer.ElapsedMilliseconds > 0 ? (float)((decimal)CountCompleted / ((decimal)Timer.ElapsedMilliseconds / 3600000)) : 0);
				LastUpdate = Timer.ElapsedMilliseconds;
			}
			//else
			//	Hud.Sound.Speak("2");
			
			//if (!Timer.IsRunning)
			//	Timer.Start();

		}
		
		private void UpdateCompletionAverage()
		{
			double totalTime = 0;
			int count = 0;
			foreach (var complete in Completed)
			{
				RiftInfo rift = (RiftInfo)complete;
				//totalTime += (double)(rift.EndedAt - rift.StartedAt);
				totalTime += (double)(rift.CompletedAt - rift.StartedAt);
				++count;
			}
			
			if (RiftOpen is object && RiftOpen.CompletedAt > 0)
			{
				totalTime += (double)(RiftOpen.CompletedAt - RiftOpen.StartedAt);
				++count;
			}
			
			AvgCompletionTicks = (int)(totalTime / (double)count);
		}
		
		public void ToggleCompleted(ILabelDecorator label)
		{
			LabelRowDecorator pair;
			
			if (View == Completed)
			{
				View = History;
				SummaryUI.Labels[0].BackgroundBrush = null;
				SummaryUI.Labels[1].BackgroundBrush = null;
				
				pair = (LabelRowDecorator)SummaryUI.Labels[0];
				pair.Hint = HintShowCompleted;
				((LabelStringDecorator)pair.Labels[0]).Font = GreenFont;
				((LabelStringDecorator)pair.Labels[1]).Font = GreenFont;
				return;
			}

			View = Completed;
			SummaryUI.Labels[0].BackgroundBrush = GreenBrush;
			SummaryUI.Labels[1].BackgroundBrush = null;

			pair = (LabelRowDecorator)SummaryUI.Labels[0];
			pair.Hint = HintShowAll;
			((LabelStringDecorator)pair.Labels[0]).Font = TimeFont;
			((LabelStringDecorator)pair.Labels[1]).Font = TimeFont;
			pair = (LabelRowDecorator)SummaryUI.Labels[1];
			pair.Hint = HintShowAbandoned;
			((LabelStringDecorator)pair.Labels[0]).Font = RedFont;
			((LabelStringDecorator)pair.Labels[1]).Font = RedFont;

		}
		
		public void ToggleAbandoned(ILabelDecorator label)
		{
			LabelRowDecorator pair;
			
			if (View == Abandoned)
			{
				View = History;
				SummaryUI.Labels[0].BackgroundBrush = null;
				SummaryUI.Labels[1].BackgroundBrush = null;
				
				pair = (LabelRowDecorator)SummaryUI.Labels[1];
				pair.Hint = HintShowAbandoned;
				((LabelStringDecorator)pair.Labels[0]).Font = RedFont;
				((LabelStringDecorator)pair.Labels[1]).Font = RedFont;
				return;
			}

			View = Abandoned;
			SummaryUI.Labels[0].BackgroundBrush = null;
			SummaryUI.Labels[1].BackgroundBrush = RedBrush;
			
			pair = (LabelRowDecorator)SummaryUI.Labels[0];
			pair.Hint = HintShowCompleted;
			((LabelStringDecorator)pair.Labels[0]).Font = GreenFont;
			((LabelStringDecorator)pair.Labels[1]).Font = GreenFont;
			pair = (LabelRowDecorator)SummaryUI.Labels[1];
			pair.Hint = HintShowAll;
			((LabelStringDecorator)pair.Labels[0]).Font = TimeFont;
			((LabelStringDecorator)pair.Labels[1]).Font = TimeFont;
		}
		
		public ITexture GetBuffTexture(ShrineType type)
		{
			switch (type)
			{
				case ShrineType.PowerPylon:
					return Hud.Texture.GetTexture("Buff_Shrine_Damage");
				case ShrineType.ConduitPylon:
					return Hud.Texture.GetTexture("Buff_Shrine_Electrified");
				case ShrineType.ChannelingPylon:
					return Hud.Texture.GetTexture("Buff_Shrine_Casting");
				case ShrineType.ShieldPylon:
					return Hud.Texture.GetTexture("Buff_Shrine_Invulnerable");
				case ShrineType.SpeedPylon:
					return Hud.Texture.GetTexture("Buff_Shrine_Running");
				case ShrineType.BlessedShrine:
					return Hud.Texture.GetTexture("Buff_Blessed");
				case ShrineType.EnlightenedShrine:
					return Hud.Texture.GetTexture("Buff_Enlightened");
				case ShrineType.FortuneShrine:
					return Hud.Texture.GetTexture("Buff_Fortune");
				case ShrineType.FrenziedShrine:
					return Hud.Texture.GetTexture("Buff_Frenzied");
				case ShrineType.EmpoweredShrine:
					return Hud.Texture.GetTexture("Buff_Empowered");
				case ShrineType.FleetingShrine:
					return Hud.Texture.GetTexture("Buff_Fleeting");
				
				/*case ShrineType.PowerPylon:
					return Hud.Texture.GetTexture(451494, 0);
				case ShrineType.ConduitPylon:
					return Hud.Texture.GetTexture(451503, 0);
				case ShrineType.ChannelingPylon:
					return Hud.Texture.GetTexture(451508, 0);
				case ShrineType.ShieldPylon:
					return Hud.Texture.GetTexture(451493, 0);
				case ShrineType.SpeedPylon:
					return Hud.Texture.GetTexture(451504, 0);
				case ShrineType.BlessedShrine:
					return Hud.Texture.GetTexture("Buff_Blessed");
				case ShrineType.EnlightenedShrine:
					return Hud.Texture.GetTexture("Buff_Enlightened");
				case ShrineType.FortuneShrine:
					return Hud.Texture.GetTexture("Buff_Fortune");
				case ShrineType.FrenziedShrine:
					return Hud.Texture.GetTexture("Buff_Frenzied");
				case ShrineType.EmpoweredShrine:
					return Hud.Texture.GetTexture("Buff_Empowered");
				case ShrineType.FleetingShrine:
					return Hud.Texture.GetTexture("Buff_Fleeting");*/
			}
			
			return null;
		}

	}
}