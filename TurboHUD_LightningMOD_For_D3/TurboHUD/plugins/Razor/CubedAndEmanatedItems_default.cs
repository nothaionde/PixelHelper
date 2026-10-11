/*

This is an enhanced version of TurboHUD's default plugin for showing cubed legendary powers, adding patch 2.7's new emanated powers to the inventory display.

Changelog
July 25, 2021
	- added Hud.Inventory.InventoryMainUiElement.Visible check to prevent drawing while player inventory is closed but follower inventory is open (TH 21.7.21.0+)
June 30, 2021
	- Added optional line breaks to hover tooltips
June 25, 2021
	- Added set data for Cains and Sages and a better way to check for emanated powers - thanks to s4000 for sharing his code!
May 3, 2021
	- Added Oculus Ring and Cord of the Sherma checks even though they're not officially flagged as emanating items.
	- Changed the Templar head icon
April 22, 2021
	- Initial version

*/

namespace Turbo.Plugins.Razor
{
	using System.Drawing;
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;
	//using Turbo.Plugins.Razor.Label;
	
    public class CubedAndEmanatedItems : BasePlugin, IInGameTopPainter, ICustomizer
    {
		public bool DisableDefaultPlugin { get; set; } = true;
		public float HintWidth { get; set; } = 400f; //set to 0 if you don't want to add line breaks to the powers tooltip
		
		public IBrush BgBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		public IFont TextFont { get; set; }
		
		//don't check every iteration
		private uint LastUpdateHero = 0;
		private int LastUpdateTick = 0;
		private int UpdateDelayInTicks = 20; //1/3rd of a second
		private List<ISnoItem> CachedCubeItems = new List<ISnoItem>();
		private List<ISnoItem> CachedEmanatedItems = new List<ISnoItem>();
		private ITexture CachedCompanion;
		//private LabelStringDecorator Tooltip;
		
		private Dictionary<uint, uint> EmanatePowers; //ISnoPower.Sno, ISnoItem.Sno
		
        public CubedAndEmanatedItems()
        {
            Enabled = true;
        }
		
		public override void Load(IController hud)
        {
            base.Load(hud);
			
			//list of all the buffs to check for emanate status
			EmanatePowers = new Dictionary<uint, uint>()
			{
				//double height items here
				{Hud.Sno.SnoPowers.BrokenCrown.Sno, Hud.Sno.SnoItems.P2_Unique_Helm_001.Sno}, //Broken Crown
				{Hud.Sno.SnoPowers.HomingPads.Sno, Hud.Sno.SnoItems.Unique_Shoulder_001_x1.Sno}, //Homing Pads
				{Hud.Sno.SnoPowers.SpauldersOfZakara.Sno, Hud.Sno.SnoItems.Unique_Shoulder_102_x1.Sno}, //Spaulders of Zakara
				{Hud.Sno.SnoPowers.Goldskin.Sno, Hud.Sno.SnoItems.Unique_Chest_001_x1.Sno}, //Goldskin
				{Hud.Sno.SnoPowers.CusterianWristguards.Sno, Hud.Sno.SnoItems.Unique_Bracer_107_x1.Sno}, //Custerian Wristguards
				{Hud.Sno.SnoPowers.NemesisBracers.Sno, Hud.Sno.SnoItems.Unique_Bracer_106_x1.Sno}, //Nemesis Bracers
				{Hud.Sno.SnoPowers.GladiatorGauntlets.Sno, Hud.Sno.SnoItems.Unique_Gloves_011_x1.Sno}, //Gladiator Gauntlets
				{318383, Hud.Sno.SnoItems.Unique_Gloves_103_x1.Sno}, //Gloves of Worship

				//set bonuses here
				{359560, Hud.Sno.SnoItems.Unique_Helm_016_x1.Sno}, //Sage's Journey (3-piece set bonus)
				{483570, Hud.Sno.SnoItems.P66_Unique_Helm_012.Sno}, //Cain's Destiny (3-piece set bonus)
				
				//single height items here
				{Hud.Sno.SnoPowers.DovuEnergyTrap.Sno, Hud.Sno.SnoItems.Unique_Amulet_107_x1.Sno}, //Dovu Energy Trap
				{Hud.Sno.SnoPowers.RakoffsGlassOfLife.Sno, Hud.Sno.SnoItems.Unique_Amulet_108_x1.Sno}, //Rakoff's Glass of Life
				{322975, Hud.Sno.SnoItems.Unique_Ring_108_x1.Sno}, //Avarice Band
				{Hud.Sno.SnoPowers.KredesFlame.Sno, Hud.Sno.SnoItems.Unique_Ring_003_x1.Sno}, //Krede's Flame
				{Hud.Sno.SnoPowers.TheFlavorOfTime.Sno, Hud.Sno.SnoItems.P66_Unique_Amulet_001.Sno} , //The Flavor of Time
				{Hud.Sno.SnoPowers.OculusRing.Sno, Hud.Sno.SnoItems.Unique_Ring_017_p4.Sno} , //Oculus Ring
				{Hud.Sno.SnoPowers.CordOfTheSherma.Sno, Hud.Sno.SnoItems.Unique_Belt_104_p2.Sno} //, //Cord of the Sherma
			};
			
			BgBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 0);
			BorderBrush = Hud.Render.CreateBrush(150, 227, 153, 25, 0);
			TextFont = Hud.Render.CreateFont("tahoma", 8f, 255, 255, 255, 255, false, false, false);
			
			//Tooltip = new LabelStringDecorator(Hud) {Font = TextFont, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 10, SpacingBottom = 10};
		}
		
		public void Customize()
		{
			if (DisableDefaultPlugin)
				Hud.TogglePlugin<InventoryKanaiCubedItemsPlugin>(false);
		}

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.Inventory || !Hud.Inventory.InventoryMainUiElement.Visible)
                return;
			
			if (LastUpdateHero != Hud.Game.Me.HeroId || LastUpdateTick > Hud.Game.CurrentGameTick || Hud.Game.CurrentGameTick - LastUpdateTick > UpdateDelayInTicks)
			{
				LastUpdateHero = Hud.Game.Me.HeroId;
				LastUpdateTick = Hud.Game.CurrentGameTick;
				
				//update cubed list
				CachedCubeItems.Clear();
				if (Hud.Game.Me.CubeSnoItem1 is object)
					CachedCubeItems.Add(Hud.Game.Me.CubeSnoItem1);
				if (Hud.Game.Me.CubeSnoItem2 is object)
					CachedCubeItems.Add(Hud.Game.Me.CubeSnoItem2);
				if (Hud.Game.Me.CubeSnoItem3 is object)
					CachedCubeItems.Add(Hud.Game.Me.CubeSnoItem3);
				if (Hud.Game.Me.CubeSnoItem4 is object)
					CachedCubeItems.Add(Hud.Game.Me.CubeSnoItem4);
				
				CachedEmanatedItems.Clear();
				CachedCompanion = null;
				//var companion = Hud.Game.Actors.FirstOrDefault(c => c.SnoActor.Sno == ActorSnoEnum._hireling_scoundrel || c.SnoActor.Sno == ActorSnoEnum._hireling_enchantress || c.SnoActor.Sno == ActorSnoEnum._hireling_templar);
				var companion = Hud.Game.Actors.FirstOrDefault(c => c.SnoActor.Kind == ActorKind.Follower);
				if (Hud.Game.NumberOfPlayersInGame == 1 && companion is object) //Hud.Game.NumberOfPlayersInGame == 1 + check for a hireling
				{
					//update emanated list
					foreach (KeyValuePair<uint, uint> pair in EmanatePowers)
					{
						if (Hud.Game.Me.Powers.BuffIsActive(pair.Key))
						{
							//var item = Hud.Inventory.GetSnoItem(pair.Value);
							//if (!CachedCubeItems.Any(i => i.LegendaryPower?.Sno == pair.Key) && !Hud.Game.Items.Any(i => (int)i.Location > 0 && (int)i.Location < 14 && i.SnoItem.LegendaryPower?.Sno == pair.Key))
							//	CachedEmanatedItems.Add(Hud.Inventory.GetSnoItem(pair.Value)); //item);
							if (companion.GetAttributeValue(Hud.Sno.Attributes.Trait, pair.Key, 0) == 1)
								CachedEmanatedItems.Add(Hud.Inventory.GetSnoItem(pair.Value));
						}
					}

					//update hireling texture
					if (companion is object)
					{
						if (companion.SnoActor.Sno == ActorSnoEnum._hireling_scoundrel)
							CachedCompanion = Hud.Texture.GetTexture(441912908); // scoundrel
						else if (companion.SnoActor.Sno == ActorSnoEnum._hireling_enchantress)
							CachedCompanion = Hud.Texture.GetTexture(2807221403); // enchantress
						else if (companion.SnoActor.Sno == ActorSnoEnum._hireling_templar)
							CachedCompanion = Hud.Texture.GetTexture(3116868919); //3116868919 //1094113362); // templar
					}
				}
			}
			
			//draw the cached lists
			var rect = Hud.Inventory.InventoryMainUiElement.Rectangle;
			var x = rect.X + rect.Width*0.065f;
			var y = rect.Y + rect.Width*0.051f;
			var itemRect = Hud.Inventory.GetRectInInventory(0, 0, 1, 1);
			var w = itemRect.Width*0.8f;//width = itemRect.Width*0.8f;
			var height = itemRect.Height*0.8f;
			//BackgroundTexture1 = Hud.Texture.ButtonTextureOrange,
			//BackgroundTexture2 = Hud.Texture.BackgroundTextureOrange,
			if (CachedCubeItems.Count > 0)
			{
				foreach (ISnoItem snoItem in CachedCubeItems)
				{
					//var h = height*snoItem.ItemHeight;
					BgBrush.DrawRectangle(x, y, w, height*2f);
					DrawItem(snoItem, x, snoItem.ItemHeight == 1 ? y + height*0.5f : y, w, height*snoItem.ItemHeight);

					BgBrush.DrawRectangle(x, y-1, w, 5f);
					BorderBrush.DrawRectangle(x, y, w, 3f);

					x += w;
				}

				//kanai's cube icon
				var icon = Hud.Texture.KanaiCubeTexture;
				var iconSize = icon.Height * 0.85f / 1200.0f * Hud.Window.Size.Height;
				icon.Draw(rect.X + rect.Width*0.039f, y - iconSize*0.6f, iconSize, iconSize);
			}
			
			if (CachedEmanatedItems.Count > 0)
			{
				x = rect.Right - rect.Width*0.065f - w;
				float stackX = x;
				float stackY = y;
				foreach (ISnoItem snoItem in CachedEmanatedItems)
				{
					var h = snoItem.ItemHeight < 1 ? height*2f : height*snoItem.ItemHeight;
					
					if (snoItem.ItemHeight == 1) //single height
					{
						BgBrush.DrawRectangle(stackX, stackY, w, h);
						DrawItem(snoItem, stackX, stackY, w, h);
						
						if (stackY != y)
						{
							stackX = x;
							stackY = y;
						}
						else
						{
							BgBrush.DrawRectangle(x, y-1, w, 5f);
							BorderBrush.DrawRectangle(x, y, w, 3f);

							stackY = y + height;
							x -= w;
						}
					}
					else //double height
					{
						BgBrush.DrawRectangle(x, y, w, h);
						DrawItem(snoItem, x, y, w, h);

						BgBrush.DrawRectangle(x, y-1, w, 5f);
						BorderBrush.DrawRectangle(x, y, w, 3f);

						x -= w;
						
						if (stackY == y)
							stackX = x;
					}
				}
				
				//companion icon
				if (CachedCompanion is object)
				{
					var iconSize = CachedCompanion.Height * 0.115f / 1200.0f * Hud.Window.Size.Height;
					//x = rect.Right - rect.Width*0.06f - iconSize;
					CachedCompanion.Draw(rect.Right - rect.Width*0.06f - iconSize, y - iconSize*0.73f, iconSize, iconSize);
				}
			}
        }

		private void DrawItem(ISnoItem snoItem, float x, float y, float w, float h)
		{
			var itemTexture = Hud.Texture.GetItemTexture(snoItem);
			if (itemTexture is object)
			{
				//draw texture
				var texture = Hud.Texture.GetItemTexture(snoItem);
				texture.Draw(x, y, w, h);
			
				//draw hover tooltip
				if (Hud.Window.CursorInsideRect(x, y, w, h))
				{
					var description = snoItem.NameLocalized;

					var power = snoItem.LegendaryPower;
					if (power != null)
					{
						description += "\n\n" + power.DescriptionLocalized;
					}

					if (HintWidth > 0)
					{
						if (TextFont.GetTextLayout(description).Metrics.Width > HintWidth)
						//if (description.Length > HintWidth)
						{
							//float lengthPerChar = layout.Metrics.Width / description.Length;
							//int charsPerLine = (int)System.Math.Floor(HintWidth / lengthPerChar); //not accurate, but close enoug
							//int breakCount = //(int)System.Math.Floor(layout.Metrics.Width / HintWidth);
							
							string[] words = description.Split(' ');
							string line = string.Empty;
							string result = string.Empty;
							foreach (string word in words)
							{
								line += word + " ";
								if (TextFont.GetTextLayout(line).Metrics.Width >= HintWidth) //line.Length >= charsPerLine)
								{
									result += line + System.Environment.NewLine;
									line = string.Empty;
								}
							}
							
							description = (result + line).Trim();
							
							/*for (int i = charsPerLine; i < description.Length; ) //breakCount; i < description.Length; )
							{
								
								description = description.Insert(i, System.Environment.NewLine);
								i += charsPerLine + 1; //breakCount + 1;
							}*/
						}
					}
					
					Hud.Render.SetHint(description);
					//Tooltip.StaticText = description;
					//LabelDecorator.SetHintLabel(Tooltip);
				}
			}
		}
    }
}