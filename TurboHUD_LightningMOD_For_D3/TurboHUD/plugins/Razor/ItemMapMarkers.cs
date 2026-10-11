/*

Remembers and marks on the minimap items that match customizable rules.

Changelog
September 23, 2021
	- rewrite of a feature from the old RunStats\LootHelper as a standalone plugin

*/

namespace Turbo.Plugins.Razor
{
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;
	
    public class ItemMapMarkers : BasePlugin, IAfterCollectHandler, INewAreaHandler, IInGameWorldPainter, IItemPickedHandler
    {
		//ItemMapMarkers properties
		public bool MarkInTown { get; set; } = true;
		public List<ItemRule> ItemRules { get; set; } = new List<ItemRule>();
		
		public class ItemRule
		{
			public System.Func<IItem, bool> Check { get; set; }
			public WorldDecoratorCollection Decorator { get; set; }
			public Dictionary<int, IItem> WorldItems { get; private set; } = new Dictionary<int, IItem>();
			public Dictionary<int, IItem> RiftItems { get; private set; } = new Dictionary<int, IItem>();
		}
		
		private int LastUpdateTick;
		private int UpdateInterval = 30; //every 30 ticks (a half second)
		private int ExpirationInterval = 60 * 60 * 60 * 2; //every 2 hrs
		private bool RiftOpen;
		
        public ItemMapMarkers()
        {
            Enabled = true;
        }
		
		public override void Load(IController hud)
        {
            base.Load(hud);
			
			ItemRules.Add(new ItemRule() {
				Check = (item) => item.Quality == ItemQuality.Legendary && item.AncientRank > 0 && (!item.AccountBound || item.BoundToMyAccount),
				Decorator = new WorldDecoratorCollection(
					new MapShapeDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(225, 255, 120, 0, -1),
						//ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
						Radius = 16f,
						ShapePainter = new CircleShapePainter(Hud),
					},
					new MapShapeDecorator(Hud)
					{
						ShapePainter = new RotatingTriangleShapePainter(Hud),
						Brush = Hud.Render.CreateBrush(225, 255, 120, 0, 3),
						ShadowBrush = Hud.Render.CreateBrush(96, 0, 0, 0, 1),
						Radius = 11f,
						RadiusTransformator = new StandardPingRadiusTransformator(Hud, 333),
					},
					new MapLabelDecorator(Hud)
					{
						LabelFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 255, 120, 0, false, false, 55, 0, 0, 0, true),
						RadiusOffset = 16f,
						Up = true,
					}
				)
			});
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				foreach (ItemRule rule in ItemRules)
				{
					rule.RiftItems.Clear();
					rule.WorldItems.Clear();
				}
			}
			else
			{
				//clear out expired items
				foreach (ItemRule rule in ItemRules)
				{
					foreach (int key in rule.WorldItems.Keys.Where(k => Hud.Game.CurrentGameTick - rule.WorldItems[k].CreatedAtInGameTick > ExpirationInterval).ToArray())
						rule.WorldItems.Remove(key);
					foreach (int key in rule.RiftItems.Keys.Where(k => Hud.Game.CurrentGameTick - rule.RiftItems[k].CreatedAtInGameTick > ExpirationInterval).ToArray())
						rule.RiftItems.Remove(key);
				}
			}
		}
		
		public void OnItemPicked(IItem item)
		{
			foreach (ItemRule rule in ItemRules)
			{
				if (rule.Check(item))
				{
					if (rule.RiftItems.ContainsKey(item.Seed))
						rule.RiftItems.Remove(item.Seed);
					else if (rule.WorldItems.ContainsKey(item.Seed))
						rule.WorldItems.Remove(item.Seed);
				}
			}
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame || (Hud.Game.IsInTown && !MarkInTown))
				return;
			
			int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
			if (diff < 0 || diff > UpdateInterval)
			{
				//delete recorded rift items when rift is closed
				if (RiftOpen != Hud.Game.Quests.Any(q => q.SnoQuest.Sno == 337492 && q.State == QuestState.started))
				{
					if (RiftOpen) //close rift
					{
						RiftOpen = false;
						
						foreach (ItemRule rule in ItemRules)
							rule.RiftItems.Clear();
					}
					else
						RiftOpen = true;
				}
				
				//poll all items seen
				bool isInRift = RiftOpen && ((Hud.Game.Me.InGreaterRift && Hud.Game.Me.InGreaterRiftRank > 0) || Hud.Game.SpecialArea == SpecialArea.Rift);
				foreach (IItem item in Hud.Game.Items)
				{
					foreach (ItemRule rule in ItemRules)
					{
						if (rule.Check(item))
						{
							var collection = isInRift ? rule.RiftItems : rule.WorldItems;
							
							if (collection.ContainsKey(item.Seed))
							{
								if (item.Location != ItemLocation.Floor)
									collection.Remove(item.Seed);
								else
									collection[item.Seed] = item;
							}
							else if (item.Location == ItemLocation.Floor)
								collection[item.Seed] = item;
						}
					}
				}
				
				//sync: remove items that are supposed to be on the screen but are not
				foreach (ItemRule rule in ItemRules)
				{
					var collection = isInRift ? rule.RiftItems : rule.WorldItems;
					foreach (int key in collection.Values.Where(i => i.WorldId == Hud.Game.Me.WorldId && i.FloorCoordinate.IsOnScreen() && !Hud.Game.Items.Any(itm => itm.Seed == i.Seed)).Select(i => i.Seed).ToArray())
						collection.Remove(key);
				}
			}
		}
		
		//public void PaintTopInGame(ClipState clipState)
		public void PaintWorld(WorldLayer layer)
        {
			if (layer != WorldLayer.Map)
				return;
			
			bool isInRift = RiftOpen && ((Hud.Game.Me.InGreaterRift && Hud.Game.Me.InGreaterRiftRank > 0) || Hud.Game.SpecialArea == SpecialArea.Rift);
			foreach (ItemRule rule in ItemRules)
			{
				var collection = isInRift ? rule.RiftItems : rule.WorldItems;
				
				foreach (IItem item in collection.Values.Where(i => i.WorldId == Hud.Game.Me.WorldId))
					rule.Decorator.Paint(layer, item, item.FloorCoordinate, item.FullNameLocalized);
			}
		}
    }
}