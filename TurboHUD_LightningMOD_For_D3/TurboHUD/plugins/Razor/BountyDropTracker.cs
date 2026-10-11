/*

This is a rewrite and expansion of Zerious' BountyCacheIndicatorPlugin.
In addition to marking nearby bounty bags (Horadric caches) on the ground and on the minimap, it remembers their last seen location for marking from a greater distance and shows a notification of dropped caches with location data on your screen.

June 25, 2021
	- implemented updated version of LocationInfo
	- CachesOnFloor is now public so its data can be shared with other plugins (e.g. MenuBounties)
Oct 10, 2020
	- implemented world area association fixes
Oct 9, 2020
	- separated location routines into LocationInfo, a background data collecting plugin, so that its polling results can be shared with other plugins (like one that marks xp pool locations)
	- added dropped cache name to map marker
Sept 29, 2020
	- fixed race condition for waypoint area name corrections (when traveling across acts to a waypoint that hasn't checked for a name correction yet, it may record the previous area name because the displayed area name (in IUiElement) hasn't updated yet)
	- attempt to auto-correct occasional situation where IItem.Seed changes mid-game
	- add minimap line hotkey (default: LAlt) toggle behavior
	- show waypoints for connected open world areas
	- fixed drop notification display for non-open world areas (i.e. rifts)
Sept 27, 2020
	- moved away from using WorldIds because they vary by game

*/

namespace Turbo.Plugins.Razor
{
	using System.Collections.Generic;
	using System.Linq;
	using SharpDX.DirectWrite; //TextLayout
	using System.Windows.Forms; //Keys
	
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Movable;
	using Turbo.Plugins.Razor.Util; //LocationInfo
 
	public class BountyDropTracker : BasePlugin, IInGameWorldPainter, IAfterCollectHandler, INewAreaHandler, IMovable
	{
		public bool AnnounceCacheDrop { get; set; }
		public int DrawBountyCacheMapLine { get; set; } = 2; //0 = off, 1 = all the time, 2 = only when hotkey is held down
		public bool MarkOnMinimap { get; set; }
		
		public string TextAct { get; set; } = "Акт";
		
		public Keys HotkeyShowLine { get; set; } = Keys.LMenu;
		
		public WorldDecoratorCollection BountyCacheDecorator { get; set; }
		public MapShapeDecorator BountyCacheMapMarker { get; set; }
		public MapShapeDecorator BountyCacheMapLine { get; set; }
		//public string GroundLabelText;
		
		public IFont TextFont { get; set; }
		public IFont ItemFont { get; set; }
		
		public class CacheLocationInfo
		{
			public int Seed { get; set; }
			public string Name { get; set; }
			public int Act { get; set; }
			public ISnoArea Area { get; set; }
			//public ISnoArea WaypointArea { get; set; }
			public WaypointInfo Waypoint { get; set; }
			public IWorldCoordinate FloorCoordinate { get; set; }
			//public int WorldId { get; set; }
			public int WorldSno { get; set; }
			
			public CacheLocationInfo(IItem item)
			{
				Seed = item.Seed;
				Name = item.FullNameLocalized;
				//Act = Area.Act(); //CacheLocationInfo.ActFix.ContainsKey(Area.Sno) ? CacheLocationInfo.ActFix[Area.Sno] : Area.Act;
				//Area = item.Scene.SnoArea;
				FloorCoordinate = item.FloorCoordinate.Offset(0, 0, 0); //Hud.Window.CreateWorldCoordinate(item.FloorCoordinate); //make a copy
				//WorldSno = Area.WorldSno();
				//, item.Scene.SnoArea.IsTown ? LocationInfo.NearestWaypointAreaSno : LocationInfo.NearestWaypointAreaSnoOutsideTown
			}
		}
		public Dictionary<int, CacheLocationInfo> CachesOnFloor = new Dictionary<int, CacheLocationInfo>();
		
		private LocationInfo LocationInfo;
 
		public BountyDropTracker()
		{
			Enabled = true;
		}
 
		public override void Load(IController hud)
		{
			base.Load(hud);
			
			TextFont = Hud.Render.CreateFont("tahoma", 9f, 255, 255, 0, 0, true, false, false);
			ItemFont = Hud.Render.CreateFont("tahoma", 9f, 200, 200, 200, 200, true, false, false);
 
			BountyCacheMapMarker = new MapShapeDecorator(Hud)
			{
				ShapePainter = new RotatingTriangleShapePainter(Hud),
				Brush = Hud.Render.CreateBrush(220, 255, 0, 0, 3),
				Radius = 10f,
				//RadiusTransformator = new StandardPingRadiusTransformator(Hud, 250)
			};

			BountyCacheDecorator = new WorldDecoratorCollection(
				new GroundShapeDecorator(Hud)
				{
                    Brush = Hud.Render.CreateBrush(240, 255, 0, 0, 4),
					ShadowBrush = Hud.Render.CreateBrush(75, 0, 0, 0, 2),
                    Radius = 2f, //1.5f,
					ShapePainter = WorldStarShapePainter.NewTriangle(Hud), //NewPentagram(Hud),
					//RadiusTransformator = new StandardPingRadiusTransformator(Hud, 300),
					RotationTransformator = new ClockwiseRotationTransformator(Hud, 3)
                },
                /*new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(220, 200, 200, 0, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 255, 255, 255, true, false, false)
                },*/
				new MapLabelDecorator(Hud)
				{
					LabelFont = Hud.Render.CreateFont("tahoma", 6f, 255, 255, 0, 0, false, false, 128, 0, 0, 0, true),
					RadiusOffset = 10,
					Up = true,
				},
				BountyCacheMapMarker
                
            );
			
			
			if (DrawBountyCacheMapLine > 0)
			{
				BountyCacheMapLine = new MapShapeDecorator(Hud)
                {
                    Brush = Hud.Render.CreateBrush(240, 255, 0, 0, -1),
                    ShapePainter = new LineFromMeShapePainter(Hud)
                };
				
				if (DrawBountyCacheMapLine == 1)
					BountyCacheDecorator.Decorators.Add(BountyCacheMapLine);
			}
			
        }
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
				CachesOnFloor.Clear();
		}
		
		public void AfterCollect()
		{
			if (LocationInfo == null)
				LocationInfo = (LocationInfo)Hud.GetPlugin<LocationInfo>();
			
			if (!LocationInfo.IsAdventureMode)
				return;
			
			//keep track of caches
			foreach (IItem item in Hud.Game.Items.Where(item => item.SnoItem.MainGroupCode == "horadriccache"))
			{
				//on the floor
				if (item.Location == ItemLocation.Floor)
				{
					if (CachesOnFloor.ContainsKey(item.Seed))
						continue;

					//add item to the list
					var cache = new CacheLocationInfo(item) {
						Area = Hud.Game.Me.SnoArea,
						WorldSno = LocationInfo.WorldSno,
						Act = LocationInfo.Act,
					};
					
					if (LocationInfo.IsInOpenWorld)
						cache.Waypoint = LocationInfo.NearestWaypoint;
					
					CachesOnFloor.Add(item.Seed, cache); //, (LocationInfo.NearestWaypointAreaSnoOutsideTown == 0 || Hud.Game.IsInTown ? LocationInfo.NearestWaypointAreaSno : LocationInfo.NearestWaypointAreaSnoOutsideTown) //Hud.Game.ActMapWaypoints.FirstOrDefault(w => w.TargetSnoArea.Sno == LocationInfo.NearestWaypointAreaSno)?.TargetSnoArea)
				}
				else if (CachesOnFloor.ContainsKey(item.Seed)) //not on the floor
				{
					//remove item from ground list
					CachesOnFloor.Remove(item.Seed);
				}
			}
			
			//possible (but rare?) race condition where cache gets picked up but marker still remains on the ground (item.Seed changed)
			foreach (CacheLocationInfo info in CachesOnFloor.Values.Where(c => c.FloorCoordinate is object && c.FloorCoordinate.IsOnScreen() && !Hud.Game.Items.Any(item => item.SnoItem.MainGroupCode == "horadriccache" && item.FloorCoordinate.Equals(c.FloorCoordinate))).ToArray())
				CachesOnFloor.Remove(info.Seed);
		}

		public void PaintWorld(WorldLayer layer)
		{            
			/*var items = Hud.Game.Items.Where(item => item.Location == ItemLocation.Floor && item.SnoItem.MainGroupCode == "horadriccache");
 
			foreach (var item in items)
			{
				//var text = string.IsNullOrWhiteSpace(GroundLabelText) ? item.SnoItem.NameLocalized : GroundLabelText;
				BountyCacheDecorator.Paint(layer, item, item.FloorCoordinate, "test " + string.Join(" ", CachesOnFloor.Values.Select(cache => cache.Act.ToString()))); //cache.Area.NameLocalized //Hud.Game.Items.Count(item => item.Location == ItemLocation.Floor && item.SnoItem.MainGroupCode == "horadriccache"));
			}*/
			
			foreach (CacheLocationInfo info in CachesOnFloor.Values.Where(c => c.Area.Sno == Hud.Game.Me.SnoArea.Sno)) // || (LocationInfo.GetWorldSno(c.WorldSno) == Hud.Game.Me.SnoArea))) //c.Area.SnoWorld?.Sno == Hud.Game.Me.WorldSno))
			{
				if (DrawBountyCacheMapLine == 2)
				{
					bool lined = BountyCacheDecorator.Decorators.Contains(BountyCacheMapLine);
					if (Hud.Input.IsKeyDown(HotkeyShowLine))
					{
						if (!lined)
							BountyCacheDecorator.Decorators.Add(BountyCacheMapLine);
					}
					else if (lined)
						BountyCacheDecorator.Decorators.Remove(BountyCacheMapLine);
				}
				
				BountyCacheDecorator.Paint(layer, null, info.FloorCoordinate, info.Name); //string.Join(" ", CachesOnFloor.Values.Select(cache => cache.Act.ToString()))); //cache.Area.NameLocalized //Hud.Game.Items.Count(item => item.Location == ItemLocation.Floor && item.SnoItem.MainGroupCode == "horadriccache"));
			}
		}
		
		public void OnRegister(MovableController mover)
		{
			//initialize position and dimension elements
			/*IPlayer me = Hud.Game.Me;
			IScreenCoordinate pos = me.FloorCoordinate.ToScreenCoordinate();
			TextLayout notify = NotifyFont.GetTextLayout(ImmunityText); //ImmunityText
			float h = Hud.Window.Size.Height * 0.001667f * BarHeight;
			float w = Hud.Window.Size.Width * 0.00155f * BarWidth;
			w = Math.Max(w, notify.Metrics.Width);
			h += notify.Metrics.Height + 3; */

			mover.CreateArea(
				this,
				"Alert", //area name
				new System.Drawing.RectangleF(Hud.Window.Size.Width*0.5f - 100, Hud.Window.Size.Height*0.2f, 200, 100), //position + dimensions
				true, //enabled at start?
				true, //save to config file?
				ResizeMode.Off //resizable
			);
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			float x = area.Rectangle.X;
			float y = area.Rectangle.Y;
			
			//TextLayout layout = TextFont.GetTextLayout(string.Format("{0} ({1}) <-> {2} ({3})\nCurrent WorldId: {4}, Current Area: {5}, Current Act: {6}, CurrentWorldSno: {7}", LocationInfo.NearestWaypointArea.Area.Sno, LocationInfo.NearestWaypointArea.Act, LocationInfo.NearestWaypointAreaOutsideTown.Area.Sno, LocationInfo.NearestWaypointAreaOutsideTown.Act, Hud.Game.Me.WorldId, Hud.Game.Me.SnoArea.Sno, Hud.Game.CurrentAct, Hud.Game.Me.SnoWorld?.Sno)); //Hud.Game.Me.SnoArea.Act() //, string.Join(".", Hud.Game.Me.SnoWorld.SnoAreas.Select(a => a.Sno))
			/*TextLayout layout = TextFont.GetTextLayout(string.Format("Missing FloorCoordinates: {0}\nCurrent Area: {1} ({2}), Current Act: {3}, CurrentWorldSno: {4} ({5})\nLastSeenWaypoint: {6} ({7}), NearestWaypointArea: {8}, NearestOutsideTown: {9}\n", string.Join(", ", LocationInfo.Waypoints.Values.Where(w => w.WorldId > 0).Select(w => w.WorldId)), Hud.Game.Me.SnoArea.Sno, Hud.Game.Me.SnoArea.NameLocalized, Hud.Game.Me.SnoArea.Act(), Hud.Game.Me.SnoArea.WorldSno(), Hud.Game.Me.WorldId, LocationInfo.LastSeenWaypoint.Area.NameLocalized, LocationInfo.LastSeenWaypoint?.Area.Sno, LocationInfo.NearestWaypointArea?.Area.Sno, LocationInfo.NearestWaypointAreaOutsideTown?.Area.Sno )); //Hud.Game.Me.SnoArea.WorldSno() //Waypoints.Values.Where(w => w.FloorCoordinate == null).Select(w => w.Area.Sno.ToString())
			TextFont.DrawText(layout, x, y);
			y += layout.Metrics.Height*1.2f;*/
			
			/*if (LocationInfo.NearestWaypointArea is object)
			{
				layout = TextFont.GetTextLayout(
					string.Format("{0} ({1}) <-> {2} ({3})\nIsInOpenWorld: {4}, IsKeyDown: {5}", 
						LocationInfo.NearestWaypointArea.Area.Sno, 
						LocationInfo.NearestWaypointArea.Act, 
						LocationInfo.NearestWaypointAreaOutsideTown?.Area.Sno, 
						LocationInfo.NearestWaypointAreaOutsideTown?.Act,
						LocationInfo.IsInOpenWorld, //(Hud.Game.Me.InGreaterRift && Hud.Game.Me.InGreaterRiftRank > 0) || Hud.Game.SpecialArea == SpecialArea.Rift
						Hud.Input.IsKeyDown(HotkeyShowLine)
					)
				);
				TextFont.DrawText(layout, x, y);
				y += layout.Metrics.Height*1.2f;
			}*/
			//y += layout.Metrics.Height*1.1f;
			TextLayout layout;
			
			foreach (CacheLocationInfo cache in CachesOnFloor.Values)
			{
				layout = ItemFont.GetTextLayout(cache.Name);
				ItemFont.DrawText(layout, x, y);
				
				float x2 = x + layout.Metrics.Width + 5;
				string areaName = LocationInfo.GetDisplayedName(cache.Area);
				
				if (cache.Act == 0)
					layout = TextFont.GetTextLayout("🡇 " + areaName); //cache.Area.NameLocalized
				else
				{
					//string areaName = LocationInfo.AreaNameCorrections.ContainsKey(cache.Area.Sno) && !string.IsNullOrEmpty(LocationInfo.AreaNameCorrections[cache.Area.Sno]) ? LocationInfo.AreaNameCorrections[cache.Area.Sno] : cache.Area.NameLocalized; //string.IsNullOrEmpty(cache.NameCorrection) ? cache.Area.NameLocalized : cache.NameCorrection;					
					if (cache.Waypoint == null || cache.Area.Sno == cache.Waypoint.Area.Sno)
						layout = TextFont.GetTextLayout(string.Format("🡇 {0} {1} - {2}", TextAct, cache.Act, areaName)); //string.Format("🡇 {0} {1} - {2} ({3})", TextAct, cache.Act, areaName, cache.Area.Sno)
					else
					{
						string waypointName = LocationInfo.GetDisplayedName(cache.Waypoint.Area);
						
						layout = TextFont.GetTextLayout(waypointName == areaName ?
							string.Format("🡇 {0} {1} - {2}", TextAct, cache.Act, areaName) : //string.Format("🡇 {0} {1} - {2} ({3})", TextAct, cache.Act, areaName, cache.Area.Sno) :
							string.Format("🡇 {0} {1} - {2} - {3}", TextAct, cache.Act, waypointName, areaName) //string.Format("🡇 {0} {1} - {2} ({3}) - {4}", TextAct, cache.Act, waypointName, cache.Waypoint.Area.Sno, areaName)
						);
					}
				}
				
				//TextLayout layout = TextFont.GetTextLayout(string.Join("\n", CachesOnFloor.Values.Select(c => string.Format("{0} - {1} - {2}", c.WorldId, Hud.Game.ActMapWaypoints.FirstOrDefault(w => w.TargetSnoArea.Sno == c.WaypointAreaSno)?.TargetSnoArea.NameLocalized, c.Area.NameLocalized)))); //c.FloorCoordinate //LocationInfo.NearestWaypointAreaSno, LocationInfo.NearestWaypointAreaSnoOutsideTown))));  //c.WaypointAreaSno
				TextFont.DrawText(layout, x2, y);
				BountyCacheMapMarker.ShapePainter.Paint(x - 15, y + 11, 8f, BountyCacheMapMarker.Brush, null);
				
				y += layout.Metrics.Height*1.15f;
			}
		}
    }
}