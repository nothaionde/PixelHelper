/*

MonsterCounter by Razorfish
Summarizes and tallies the statuses and values of monsters nearby. Hovering over a status bar will highlight which monsters on the screen do not have the status effect.

This plugin is based on SeaDragon's MonstersCountPlugin (glq), but I wanted to display the information in a different way.

Changelog:
April 23, 2021
	- updated Ignite rule check for Season 23
	- fixed some time alignment issues
	- split progress orb value for yellows (rares) from the monster kill value
	- show the total progress time value
	- show the progress time value only when in a greater rift
	- fixed a rounding error with the TimeSpan calculation
	- fixed the total progress values to include orphaned orbs
March 5, 2021
	- fixed progress time icon alignment when no blues (champions) are present

*/

namespace Turbo.Plugins.Razor
{
	using SharpDX.Direct2D1; //CapStyle
	using SharpDX.DirectWrite; //TextLayout
	using System;
	using System.Collections.Generic; //Dictionary
	using System.Globalization; //CultureInfo
	using System.Linq; //Where
	using System.Windows.Forms; //Keys
	
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Movable; //referencing the Movable library

	public class MonsterCounter : BasePlugin, IMovable//, IAfterCollectHandler //implements the IMovable interface
	{
		public bool IncludeRareMinionProgress { get; set; } = false; //include the progress from killing yellow pack minions in the yellow pack progress totals
		
		public Keys? CursorMode { get; set; } = Keys.LControlKey; //(optional) hold this key down to show density around your mouse cursor instead of your character
		public string TextRiftProgress { get; set; } = "%"; //"Rift %";
		public string TextTimeProgress { get; set; } = "🕓"; //"Time Value";
		
		//public IShapePainter test { get; set; }
		public IBrush BackgroundBrush { get; set; }
		public IBrush ShadowBrush { get; set; }
		
		public IFont CountFont { get; set; }
		public IFont OtherFont { get; set; }
		public IFont ProgressFont { get; set; }
		public IFont LabelFont { get; set; }
		public IFont OrbFont { get; set; }
		public IFont GreaterOrbFont { get; set; }
		public IFont LesserFont { get; set; }
		public IFont TimeFont { get; set; }
		
		public IBrush RareBgBrush { get; set; }
		public IBrush ChampionBgBrush { get; set; }
		public IBrush TrashBgBrush { get; set; }
		public IBrush OrbBgBrush { get; set; }
		
		//public IFont RareFont { get; set; } //yellow
		//public IFont ChampionFont { get; set; } //blue
		
		public float Spacing { get; set; } = 4f;
		public float Distance { get; set; } = 50f;
		public float CountableBarHeight { get; set; } = 12f;
		
		public WorldDecoratorCollection SelectedDecorator { get; set; }
		
		private float offsetX = 0;
		private float offsetY = 0;
		//private Dictionary<ActorRarity, Dictionary<int, float>> Counts;
		
		//public enum CountType { Haunt, Locust, Frailty, Leech, Decrepify, Strongarm, Entangle, Falter, Ignite, Bleed, Krysbin, Mark, Palm }
		public class Countable
		{
			//public CountType Type { get; set; }
			public string Name { get; set; }
			public Func<bool> IsRelevant { get; set; }
			public Func<IMonster, bool> IsActive { get; set; }
			
			public IFont Font { get; set; }
			public IBrush Brush { get; set; }
		}
		public List<Countable> Countables { get; set; }
		
		public MonsterCounter()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			
			//CursorMode = Keys.LControlKey;
			
			BackgroundBrush = Hud.Render.CreateBrush(200, 150, 199, 246, 0, DashStyle.Solid, CapStyle.Square, CapStyle.Triangle);
			ShadowBrush = Hud.Render.CreateBrush(100, 0, 0, 0, 0, DashStyle.Solid, CapStyle.Flat, CapStyle.Triangle);
			
			CountFont = Hud.Render.CreateFont("tahoma", 14f, 255, 0, 0, 0, true, false, 35, 255, 255, 255, true);
			OtherFont = Hud.Render.CreateFont("tahoma", 14f, 255, 125, 125, 125, true, false, 55, 0, 0, 0, true);
			ProgressFont = Hud.Render.CreateFont("tahoma", 10f, 255, 255, 255, 255, false, false, 55, 0, 0, 0, true);
			OrbFont = Hud.Render.CreateFont("tahoma", 10f, 255, 191, 160, 109, false, false, 55, 0, 0, 0, true);
			GreaterOrbFont = Hud.Render.CreateFont("tahoma", 10f, 255, 240, 120, 240, false, false, 55, 0, 0, 0, true);
			LesserFont = Hud.Render.CreateFont("tahoma", 8f, 255, 175, 175, 175, true, false, 55, 0, 0, 0, true);
			LabelFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, 75, 0, 0, 0, true);
			TimeFont = Hud.Render.CreateFont("tahoma", 8f, 190, 255, 255, 0, false, false, 75, 0, 0, 0, true);
			
			ChampionBgBrush = Hud.Render.CreateBrush(175, 64, 128, 255, 0, DashStyle.Solid, CapStyle.Flat, CapStyle.Triangle);
			RareBgBrush = Hud.Render.CreateBrush(175, 255, 148, 20, 0, DashStyle.Solid, CapStyle.Flat, CapStyle.Triangle);
			TrashBgBrush = Hud.Render.CreateBrush(175, 200, 200, 200, 0, DashStyle.Solid, CapStyle.Flat, CapStyle.Triangle);
			OrbBgBrush = Hud.Render.CreateBrush(175, 240, 120, 240, 0);
			
			//define all the types of accounting to be done
			Countables = new List<Countable>()
			{
				new Countable() { Name = "Саранча", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.WitchDoctor_LocustSwarm.Sno)), IsActive = (monster) => monster.Locust, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 255, 0, 0) },
				new Countable() { Name = "Преследует", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.WitchDoctor_Haunt.Sno)), IsActive = (monster) => monster.Haunted, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 0, 255, 0, 0) },
				new Countable() { Name = "Пираньи", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.WitchDoctor_Piranhas.Sno)), IsActive = (monster) => monster.Piranhas, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 0, 255, 0, 0) },

				new Countable() { Name = "Хрупкость", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Frailty.Sno) || p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_GrimScythe.Sno && (x.Rune == 4 || p.Powers.BuffIsActive(Hud.Sno.SnoPowers.TragOulsCorrodedFang.Sno)))), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 471845) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 252, 235, 191, 0) },
				new Countable() { Name = "Пиявка", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Leech.Sno) || p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_GrimScythe.Sno && (x.Rune == 4 || p.Powers.BuffIsActive(Hud.Sno.SnoPowers.TragOulsCorrodedFang.Sno)))), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 471869) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 252, 235, 191, 0) },
				new Countable() { Name = "Расшифровать", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Decrepify.Sno) || p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_GrimScythe.Sno && (x.Rune == 4 || p.Powers.BuffIsActive(Hud.Sno.SnoPowers.TragOulsCorrodedFang.Sno)))), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 471738) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 252, 235, 191, 0) },
				
				new Countable() { Name = "Кровотечение", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.PainEnhancerSecondary.Sno)), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Bleeding, uint.MaxValue) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 0, 75, 0) },
				new Countable() { Name = "Метка смерти", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.DemonHunter_MarkedForDeath.Sno) || p.Powers.BuffIsActive(Hud.Sno.SnoPowers.Calamity.Sno)), IsActive = (monster) => monster.MarkedForDeath, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 0, 0, 0) },
				//new Countable() { Type = CountType.Palm, Name = "Palm", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Monk_ExplodingPalm.Sno) || p.Powers.BuffIsActive(---Uliana's 2pc bonus id---)), IsActive = (monster) => monster.Palmed, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 25, 0, 0) },

				new Countable() { Name = "Сильная рука", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.StrongarmBracers.Sno)), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_2_Visual_Effect_None, 318772) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 100, 100, 100, 0) },
				new Countable() { Name = "Запутать", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.DemonHunter_EntanglingShot.Sno) && p.Powers.BuffIsActive(Hud.Sno.SnoPowers.OdysseysEnd.Sno)), IsActive = (monster) => monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None, 361936) == 1 || monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_A, 361936) == 1 || monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_B, 361936) == 1 || monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_C, 361936) == 1 || monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_D, 361936) == 1 || monster.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_E, 361936) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 0, 0, 255, 0) },
				new Countable() { Name = "Falter", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Barbarian_ThreateningShout.Sno && x.Rune == 3)), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_D, 79077) ==	1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 100, 0, 0) },
				new Countable() { Name = "Krysbin", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.KrysbinsSentence.Sno)), IsActive = (monster) => monster.Frozen || monster.Stunned || monster.Blind || monster.Slow || monster.Chilled, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 150, 199, 246, 0) },

				//	attr	Power_Buff_4_Visual_Effect_None	485318	1	power: P7_ItemPassive_Unique_Ring_001
				//	attr	Power_Buff_7_Visual_Effect_A	91549	1	power: Disintegrate
				//new Countable() { Name = "Ignite", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.BuffIsActive(359580)), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_None, 359581) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 76, 0, 0) }, //359580 = firebird's finery (2pc) //monster.Phoenixed
				new Countable() { Name = "Зажечь", IsRelevant = () => Hud.Game.Players.Any(p => p.Powers.BuffIsActive(485318)), IsActive = (monster) => monster.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_4_Visual_Effect_None, 485318) == 1, Font = LabelFont, Brush = Hud.Render.CreateBrush(185, 255, 76, 0, 0) }, //359580 = firebird's finery (2pc) //monster.Phoenixed
			}; 
			
			SelectedDecorator = new WorldDecoratorCollection(
				new GroundCircleDecorator(Hud) {
					Brush = Hud.Render.CreateBrush(200, 255, 255, 255, 3),
					HasShadow = true,
					Radius = -1f
				},
				new GroundShapeDecorator(Hud)
				{
					Brush = Hud.Render.CreateBrush(200, 255, 0, 0, 6),
					ShadowBrush = Hud.Render.CreateBrush(55, 0, 0, 0, 2),
					Radius = 1f,
					ShapePainter = WorldStarShapePainter.NewCross(Hud), //NewCross(Hud), //NewPentagram(Hud),
				}
			);
		}
		
		//required for IMovable, this function is called when MovableController first acknowledges this plugin's existence
		public void OnRegister(MovableController mover)
		{
			TextLayout layout = CountFont.GetTextLayout("555");
			offsetY = layout.Metrics.Height*0.5f;
			offsetX = layout.Metrics.Width*1.1f;
			
			mover.CreateArea(
				this,
				"Counter", //area name
				new System.Drawing.RectangleF(Hud.Window.Size.Width*0.84f, Hud.Window.Size.Height*0.74f, (layout.Metrics.Width + Spacing)*4, layout.Metrics.Height), //position + dimensions
				true, //enabled at start? (visible upon creation)
				true, //save to config file?
				ResizeMode.Off //resize mode
			);
		}

		//required for IMovable, this is called whenever MovableController wants this plugin to draw something on the screen
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			IEnumerable<IMonster> monsters = null;
			bool isCursorMode = CursorMode.HasValue && Hud.Input.IsKeyDown(CursorMode.Value);
			IWorldCoordinate searchPos = null;
			
			if (isCursorMode)
			{
				//searchPos = (Hud.Game.SelectedMonster2 is object ? Hud.Game.SelectedMonster2.FloorCoordinate : Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY).ToWorldCoordinate());
				if (Hud.Game.SelectedMonster2 is object)
					searchPos = Hud.Game.SelectedMonster2.FloorCoordinate;
				else
				{
					var pos = Hud.Window.CreateScreenCoordinate(Hud.Window.CursorX, Hud.Window.CursorY);
					if (pos is object)
						searchPos = pos.ToWorldCoordinate();
				}
				
				if (searchPos is object && !searchPos.IsValid)
					searchPos = null;
			}
			
			monsters = (searchPos is object ? 
				Hud.Game.AliveMonsters.Where(m => m.FloorCoordinate.XYDistanceTo(searchPos) <= Distance) :
				Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= Distance)
			);
			
			if (monsters == null) // || !monsters.Any()
				return;

			//if the area is currently being moved (on the cursor) from its original position, deltaX and deltaY will be > 0
			//the movable area dimensions you can reference for drawing on the screen
			var x = area.Rectangle.X + deltaX;
			var y = area.Rectangle.Y + deltaY;
			var width = area.Rectangle.Width;
			var height = area.Rectangle.Height;

			int countTotal = 0;
			int countChampion = 0;
			int countChampionPack = 0;
			int countChampionIllusion = 0;
			int countRare = 0;
			int countRareMinion = 0;
			int countRareIllusion = 0;
			int countTrash = 0;
			int countBoss = 0;
			//int countGoblin = 0;
			int countOrb = Hud.Game.Actors.Count(o => o.SnoActor.Kind == ActorKind.RiftOrb);
			
			float progressChampion = 0;
			float progressChampionOrb = 0;
			float progressRare = 0;
			float progressRareOrb = 0;
			float progressRareMinion = 0;
			float progressTrash = 0;
			float progressTotal = 0;
			
			Dictionary<int, bool> relevance = new Dictionary<int, bool>();
			List<int> relevantCountables = new List<int>();
			//foreach (Countable c in Countables)
			for (int i = 0; i < Countables.Count; ++i) //Countable c in Countables)
			{
				relevance[i] = Countables[i].IsRelevant();
				if (relevance[i])
					relevantCountables.Add(i);
			}
			
			Dictionary<int, int> championCounts = new Dictionary<int, int>();
			Dictionary<int, int> rareCounts = new Dictionary<int, int>();
			Dictionary<int, int> trashCounts = new Dictionary<int, int>();
			List<IMonsterPack> packs = new List<IMonsterPack>();
			bool isInGreaterRift = Hud.Game.SpecialArea == SpecialArea.GreaterRift;
			bool isInRift = Hud.Game.SpecialArea == SpecialArea.Rift || isInGreaterRift;
			IFont orbFont = GreaterOrbFont; //(isInRift ? (isInGreaterRift ? GreaterOrbFont : OrbFont) : null);
			
			foreach (var monster in monsters) //Hud.Game.AliveMonsters.Where(m => m.NormalizedXyDistanceToMe <= Distance))
			{
				++countTotal;
				
				Dictionary<int, int> counter = null;
				switch (monster.Rarity)
				{
					case ActorRarity.Champion:
						counter = championCounts;
						
						if (isInRift)
						{
							progressChampion += monster.SnoMonster.RiftProgression;
							progressTotal += monster.SnoMonster.RiftProgression;
							
							if (monster.Pack is object && !packs.Contains(monster.Pack))
								packs.Add(monster.Pack);
						}

						++countChampion;
						if (monster.Illusion)
							++countChampionIllusion;
						
						break;
					case ActorRarity.Rare:
						counter = rareCounts;
						if (isInRift)
						{
							progressRare += monster.SnoMonster.RiftProgression;
							progressRareOrb += 4 * 1.15f;
							progressTotal += monster.SnoMonster.RiftProgression;
						}

						++countRare;
						if (monster.Illusion)
							++countRareIllusion;

						break;
					case ActorRarity.RareMinion:
						if (isInRift) 
						{
							progressRareMinion += monster.SnoMonster.RiftProgression;
							progressTotal += monster.SnoMonster.RiftProgression;
						}
						++countRareMinion;
						break;
					case ActorRarity.Boss:
					case ActorRarity.Unique:
						//counter = bossCounts;
						//if (isInRift) progressBoss += monster.SnoMonster.RiftProgression;
						++countBoss;
						break;
					default:
						counter = trashCounts;
						if (isInRift)
						{
							progressTrash += monster.SnoMonster.RiftProgression;
							progressTotal += monster.SnoMonster.RiftProgression;
						}
						++countTrash;
						break;
				}
				
				//accounting
				if (counter is object)
				{
					//foreach (Countable c in Countables)
					for (int i = 0; i < Countables.Count; ++i) //Countable c in Countables)
					{
						if (relevance[i])
						{
							if (Countables[i].IsActive(monster))
								counter[i] = counter.ContainsKey(i) ? counter[i] + 1 : 1;
							else if (!counter.ContainsKey(i))
								counter[i] = 0;
						}
					}
				}
			}
		
			foreach (var pack in packs) //Hud.Game.MonsterPacks
			{
				if (!pack.IsFullChampionPack)
					continue;

				if (isCursorMode)
				{
					if (!pack.MonstersAlive.Any(m => m.FloorCoordinate.XYDistanceTo(searchPos) > Distance))
					{
						progressChampionOrb += 3 * 1.15f;
						++countChampionPack;
					}
				}
				else if (!pack.MonstersAlive.Any(m => m.NormalizedXyDistanceToMe > Distance))
				{
						progressChampionOrb += 3 * 1.15f;
						++countChampionPack;
				}
			}
			
			//var relevantCountables = Countables.Where(c => relevance[c.Type]);
			
			int shownCount = 0;
			if (countRare > 0 || countRareMinion > 0)
			{
				++shownCount;
				
				TextLayout layout = CountFont.GetTextLayout(countRare.ToString());
				ShadowBrush.DrawLine(x + offsetX*0.5f, y - 1, x + offsetX*0.5f, y + height + 1, offsetX + 2);
				RareBgBrush.DrawLine(x + offsetX*0.5f, y, x + offsetX*0.5f, y + height, offsetX);
				CountFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y);
				
				if (countRareMinion > 0)
				{
					TextLayout minion = LesserFont.GetTextLayout(countRareIllusion > 0 ? countRareMinion.ToString() + "+(" + (countRare - countRareIllusion) + ")" : countRareMinion.ToString()); //"+" + 
					LesserFont.DrawText(minion, x + offsetX*0.5f - minion.Metrics.Width*0.5f, y + layout.Metrics.Height);
				}				
				else if (countRareIllusion > 0)
				{
					TextLayout minion = LesserFont.GetTextLayout("(" + (countRare - countRareIllusion) + ")"); //"+" + 
					LesserFont.DrawText(minion, x + offsetX*0.5f - minion.Metrics.Width*0.5f, y + layout.Metrics.Height);
				}

				
				float yCountables = y;
				//foreach (Countable c in relevantCountables)
				foreach (int i in relevantCountables)
				{
					var c = Countables[i];
					
					yCountables -= Spacing*1.5f + CountableBarHeight;
					
					if (c.Brush is object)
					{
						ShadowBrush.DrawRectangle(x, yCountables, offsetX, CountableBarHeight);
						
						c.Brush.StrokeWidth = 2f;
						c.Brush.DrawRectangle(x, yCountables, offsetX, CountableBarHeight);
						
						c.Brush.StrokeWidth = 0f;
						c.Brush.DrawRectangle(x, yCountables, offsetX*(countRare > 0 ? (float)rareCounts[i] / (float)countRare : 0), CountableBarHeight);
						
						if (SelectedDecorator is object && Hud.Window.CursorInsideRect(x, yCountables, offsetX, CountableBarHeight))
						{
							foreach (IMonster monster in Hud.Game.AliveMonsters.Where(m => m.IsOnScreen && m.IsElite && m.Rarity == ActorRarity.Rare && !c.IsActive(m))) //SelectedDecorator.Paint();
								PaintDecoratorCollection(SelectedDecorator, monster);
						}
					}
					
					if (c.Font is object)
					{
						layout = c.Font.GetTextLayout(countRare > 0 ? rareCounts[i].ToString() : "0");
						c.Font.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, yCountables + CountableBarHeight*0.5f - layout.Metrics.Height*0.5f);
					}
				}

				if (isInRift)
				{
					float y2 = y + height*2f;
					
					double progress = ((double)(IncludeRareMinionProgress ? progressRare + progressRareMinion : progressRare) * 100.0d / Hud.Game.MaxQuestProgress); //+ progressRareOrb;
					
					layout = ProgressFont.GetTextLayout(progress.ToString("F2"));
					ProgressFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y2);
					
					y2 += layout.Metrics.Height;
					
					layout = orbFont.GetTextLayout(progressRareOrb.ToString("F2"));
					orbFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y2);
					
					if (isInGreaterRift)
					{
						TimeSpan time = new TimeSpan((long)((progress + progressRareOrb) * 90000000));
						//TextLayout layoutTime = TimeFont.GetTextLayout(time.TotalSeconds < 60 ? time.TotalSeconds.ToString("F0") + "s" : time.ToString(@"m\m\ ss\s", CultureInfo.InvariantCulture));
						//TimeFont.DrawText(layoutTime, x + offsetX*0.5f - layoutTime.Metrics.Width*0.5f, y2 + (countRare > 0 || countChampion > 0 ? layout.Metrics.Height*2 : layout.Metrics.Height) + layout.Metrics.Height*0.5f - layoutTime.Metrics.Height*0.5f);
						TextLayout layoutTime = TimeFont.GetTextLayout(time.TotalSeconds < 60 ? time.TotalSeconds.ToString("F0") + "s" : time.ToString(@"m\m\ ss\s", CultureInfo.InvariantCulture));
						TimeFont.DrawText(layoutTime, x + offsetX*0.5f - layoutTime.Metrics.Width*0.5f, y2 + layout.Metrics.Height + layout.Metrics.Height*0.5f - layoutTime.Metrics.Height*0.5f);
					}
				}
				
				x += offsetX + Spacing; //+ Spacing;
			}
			
			//ShadowBrush.DrawLine(x, y, x, y+height, 50f);
			if (countChampion > 0)
			{
				++shownCount;
				
				TextLayout layout = CountFont.GetTextLayout(countChampion.ToString());
				ShadowBrush.DrawLine(x + offsetX*0.5f, y - 1, x + offsetX*0.5f, y + height + 1, offsetX + 2);
				ChampionBgBrush.DrawLine(x + offsetX*0.5f, y, x + offsetX*0.5f, y + height, offsetX);
				CountFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y);
				
				if (countChampionIllusion > 0)
				{
					TextLayout minion = LesserFont.GetTextLayout("(" + (countChampion - countChampionIllusion).ToString() + ")"); //"+" + 
					LesserFont.DrawText(minion, x + offsetX*0.5f - minion.Metrics.Width*0.5f, y + layout.Metrics.Height);
				}
				
				float yCountables = y;
				//foreach (Countable c in relevantCountables)
				foreach (int i in relevantCountables)
				{
					var c = Countables[i];

					yCountables -= Spacing*1.5f + CountableBarHeight;
					
					if (c.Brush is object)
					{
						ShadowBrush.DrawRectangle(x, yCountables, offsetX, CountableBarHeight);
						
						c.Brush.StrokeWidth = 2f;
						c.Brush.DrawRectangle(x, yCountables, offsetX, CountableBarHeight);
						
						c.Brush.StrokeWidth = 0f;
						c.Brush.DrawRectangle(x, yCountables, offsetX*(countChampion > 0 ? (float)championCounts[i] / (float)countChampion : 0), CountableBarHeight);
						
						if (SelectedDecorator is object && Hud.Window.CursorInsideRect(x, yCountables, offsetX, CountableBarHeight))
						{
							foreach (IMonster monster in Hud.Game.AliveMonsters.Where(m => m.IsOnScreen && m.IsElite && m.Rarity == ActorRarity.Champion && !c.IsActive(m))) //SelectedDecorator.Paint();
								PaintDecoratorCollection(SelectedDecorator, monster);
						}
					}
					
					if (c.Font is object)
					{
						layout = c.Font.GetTextLayout(countChampion > 0 ? championCounts[i].ToString() : "0");
						c.Font.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, yCountables + CountableBarHeight*0.5f - layout.Metrics.Height*0.5f);
					}
				}
				
				if (isInRift)
				{
					float y2 = y + height*2f;
					
					double progress = (double)progressChampion * 100.0d / Hud.Game.MaxQuestProgress;
					
					layout = ProgressFont.GetTextLayout(progress.ToString("F2"));
					ProgressFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y2);
					
					y2 += layout.Metrics.Height;
					
					layout = orbFont.GetTextLayout(progressChampionOrb.ToString("F2"));
					orbFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y2);
					
					if (isInGreaterRift)
					{
						TimeSpan time = new TimeSpan((long)((progress + progressChampionOrb) * 90000000));
						TextLayout layoutTime = TimeFont.GetTextLayout(time.TotalSeconds < 60 ? time.TotalSeconds.ToString("F0") + "s" : time.ToString(@"m\m\ ss\s", CultureInfo.InvariantCulture));
						TimeFont.DrawText(layoutTime, x + offsetX*0.5f - layoutTime.Metrics.Width*0.5f, y2 + layout.Metrics.Height + layout.Metrics.Height*0.5f - layoutTime.Metrics.Height*0.5f);
					}
				}
				
				x += offsetX + Spacing;
			}
			
			if (countTrash > 0)
			{
				++shownCount;
				
				TextLayout layout = CountFont.GetTextLayout(countTrash.ToString());
				ShadowBrush.DrawLine(x + offsetX*0.5f, y - 1, x + offsetX*0.5f, y + height + 1, offsetX + 2);
				TrashBgBrush.DrawLine(x + offsetX*0.5f, y, x + offsetX*0.5f, y + height, offsetX);
				CountFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y);
				
				float yCountables = y;
				//foreach (Countable c in relevantCountables)
				foreach (int i in relevantCountables)
				{
					var c = Countables[i];
				
					yCountables -= Spacing*1.5f + CountableBarHeight;
					
					if (c.Brush is object)
					{
						ShadowBrush.DrawRectangle(x, yCountables, offsetX, CountableBarHeight);
						
						c.Brush.StrokeWidth = 2f;
						c.Brush.DrawRectangle(x, yCountables, offsetX, CountableBarHeight);
						
						c.Brush.StrokeWidth = 0f;
						c.Brush.DrawRectangle(x, yCountables, offsetX*(countTrash > 0 ? (float)trashCounts[i] / (float)countTrash : 0), CountableBarHeight);
						
						if (SelectedDecorator is object && Hud.Window.CursorInsideRect(x, yCountables, offsetX, CountableBarHeight))
						{
							foreach (IMonster monster in Hud.Game.AliveMonsters.Where(m => m.IsOnScreen && !m.IsElite && !c.IsActive(m))) //SelectedDecorator.Paint();
								PaintDecoratorCollection(SelectedDecorator, monster);
						}
					}
					
					if (c.Font is object)
					{
						layout = c.Font.GetTextLayout(countTrash > 0 ? trashCounts[i].ToString() : "0");
						c.Font.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, yCountables + CountableBarHeight*0.5f - layout.Metrics.Height*0.5f);
					}
				}
				
				if (isInRift)
				{
					float y2 = y + height*2f;
					
					double progress = (double)progressTrash * 100.0d / Hud.Game.MaxQuestProgress;
					layout = ProgressFont.GetTextLayout(progress.ToString("F2")); // + " / " + Hud.Game.MaxQuestProgress.ToString());
					ProgressFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y2);
					
					if (isInGreaterRift)
					{
						TimeSpan time = new TimeSpan((long)(progress * 90000000));
						TextLayout layoutTime = TimeFont.GetTextLayout(time.TotalSeconds < 60 ? time.TotalSeconds.ToString("F0") + "s" : time.ToString(@"m\m\ ss\s", CultureInfo.InvariantCulture));
						TimeFont.DrawText(layoutTime, x + offsetX*0.5f - layoutTime.Metrics.Width*0.5f, y2 + (countChampion > 0 || countRare > 0 ? layout.Metrics.Height*2 : layout.Metrics.Height) + layout.Metrics.Height*0.5f - layoutTime.Metrics.Height*0.5f);
					}
				}
				
				x += offsetX + Spacing;
			}
			
			if (countOrb > 0)
			{
				float radius = offsetX*0.3f;
				TextLayout layout = CountFont.GetTextLayout(countOrb.ToString());
				ShadowBrush.DrawEllipse(x + offsetX*0.5f, y + radius, radius + 1, radius + 1);
				OrbBgBrush.DrawEllipse(x + offsetX*0.5f, y + radius, radius, radius);
				CountFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y);
				
				float y2 = y + height*2f;
				
				double progress = 1.15d * countOrb;
				layout = orbFont.GetTextLayout(progress.ToString("F2")); // + " / " + Hud.Game.MaxQuestProgress.ToString());
				orbFont.DrawText(layout, x + offsetX*0.5f - layout.Metrics.Width*0.5f, y2);
				
				if (isInGreaterRift)
				{
					TimeSpan time = new TimeSpan((long)(progress * 90000000));
					TextLayout layoutTime = TimeFont.GetTextLayout(time.TotalSeconds < 60 ? time.TotalSeconds.ToString("F0") + "s" : time.ToString(@"m\m\ ss\s", CultureInfo.InvariantCulture));
					TimeFont.DrawText(layoutTime, x + offsetX*0.5f - layoutTime.Metrics.Width*0.5f, y2 + (countChampion > 0 || countRare > 0 ? layout.Metrics.Height*2 : layout.Metrics.Height) + layout.Metrics.Height*0.5f - layoutTime.Metrics.Height*0.5f);
				}
				
				x += offsetX + Spacing;
			}
			
			if (shownCount > 0)
			{
				//labels
				float x3 = area.Rectangle.X + deltaX;
				float yCountables = y;
				//foreach (Countable c in relevantCountables)
				foreach (int i in relevantCountables)
				{
					var c = Countables[i];
				
					yCountables -= Spacing*1.5f + CountableBarHeight;
					
					if (c.Font is object)
					{
						TextLayout label = c.Font.GetTextLayout(c.Name.ToUpper()); //c.Type.ToString()
						c.Font.DrawText(label, x3 - Spacing*2 - label.Metrics.Width, yCountables + CountableBarHeight*0.5f - label.Metrics.Height*0.5f);
					}
				}
				
				//total count
				TextLayout total = OtherFont.GetTextLayout("= " + countTotal); //"+" + 
				OtherFont.DrawText(total, x, y + height*0.5f - total.Metrics.Height*0.5f);
				
				if (isInRift)
				{
					float y2 = y + height*2f;
					float progressOrbTotal = progressRareOrb + progressChampionOrb + countOrb*1.15f;
					double progress = ((double)progressTotal * 100.0d / Hud.Game.MaxQuestProgress) + progressOrbTotal;
					IFont font = (progressOrbTotal > 0 ? orbFont : ProgressFont);
					TextLayout layout = font.GetTextLayout("= " + progress.ToString("F2")); // + " / " + Hud.Game.MaxQuestProgress.ToString());
					font.DrawText(layout, x + Spacing, y + height*2f);
					
					//progress label
					TextLayout label = LabelFont.GetTextLayout(TextRiftProgress.ToUpper());
					float fontSize = (float)label.GetFontSize(0);
					LabelFont.DrawText(label, x3 - Spacing - label.Metrics.Width, y2 + layout.Metrics.Height*0.5f - label.Metrics.Height*0.5f);
					
					if (isInGreaterRift)
					{
						//time label
						y2 += (countChampion > 0 || countRare > 0 ? layout.Metrics.Height*2 : layout.Metrics.Height);
						label = TimeFont.GetTextLayout(TextTimeProgress.ToUpper());
						label.SetFontSize(fontSize, new TextRange(0, TextTimeProgress.Length));
						TimeFont.DrawText(label, x3 - Spacing - label.Metrics.Width, y2 + layout.Metrics.Height*0.5f - label.Metrics.Height*0.5f);
						
						//total time
						TimeSpan time = new TimeSpan((long)(progress * 90000000));
						TextLayout layoutTime = TimeFont.GetTextLayout("= " + (time.TotalSeconds < 60 ? time.TotalSeconds.ToString("F0") + "s" : time.ToString(@"m\m\ ss\s", CultureInfo.InvariantCulture)));
						TimeFont.DrawText(layoutTime, x + offsetX*0.5f - layoutTime.Metrics.Width*0.5f, y2 + layout.Metrics.Height*0.5f - layoutTime.Metrics.Height*0.5f);
					}
				}
			}
		}
		
		private void PaintDecoratorCollection(WorldDecoratorCollection d, IActor a, string text = null)
		{
			foreach (var decorator in d.Decorators)
			{
				if (decorator.Enabled)
					decorator.Paint(a, a.FloorCoordinate, text);
			}
		}
	}
}