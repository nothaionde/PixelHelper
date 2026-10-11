/*

Draws a horizontal row of labels that are of a uniform height (determined by whichever label is the tallest)

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelAlignedDecorator : ILabelDecorator, ILabelDecoratorCollection
    {
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
		
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center;
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

        public LabelAlignedDecorator(IController hud)
        {
			Hud = hud;
			Labels = new List<ILabelDecorator>();
        }

        public LabelAlignedDecorator(IController hud, params ILabelDecorator[] labels)
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
			var labels = Labels.Where(lbl => LabelDecorator.IsVisible(lbl));
			if (!labels.Any())
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				HoveredLabel = null;
				return;
			}
			/*ContentWidth = labels.Sum(lbl => lbl.Width);
			ContentHeight = labels.Max(lbl => lbl.Height);
			var w = ContentWidth + SpacingLeft + SpacingRight;
			var h = ContentHeight + SpacingTop + SpacingBottom;
			if (Width < w)
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
			//LabelDecorator.DebugWrite(Width.ToString("F0"), x + Width, y);
			
			//LabelDecorator.DebugBrush?.DrawLine(x, y, x, y - Height);
			//LabelDecorator.DebugWrite(x.ToString("F0"), x + 5, y - Height);

			

			//calculate the width
			ContentHeight = 0;
			Dictionary<HorizontalAlign, List<ILabelDecorator>> groups = new Dictionary<HorizontalAlign, List<ILabelDecorator>>();
			foreach (var label in labels)
			{
				if (ContentHeight < label.Height)
					ContentHeight = label.Height; // + decorator.SpacingTop + decorator.SpacingBottom;
				
				if (groups.ContainsKey(label.Alignment))
					groups[label.Alignment].Add(label);
				else
					groups.Add(label.Alignment, new List<ILabelDecorator>() {label});
			}

			float leftWidth = groups.ContainsKey(HorizontalAlign.Left) ? groups[HorizontalAlign.Left].Sum(lbl => lbl.Width) : 0;
			float centerWidth = groups.ContainsKey(HorizontalAlign.Center) ? groups[HorizontalAlign.Center].Sum(lbl => lbl.Width) : 0;
			float rightWidth = groups.ContainsKey(HorizontalAlign.Right) ? groups[HorizontalAlign.Right].Sum(lbl => lbl.Width) : 0;
			float ContentWidth = centerWidth > 0 ? centerWidth + (leftWidth > rightWidth ? leftWidth : rightWidth)*2f : leftWidth + rightWidth;	
			
			//var w = ContentWidth + SpacingLeft + SpacingRight;
			//var h = ContentHeight + SpacingTop + SpacingBottom;
			var w = Width - SpacingLeft - SpacingRight;
			var h = Height - SpacingTop - SpacingBottom;
			if (ContentWidth > w)
				w = ContentWidth;
			/*if (Width < w)
				Width = w;
			if (Height < h)
				Height = h;*/
			
			//calculate starting positions
			//var labels = Labels.Where(lbl => LabelDecorator.IsVisible(lbl));
			//var maxHeight = labels.Max(lbl => lbl.Height);
			//var w = ContentWidth + SpacingLeft + SpacingRight;
			//var h = ContentHeight; // + SpacingTop + SpacingBottom;
			//var y2 = y + labels.Max(lbl => lbl.Height)*0.5f;
			//var y2 = y + SpacingTop; // + (Height - SpacingTop - SpacingBottom)*0.5f; //y + ContentHeight*0.5f + SpacingTop;//SpacingTop; //h*0.5f
			/*var x2 = x + SpacingLeft;
			if (Alignment == HorizontalAlign.Center)
				x2 += (Width - SpacingLeft - SpacingRight)*0.5f - ContentWidth*0.5f; //x + Width*0.5f - w*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
			else if (Alignment == HorizontalAlign.Right)
				x2 = x + Width - ContentWidth - SpacingRight; //labels.Sum(lbl => lbl.Width);*/
			//LabelDecorator.DebugBrush2?.DrawRectangle(x2 + SpacingLeft, y + SpacingTop, ContentWidth, ContentHeight);
			
			HoveredLabel = null;
			//ContentWidth = 0;
			//ContentHeight = 0;
			//w = 0;
			var y2 = y + SpacingTop + (Height - SpacingTop - SpacingBottom)*0.5f;
			if (groups.ContainsKey(HorizontalAlign.Left))
			{
				var x2 = x + SpacingLeft;
				foreach (ILabelDecorator label in groups[HorizontalAlign.Left])
				{
					label.Resize();
					var tmpW = label.Width;
					label.Alignment = HorizontalAlign.Center;
					label.Paint(x2, y2 - label.Height*0.5f);
					label.Alignment = HorizontalAlign.Left;
					x2 += tmpW;
					
					if (label.Visible)
						Visible = true;
					if (label.Hovered)
						HoveredLabel = label;
				}
			}
			if (groups.ContainsKey(HorizontalAlign.Center))
			{
				//var x2 = x + SpacingLeft + (Width - SpacingLeft - SpacingRight)*0.5f - centerWidth*0.5f;
				var x2 = x + SpacingLeft + w*0.5f - centerWidth*0.5f;
				foreach (ILabelDecorator label in groups[HorizontalAlign.Center])
				{
					label.Resize();
					var tmpW = label.Width;
					label.Paint(x2, y2 - label.Height*0.5f);
					x2 += tmpW;

					if (label.Visible)
						Visible = true;
					if (label.Hovered)
						HoveredLabel = label;
				}
			}
			if (groups.ContainsKey(HorizontalAlign.Right))
			{
				//var x2 = x + Width - rightWidth - SpacingRight;
				var x2 = x + SpacingLeft + w - rightWidth; //- SpacingRight;
				foreach (ILabelDecorator label in groups[HorizontalAlign.Right])
				{
					label.Resize();
					var tmpW = label.Width;
					label.Alignment = HorizontalAlign.Center;
					label.Paint(x2, y2 - label.Height*0.5f);
					label.Alignment = HorizontalAlign.Right;
					x2 += tmpW;

					if (label.Visible)
						Visible = true;
					if (label.Hovered)
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
			
			ContentHeight = 0;
			Dictionary<HorizontalAlign, List<ILabelDecorator>> groups = new Dictionary<HorizontalAlign, List<ILabelDecorator>>();
			foreach (var label in Labels.Where(lbl => LabelDecorator.IsVisible(lbl)))
			{
				label.Resize();
				
				if (ContentHeight < label.Height)
					ContentHeight = label.Height; // + decorator.SpacingTop + decorator.SpacingBottom;
				
				if (groups.ContainsKey(label.Alignment))
					groups[label.Alignment].Add(label);
				else
					groups.Add(label.Alignment, new List<ILabelDecorator>() {label});
			}

			float leftWidth = groups.ContainsKey(HorizontalAlign.Left) ? groups[HorizontalAlign.Left].Sum(lbl => lbl.Width) : 0;
			float centerWidth = groups.ContainsKey(HorizontalAlign.Center) ? groups[HorizontalAlign.Center].Sum(lbl => lbl.Width) : 0;
			float rightWidth = groups.ContainsKey(HorizontalAlign.Right) ? groups[HorizontalAlign.Right].Sum(lbl => lbl.Width) : 0;
			float ContentWidth = centerWidth > 0 ? centerWidth + (leftWidth > rightWidth ? leftWidth : rightWidth)*2f : leftWidth + rightWidth;	
			
			var w = ContentWidth + SpacingLeft + SpacingRight;
			var h = ContentHeight + SpacingTop + SpacingBottom;
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
		}
	}
}