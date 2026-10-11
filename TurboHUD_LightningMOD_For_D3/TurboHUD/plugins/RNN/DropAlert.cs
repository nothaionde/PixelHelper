using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;
using System.Windows.Forms;

namespace Turbo.Plugins.RNN
{
    public class DropAlert : BasePlugin, INewAreaHandler, IAfterCollectHandler,  ILootGeneratedHandler, ICustomizer
    {
		private List<int> ItemsOlder { get; set; } = new List<int>();
		private List<string> ItemsOlder3 { get; set; } = new List<string>();
		private string Comodin { get; set;}
        private GroundLabelDecorator ItemDecorator { get; set; }

        public Dictionary<ISnoItem, string> ItemsAlert { get; } = new Dictionary<ISnoItem, string>();
		public bool SkipNormalLegendary { get; set; }
        public bool SpeakALLPrimal { get; set; }
		public bool SpeakALLAncient { get; set; }
		public string PrimalText { get; set; }
		public string AncientText { get; set; }
		public int LevelAlert { get; set; }

        public DropAlert()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30001;

			PrimalText = "Primal <name>";		// <name> is replaced with Custom Name (if is added to ItemAlert) or with NameLocalized
			AncientText = "<name> Ancient";		// <name> is replaced with Custom Name (if is added to ItemAlert) or with NameLocalized
			SkipNormalLegendary = false;		// Not speak legendary included in ItemsAlert that is not Primal or Ancient
			SpeakALLPrimal = true;				// Speak All items Primal even if the item is not in ItemsAlert
			SpeakALLAncient = false;			// Speak All items Ancient even if the item is not in ItemsAlert
			LevelAlert = 3;						// Values: 1 (Notify only loot) , 2 (Notify also identified items) , 3 (Notify all, and each time the coordinates of the items change)

			ItemDecorator = new GroundLabelDecorator(Hud)
			{
				TextFont = Hud.Render.CreateFont("consolas", 10, 255, 255, 255, 255, true, false, false)
			};
		}

        public void Customize()
        {
			if ( (LevelAlert != 1) && (LevelAlert != 2) && (LevelAlert != 3) )	LevelAlert = 3 ;
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				if (LevelAlert == 3)	ItemsOlder3.Clear();
				else	ItemsOlder.Clear();
				Comodin = Hud.Game.Me.FloorCoordinate.ToStringCompact();
			}
		}

		private void SpeakCheck(IItem item, bool floor)
		{
			if (item.Seed == 0) return;
			string text = string.Empty;

			if (ItemsAlert.ContainsKey(item.SnoItem))
			{
				if (item.Quality == ItemQuality.Legendary)
				{
					if 		(item.AncientRank == 1) { text = AncientText.Replace("<name>",ItemsAlert[item.SnoItem]); }
					else if (item.AncientRank == 2) { text = PrimalText.Replace("<name>",ItemsAlert[item.SnoItem]); }
					else if (!SkipNormalLegendary || item.SnoItem.MainGroupCode == "consumable" || item.SnoItem.MainGroupCode == "potion" || item.SnoItem == Hud.Sno.SnoItems.Unique_Ring_004_x1 || item.SnoItem == Hud.Sno.SnoItems.Unique_Polearm_101_x1)	{ text = ItemsAlert[item.SnoItem]; }
					else return;
				}
				else { text = ItemsAlert[item.SnoItem]; }

			}
			else
			{
				if (item.Quality == ItemQuality.Legendary)
				{
					if (item.AncientRank == 1)
					{
						if (SpeakALLAncient)	{ text = AncientText.Replace("<name>", item.SnoItem.NameLocalized); }
					}
					else if (item.AncientRank == 2)
					{
						if (SpeakALLPrimal)		{ text = PrimalText.Replace("<name>", item.SnoItem.NameLocalized); }
					}
					else return;
				}
				else return;
			}

			if ( (!item.AccountBound || item.BoundToMyAccount) )
			{
				if (LevelAlert == 3)
				{
					string s = item.Seed + "@" + (floor? item.FloorCoordinate.ToStringCompact():Comodin) ;
                    if (!ItemsOlder3.Contains(s))   ItemsOlder3.Add(s);
                    else return;
				}
				else
				{
                    if (!ItemsOlder.Contains(item.Seed))    ItemsOlder.Add(item.Seed);
                    else return;
				}
				if (!string.IsNullOrEmpty(text))	Hud.Sound.Speak(text);
			}
		}

        public void OnLootGenerated(IItem item, bool gambled)
        {
			if ( (LevelAlert == 3) && (item.Location == ItemLocation.Floor) ) return;  //if ( (LevelAlert == 3) && (!gambled) ) return;
			SpeakCheck(item, (item.Location == ItemLocation.Floor)? true:false);
        }

		public void AfterCollect()
        {
			if (LevelAlert == 1) return;
			var items = Hud.Game.Items.Where(item => (item.Location == ItemLocation.Floor) && ( (LevelAlert == 3) || !item.Unidentified )  );
			foreach (var item in items)
			{
				SpeakCheck(item, true);
			}
        }
    }
}