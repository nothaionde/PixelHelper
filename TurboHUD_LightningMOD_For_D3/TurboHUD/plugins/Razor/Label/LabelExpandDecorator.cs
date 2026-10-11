/*

Draws a label that also draws a panel adjacent to it when hovered

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelExpandDecorator : ILabelDecorator //, ILabelDecoratorCollection
    {
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		public IBrush BackgroundBrush { get; set; }
		public IBrush BorderBrush { get; set; }

		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center; //Alignment refers to the position of Label relative to its parent display
		public float SpacingLeft { get; set; }
		public float SpacingRight { get; set; }
		public float SpacingTop { get; set; }
		public float SpacingBottom { get; set; }

		public float ContentWidth { get; set; }
		public float ContentHeight { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		public float LastX { get; private set; }
		public float LastY { get; private set; }

		//ILabelDecoratorCollection
		//public List<ILabelDecorator> Labels { get; set; }
		//public ILabelDecorator HoveredLabel { get; private set; }
		
		//properties specific to LabelExpandDecorator
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator Panel { get; set; }

		public IController Hud { get; private set; }

        public LabelExpandDecorator(IController hud, ILabelDecorator label, ILabelDecorator panel) //params ILabelDecorator[] labels
        {
			Hud = hud;
			Label = label;
			Panel = panel;
        }

        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (!Enabled)
			//	return;
			
			//delay drawing of nested labels so that they are drawn on top
			LabelDecorator.Queue(() => Draw(x, y));
		}
		
		public void Draw(float x, float y) //this IRenderController Render,
		{
			if (Label == null || Panel == null || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				return;
			}
			
			//if (LabelDecorator.IsVisible(Label))
			//{
				//draw background and border
				BackgroundBrush?.DrawRectangle(x, y, Width, Height);
				BorderBrush?.DrawRectangle(x, y, Width, Height);
				LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
				LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
				//LabelDecorator.DebugWrite(Width.ToString("F0"), x + Width, y);
				
				//LabelDecorator.DebugBrush?.DrawLine(x, y, x, y - Height);
				//LabelDecorator.DebugWrite(x.ToString("F0"), x + 5, y - Height);
				var hovered = Label.Hovered; //save the hover value from the previous iteration
				Label.Paint(x, y);
				
				if (LabelDecorator.IsVisible(Panel) && (hovered || Panel.Hovered))
				{
					float x2 = 0;
					if (Alignment == HorizontalAlign.Left)
					{
						x2 = x - Panel.Width;
						if (x2 < 0)
							x2 = x + Label.Width; //right
					}
					else
					{
						x2 = x + Label.Width;
						if (x2 + Panel.Width > Hud.Window.Size.Width)
							x2 = x - Panel.Width; //left
					}
					
					float y2 = y;
					if (y2 + Panel.Height > Hud.Window.Size.Height)
						y2 = Hud.Window.Size.Height - Panel.Height; //up
					
					Panel.Paint(x2, y2);
				}
			//}
			
			Hovered = true;
			if (Label.Hovered)
				LabelDecorator.SetHint(Label);
			else if (Panel.Hovered)
				LabelDecorator.SetHint(Panel);
			else
				Hovered = false;
			
			ContentHeight = Label.ContentHeight;
			ContentWidth = Label.ContentWidth;
			Height = Label.Height;
			Width = Label.Width;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			Label.Resize();
			
			ContentHeight = Label.Height;
			ContentWidth = Label.Width;
			Height = Label.Height;
			Width = Label.Width;
		}
	}
}