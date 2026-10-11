namespace Turbo.Plugins.Razor.Menu
{
	//using SharpDX.DirectWrite;
	using System;
	//using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Log;
	using Turbo.Plugins.Razor.Label;
	//using Turbo.Plugins.Razor.Menu;

	public class MenuBuffs : BaseMenuAddon, IMenuAddon, IAfterCollectHandler /* , IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public int HighlightChangeDuration { get; set; } = 10; //in seconds
		public IBrush HighlightBrush { get; set; }
		
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }

		public IFont TextFont { get; set; }
		public IFont FadedFont { get; set; }
		public IFont EnabledFont { get; set; }
		public IFont DisabledFont { get; set; }
		
		public string TextEnabled { get; set; } = "✔️";
		public string TextDisabled { get; set; } = "❌";
		
		private LabelTableDecorator Table;
		private Dictionary<uint, int[]> BuffState = new Dictionary<uint, int[]>();
		private Dictionary<uint, Dictionary<int, DateTime>> BuffLastChanged = new Dictionary<uint, Dictionary<int, DateTime>>();
		private uint[] Sorted;
		private List<uint> BuffPinned = new List<uint>();
		private int Skip = 0;
		private int PageSize = 0;
		
		//private MenuPlugin Plugin;
		
        public MenuBuffs()
        {
            Enabled = true;
			Priority = 10;
			DockId = "BottomLeft";
        }
		
		public override void OnRegister(MenuPlugin plugin)
		{
			//plugin.FontSize
			TextFont = Hud.Render.CreateFont("Consolas", 6.5f, 255, 255, 255, 255, false, false, true);
			FadedFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 150, 150, 150, false, false, true);
			EnabledFont = Hud.Render.CreateFont("Consolas", 6.5f, 255, 0, 255, 0, false, false, true);
			DisabledFont = Hud.Render.CreateFont("Consolas", 6.5f, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			//Label
			Label = new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(329944)) {TextureHeight = plugin.MenuHeight - 2};
			
			//var layout = TextFont.GetTextLayout("000");
			//var width = TextFont.GetTextLayout("000").Metrics.Width;
			
			Table = new LabelTableDecorator(Hud,
				new LabelRowDecorator(Hud, 
					new LabelStringDecorator(Hud) {Alignment = HorizontalAlign.Right, Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3}, //sno
					new LabelStringDecorator(Hud) {Alignment = HorizontalAlign.Left, Font = FadedFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3}//, //sno
					//new LabelStringDecorator(Hud) {Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3} //sno
					/*new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = FadedFont}, //namespace
						new LabelStringDecorator(Hud) {Font = TextFont}, //plugin name
						new LabelStringDecorator(Hud, "📊") {Hint = HintPerformance, Font = TextFont, SpacingLeft = 3} //plugin name //📊📈
					) {Alignment = HorizontalAlign.Left, SpacingTop = 3, SpacingBottom = 3, SpacingLeft = 3, SpacingRight = 3}*/
					
				) //{OnClick = OnClick}
			) {
				Header = new LabelRowDecorator(Hud, 
					new LabelStringDecorator(Hud, "Sno") {Alignment = HorizontalAlign.Right, Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3}, //sno
					new LabelStringDecorator(Hud, "Code") {Alignment = HorizontalAlign.Left, Font = FadedFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3} //sno
				),
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				SpacingLeft = 5, 
				SpacingRight = 5,
				SpacingTop = 5,
				SpacingBottom = 5,
				FillWidth = false,
				//Count = () => Filters.Count > 0 ? Filters.Max(f => f.Plugins.Length) : 0, //SelectedFilter is object ? SelectedFilter.Plugins.Length : 0, //optional, let the table know in advance how many rows of data it needs to render, it will try to partition the table to fit it all on screen
				OnBeforeRender = (label) => {
					Sorted = null;
					return true;
				},
				OnFillRow = (row, index) => {
					if (Sorted == null)
					{
						//need to truncate the list
						if (Table.LastY < Table.MaxRowHeight)
						{
							PageSize = (int)Math.Floor(((Table.LastY + Table.Height) - Table.MaxRowHeight) / Table.MaxRowHeight);
							//Hud.Sound.Speak((Table.MaxRowHeight).ToString());
						}
						else if (Table.LastY + Table.Height > Hud.Window.Size.Height - Table.MaxRowHeight*2)
							PageSize = (int)Math.Floor(((Hud.Window.Size.Height - Table.MaxRowHeight) - Table.LastY) / Table.MaxRowHeight);
						
						if (PageSize > 0)
							Sorted = BuffLastChanged.Keys.OrderByDescending(k => BuffLastChanged[k].Values.Max()).Skip(Skip).Take(PageSize).ToArray();
						else
							Sorted = BuffLastChanged.Keys.OrderByDescending(k => BuffLastChanged[k].Values.Max()).ToArray();

					}
					
					if (index >= Sorted.Length)
						return false;

					var sno = Sorted[index];
					var timestamps = BuffLastChanged[sno];
					var buff = Hud.Game.Me.Powers.GetBuff(sno);

					if ((Hud.Time.Now - timestamps.Values.Max()).TotalSeconds < HighlightChangeDuration)
						row.BackgroundBrush = plugin.HighlightBrush;
					else
						row.BackgroundBrush = null;
					
					LabelStringDecorator ui = (LabelStringDecorator)row.Labels[0];
					ui.StaticText = sno.ToString();
					ui = (LabelStringDecorator)row.Labels[1];
					ui.StaticText = buff.SnoPower.Code; //NameLocalized;
					
					if (string.IsNullOrEmpty(ui.StaticText))
						ui.StaticText = " ";
					
					for (int i = 0; i < buff.IconCounts.Length; ++i)
					{
						if (row.Labels.Count <= i + 2)
						{
							ui = new LabelStringDecorator(Hud) {Font = ui.Font, SpacingLeft = ui.SpacingLeft, SpacingRight = ui.SpacingRight, SpacingTop = ui.SpacingTop, SpacingBottom = ui.SpacingBottom};
							row.Labels.Add(ui);
						}
						else
							ui = (LabelStringDecorator)row.Labels[i + 2];
						
						string lbl = buff.IconCounts[i].ToString();
						if (lbl.Length == 1)
							lbl += "  ";
						else if (lbl.Length == 2)
							lbl += " ";
						ui.StaticText = lbl; //buff.IconCounts[i].ToString();

						if (timestamps.ContainsKey(i))
						{
							if ((Hud.Time.Now - BuffLastChanged[sno][i]).TotalSeconds < HighlightChangeDuration)
							{
								ui.Font = DisabledFont;
								ui.BackgroundBrush = HighlightBrush;
							}
							else
							{
								ui.Font = TextFont;
								ui.BackgroundBrush = null;
								//timestamps.Remove(i);
							}
						}
						else
						{
							ui.Font = TextFont;
							ui.BackgroundBrush = null;
						}
						//ui.ContentWidth = width;
					}
					
					return true;
				}
			};

			//fill out the table
			var firstbuff = Hud.Game.Me.Powers.AllBuffs.First();
			for (int i = 0; i < firstbuff.IconCounts.Length; ++i)
			{
				string lbl = i.ToString();
				if (lbl.Length == 1)
					lbl += "--";
				else if (lbl.Length == 2)
					lbl += "-";
				Table.Header.Labels.Add(new LabelStringDecorator(Hud, lbl) {Alignment = HorizontalAlign.Left, Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3});
				Table.Row.Labels.Add(new LabelStringDecorator(Hud) {Alignment = HorizontalAlign.Left, Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3});
			}
			
			//Menu
			//Panel = new LabelStringDecorator(Hud, "Test") {Font = TextFont, SpacingLeft = 15, SpacingRight = 15};
			Panel = new LabelColumnDecorator(Hud,
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, () => "BUFF INSPECTOR") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				//SelectUI,
				Table
			);
			
			//Plugin = plugin;
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			foreach(IBuff buff in Hud.Game.Me.Powers.AllBuffs)
			{
				if (BuffState.ContainsKey(buff.SnoPower.Sno))
				{
					int[] state = BuffState[buff.SnoPower.Sno];
					for(int i = 0; i < buff.IconCounts.Length; ++i)
					{
						if (state[i] != buff.IconCounts[i])
						{
							//if (BuffLastChanged[buff.SnoPower.Sno].ContainsKey(i))
							BuffLastChanged[buff.SnoPower.Sno][i] = Hud.Time.Now;
							state[i] = buff.IconCounts[i];
						}
					}
				}
				else
				{
					BuffState[buff.SnoPower.Sno] = (int[])buff.IconCounts.Clone();
					BuffLastChanged[buff.SnoPower.Sno] = new Dictionary<int, DateTime>() {{-1, Hud.Time.Now}};
				}
			}
		}
		
		public void OnClick(ILabelDecorator label)
		{
			
		}
	}
}