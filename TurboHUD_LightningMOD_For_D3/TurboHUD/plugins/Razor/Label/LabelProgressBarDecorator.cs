/*

Wrapper with timer

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelProgressBarDecorator : ILabelDecorator, ILabelDecoratorCollection
    {
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public List<ILabelDecorator> Labels { get; set; }
		public ILabelDecorator HoveredLabel { get; private set; }
		
		public float Progress { get; set; } //% (1f = 100%)
		public float BarHeight { get; set; } = 0; //0 = automatic background height fill
		public float BarWidth { get; set; } = 0; //0 = automatic background width fill

		public IBrush BackgroundBrush { get; set; } //drawn above BarBrushUnderlay
		public IBrush BarBrush { get; set; }
		public IBrush BarBrushUnderlay { get; set; } //drawn below BarBrush
		public IBrush BorderBrush { get; set; }
		
		public HorizontalAlign Direction { get; set; } = HorizontalAlign.Left; //HorizontalAlign.Center;
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Left; //HorizontalAlign.Center;
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

		public IController Hud { get; private set; }

        public LabelProgressBarDecorator(IController hud, ILabelDecorator label = null)
        {
			Hud = hud;
			Labels = label is object ? new List<ILabelDecorator>() {label} : new List<ILabelDecorator>();
        }

        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (!Enabled)
			//	return;
			if (!LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this))) //(Labels == null || Labels.Count == 0)
			{
				//Width = 0;
				//Height = 0;
				Visible = false;
				Hovered = false;
				HoveredLabel = null;
				return;
			}
			
			//width and height for a row are always based on their child values
			//var label = Labels.FirstOrDefault(lbl => LabelDecorator.IsVisible(lbl));
			//ContentWidth = label.Width;
			//ContentHeight = label.Height;
			//ContentWidth = BarWidth > label.Width ? BarWidth : label.Width;
			//ContentHeight = BarHeight > label.Height ? BarHeight : label.Height;
			/*var w = ContentWidth + SpacingLeft + SpacingRight;
			var h = ContentHeight + SpacingTop + SpacingBottom;
			if (Width < w)
				Width = w;
			if (Height < h)
				Height = h;*/
			//Width = ContentWidth + SpacingLeft + SpacingRight;
			//Height = ContentHeight + SpacingTop + SpacingBottom;
			
			//draw background and border 
			var pWidth = Width*Progress;
			var pHeight = Height;
			var pY = y;
			if (BarHeight > 0)
			{
				pHeight = BarHeight;
				pY = y + Height*0.5f - pHeight*0.5f;
			}
			var pX = x;
			if (BarWidth > 0)
			{
				if (Alignment == HorizontalAlign.Center)
					pX += Width*0.5f - BarWidth*0.5f;
				else if (Alignment == HorizontalAlign.Right)
					pX += Width - BarWidth;
			}
			
			BackgroundBrush?.DrawRectangle(pX, pY, Width, pHeight);
			if (Direction == HorizontalAlign.Center)
			{
				BarBrushUnderlay?.DrawRectangle(pX + Width*0.5f - pWidth*0.5f, pY, pWidth, pHeight);
				BarBrush?.DrawRectangle(pX + Width*0.5f - pWidth*0.5f, pY, pWidth, pHeight);
			}
			else if (Direction == HorizontalAlign.Right)
			{
				BarBrushUnderlay?.DrawRectangle(pX + Width - pWidth, pY, pWidth, pHeight);
				BarBrush?.DrawRectangle(pX + Width - pWidth, pY, pWidth, pHeight);
			}
			else
			{
				BarBrushUnderlay?.DrawRectangle(pX, pY, pWidth, pHeight);
				BarBrush?.DrawRectangle(pX, pY, pWidth, pHeight);
			}
			
			if (BorderBrush is object)
				BorderBrush.DrawRectangle(pX - BorderBrush.StrokeWidth*0.5f, pY - BorderBrush.StrokeWidth*0.5f, Width + BorderBrush.StrokeWidth, pHeight + BorderBrush.StrokeWidth); //pWidth + BorderBrush.StrokeWidth, pHeight + BorderBrush.StrokeWidth);
			
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			//LabelDecorator.DebugWrite(Width.ToString("F0"), x + Width, y);
			
			//LabelDecorator.DebugBrush?.DrawLine(x, y, x, y - Height);
			//LabelDecorator.DebugWrite(x.ToString("F0"), x + 5, y - Height);


			//calculate starting positions
			//var labels = Labels.Where(lbl => LabelDecorator.IsVisible(lbl));
			//var maxHeight = labels.Max(lbl => lbl.Height);
			//var w = ContentWidth + SpacingLeft + SpacingRight;
			//var h = ContentHeight; // + SpacingTop + SpacingBottom;
			//var y2 = y + labels.Max(lbl => lbl.Height)*0.5f;
			//LabelDecorator.DebugBrush2?.DrawRectangle(x2 + SpacingLeft, y + SpacingTop, ContentWidth, ContentHeight);
			
			HoveredLabel = null;
			Hovered = false;
			
			float y2 = y + SpacingTop; // + (Height - SpacingTop - SpacingBottom)*0.5f; //y + ContentHeight*0.5f + SpacingTop;//SpacingTop; //h*0.5f
			float x2 = x + SpacingLeft;
				/*if (Alignment == HorizontalAlign.Center)
					x2 += (Width - SpacingLeft - SpacingRight)*0.5f - ContentWidth*0.5f; //x + Width*0.5f - w*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
				else if (Alignment == HorizontalAlign.Right)
					x2 = x + Width - ContentWidth - SpacingRight; //labels.Sum(lbl => lbl.Width);*/
				
			//float lWidth = label.Width;
			var label = Labels.FirstOrDefault(lbl => LabelDecorator.IsVisible(lbl));
			if (label is object)
			{
				var w = Width - SpacingLeft - SpacingRight;
				var h = Height - SpacingTop - SpacingBottom;
				if (label.Width < w)
					label.Width = w;
				if (label.Height < h)
					label.Height = h;

				label.Paint(x2, y2);
				
				ContentWidth = BarWidth > label.Width ? BarWidth : label.Width;
				ContentHeight = BarHeight > label.Height ? BarHeight : label.Height;
				Height = ContentHeight + SpacingTop + SpacingBottom;
				Width = ContentWidth + SpacingLeft + SpacingRight;

				if (label.Hovered)
				{
					if (label is ILabelDecoratorCollection && ((ILabelDecoratorCollection)label).HoveredLabel is object)
						HoveredLabel = ((ILabelDecoratorCollection)label).HoveredLabel;
					else
						HoveredLabel = label;
				}
				
				Hovered = HoveredLabel is object || Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			}
			else
				Hovered = Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			
			/*w = Width - SpacingLeft - SpacingRight;
			h = Height - SpacingTop - SpacingBottom;
			if (label.Width < w)
				label.Width = w;
			if (label.Height < h)
				label.Height = h;*/
			
			
			//Visible = label.Visible;
			Visible = true;
			//x2 += lWidth;

			
			//Hovered = HoveredLabel is object || Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			if (Hovered)
				LabelDecorator.SetHint(this);
			
			//Height = ContentHeight + SpacingTop + SpacingBottom;
			//Width = ContentWidth + SpacingLeft + SpacingRight;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			var label = Labels.FirstOrDefault(lbl => LabelDecorator.IsVisible(lbl));
			if (label is object)
			{
				ContentWidth = BarWidth > label.Width ? BarWidth : label.Width;
				ContentHeight = BarHeight > label.Height ? BarHeight : label.Height;
				Height = ContentHeight + SpacingTop + SpacingBottom;
				Width = ContentWidth + SpacingLeft + SpacingRight;
			}
			else
			{
				ContentWidth = BarWidth;
				ContentHeight = BarHeight;
				Width = ContentWidth > 0 ? ContentWidth + SpacingLeft + SpacingRight : 0;
				Height = ContentHeight > 0 ? ContentHeight + SpacingTop + SpacingBottom : 0;
			}
		}
	}
}