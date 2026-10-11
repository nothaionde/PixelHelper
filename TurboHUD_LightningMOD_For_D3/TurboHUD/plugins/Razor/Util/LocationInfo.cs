/*

This is a background plugin collecting and correcting locational data for other plugins to use.

Dec 11, 2020
- fixed some NullPointerExceptions that only happen in certain situations
Oct 12, 2020
- WorldId added back in because IMarker has no access to worldsno
Oct 10, 2020
- fixed Act 4 "realm" areas not being associated with each other
- fixed Act 2 Howling bridge not being associated with that world area group

*/

namespace Turbo.Plugins.Razor.Util
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	
	using Turbo.Plugins.Default;
 
	public class LocationInfo : BasePlugin, IAfterCollectHandler, INewAreaHandler
	{
		public Dictionary<uint, WaypointInfo> Waypoints { get; private set; }
		public Dictionary<uint, string> DisplayedAreaNames { get; private set; } = new Dictionary<uint, string>(); //area sno, namelocalized
		public int Act { get; private set; }
		public int WorldSno { get; private set; }
		public bool IsAdventureMode { get; private set; }
		public bool IsInOpenWorld { get; private set; }
		public WaypointInfo NearestWaypoint { get; private set; }
		
		//private Dictionary<uint, int> CheckAreaNames = new Dictionary<uint, int>(); //area sno, game tick
		private uint LastSeenAreaSno;
		private int LastSeenAreaTick;
		private int LastUpdateTick;
		
		//act fix data by RNN + a correction
		private Dictionary<uint, int> AreaActFix = new Dictionary<uint, int> {
			{ 288482, 0 }, { 288684, 0 }, { 288686, 0 }, { 288797, 0 }, { 288799, 0 }, // GR
			{ 288801, 0 }, { 288803, 0 }, { 288809, 0 }, { 288812, 0 }, { 288813, 0 }, // GR
			{ 445426, 1 }, //{  63666, 1 }, //63666 is correctly identified as Act 2 by default (Stinging Winds wp)
			{ 456638, 2 }, { 460671, 2 }, { 464092, 2 }, { 464830, 2 }, { 465885, 2 }, { 467383, 2 },
			{ 444307, 3 }, { 445762, 3 },
			{ 444396, 4 }, { 445792, 4 }, { 446367, 4 }, { 446550, 4 }, { 448011, 4 }, { 448039, 4 },
			{ 464063, 4 }, { 464065, 4 }, { 464066, 4 }, { 464810, 4 }, { 464820, 4 }, { 464821, 4 },
			{ 464822, 4 }, { 464857, 4 }, { 464858, 4 }, { 464865, 4 }, { 464867, 4 }, { 464868, 4 },
			{ 464870, 4 }, { 464871, 4 }, { 464873, 4 }, { 464874, 4 }, { 464875, 4 }, { 464882, 4 },
			{ 464886, 4 }, { 464889, 4 }, { 464890, 4 }, { 464940, 4 }, { 464941, 4 }, { 464942, 4 },
			{ 464943, 4 }, { 464944, 4 }, { 475854, 4 }, { 475856, 4 },
			{ 448391, 5 }, { 448368, 5 }, { 448375, 5 }, { 448398, 5 }, { 448404, 5 }, { 448411, 5 },
		};

		//for stitched worlds like the "realms" in Act 4 that have no SnoWorld or WorldId
		private Dictionary<uint, int> AreaWorldFix = new Dictionary<uint, int> {
			{ 464857, -10 }, { 464858, -10 }, { 464865, -10 }, { 464873, -10 }, { 464867, -10 }, { 464868, -10 }, { 464870, -10 }, { 464875, -10 }, { 464874, -10 }, { 464871, -10 }, //Fractured Fate
			{ 475854, -11 }, { 464890, -11 }, { 464889, -11 }, { 464886, -11 }, { 464882, -11 }, { 464063, -11 }, //Lower + Upper Infernal Fate
			{ 475856, -12 }, { 464943, -12 }, { 464942, -12 }, { 464941, -12 }, { 464940, -12 }, { 464066, -12 }, //Upper + Lower Cursed Fate
			{ 464065, -13 }, { 464820, -13 }, { 464810, -13 }, { 464821, -13 }, { 464822, -13 }, //Unbending Fate (there is a tiny gap area that TH doesn't recognize as a valid area)
			{ 170118, 70885 }, //Howling Plateau Bridge
			//{ 57425, -9 }, //Dalgur Oasis - should be disconnected from other areas of this snoworld
		};

		
		public LocationInfo()
		{
			Enabled = true;
			Order = -1000;
		}
 
		public override void Load(IController hud)
		{
			base.Load(hud);
			
			//register for access to displayed area names
			Hud.Render.RegisterUiElement("Root.NormalLayer.minimap_dialog_backgroundScreen.minimap_dialog_pve.area_name", Hud.Render.MinimapUiElement, null);
			
			//waypoint data for towns
			/*InitWaypointLocations.Add(332339, Hud.Window.CreateWorldCoordinate(401.731f, 555.009f, 24.7f)); //act 1
			InitWaypointLocations.Add(168314, Hud.Window.CreateWorldCoordinate(324.870f, 291.031f, 0.8f)); //act 2
			InitWaypointLocations.Add(92945, Hud.Window.CreateWorldCoordinate(402.540f, 414.342f, 0.7f)); //act 3 + 4
			InitWaypointLocations.Add(270011, Hud.Window.CreateWorldCoordinate(557.546f, 765.339f, 2.7f)); //act 5
			
			//waypoint data from s4000 - some positions can vary, so LocationInfo will update these dynamically as it "sees" waypoints
			Waypoints.Add(91133, new WaypointInfo(1976.710f, 2788.146f, 41.2f));
            Waypoints.Add(19780, new WaypointInfo(820.000f, 749.307f, 0.0f));
            Waypoints.Add(19783, new WaypointInfo(820.000f, 701.500f, 0.0f));
            Waypoints.Add(19785, new WaypointInfo(820.000f, 1229.307f, 0.0f));
            Waypoints.Add(19787, new WaypointInfo(1059.357f, 578.648f, 0.0f));
            Waypoints.Add(19954, new WaypointInfo(2895.395f, 2371.276f, 0.0f));
            Waypoints.Add(72712, new WaypointInfo(2161.802f, 1826.882f, 1.9f));
            Waypoints.Add(19952, new WaypointInfo(2037.839f, 910.656f, 1.9f));
            Waypoints.Add(61573, new WaypointInfo(1263.054f, 827.765f, 63.1f));
            Waypoints.Add(19953, new WaypointInfo(423.106f, 781.156f, 21.2f));
            Waypoints.Add(93632, new WaypointInfo(2310.500f, 4770.000f, 1.0f));
            Waypoints.Add(78572, new WaypointInfo(1339.771f, 1159.466f, 0.0f));
            Waypoints.Add(19941, new WaypointInfo(1688.828f, 3890.089f, 41.9f));
            Waypoints.Add(19943, new WaypointInfo(1080.022f, 3444.448f, 62.4f));
            Waypoints.Add(19774, new WaypointInfo(820.000f, 736.500f, 0.0f));
            Waypoints.Add(119870, new WaypointInfo(56.772f, 146.284f, 0.0f));
            Waypoints.Add(19775, new WaypointInfo(452.500f, 820.000f, 0.0f));
            Waypoints.Add(19776, new WaypointInfo(979.000f, 1060.000f, 0.0f));
 
            Waypoints.Add(19836, new WaypointInfo(2760.302f, 1631.820f, 187.5f));
            Waypoints.Add(63666, new WaypointInfo(1419.449f, 321.863f, 176.3f));
            Waypoints.Add(19835, new WaypointInfo(1427.769f, 1185.675f, 186.2f));
            Waypoints.Add(460671, new WaypointInfo(1747.454f, 549.045f, 0.5f));
            Waypoints.Add(456638, new WaypointInfo(400.119f, 889.819f, 0.2f));
            Waypoints.Add(210451, new WaypointInfo(612.485f, 936.556f, -29.8f));
            Waypoints.Add(57425, new WaypointInfo(3548.838f, 4269.278f, 101.9f));
            Waypoints.Add(62752, new WaypointInfo(90.000f, 59.500f, 2.7f));
            Waypoints.Add(53834, new WaypointInfo(1257.886f, 3968.300f, 111.8f));
            Waypoints.Add(19800, new WaypointInfo(799.995f, 680.024f, -0.1f));
 
            Waypoints.Add(75436, new WaypointInfo(1065.148f, 472.449f, 0.0f));
            Waypoints.Add(93103, new WaypointInfo(825.148f, 1192.449f, 0.0f));
            Waypoints.Add(136448, new WaypointInfo(825.148f, 472.449f, 0.0f));
            Waypoints.Add(93173, new WaypointInfo(4272.158f, 4252.097f, -24.8f));
            Waypoints.Add(154644, new WaypointInfo(4356.238f, 408.241f, -2.9f));
            Waypoints.Add(155048, new WaypointInfo(3452.838f, 609.227f, 0.2f));
            Waypoints.Add(69504, new WaypointInfo(2708.557f, 586.267f, 0.0f)); //Rakkis Crossing //this worldid + areasno has also shown up as act 1 cave of the moon clan
            Waypoints.Add(86080, new WaypointInfo(1679.054f, 744.707f, 0.0f));
            Waypoints.Add(80791, new WaypointInfo(1041.245f, 983.039f, -10.0f));
            Waypoints.Add(119305, new WaypointInfo(2573.769f, 1206.666f, 0.0f));
            Waypoints.Add(119653, new WaypointInfo(959.404f, 1097.913f, -10.0f));
            Waypoints.Add(119306, new WaypointInfo(1162.255f, 686.218f, 0.0f));
            Waypoints.Add(428494, new WaypointInfo(606.110f, 1065.332f, 0.0f));
 
            Waypoints.Add(109526, new WaypointInfo(1073.872f, 786.390f, 0.0f));
            Waypoints.Add(464857, new WaypointInfo(1340.086f, 110.277f, 0.0f));
            Waypoints.Add(409001, new WaypointInfo(1339.833f, 359.946f, -1.3f));
            Waypoints.Add(464065, new WaypointInfo(1030.028f, 870.214f, 15.0f));
            Waypoints.Add(409512, new WaypointInfo(2072.500f, 2747.500f, -30.0f));
            Waypoints.Add(109514, new WaypointInfo(873.785f, 1114.032f, -14.9f));
            Waypoints.Add(409517, new WaypointInfo(2520.398f, 1979.950f, 15.0f));
            Waypoints.Add(109538, new WaypointInfo(1079.837f, 856.173f, -1.3f));
            Waypoints.Add(109540, new WaypointInfo(345.398f, 359.990f, -1.3f));
            Waypoints.Add(464066, new WaypointInfo(2500.083f, 1390.092f, 31.0f));
            Waypoints.Add(475856, new WaypointInfo(348.659f, 341.491f, 50.8f));
            Waypoints.Add(475854, new WaypointInfo(1559.163f, 3636.186f, -23.3f));
            Waypoints.Add(464063, new WaypointInfo(350.023f, 350.276f, 0.0f));
 
            Waypoints.Add(263493, new WaypointInfo(1026.961f, 418.969f, 10.8f));
            Waypoints.Add(338946, new WaypointInfo(1260.250f, 540.750f, 3.3f));
            Waypoints.Add(261758, new WaypointInfo(402.102f, 419.735f, 10.3f));
            Waypoints.Add(283553, new WaypointInfo(608.812f, 417.468f, 0.0f));
            Waypoints.Add(258142, new WaypointInfo(1140.714f, 540.347f, 12.4f));
            Waypoints.Add(283567, new WaypointInfo(877.314f, 399.194f, 0.0f));
            Waypoints.Add(338602, new WaypointInfo(1433.629f, 220.720f, 0.3f));
            Waypoints.Add(271234, new WaypointInfo(1069.580f, 679.856f, 20.2f));
            Waypoints.Add(459863, new WaypointInfo(1000.500f, 1160.500f, 0.5f));
            Waypoints.Add(427763, new WaypointInfo(661.430f, 423.897f, 2.9f));
			
			//register for access to displayed area names
			Hud.Render.RegisterUiElement("Root.NormalLayer.minimap_dialog_backgroundScreen.minimap_dialog_pve.area_name", Hud.Render.MinimapUiElement, null);
			
			
			/*Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 91133, Hud.Window.CreateWorldCoordinate(1976.710f, 2788.146f, 41.2f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999962119, 19780, Hud.Window.CreateWorldCoordinate(820.000f, 749.307f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000027656, 19783, Hud.Window.CreateWorldCoordinate(820.000f, 701.500f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000093193, 19785, Hud.Window.CreateWorldCoordinate(820.000f, 1229.307f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000158730, 19787, Hud.Window.CreateWorldCoordinate(1059.357f, 578.648f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 19954, Hud.Window.CreateWorldCoordinate(2895.395f, 2371.276f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 72712, Hud.Window.CreateWorldCoordinate(2161.802f, 1826.882f, 1.9f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 19952, Hud.Window.CreateWorldCoordinate(2037.839f, 910.656f, 1.9f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 61573, Hud.Window.CreateWorldCoordinate(1263.054f, 827.765f, 63.1f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 19953, Hud.Window.CreateWorldCoordinate(423.106f, 781.156f, 21.2f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 93632, Hud.Window.CreateWorldCoordinate(2310.500f, 4770.000f, 1.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000617489, 78572, Hud.Window.CreateWorldCoordinate(1339.771f, 1159.466f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 19941, Hud.Window.CreateWorldCoordinate(1688.828f, 3890.089f, 41.9f)));
            Waypoints.Add(new WaypointLocationInfo(1, 1999831045, 19943, Hud.Window.CreateWorldCoordinate(1080.022f, 3444.448f, 62.4f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000814100, 19774, Hud.Window.CreateWorldCoordinate(820.000f, 736.500f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2001207322, 119870, Hud.Window.CreateWorldCoordinate(56.772f, 146.284f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000879637, 19775, Hud.Window.CreateWorldCoordinate(452.500f, 820.000f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(1, 2000945174, 19776, Hud.Window.CreateWorldCoordinate(979.000f, 1060.000f, 0.0f)));
 
            Waypoints.Add(new WaypointLocationInfo(2, 2001272859, 19836, Hud.Window.CreateWorldCoordinate(2760.302f, 1631.820f, 187.5f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001272859, 63666, Hud.Window.CreateWorldCoordinate(1419.449f, 321.863f, 176.3f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001272859, 19835, Hud.Window.CreateWorldCoordinate(1427.769f, 1185.675f, 186.2f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001403933, 460671, Hud.Window.CreateWorldCoordinate(1747.454f, 549.045f, 0.5f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001469470, 456638, Hud.Window.CreateWorldCoordinate(400.119f, 889.819f, 0.2f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001600544, 210451, Hud.Window.CreateWorldCoordinate(612.485f, 936.556f, -29.8f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001272859, 57425, Hud.Window.CreateWorldCoordinate(3548.838f, 4269.278f, 101.9f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001797155, 62752, Hud.Window.CreateWorldCoordinate(90.000f, 59.500f, 2.7f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2001272859, 53834, Hud.Window.CreateWorldCoordinate(1257.886f, 3968.300f, 111.8f)));
            Waypoints.Add(new WaypointLocationInfo(2, 2002124840, 19800, Hud.Window.CreateWorldCoordinate(799.995f, 680.024f, -0.1f)));
 
            Waypoints.Add(new WaypointLocationInfo(3, 1999568897, 75436, Hud.Window.CreateWorldCoordinate(1065.148f, 472.449f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2002321451, 93103, Hud.Window.CreateWorldCoordinate(825.148f, 1192.449f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2002386988, 136448, Hud.Window.CreateWorldCoordinate(825.148f, 472.449f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2002452525, 93173, Hud.Window.CreateWorldCoordinate(4272.158f, 4252.097f, -24.8f)));
            Waypoints.Add(new WaypointLocationInfo(3, 1999699971, 154644, Hud.Window.CreateWorldCoordinate(4356.238f, 408.241f, -2.9f)));
            Waypoints.Add(new WaypointLocationInfo(3, 1999699971, 155048, Hud.Window.CreateWorldCoordinate(3452.838f, 609.227f, 0.2f)));
            Waypoints.Add(new WaypointLocationInfo(3, 1999699971, 69504, Hud.Window.CreateWorldCoordinate(2708.557f, 586.267f, 0.0f))); //this worldid + areasno has also shown up as act 1 cave of the moon clan
            Waypoints.Add(new WaypointLocationInfo(3, 2002583599, 86080, Hud.Window.CreateWorldCoordinate(1679.054f, 744.707f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2002714673, 80791, Hud.Window.CreateWorldCoordinate(1041.245f, 983.039f, -10.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2002845747, 119305, Hud.Window.CreateWorldCoordinate(2573.769f, 1206.666f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2002976821, 119653, Hud.Window.CreateWorldCoordinate(959.404f, 1097.913f, -10.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2003107895, 119306, Hud.Window.CreateWorldCoordinate(1162.255f, 686.218f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(3, 2003238969, 428494, Hud.Window.CreateWorldCoordinate(606.110f, 1065.332f, 0.0f)));
 
            Waypoints.Add(new WaypointLocationInfo(4, 2003435580, 109526, Hud.Window.CreateWorldCoordinate(1073.872f, 786.390f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003501117, 464857, Hud.Window.CreateWorldCoordinate(1340.086f, 110.277f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003566654, 409001, Hud.Window.CreateWorldCoordinate(1339.833f, 359.946f, -1.3f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003697728, 464065, Hud.Window.CreateWorldCoordinate(1030.028f, 870.214f, 15.0f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003632191, 409512, Hud.Window.CreateWorldCoordinate(2072.500f, 2747.500f, -30.0f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003763265, 109514, Hud.Window.CreateWorldCoordinate(873.785f, 1114.032f, -14.9f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003828802, 409517, Hud.Window.CreateWorldCoordinate(2520.398f, 1979.950f, 15.0f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2003894339, 109538, Hud.Window.CreateWorldCoordinate(1079.837f, 856.173f, -1.3f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2004090950, 109540, Hud.Window.CreateWorldCoordinate(345.398f, 359.990f, -1.3f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2004222024, 464066, Hud.Window.CreateWorldCoordinate(2500.083f, 1390.092f, 31.0f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2004222024, 475856, Hud.Window.CreateWorldCoordinate(348.659f, 341.491f, 50.8f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2004287561, 475854, Hud.Window.CreateWorldCoordinate(1559.163f, 3636.186f, -23.3f)));
            Waypoints.Add(new WaypointLocationInfo(4, 2004287561, 464063, Hud.Window.CreateWorldCoordinate(350.023f, 350.276f, 0.0f)));
 
            Waypoints.Add(new WaypointLocationInfo(5, 2004353098, 263493, Hud.Window.CreateWorldCoordinate(1026.961f, 418.969f, 10.8f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2004484172, 338946, Hud.Window.CreateWorldCoordinate(1260.250f, 540.750f, 3.3f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2004549709, 261758, Hud.Window.CreateWorldCoordinate(402.102f, 419.735f, 10.3f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2004680783, 283553, Hud.Window.CreateWorldCoordinate(608.812f, 417.468f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2004746320, 258142, Hud.Window.CreateWorldCoordinate(1140.714f, 540.347f, 12.4f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2004877394, 283567, Hud.Window.CreateWorldCoordinate(877.314f, 399.194f, 0.0f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2004942931, 338602, Hud.Window.CreateWorldCoordinate(1433.629f, 220.720f, 0.3f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2005139542, 271234, Hud.Window.CreateWorldCoordinate(1069.580f, 679.856f, 20.2f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2005205079, 459863, Hud.Window.CreateWorldCoordinate(1000.500f, 1160.500f, 0.5f)));
            Waypoints.Add(new WaypointLocationInfo(5, 2005270616, 427763, Hud.Window.CreateWorldCoordinate(661.430f, 423.897f, 2.9f)));*/
        }
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			//Hud.Sound.Speak("new area");
			
			if (!IsInOpenWorld)
				return;
			
			//Hud.Sound.Speak("got here");
			//if (newGame)
			//{}
			
			if (area == null) //sanity check
			{
				//LastSeenAreaSno = 0;
				//LastUpdateTick = Hud.Game.CurrentGameTick;
				return;
			}
			
			
			if (!DisplayedAreaNames.ContainsKey(area.Sno) && !string.IsNullOrEmpty(area.NameLocalized))
			{
				DisplayedAreaNames.Add(area.Sno, CultureInfo.InvariantCulture.TextInfo.ToTitleCase(area.NameLocalized.ToLower()));
				//CheckAreaNames.Add(area.Sno, Hud.Game.CurrentGameTick);
				LastSeenAreaSno = area.Sno;
				LastSeenAreaTick = Hud.Game.CurrentGameTick;
			}
			
			//validate or update waypoint data whenever seen because some waypoint positions have some RNG
			foreach (IActor waypoint in Hud.Game.Actors.Where(a => a.GizmoType == GizmoType.Waypoint && a.Scene is object && a.Scene.SnoArea is object)) //there is a waypoint area in act 2 that causes exceptions because SnoArea is not yet revealed (null)
			{
				if (!Waypoints.ContainsKey(waypoint.Scene.SnoArea.Sno))
					continue;
				
				if (!waypoint.FloorCoordinate.Equals(Waypoints[waypoint.Scene.SnoArea.Sno].Coordinate))
					Waypoints[waypoint.Scene.SnoArea.Sno].Coordinate = waypoint.FloorCoordinate;
			}
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame || Hud.Game.SpecialArea == SpecialArea.ChallengeRiftHub || Hud.Game.SpecialArea == SpecialArea.ChallengeRift)
			{
				Act = -1;
				WorldSno = 0;
				IsAdventureMode = false;
				IsInOpenWorld = false;
				return;
			}
			
			WorldSno = GetWorldSno(Hud.Game.Me.SnoArea);
			Act = GetAct(Hud.Game.Me.SnoArea); // == null ? -1 : Hud.Game.Me.SnoArea.Act(); //0 = rift //Hud.Game.Me.SnoArea.Act
			IsAdventureMode = Hud.Game.IsInGame && Hud.Game.Bounties is object && Hud.Game.Bounties.Any();
			IsInOpenWorld = IsAdventureMode && Act > 0; //Hud.Game.SpecialArea == SpecialArea.None
			
			if (Waypoints == null)
				InitializeWaypoints();
			
			if (IsInOpenWorld && !Hud.Game.IsInTown) // && Waypoints is object) //IsAdventureMode && Act > 0)
			{
				var diff = Hud.Game.CurrentGameTick - LastUpdateTick;
				if (diff < 0 || diff > 30 || NearestWaypoint == null || Act != NearestWaypoint.Act)
				{
					LastUpdateTick = Hud.Game.CurrentGameTick;
					
					var myAreaSno = Hud.Game.Me.SnoArea.Sno;
					if (myAreaSno == 0)
						return;
					
					//check for location name mismatches
					if (LastSeenAreaSno == myAreaSno) //if (CheckAreaNames.ContainsKey(myAreaSno)) //LastSeenAreaSno > 0 && myAreaSno == LastSeenAreaSno) //CheckLocationNames.ContainsKey(Hud.Game.Me.SnoArea.Sno))
					{
						var mapTitleUI = Hud.Render.GetUiElement("Root.NormalLayer.minimap_dialog_backgroundScreen.minimap_dialog_pve.area_name");
						if (mapTitleUI is object)
						{
							string displayedAreaName = mapTitleUI.ReadText(System.Text.Encoding.Default, false);
							if (!string.IsNullOrEmpty(displayedAreaName))// && DisplayedAreaNames[myAreaSno] != displayedAreaName)
							{
								displayedAreaName = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(displayedAreaName.Trim().ToLower());
								if (DisplayedAreaNames[myAreaSno] != displayedAreaName)
									DisplayedAreaNames[myAreaSno] = displayedAreaName;
							}

							diff = Hud.Game.CurrentGameTick - LastSeenAreaTick; //CheckAreaNames[myAreaSno];
							if (diff < 0 || diff > 120)
								LastSeenAreaSno = 0; //CheckAreaNames.Remove(myAreaSno); //DisplayedAreaNames[myAreaSno] = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(displayedAreaName);
						}
					}
					
					//validate or update waypoint data whenever seen because some waypoint positions have some RNG
					foreach (IActor waypoint in Hud.Game.Actors.Where(a => a.GizmoType == GizmoType.Waypoint && a.Scene is object && a.Scene.SnoArea is object)) //there is a waypoint area in act 2 that causes exceptions because SnoArea is not yet revealed (null)
					{
						if (!Waypoints.ContainsKey(waypoint.Scene.SnoArea.Sno))
							continue;
						
						if (!waypoint.FloorCoordinate.Equals(Waypoints[waypoint.Scene.SnoArea.Sno].Coordinate))
							Waypoints[waypoint.Scene.SnoArea.Sno].Coordinate = waypoint.FloorCoordinate;
					}
					
					//check for closest waypoint
					var myWorldSno = GetWorldSno(Hud.Game.Me.SnoArea);//Hud.Game.Me.SnoArea.AreaGroupInWorld;
					if (myWorldSno != 0)
					{
						if (Waypoints.ContainsKey(Hud.Game.Me.SnoArea.Sno))
						{
							WaypointInfo favoredWP = Waypoints[Hud.Game.Me.SnoArea.Sno];
							var distance = favoredWP.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate);
							
							var waypointsInWorld = Waypoints.Values.Where(w => w.Area is object && w != favoredWP && w.WorldSno == myWorldSno);
							if (waypointsInWorld.Any())
							{
								var waypoint = waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < right.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) ? left : right); //waypointsInWorld.OrderBy(w => w.Coordinate.XYDistanceTo(pool.Coordinate)).First();
								var altDistance = waypoint.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate);
								NearestWaypoint = distance > altDistance * 1.4f ? waypoint : favoredWP;
							}
							else
								NearestWaypoint = favoredWP;
						}
					}
					else //in a rift
						NearestWaypoint = null;
						/*else
						{
							var waypointsInWorld = Waypoints.Values.Where(w => w.Area is object && w.WorldSno == myAreaWorld);
							if (waypointsInWorld.Any())
								NearestWaypoint = waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) < right.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) ? left : right);
							else if (NearestWaypoint is object && NearestWaypoint.Act != Act)
								NearestWaypoint = null;
						}
					}
					else if (Waypoints.ContainsKey(Hud.Game.Me.SnoArea.Sno))
						NearestWaypoint = null;*/
				}
			}
		}
		
		/*public WaypointInfo GetNearestWaypoint(IWorldCoordinate coord)
		{
			if (Hud.Game.Me.SnoArea == null || Waypoints == null)
				return null;
			
			if (Waypoints.ContainsKey(Hud.Game.Me.Sno))
			{
				WaypointInfo favoredWP = LocationPlugin.Waypoints[pool.Area.Sno];
				var distance = favoredWP.Coordinate.XYDistanceTo(pool.Coordinate);
				
				if (pool.WorldSno != 0)
				{
					var waypointsInWorld = Waypoints.Values.Where(w => w.Area is object && w.Id != favoredWP.Id && w.WorldSno == pool.WorldSno);
					if (waypointsInWorld.Any())
					{
						var waypoint = waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(pool.Coordinate) < right.Coordinate.XYDistanceTo(pool.Coordinate) ? left : right); //waypointsInWorld.OrderBy(w => w.Coordinate.XYDistanceTo(pool.Coordinate)).First();
						var altDistance = waypoint.Coordinate.XYDistanceTo(pool.Coordinate);
						
						//if (distance < altDistance)
						//	return favoredWP;
						
						if (distance > altDistance * 1.35f)
							return waypoint;
						
						return favoredWP;
						
						//return waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(coord) < right.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) ? left : right);
					}
				}
			}
			
			//Hud.Sound.Speak("world " + Hud.Game.Me.WorldSno);
			return NearestWaypoint;
		}*/
		
		public string GetDisplayedName(ISnoArea area)
		{
			if (area is object)
			{
				if (DisplayedAreaNames.ContainsKey(area.Sno))
					return DisplayedAreaNames[area.Sno];
				
				return area.NameLocalized ?? area.NameEnglish;
			}
			
			return null;
		}
		
		public int GetAct(ISnoArea area)
		{
			if (area is object)
			{
				if (AreaActFix.ContainsKey(area.Sno))
					return AreaActFix[area.Sno];
				
				return area.Act;
			}
			
			return -1;
		}
		
		public int GetWorldSno(ISnoArea area = null)
		{
			if (area == null)
				area = Hud.Game.Me.SnoArea;
			
			if (area is object)
			{
				if (AreaWorldFix.ContainsKey(area.Sno))
					return AreaWorldFix[area.Sno];
				
				if (area.SnoWorld is object)
					return (int)area.SnoWorld.Sno;
			}
			
			return 0;
		}
		
		private void InitializeWaypoints()
		{
			Waypoints = new Dictionary<uint, WaypointInfo>();
			var InitialWaypointPositions = new Dictionary<uint, IWorldCoordinate>() {
				//waypoint data for towns
				{332339, Hud.Window.CreateWorldCoordinate(401.731f, 555.009f, 24.7f)}, //act 1
				{168314, Hud.Window.CreateWorldCoordinate(324.870f, 291.031f, 0.8f)}, //act 2
				{92945, Hud.Window.CreateWorldCoordinate(402.540f, 414.342f, 0.7f)}, //act 3 + 4
				{270011, Hud.Window.CreateWorldCoordinate(557.546f, 765.339f, 2.7f)}, //act 5
				
				//act 1
				{91133, Hud.Window.CreateWorldCoordinate(1976.710f, 2788.146f, 41.2f)},
				{19780, Hud.Window.CreateWorldCoordinate(820.000f, 749.307f, 0.0f)},
				{19783, Hud.Window.CreateWorldCoordinate(820.000f, 701.500f, 0.0f)},
				{19785, Hud.Window.CreateWorldCoordinate(820.000f, 1229.307f, 0.0f)},
				{19787, Hud.Window.CreateWorldCoordinate(1059.357f, 578.648f, 0.0f)},
				{19954, Hud.Window.CreateWorldCoordinate(2895.395f, 2371.276f, 0.0f)},
				{72712, Hud.Window.CreateWorldCoordinate(2161.802f, 1826.882f, 1.9f)},
				{19952, Hud.Window.CreateWorldCoordinate(2037.839f, 910.656f, 1.9f)},
				{61573, Hud.Window.CreateWorldCoordinate(1263.054f, 827.765f, 63.1f)},
				{19953, Hud.Window.CreateWorldCoordinate(423.106f, 781.156f, 21.2f)},
				{93632, Hud.Window.CreateWorldCoordinate(2310.500f, 4770.000f, 1.0f)},
				{78572, Hud.Window.CreateWorldCoordinate(1339.771f, 1159.466f, 0.0f)},
				{19941, Hud.Window.CreateWorldCoordinate(1688.828f, 3890.089f, 41.9f)},
				{19943, Hud.Window.CreateWorldCoordinate(1080.022f, 3444.448f, 62.4f)},
				{19774, Hud.Window.CreateWorldCoordinate(820.000f, 736.500f, 0.0f)},
				{119870, Hud.Window.CreateWorldCoordinate(56.772f, 146.284f, 0.0f)},
				{19775, Hud.Window.CreateWorldCoordinate(452.500f, 820.000f, 0.0f)},
				{19776, Hud.Window.CreateWorldCoordinate(979.000f, 1060.000f, 0.0f)},

				//act 2
				{19836, Hud.Window.CreateWorldCoordinate(2760.302f, 1631.820f, 187.5f)},
				{63666, Hud.Window.CreateWorldCoordinate(1419.449f, 321.863f, 176.3f)},
				{19835, Hud.Window.CreateWorldCoordinate(1427.769f, 1185.675f, 186.2f)},
				{460671, Hud.Window.CreateWorldCoordinate(1747.454f, 549.045f, 0.5f)},
				{456638, Hud.Window.CreateWorldCoordinate(400.119f, 889.819f, 0.2f)},
				{210451, Hud.Window.CreateWorldCoordinate(612.485f, 936.556f, -29.8f)},
				{57425, Hud.Window.CreateWorldCoordinate(3548.838f, 4269.278f, 101.9f)},
				{62752, Hud.Window.CreateWorldCoordinate(90.000f, 59.500f, 2.7f)},
				{53834, Hud.Window.CreateWorldCoordinate(1257.886f, 3968.300f, 111.8f)},
				{19800, Hud.Window.CreateWorldCoordinate(799.995f, 680.024f, -0.1f)},

				//act 3
				{75436, Hud.Window.CreateWorldCoordinate(1065.148f, 472.449f, 0.0f)},
				{93103, Hud.Window.CreateWorldCoordinate(825.148f, 1192.449f, 0.0f)},
				{136448, Hud.Window.CreateWorldCoordinate(825.148f, 472.449f, 0.0f)},
				{93173, Hud.Window.CreateWorldCoordinate(4272.158f, 4252.097f, -24.8f)},
				{154644, Hud.Window.CreateWorldCoordinate(4356.238f, 408.241f, -2.9f)},
				{155048, Hud.Window.CreateWorldCoordinate(3452.838f, 609.227f, 0.2f)},
				{69504, Hud.Window.CreateWorldCoordinate(2708.557f, 586.267f, 0.0f)}, //Rakkis Crossing //this worldid + areasno has also shown up as act 1 cave of the moon clan
				{86080, Hud.Window.CreateWorldCoordinate(1679.054f, 744.707f, 0.0f)},
				{80791, Hud.Window.CreateWorldCoordinate(1041.245f, 983.039f, -10.0f)},
				{119305, Hud.Window.CreateWorldCoordinate(2573.769f, 1206.666f, 0.0f)},
				{119653, Hud.Window.CreateWorldCoordinate(959.404f, 1097.913f, -10.0f)},
				{119306, Hud.Window.CreateWorldCoordinate(1162.255f, 686.218f, 0.0f)},
				{428494, Hud.Window.CreateWorldCoordinate(606.110f, 1065.332f, 0.0f)},

				//act 4
				{109526, Hud.Window.CreateWorldCoordinate(1073.872f, 786.390f, 0.0f)},
				{464857, Hud.Window.CreateWorldCoordinate(1340.086f, 110.277f, 0.0f)},
				{409001, Hud.Window.CreateWorldCoordinate(1339.833f, 359.946f, -1.3f)},
				{464065, Hud.Window.CreateWorldCoordinate(1030.028f, 870.214f, 15.0f)},
				{409512, Hud.Window.CreateWorldCoordinate(2072.500f, 2747.500f, -30.0f)},
				{109514, Hud.Window.CreateWorldCoordinate(873.785f, 1114.032f, -14.9f)},
				{409517, Hud.Window.CreateWorldCoordinate(2520.398f, 1979.950f, 15.0f)},
				{109538, Hud.Window.CreateWorldCoordinate(1079.837f, 856.173f, -1.3f)},
				{109540, Hud.Window.CreateWorldCoordinate(345.398f, 359.990f, -1.3f)},
				{464066, Hud.Window.CreateWorldCoordinate(2500.083f, 1390.092f, 31.0f)},
				{475856, Hud.Window.CreateWorldCoordinate(348.659f, 341.491f, 50.8f)},
				{475854, Hud.Window.CreateWorldCoordinate(1559.163f, 3636.186f, -23.3f)},
				{464063, Hud.Window.CreateWorldCoordinate(350.023f, 350.276f, 0.0f)},

				//act 5
				{263493, Hud.Window.CreateWorldCoordinate(1026.961f, 418.969f, 10.8f)},
				{338946, Hud.Window.CreateWorldCoordinate(1260.250f, 540.750f, 3.3f)},
				{261758, Hud.Window.CreateWorldCoordinate(402.102f, 419.735f, 10.3f)},
				{283553, Hud.Window.CreateWorldCoordinate(608.812f, 417.468f, 0.0f)},
				{258142, Hud.Window.CreateWorldCoordinate(1140.714f, 540.347f, 12.4f)},
				{283567, Hud.Window.CreateWorldCoordinate(877.314f, 399.194f, 0.0f)},
				{338602, Hud.Window.CreateWorldCoordinate(1433.629f, 220.720f, 0.3f)},
				{271234, Hud.Window.CreateWorldCoordinate(1069.580f, 679.856f, 20.2f)},
				{459863, Hud.Window.CreateWorldCoordinate(1000.500f, 1160.500f, 0.5f)},
				{427763, Hud.Window.CreateWorldCoordinate(661.430f, 423.897f, 2.9f)},
			};
			
			foreach (IWaypoint waypoint in Hud.Game.ActMapWaypoints)
			{
				if (waypoint.TargetSnoArea == null || Waypoints.ContainsKey(waypoint.TargetSnoArea.Sno))
					continue;
				
				if (InitialWaypointPositions.ContainsKey(waypoint.TargetSnoArea.Sno))
				{
					WaypointInfo info = new WaypointInfo(waypoint);
					info.Act = GetAct(info.Area);
					info.WorldSno = GetWorldSno(info.Area);
					info.Coordinate = InitialWaypointPositions[info.Area.Sno];
					Waypoints.Add(info.Area.Sno, info);
				}
			}
		}
    }

	//Helper class
	public class WaypointInfo
	{
		public int Act { get; set; }
		//public uint WorldId { get; set; } //need this for IMarker matching
		public ISnoArea Area { get; set; }
		//public uint AreaSno { get; set; }
		public int WorldSno { get; set; } //use this when you don't have access to the IActor
		public IWorldCoordinate Coordinate { get; set; }
		//public bool IsInTown { get; set; }
		
		public WaypointInfo(IWaypoint waypoint)
		{
			Area = waypoint.TargetSnoArea;
			//Act = GetAct(Area);
			//WorldSno = GetWorldSno(Area);
			//AreaSno = Area is object ? Area.Sno : 0;
		}
		
		/*public WaypointLocationInfo(int act, ISnoArea area, IWorldCoordinate coord)
		{
			Act = act;
			Area = area;
			FloorCoordinate = coord;
			WorldSno = area.WorldSno();
		}*/
		
		public override string ToString()
		{
			return string.Format("WP [Act = {0}, Area = {1}, WorldSno = {2}, Coordinate = {3}]", Act, Area.NameLocalized, WorldSno, Coordinate);
		}
	}
}