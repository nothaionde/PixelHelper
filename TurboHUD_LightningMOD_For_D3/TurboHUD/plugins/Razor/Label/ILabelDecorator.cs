/*

a helper class to display variably-sized labels that know their own width and height on the screen

*/

namespace Turbo.Plugins.Razor.Label
{
	using System;
	using System.Collections.Generic;
	using Turbo.Plugins.Default;

	public interface ILabelDecorator
	{
		IController Hud { get; }
		
		bool Enabled { get; set; }
		bool Hovered { get; }
		bool Visible { get; }
		ILabelDecorator Hint { get; set; }
		
		Func<ILabelDecorator, bool> OnBeforeRender { get; set; } //any code that runs prior to render, returns true if the label should render
		Action<ILabelDecorator> OnClick { get; set; } //any code that runs when the label is clicked
		
		HorizontalAlign Alignment { get; set; } //how is the content aligned within this label?
		float SpacingLeft { get; set; }
		float SpacingRight { get; set; }
		float SpacingTop { get; set; }
		float SpacingBottom { get; set; }
		
		IBrush BackgroundBrush { get; set; }
		IBrush BorderBrush { get; set; }
		
		float Width { get; set; } //value of the previous computed ContentWidth + SpacingLeft + SpacingRight, or a custom value
		float Height { get; set; } //value of the previous computed ContentHeight + SpacingTop + SpacingBottom, or a custom value
		float LastX { get; }
		float LastY { get; }
		float ContentWidth { get; set; }
		float ContentHeight { get; set; }

		void Paint(float x, float y, IBrush debugBrush = null);
		void Resize(); //compute the dimensions of the display without drawing it (updates ContentWidth, ContentHeight, Width, Height)
	}
}