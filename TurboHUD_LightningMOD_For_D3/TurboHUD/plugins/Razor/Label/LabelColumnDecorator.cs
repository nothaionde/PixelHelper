/*

Draws a vertical column of labels that are of a uniform width (determined by whichever label is the widest)

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelColumnDecorator : ILabelDecorator, ILabelDecoratorCollection
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
		public IBrush HoveredBrush { get; set; }
		
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

        public LabelColumnDecorator(IController hud, params ILabelDecorator[] labels)
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
			
			//draw background and border
			BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			BorderBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			//LabelDecorator.DebugWrite(Width.ToString("F0"), x + Width, y);
			
			//LabelDecorator.DebugBrush?.DrawLine(x, y, x, y - Height);
			//LabelDecorator.DebugWrite(x.ToString("F0"), x + 5, y - Height);

			Visible = false;

			//calculate starting positions
			var labels = Labels.Where(lbl => LabelDecorator.IsVisible(lbl));
			if (!labels.Any())
			{
				Width = 0;
				Height = 0;
				//Visible = false;
				Hovered = false;
				HoveredLabel = null;
				return;
			}
			
			//ContentWidth = labels.Max(lbl => lbl.Width);
			//ContentHeight = labels.Sum(lbl => lbl.Height);
			var w = Width - SpacingLeft - SpacingRight;
			var h = Height - SpacingTop - SpacingBottom;
			
			//var maxHeight = labels.Max(lbl => lbl.Height);
			//var w = ContentWidth + SpacingLeft + SpacingRight;
			//var h = ContentHeight + SpacingTop + SpacingBottom;
			//float y2 = y + labels.Max(lbl => lbl.Height)*0.5f;
			float y2 = y + SpacingTop; //not trying to align it with sibling labels because this is a column // + (Height - SpacingTop - SpacingBottom)*0.5f; //y + ContentHeight*0.5f + SpacingTop;//SpacingTop; //h*0.5f
			//float y2 = y + ContentHeight*0.5f + SpacingTop;
			float x2 = x + SpacingLeft;
			if (Alignment == HorizontalAlign.Center)
				//x2 = x + Width*0.5f - w*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
				x2 = x + w*0.5f - ContentWidth*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
			else if (Alignment == HorizontalAlign.Right)
				x2 = x + Width - ContentWidth - SpacingRight; //labels.Sum(lbl => lbl.Width);
			//LabelDecorator.DebugBrush2?.DrawRectangle(x2 + SpacingLeft, y + SpacingTop, ContentWidth, ContentHeight);
			/*if (LabelDecorator.DebugFont is object)
			{
				var layout = LabelDecorator.DebugFont.GetTextLayout(Width.ToString() + " - " + w);
				LabelDecorator.DebugFont.DrawText(layout, x, y);
			}*/
			
			HoveredLabel = null;
			w = ContentWidth;
			ContentWidth = 0;
			ContentHeight = 0;
			//Hovered = false;
			foreach (ILabelDecorator label in labels)
			{
				if (Hud.Window.CursorInsideRect(x, y2, Width, label.Height))
				{
					if (HoveredBrush is object)
						HoveredBrush.DrawRectangle(x, y2, Width, label.Height);
					
					//Hovered = true;
				}
				
				//render width is the previous value set in Width and Height (this allow custom width settings to be dictated before drawing)
				float lHeight = label.Height;
				
				//LabelDecorator.DebugBrush2?.DrawRectangle(x2, y2 - label.Height*0.5f, w, label.Height);
				//LabelDecorator.Draw(label, x2, y2 - label.Height*0.5f);
				label.Width = w; //ContentWidth
				

				//LabelDecorator.DebugBrush3?.DrawRectangle(x2, y2 /*- lHeight*0.5f*/, label.Width, lHeight); //y2 - label.Height*0.5f
				label.Paint(x2, y2 /*- lHeight*0.5f*/);
				//LabelDecorator.DebugWrite(lHeight.ToString("F0"), x2 + w, y2);  //label.Width.ToString("F0") + "x" + label.Height.ToString("F0"), x2 + w, y2);
				//LabelDecorator.DebugWrite(label.Width.ToString("F0"), x + Width, y2);
				if (label.Visible)
				{
					Visible = true;

					y2 += lHeight;
					//y2 += label.Height;

					ContentHeight += label.Height;
					if (ContentWidth < label.Width)
						ContentWidth = label.Width;
					
					if (label.Hovered)
					{
						if (label is ILabelDecoratorCollection && ((ILabelDecoratorCollection)label).HoveredLabel is object)
							HoveredLabel = ((ILabelDecoratorCollection)label).HoveredLabel;
						else
							HoveredLabel = label;
					}
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
				
				ContentHeight += label.Height;
				if (ContentWidth < label.Width)
					ContentWidth = label.Width;
			}
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
		}
	}
}