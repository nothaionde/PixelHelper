/*

Additional functions for TH's API that I use in various plugins

*/

namespace Turbo.Plugins.Razor.Seasonal
{
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

	public static class EtherealExtensions
	{
		//public static int[] EtherealColor { get; set; } = new int[3] {79, 154, 143};
		public static uint[] EtherealItemSnos { get; private set; } = new uint[] {
			3130080131, // Arioc's Needle
			2176389839, // Arioc's Needle
			3130080070, // Astreon's Iron Ward
			2176389778, // Astreon's Iron Ward
			3130080069, // Bartuc's Cut-Throat
			2176389777, // Bartuc's Cut-Throat
			3130080104, // Blackbog's Sharp
			2176389812, // Blackbog's Sharp
			3130080067, // Blackhand Key
			2176389775, // Blackhand Key
			3130080073, // Buriza-Do Kyanon
			2176389781, // Buriza-Do Kyanon
			3130080102, // Doombringer
			2176389810, // Doombringer
			3130080103, // Doomslinger
			2176389811, // Doomslinger
			3130080101, // Ghostflame
			2176389809, // Ghostflame
			3130080072, // Gimmershred
			2176389780, // Gimmershred
			3130080106, // Jade Talon
			2176389814, // Jade Talon
			3130080130, // Khalim's Will
			2176389838, // Khalim's Will
			3130080098, // Mang Song's Lesson
			2176389806, // Mang Song's Lesson
			3130080099, // Shadow Killer
			2176389807, // Shadow Killer
			3130080097, // Soul Harvest
			2176389805, // Soul Harvest
			3130080071, // The Gidbinn
			2176389779, // The Gidbinn
			3130080065, // The Grandfather
			2176389773, // The Grandfather
			3130080068, // The Oculus
			2176389776, // The Oculus
			3130080100, // The Redeemer
			2176389808, // The Redeemer
			3130080066, // Windforce
			2176389774, // Windforce
			3130080105, // Wizardspike
			2176389813, // Wizardspike
		};
		
		public static bool IsEthereal(this IItem item)
		{
			return EtherealItemSnos.Contains(item.SnoItem.Sno);
		}
	}
}