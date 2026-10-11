namespace Turbo.Plugins.Razor.Monster
{
	using System;
	using SharpDX.DirectWrite; //TextLayout
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

	public class MonsterMarkers : BasePlugin, IInGameTopPainter //IInGameWorldPainter
	{
		public bool MarkAllMonsters { get; set; } = true; // will use DefaultMarker if it is defined and no other marker definitions apply to that monster
		
		public MonsterMarker DefaultMarker { get; set; }
		public Func<IMonster, IWorldCoordinate> DecoratorPosition { get; set; } = (m) => m.FloorCoordinate;
		public Func<IMonster, IScreenCoordinate> SymbolPosition { get; set; } = (m) => m.FloorCoordinate.ToScreenCoordinate();
		
		public List<MonsterMarker> Markers { get; set; } = new List<MonsterMarker>(); //marker, relevance cache
		public float Spacing { get; set; } = 2f; //horizontal spacing between markers

		public MonsterMarkers()
		{
			Enabled = false;
			Order = 60004;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);

			DefaultMarker = new MonsterMarker() {
				IsRelevant = () => Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.WitchDoctor, //already have these being marked with a WD plugin
				IsMarked = (m) => true,
				Font = Hud.Render.CreateFont("tahoma", 11f, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true),
				Symbol = "☻", //•
			};
		}

		//public void PaintWorld(WorldLayer layer)
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip || Hud.Game.IsInTown)
				return;
			
			//var monsters = Hud.Game.AliveMonsters.Where(m => m.Attackable);
			Dictionary<MonsterMarker, TextLayout> cached = new Dictionary<MonsterMarker, TextLayout>();
			var rMarkers = Markers.Where(m => m.IsRelevant());
			foreach (var monster in Hud.Game.AliveMonsters) //.Where(m => m.Attackable)
			{
				List<MonsterMarker> symbols = new List<MonsterMarker>();
				foreach (MonsterMarker marker in rMarkers)
				{
					if (marker.IsMarked(monster))
					{
						if (marker.Font is object && !string.IsNullOrEmpty(marker.Symbol) && !cached.ContainsKey(marker))
							cached[marker] = marker.Font.GetTextLayout(marker.Symbol);
						
						symbols.Add(marker);
					}
				}
				
				if (symbols.Count == 0 && MarkAllMonsters && DefaultMarker is object)
				{
					if (DefaultMarker.Font is object && !cached.ContainsKey(DefaultMarker))
						cached[DefaultMarker] = DefaultMarker.Font.GetTextLayout(DefaultMarker.Symbol);

					symbols.Add(DefaultMarker);
				}
				
				if (symbols.Count > 0)
				{
					float totalWidth = symbols.Where(s => s.Font is object).Sum(s => cached[s].Metrics.Width) + Spacing*(symbols.Count-1);
					IScreenCoordinate floor = SymbolPosition(monster); //monster.FloorCoordinate.ToScreenCoordinate();
					float x = floor.X - totalWidth*0.5f;
					foreach (MonsterMarker symbol in symbols)
					{
						TextLayout layout = cached[symbol];
						symbol.Font.DrawText(layout, x, floor.Y - layout.Metrics.Height*0.5f);
						x += layout.Metrics.Width + Spacing;
						
						if (symbol.Decorator is object)
						{
							var pos = symbol.DecoratorPosition is object ? symbol.DecoratorPosition(monster) : DecoratorPosition(monster); //monster.FloorCoordinate
							symbol.Decorator.Paint(WorldLayer.Ground, monster, pos, symbol.Symbol);
							symbol.Decorator.Paint(WorldLayer.Map, monster, pos, symbol.Symbol);
						}
					}
				}
			}
		}
	}
	
	public class MonsterMarker
	{
		public Func<bool> IsRelevant { get; set; }
		public Func<IMonster, bool> IsMarked { get; set; }
		public IFont Font { get; set; }
		public string Symbol { get; set; }
		public WorldDecoratorCollection Decorator { get; set; }
		public Func<IMonster, IWorldCoordinate> DecoratorPosition { get; set; } //optional, default is at feet
		
		//public MonsterMarker() {}			
	}
}