/*

Wrapper with timer

*/

namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using System.Linq;

	using Turbo.Plugins.Default;

    public class LabelDelayedDecorator : ILabelDecorator, ILabelDecoratorCollection
    {
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public List<ILabelDecorator> Labels { get; set; }
		public ILabelDecorator HoveredLabel { get; private set; }
		
		public int Delay { 
			get { return _delay; }
			set {
				if (_delay != value)
				{
					_delay = value;
					
					Reset();
					/*if (Timer is object)
					{
						Timer.Stop();
						Timer.Reset();
					}*/					
				}
			}
		}
		private int _delay = 0;
		private IWatch Timer;

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

        public LabelDelayedDecorator(IController hud, ILabelDecorator label = null)
        {
			Hud = hud;
			Labels = label is object ? new List<ILabelDecorator>() {label} : new List<ILabelDecorator>();
			Timer = Hud.Time.CreateWatch();
        }

        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (!Enabled)
			//	return;
			if ((Labels == null || Labels.Count == 0) || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				//Width = 0;
				//Height = 0;
				Resize();
				Hovered = false;
				HoveredLabel = null;
				if (Timer.IsRunning)
					Timer.Stop();
				return;
			}
			
			//width and height for a row are always based on their child values
			var label = Labels.FirstOrDefault(lbl => LabelDecorator.IsVisible(lbl));
			ContentWidth = label.Width;
			ContentHeight = label.Height;
			var w = ContentWidth + SpacingLeft + SpacingRight;
			var h = ContentHeight + SpacingTop + SpacingBottom;
			if (Width < w)
				Width = w;
			if (Height < h)
				Height = h;
			//Width = ContentWidth + SpacingLeft + SpacingRight;
			//Height = ContentHeight + SpacingTop + SpacingBottom;
			
			//draw background and border
			// BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			// BorderBrush?.DrawRectangle(x, y, Width, Height);
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
			
			if (Delay == 0 || Timer.ElapsedMilliseconds > Delay) //Timer.TimerTest(Delay))
			{
				if (Timer.IsRunning)
					Timer.Stop();
				
				//draw background and border
				BackgroundBrush?.DrawRectangle(x, y, Width, Height);
				BorderBrush?.DrawRectangle(x, y, Width, Height);

				float y2 = y + SpacingTop; // + (Height - SpacingTop - SpacingBottom)*0.5f; //y + ContentHeight*0.5f + SpacingTop;//SpacingTop; //h*0.5f
				float x2 = x + SpacingLeft;
				/*if (Alignment == HorizontalAlign.Center)
					x2 += (Width - SpacingLeft - SpacingRight)*0.5f - ContentWidth*0.5f; //x + Width*0.5f - w*0.5f + SpacingLeft; //ContentWidth //labels.Sum(lbl => lbl.Width)*0.5f;
				else if (Alignment == HorizontalAlign.Right)
					x2 = x + Width - ContentWidth - SpacingRight; //labels.Sum(lbl => lbl.Width);*/
				
				//float lWidth = label.Width;
				w = Width - SpacingLeft - SpacingRight;
				h = Height - SpacingTop - SpacingBottom;
				if (label.Width < w)
					label.Width = w;
				if (label.Height < h)
					label.Height = h;
				
				label.Paint(x2, y2);
				Visible = label.Visible;
				//x2 += lWidth;

				if (label.Hovered)
				{
					if (label is ILabelDecoratorCollection && ((ILabelDecoratorCollection)label).HoveredLabel is object)
						HoveredLabel = ((ILabelDecoratorCollection)label).HoveredLabel;
					else
						HoveredLabel = label;
				}
				
				Hovered = HoveredLabel is object || Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
				if (Hovered)
					LabelDecorator.SetHint(this);
			}
			else if (Delay > 0)
			{
				if (!Timer.IsRunning)
				{
					Timer.Reset();
					Timer.Start();
				}
			}
			
			//LabelDecorator.DebugWrite(Delay.ToString() + " - " + Timer.ElapsedMilliseconds, x + Width, y);
			
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
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
				ContentWidth = label.Width;
				ContentHeight = label.Height;
				Height = ContentHeight + SpacingTop + SpacingBottom;
				Width = ContentWidth + SpacingLeft + SpacingRight;
			}
			else
			{
				ContentWidth = 0;
				ContentHeight = 0;
				Width = 0;
				Height = 0;
			}
		}
		
		public void Reset()
		{
			if (Timer.IsRunning)
				Timer.Stop();
			
			Timer.Reset();
		}
	}
}