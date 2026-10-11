/*

a helper class to display variably-sized labels that know their own width and height on the screen

*/

namespace Turbo.Plugins.Razor.Menu
{
	//using System;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;

	public interface IMenuAddon : IPlugin
	//public class MenuDecorator : TopLabelDecorator
	{
		ILabelDecorator Panel { get; set; } //optional
		ILabelDecorator Label { get; set; } //menu label
		ILabelDecorator LabelHint { get; set; } //optional, visible only when pinned
		float LabelSize { get; set; } //optional label minimum dimension (width for horizontal, height for vertical
		
		string Id { get; set; }
		int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		string DockId { get; set; } //the name of the dock to show this addon in (null = Default as specified in RunStatsPlugin)
		string Config { get; set; }
		//MenuPlugin Plugin { get; set; }
		
		void OnRegister(MenuPlugin plugin);
		
		//float SpacingLeft { get; set; }
		//float SpacingRight { get; set; }
		//float SpacingTop { get; set; }
		//float SpacingBottom { get; set; }
		
	}
}