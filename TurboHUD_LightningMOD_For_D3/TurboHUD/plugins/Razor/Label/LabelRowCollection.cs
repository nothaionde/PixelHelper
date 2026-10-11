/*

Draws a stack of rows - like a table, but each row is explicitly defined

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelRowCollection : ILabelDecorator, ILabelDecoratorCollection
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
		
		public float[] RowWidths { get; private set; }

		public IController Hud { get; private set; }

        public LabelRowCollection(IController hud, params LabelRowDecorator[] rows)
        {
			Hud = hud;
			Labels = new List<ILabelDecorator>(rows);
			if (rows.Length > 0)
				RowWidths = new float[rows[0].Labels.Count];
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


			//calculate starting positions
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
			
			HoveredLabel = null;
			ContentHeight = 0;
			
			float[] widths = new float[RowWidths.Length];
			foreach (ILabelDecorator label in labels)
			{
				LabelRowDecorator row = (LabelRowDecorator)label;
				for (int i = 0; i < row.Labels.Count; ++i)
					row.Labels[i].Width = RowWidths[i];
				
				row.BackgroundBrush = row.Hovered && HoveredBrush is object ? HoveredBrush : null;
					
				row.Paint(x2, y2);
				y2 += row.Height;
				ContentHeight += row.Height;
				
				for (int i = 0; i < row.Labels.Count; ++i)
				{
					if (widths[i] < row.Labels[i].Width)
						widths[i] = row.Labels[i].Width;
				}
				
				if (row.Hovered)
				{
					if (row is ILabelDecoratorCollection && ((ILabelDecoratorCollection)row).HoveredLabel is object)
						HoveredLabel = ((ILabelDecoratorCollection)label).HoveredLabel;
					else
						HoveredLabel = row;
				}
			}
			
			Hovered = HoveredLabel is object || Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			if (Hovered)
				LabelDecorator.SetHint(this);

			ContentWidth = widths.Sum();
			RowWidths = widths;
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
			
			List<float> widths = new List<float>();
			foreach (ILabelDecorator row in Labels.Where(lbl => LabelDecorator.IsVisible(lbl)))
			{
				row.Resize();
				ContentHeight += row.Height;
				
				int i = 0;
				foreach (ILabelDecorator cell in ((LabelRowDecorator)row).Labels)
				{
					if (widths.Count <= i)
						widths.Add(cell.Width);
					else
					{
						if (widths[i] < cell.Width)
							widths[i] = cell.Width;
					}
					
					++i;
				}
			}
			
			RowWidths = widths.ToArray();
			ContentWidth = RowWidths.Sum();
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
		}
		
		public void RecalculateWidth()
		{
			if (Labels.Count == 0)
				return;
			
			LabelRowDecorator row = (LabelRowDecorator)Labels[0];
			Width = 0;
			for (int i = 0; i < row.Labels.Count; ++i)
			{
				if (row.Labels[i].Enabled)
					Width += RowWidths[i];
			}
		}
	}
}