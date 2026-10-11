/*

Draws a horizontal row of labels that are of a uniform height (determined by whichever label is the tallest). Width of each label is expected to be explicitly specified before rendering.

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelRowDecorator : ILabelDecorator, ILabelDecoratorCollection
    {
		public bool EnforceWidths { get; set; } = false;
		
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public List<ILabelDecorator> Labels { get; set; }
		public ILabelDecorator HoveredLabel { get; private set; }

		public IBrush BackgroundBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		public IBrush HoveredBrush { get; set; }
		
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center;
		public float SpacingLeft { get; set; }
		public float SpacingRight { get; set; }
		public float SpacingTop { get; set; }
		public float SpacingBottom { get; set; }
		public float Gap { get; set; }

		public float ContentWidth { get; set; }
		public float ContentHeight { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		public float LastX { get; private set; }
		public float LastY { get; private set; }

		public IController Hud { get; private set; }

        public LabelRowDecorator(IController hud)
        {
			Hud = hud;
			Labels = new List<ILabelDecorator>();
        }

        public LabelRowDecorator(IController hud, params ILabelDecorator[] labels)
        {
			Hud = hud;
			Labels = new List<ILabelDecorator>(labels);
        }

        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (!Enabled)
			//	return;
			if ((Labels == null || Labels.Count == 0) || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				HoveredLabel = null;
				return;
			}
			
			//width and height for a row are always based on their child values
			var labels = Labels.Where(lbl => lbl.Enabled); //LabelDecorator.IsVisible(lbl));
			if (!labels.Any())
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				HoveredLabel = null;
				return;
			}
			
			ContentWidth = labels.Sum(lbl => lbl.Width); //+ Gap*(count-1);
			ContentHeight = labels.Max(lbl => lbl.Height);
			//var w = ContentWidth + SpacingLeft + SpacingRight;
			//var h = ContentHeight + SpacingTop + SpacingBottom;
			/*if (Width < w)
				Width = w;
			if (Height < h)
				Height = h;*/
			//Width = ContentWidth + SpacingLeft + SpacingRight;
			//Height = ContentHeight + SpacingTop + SpacingBottom;
			
			//draw background and border
			BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			BorderBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			
			
			//LabelDecorator.DebugBrush?.DrawLine(x, y, x, y - Height);
			//LabelDecorator.DebugWrite(x.ToString("F0"), x + 5, y - Height);


			//calculate starting positions
			//var labels = Labels.Where(lbl => LabelDecorator.IsVisible(lbl));
			//var maxHeight = labels.Max(lbl => lbl.Height);
			//var w = ContentWidth + SpacingLeft + SpacingRight;
			//var h = ContentHeight; // + SpacingTop + SpacingBottom;
			//var y2 = y + labels.Max(lbl => lbl.Height)*0.5f;
			
			//takes into consideration the retained Width and Height settings
			var w = Width - SpacingLeft - SpacingRight;
			var h = Height - SpacingTop - SpacingBottom;
			//float count = labels.Count();
			//float w = ContentWidth + Gap*(count-1);
			//float h = ContentHeight;
			
			
			//var y2 = y + SpacingTop + (Height - SpacingTop - SpacingBottom)*0.5f - ContentHeight*0.5f; //y + ContentHeight*0.5f + SpacingTop;//SpacingTop; //h*0.5f
			var y2 = y + SpacingTop + h*0.5f - ContentHeight*0.5f;
			var x2 = x + SpacingLeft;
			if (Alignment == HorizontalAlign.Center)
				x2 += w*0.5f - ContentWidth*0.5f; //x + Width*0.5f - w*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
			else if (Alignment == HorizontalAlign.Right)
				x2 = x + Width - ContentWidth - SpacingRight; //labels.Sum(lbl => lbl.Width);
			//LabelDecorator.DebugBrush2?.DrawRectangle(x2 + SpacingLeft, y + SpacingTop, ContentWidth, ContentHeight);
			
			HoveredLabel = null;
			//ContentWidth = 0;
			//ContentHeight = 0;
			//w = 0;
			//h = 0;
			foreach (ILabelDecorator label in labels)
			{
				if (!LabelDecorator.IsVisible(label))
				{
					if (EnforceWidths && label.Width > 0)
						x2 += label.Width;
					continue;
				}
				
				if (HoveredBrush is object && label.Hovered)
					HoveredBrush.DrawRectangle(x2, y2, label.Width, label.Height);
				
				//render width is the previous value set in Width and Height (this allow custom width settings to be dictated before drawing)
				float lWidth = label.Width;
				//label.Height = ContentHeight; //sometimes causes popping displacement effect
				
				//LabelDecorator.DebugBrush2?.DrawRectangle(x2, y2 - label.Height*0.5f, w, label.Height);
				//LabelDecorator.Draw(label, x2, y2 - label.Height*0.5f);
				//LabelDecorator.DebugBrush3?.DrawRectangle(x2, y2 - label.Height*0.5f, lWidth, label.Height);
				//label.Paint(x2, y2 - label.Height*0.5f);
				//label.Paint(x2, y2);
				label.Paint(x2, y2 + ContentHeight*0.5f - label.Height*0.5f);
				
				if (label.Visible)
					Visible = true;

				x2 += lWidth + Gap; //label.Width + Gap; //lWidth + Gap; //label.Width; //

				//ContentWidth += label.Width;
				//if (ContentHeight < label.Height)
				//	ContentHeight = label.Height;
				//w += label.Width;
				
				if (label.Hovered)
				{
					if (label is ILabelDecoratorCollection && ((ILabelDecoratorCollection)label).HoveredLabel is object)
						HoveredLabel = ((ILabelDecoratorCollection)label).HoveredLabel;
					else
						HoveredLabel = label;
				}
			}
			
			Hovered = HoveredLabel is object || Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			if (Hovered)
				LabelDecorator.SetHint(this);
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			ContentWidth = 0;
			ContentHeight = 0;
			
			foreach (ILabelDecorator label in Labels.Where(lbl => LabelDecorator.IsVisible(lbl)))
			{
				label.Resize();
				
				ContentWidth += label.Width;
				if (ContentHeight < label.Height)
					ContentHeight = label.Height;
			}
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
		}

	}
}