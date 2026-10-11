namespace Turbo.Plugins.Razor.Label
{
	using System; //Action
	using System.Collections.Generic;
	using SharpDX.DirectWrite; //TextLayout

	using Turbo.Plugins.Default;

    public class LabelCanvasDecorator : ILabelDecorator
    {
		public bool IgnoreHeight { get; set; } = true; //if containing labels shouldn't size themselves based on these dimensions
		
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public Action<LabelCanvasDecorator, float, float> DrawFunc { get; set; }

		public IBrush BackgroundBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center;
		public float SpacingLeft { get; set; }
		public float SpacingRight { get; set; }
		public float SpacingTop { get; set; }
		public float SpacingBottom { get; set; }

		public float ContentWidth { get; set; } //stays the same
		public float ContentHeight { get; set; } //stays the same
		public float Width { get; set; } //dynamic
		public float Height { get; set; } //dynamic
		public float LastX { get; private set; }
		public float LastY { get; private set; }

		public IController Hud { get; private set; }

        public LabelCanvasDecorator(IController hud, Action<LabelCanvasDecorator, float, float> draw)
        {
			Hud = hud;
			DrawFunc = draw;
        }
		
        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			if (DrawFunc == null || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				return;
			}
			
			//float w = Width - SpacingLeft - SpacingRight;
			//float h = Height - SpacingTop - SpacingBottom;
						
			Visible = true;
			
			BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			BorderBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			
			DrawFunc(this, x + SpacingLeft, y + SpacingTop);
			
			Hovered = Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			if (Hovered)
				LabelDecorator.SetHint(this);
			//Width = ContentWidth + SpacingLeft + SpacingRight;
			//Height = IgnoreHeight ? 1f : ContentHeight + SpacingTop + SpacingBottom;
			//LabelDecorator.DebugWrite(Width.ToString("F0") + " x " + Height.ToString("F0"), x + SpacingLeft, y + SpacingTop);
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
		}
	}
}