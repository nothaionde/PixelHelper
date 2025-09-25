/*

Additional functions for TH's API that I use in various plugins

*/

namespace Turbo.Plugins.Razor.Util
{
	using SharpDX.Direct2D1;
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

	public static class UtilExtensions
	{
		public static Dictionary<HeroClass, int[]> HeroColors { get; set; } = new Dictionary<HeroClass, int[]>() 
		{
			{HeroClass.Barbarian, new int[3] {255, 128, 64}}, //255, 67, 0
			{HeroClass.Crusader, new int[3] {0, 200, 250}}, //0, 200, 250
			{HeroClass.DemonHunter, new int[3] {0, 100, 255}}, //0, 0, 255
			{HeroClass.Monk, new int[3] {252, 239, 0}}, //255, 255, 0
			{HeroClass.Necromancer, new int[3] {252, 235, 191}}, //0, 190, 190 //240, 240, 240
			{HeroClass.WitchDoctor, new int[3] {163, 244, 65}},
			{HeroClass.Wizard, new int[3] {153, 51, 255}},
			{HeroClass.None, new int[3] {255, 255, 255}}
		};
		private static Dictionary<HeroClass, IBrush> HeroBrushes { get; set; } = new Dictionary<HeroClass, IBrush>();
		private static Dictionary<string, IFont> HeroFonts { get; set; } = new Dictionary<string, IFont>();
		
		//for testing cube status
		public static bool HasCubedItem(this IPlayer player, uint sno)
		{
			return (player.CubeSnoItem1?.Sno == sno) || (player.CubeSnoItem2?.Sno == sno) || (player.CubeSnoItem3?.Sno == sno) || (player.CubeSnoItem4?.Sno == sno);
		}
		
		public static void TryTogglePlugin(this IController hud, string path, bool enabled)
		{
			//split the path into a plugin name and its abbreviated namespace
			/*int index = path.LastIndexOf('.');
			if (index < 1)
				name = path;
			else
			{
				name = path.Substring(index + 1);
				path = path.Substring(0, index);
			}*/
			IPlugin plugin = hud.AllPlugins.FirstOrDefault(p => p.GetType().ToString().ToLower().Contains(path.ToLower()));
			if (plugin is object)
				plugin.Enabled = enabled;
		}
		
		public static IPlugin TryGetPlugin(this IController hud, string path)
		{
			return hud.AllPlugins.FirstOrDefault(p => p.GetType().ToString().ToLower().Contains(path.ToLower()));
		}
		
		public static IBrush GetHeroBrush(this IRenderController render, HeroClass heroClass)
		{
			if (!HeroBrushes.ContainsKey(heroClass))
			{
				int[] color = HeroColors[heroClass];
				HeroBrushes[heroClass] = render.CreateBrush(255, color[0], color[1], color[2], 0, DashStyle.Solid, CapStyle.Triangle, CapStyle.Triangle);
			}
			
			return HeroBrushes[heroClass];
		}
		
		public static IFont GetHeroFont(this IRenderController render, HeroClass heroClass, float fontSize, bool bold, bool italic, bool shadow)
		{
			int[] color = HeroColors[heroClass];
			string key = string.Format("{0}_{1}_{2},{3},{4}_{5}_{6}_{7}", (uint)heroClass, fontSize, color[0], color[1], color[2], (bold ? 1 : 0), (italic ? 1 : 0), (shadow ? 1 : 0));
			
			if (!HeroFonts.ContainsKey(key))
				HeroFonts[key] = shadow ?
					render.CreateFont("tahoma", fontSize, 255, color[0], color[1], color[2], bold, italic, 100, 0, 0, 0, true) :
					render.CreateFont("tahoma", fontSize, 255, color[0], color[1], color[2], bold, italic, false);
			
			return HeroFonts[key];
		}
		
		public static ITexture GetHeroHead(this ITextureController ctrl, HeroClass cls, bool isMale)
		{
			//borrowed the texture numbers from OtherPlayersHeadsPlugin.cs
			switch(cls)
			{ 
				case HeroClass.Barbarian:
					return ctrl.GetTexture(isMale ? 3921484788 : 1030273087);
				case HeroClass.Crusader:
					return ctrl.GetTexture(isMale ? 3742271755 : 3435775766);
				case HeroClass.DemonHunter:
					return ctrl.GetTexture(isMale ? 3785199803 : 2939779782);
				case HeroClass.Monk:
					return ctrl.GetTexture(isMale ? 2227317895 : 2918463890);
				case HeroClass.Necromancer:
					return ctrl.GetTexture(isMale ? 3285997023 : 473831658);
				case HeroClass.WitchDoctor:
					return ctrl.GetTexture(isMale ? 3925954876 : 1603231623);
				case HeroClass.Wizard:
					return ctrl.GetTexture(isMale ? (uint)44435619 : 876580014);
				default:
					break;
			}
			
			return null;
		}
		
		public static bool IsUiVisible(this IRenderController render, string path)
		{
			var ui = render.GetUiElement(path);
			if (ui is object)
				return ui.Visible;
			
			return false;
		}
		
		/*public static double GetPowerValue(this IItem item)
		{
			IItemPerfection goldStat = item.Perfections.FirstOrDefault(p => p.Attribute == Hud.Sno.Attributes.Item_Power_Passive);
			return (goldStat is object ? goldStat.Cur : 0);
		}*/
		
		//recursive function adapted from https://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals
		public static string ToRoman(this IController hud, int n)
		{
			if ((n < 1) || (n > 10)) return string.Empty;
			if (n == 10) return "X" + hud.ToRoman(n - 10);
			if (n >= 9) return "IX" + hud.ToRoman(n - 9);
			if (n >= 5) return "V" + hud.ToRoman(n - 5);
			if (n >= 4) return "IV" + hud.ToRoman(n - 4);
			if (n >= 1) return "I" + hud.ToRoman(n - 1);
			return string.Empty;
		}
	}
}