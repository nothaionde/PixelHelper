/*

Draws a table of labels with columns and rows

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelTableDecorator : ILabelDecorator, ILabelDecoratorCollection
    {
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		public Func</*ILabelDecoratorCollection*/LabelRowDecorator, int, bool> OnFillRow { get; set; } //pass in the row labels to be filled, the row index, and return whether or not it is the last row
		public bool FillWidth { get; set; } = false;
		public Func<int> Count { get; set; } //optional, tell the table how many data rows there are to automatically determine if multiple columns of rows need to be partitioned
		//private int _count;
		
		public LabelRowDecorator Header { get; set; }
		public LabelRowDecorator Row { get; set; }
		public List<ILabelDecorator> Labels { get; set; } //row accessor for recursive checks using ILabelDecoratorCollection type
		public ILabelDecorator HoveredLabel { get; private set; } //hover accessor for recursive checks

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
		
		public float[] RowWidths { get; private set; }
		public float MaxRowWidth { get; private set; }
		public float MaxRowHeight { get; private set; }
		public int MaxColumnsPerPage { get; private set; }
		public int MaxRowsPerColumn { get; private set; }
		
		//private LabelCellDecorator HoveredCell;
		public int HoveredRow { get; private set; } = -1;
		public int HoveredCol { get; private set; } = -1;

        public LabelTableDecorator(IController hud, LabelRowDecorator row)
        {
			Hud = hud;
			Row = row;
			Labels = row.Labels;
			Row.EnforceWidths = true;
			//HoveredCell = new LabelCellDecorator(this);
        }

        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (!Enabled)
			//	return;
			if (((Labels == null || Labels.Count == 0) && !LabelDecorator.IsVisible(Header)) || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				HoveredLabel = null;
				return;
			}
			
			//Hud.Sound.Speak("test");

			//LabelDecorator.DebugWrite(Width.ToString("F0"), x + Width, y);
			
			//LabelDecorator.DebugBrush?.DrawLine(x, y, x, y - Height);
			//LabelDecorator.DebugWrite(x.ToString("F0"), x + 5, y - Height);

			
			//calculate starting positions
			var labels = Labels.Where(lbl => LabelDecorator.IsVisible(lbl));
			//var maxHeight = labels.Max(lbl => lbl.Height);
			var w = Width - SpacingLeft - SpacingRight; //ContentWidth + SpacingLeft + SpacingRight;
			var h = Height - SpacingTop - SpacingBottom; //ContentHeight + SpacingTop + SpacingBottom;
			//float y2 = y + labels.Max(lbl => lbl.Height)*0.5f;
			float y2 = y + SpacingTop; //not trying to align it with sibling labels because this is a column // + (Height - SpacingTop - SpacingBottom)*0.5f; //y + ContentHeight*0.5f + SpacingTop;//SpacingTop; //h*0.5f
			//float y2 = y + ContentHeight*0.5f + SpacingTop;
			float x2 = x + SpacingLeft;

			//recalculate if data set is too large to fit vertically
			//float cols = 1;
			int rowsPerCol = 0;
			int colsPerPage = 0;
			/*if (FitCountToScreen > 0)
			{
				if (y2 < 0)
					y2 = Row.Height;
			
				//float maxScreenHeight = Hud.Window.Size.Height;
				if (y2 + FitCountToScreen*Row.Height > Hud.Window.Size.Height)
				{
					float maxRowsPerCol = (float)Math.Floor(((Hud.Window.Size.Height - Row.Height) - y2) / Row.Height);
					float cols = (float)Math.Ceiling((float)FitCountToScreen / maxRowsPerCol);
					rowsPerCol = (int)Math.Floor((float)FitCountToScreen / cols); //distribute evenly across columns
					
					ContentWidth = Row.Width * cols;
					ContentHeight = Row.Height * rowsPerCol;
					Width = ContentWidth + SpacingLeft + SpacingRight;
					Height = ContentHeight + SpacingTop + SpacingBottom;
				}
			}*/
			if (Count is object)
			{
				int count = Count();
				//float y = LastY;
				if (y < 0)
					y = MaxRowHeight;
			
				//float maxScreenHeight = Hud.Window.Size.Height;
				if (y + count*MaxRowHeight > Hud.Window.Size.Height)
				{
					float maxRowsPerCol = (float)Math.Floor(((Hud.Window.Size.Height - MaxRowHeight) - y) / MaxRowHeight);
					float cols = (float)Math.Ceiling((float)count / maxRowsPerCol);
					rowsPerCol = (int)Math.Ceiling((float)count / cols); //distribute evenly across columns
					
					//Hud.Sound.Speak(cols.ToString("F0") + " with " + rowsPerCol + " per column");
					
					//limit number of columns drawn if the width of the table exceeds the width of the visible space
					if (cols > 1)
					{
						if (x < 0)
							cols = (float)Math.Floor((x + ContentWidth + SpacingLeft) / MaxRowWidth); //ContentWidth + SpacingLeft //-= (float)Math.Ceiling((x*-1) / MaxRowWidth);
						else if (x + cols*MaxRowWidth > Hud.Window.Size.Width)
							cols = (float)Math.Floor((Hud.Window.Size.Width - x) / MaxRowWidth);
							//cols -= (float)Math.Ceiling((x + cols*MaxRowWidth - Hud.Window.Size.Width) / MaxRowWidth);
						
						//Hud.TextLog.Log("_toggle", "LabelTableDecorator cols = (" + x + " + " + (ContentWidth+SpacingLeft) + ") / " + MaxRowWidth, false, true);
					}
					MaxColumnsPerPage = (int)cols;
					MaxRowsPerColumn = rowsPerCol;
					
					//Hud.Sound.Speak((MaxColumnsPerPage*MaxRowsPerColumn).ToString()); //MaxColumnsPerPage.ToString() + " " + MaxRowsPerColumn + " " + 
					
					//ContentWidth = maxRowWidth * cols;
					ContentWidth = (float)Math.Max(ContentWidth, MaxRowWidth * cols); //MaxRowWidth * cols; //
					ContentHeight = MaxRowHeight * rowsPerCol;
					if (Header is object)
						ContentHeight += Header.Height;
					
					Width = ContentWidth + SpacingLeft + SpacingRight;
					Height = ContentHeight + SpacingTop + SpacingBottom;
					w = ContentWidth;
					h = ContentHeight;
				}
			}

			//draw background and border
			BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			BorderBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			
			//stretch to fill specified Width?
			float fillRatio = 1f;
			if (FillWidth && rowsPerCol == 0 && ContentWidth > 0)
				//float fillWidth = Width - SpacingLeft - SpacingRight;
				fillRatio = (Width - SpacingLeft - SpacingRight) / ContentWidth;
			/*else
			{
				if (Alignment == HorizontalAlign.Center)
					x2 = x + SpacingLeft + w*0.5f - ContentWidth*0.5f; //Width*0.5f - w*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
				else if (Alignment == HorizontalAlign.Right)
					x2 = x + Width - ContentWidth - SpacingRight;
			}*/
			//LabelDecorator.DebugBrush2?.DrawRectangle(x2 + SpacingLeft, y + SpacingTop, ContentWidth, ContentHeight);

			int lastHoveredRow = HoveredRow;
			int lastHoveredCol = HoveredCol;
			HoveredLabel = null;
			HoveredRow = -1;
			HoveredCol = -1;
			//ContentWidth = 0;
			//ContentHeight = 0;
			//ContentWidth = 0;
			float nextContentWidth = 0;
			float nextContentHeight = 0;
			MaxRowWidth = 0;
			MaxRowHeight = 0;
			float[] widths = new float[Row.Labels.Count];
			
			int highlightColumn = -1;
			if (Header is object && Header.Enabled)
			{
				//update widths
				if (Header.Labels.Count == RowWidths.Length)
				{
					for (int j = 0; j < Header.Labels.Count; ++j)
						Header.Labels[j].Width = RowWidths[j] * fillRatio;
				}
				
				var x3 = x + SpacingLeft;
				if (Header.Alignment == HorizontalAlign.Left)
					x3 += w*0.5f - Header.Width*0.5f;
				else if (Header.Alignment == HorizontalAlign.Right)
					x3 = x + Width - Header.Width*0.5f - SpacingRight;
				
				//var hHeight = Header.Height;
				Header.Paint(x3, y2);
				//y2 += hHeight;
				y2 += Header.Height;
				
				if (Header.Labels.Count == RowWidths.Length)
				{
					for (int j = 0; j < Header.Labels.Count; ++j)
					{
						if (Header.Labels[j].Hovered)
							highlightColumn = j;
						
						if (widths[j] < Header.Labels[j].Width)
							widths[j] = Header.Labels[j].Width;
					}
				}
				
				ContentHeight += Header.Height;
				//ContentWidth = Header.Width;
				nextContentHeight += Header.Height;
				nextContentWidth = Header.Width;
				Visible = true;
				
			}
			
			//bool resize = false;
			for (int i = 0, r = 0; OnFillRow(Row, i); ++i)
			{
				if (!Row.Enabled)
				{
					//resize = true;
					continue;
				}
				
				Visible = true;
				//Hud.Sound.Speak(i.ToString());
				
				//float rHeight = Row.Height;
				//Row.Labels[i].Width = RowWidths[i];
				//Row.Paint(x2, y2);
				
				//y2 += rHeight;
				//ContentHeight += rHeight;
				
				if (rowsPerCol > 0)
				{
					if (r >= rowsPerCol)
					{
						r = 0;
						x2 += Row.Width;
						y2 = y + SpacingTop;
					}
					
					r++;
				}
				
				//update widths
				for (int j = 0; j < Row.Labels.Count; ++j)
					Row.Labels[j].Width = RowWidths[j] * fillRatio;
				
				var hHeight = Row.Height;
				
				if (HoveredBrush is object)
				{
					//if (lastHoveredCol > -1 && HoveredCol != lastHoveredCol)
					//	Row.Labels[lastHoveredCol].BackgroundBrush = null;

					if (lastHoveredRow == i)
					{
						var tmp = Row.BackgroundBrush;
						Row.BackgroundBrush = HoveredBrush;
						Row.Paint(x2, y2);
						Row.BackgroundBrush = tmp;
					}
					else if (highlightColumn > -1)//(HoveredCol > -1 && lastHoveredCol != HoveredCol)
					{
						var tmp = Row.Labels[highlightColumn].BackgroundBrush;
						Row.Labels[highlightColumn].BackgroundBrush = HoveredBrush;
						Row.Paint(x2, y2);
						Row.Labels[highlightColumn].BackgroundBrush = tmp;
					}
					else
						Row.Paint(x2, y2);
				}
				else
					Row.Paint(x2, y2);
				
				//LabelDecorator.DebugWrite(x2.ToString("F0"), x + Width, y2);
				
				//x2 = x + SpacingLeft;
				//var hHeight = Row.Height;
				//Row.Paint(x2, y2); //Row.Paint(x + SpacingLeft, y2);
				y2 += hHeight; //Row.Height; //rHeight;
				
				if (MaxRowHeight < Row.Height)
					MaxRowHeight = Row.Height;

				//ContentHeight += Row.Height; //rHeight;
				//if (ContentWidth < Row.Width)
				//	ContentWidth = Row.Width;
				nextContentHeight += Row.Height;
				if (nextContentWidth < Row.Width)
					nextContentWidth = Row.Width;
				
				for (int j = 0; j < Row.Labels.Count; ++j)
				{
					if (widths[j] < Row.Labels[j].Width)
						widths[j] = Math.Max(RowWidths[j], Row.Labels[j].Width); //Row.Labels[j].Width
				}
				
				//check on hover state
				if (Row.Hovered)
				{
					if (Row.HoveredLabel is object)
					{
						HoveredLabel = Row.HoveredLabel; //Row.HoveredLabel ?? Row;
						HoveredCol = ((ILabelDecoratorCollection)Row).Labels.IndexOf(HoveredLabel);
					}
					else
						HoveredLabel = Row;
					
					HoveredRow = i;
				}
			}
			
			RowWidths = widths;
			MaxRowWidth = widths.Sum();
			//ContentWidth = Row.Width; //widths.Sum();
			//Hud.TextLog.Log("_toggle", "LabelTableDecorator widths = (" + string.Join(", ", widths) + ")", false, true);
			
			Hovered = HoveredLabel is object || Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			if (Hovered)
			{
				if (HoveredRow >= 0)
					OnFillRow(Row/*this*/, HoveredRow);

				LabelDecorator.SetHint(this);
			}

			if (rowsPerCol > 0)
			{
				Height = ContentHeight + SpacingTop + SpacingBottom;
				Width = ContentWidth + SpacingLeft + SpacingRight;
			}
			else
			{
				Height = nextContentHeight + SpacingTop + SpacingBottom;
				Width = nextContentWidth + SpacingLeft + SpacingRight;
			}
			LastX = x;
			LastY = y;
			
			//LabelDecorator.DebugWrite(Height.ToString("F0"), x, y + Height);
			//LabelDecorator.DebugWrite(ContentWidth.ToString("F0"), x + Width, y);
			//if (resize)
			//	Resize();
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			//ContentWidth = 0;
			ContentHeight = 0;
			MaxRowHeight = 0; //Row.Height;
			MaxRowWidth = 0; //Row.Width;
			
			float[] widths = new float[Row.Labels.Count];
			//float maxRowWidth = 0;
			//float maxRowHeight = 0;
			for (int i = 0; OnFillRow(Row, i); ++i)
			{
				Row.Resize();
				
				if (MaxRowHeight < Row.Height)
					MaxRowHeight = Row.Height;
		      		
				//update widths
				for (int j = 0; j < Row.Labels.Count; ++j)
				{
					ILabelDecorator label = Row.Labels[j];
					if (widths[j] < label.Width)
						widths[j] = label.Width;
				}
				
				ContentHeight += Row.Height;
			}
			
			if (Header is object)
			{
				Header.Resize();
				
				//update widths
				if (Header.Labels.Count == Row.Labels.Count)
				{
					for (int j = 0; j < Header.Labels.Count; ++j)
					{
						ILabelDecorator label = Header.Labels[j];
						if (widths[j] < label.Width)
							widths[j] = label.Width;
					}
				}
				else
					ContentWidth = Header.Width;
				
				ContentHeight += Header.Height;
			}
			
			MaxRowWidth = widths.Sum();
			RowWidths = widths;
			
			if (Count is object)
			{
				int count = Count();
				float y = LastY;
				if (y <= 0)
					y = MaxRowHeight;
			
				//float maxScreenHeight = Hud.Window.Size.Height;
				if (y + count*MaxRowHeight > Hud.Window.Size.Height)
				{
					float maxRowsPerCol = (float)Math.Floor(((Hud.Window.Size.Height - MaxRowHeight) - y) / MaxRowHeight);
					float cols = (float)Math.Ceiling((float)count / maxRowsPerCol);
					int rowsPerCol = (int)Math.Floor((float)count / cols); //distribute evenly across columns
					
					if (cols > 1)
					{
						float x = LastX;
						if (x < 0)
							cols -= (float)Math.Ceiling((x*-1) / MaxRowWidth);
						else if (x + cols*MaxRowWidth > Hud.Window.Size.Width)
							cols -= (float)Math.Ceiling((x + cols*MaxRowWidth - Hud.Window.Size.Width) / MaxRowWidth);
					}
					
					MaxColumnsPerPage = (int)cols;
					MaxRowsPerColumn = rowsPerCol;
					
					//ContentWidth = maxRowWidth * cols;
					ContentWidth = (float)Math.Max(ContentWidth, MaxRowWidth * MaxColumnsPerPage); //(float)Math.Max(ContentWidth, MaxRowWidth * cols);
					ContentHeight = MaxRowHeight * rowsPerCol;
					if (Header is object)
						ContentHeight += Header.Height;
					Width = ContentWidth + SpacingLeft + SpacingRight;
					Height = ContentHeight + SpacingTop + SpacingBottom;
					
					
					return;
				}
			}
		
			ContentWidth = (float)Math.Max(ContentWidth, MaxRowWidth); //widths.Sum()
			//RowWidths = widths;
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
		}
		
		public void RecalculateWidth()
		{
			for (int i = 0; OnFillRow(Row, i); ++i)
			{
				if (Row.Enabled)
				{
					Width = 0;
					for (int j = 0; j < Row.Labels.Count; ++j)
					{
						if (Row.Labels[j].Enabled)
							Width += RowWidths[j];
					}
					
					break;
				}
			}
		}
	}
}