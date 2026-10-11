namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using SharpDX.DirectWrite; //TextLayout

	using Turbo.Plugins.Default;

    public class LabelDeltaDecorator : ILabelDecorator
    {
		public double? FixedStartValue { get; set; } = null; //optional fixed starting value
		public Func<double> Value { get; set; }
		public Func<double, string> Format { get; set; } = (d) => d.ToString("0.##");
		public Func<double, double, double> DeltaFunc { get; set; } = (dNow, dPrev) => dNow - dPrev;
		public IFont Font { get; set; }
		//public int DeltaShowTime { get; set; } = 2000; //milliseconds
		public float DeltaInterval { get; set; } = 4; //in seconds
		
		//public TopLabelDecorator ArrowUp { get; set; }
		//public TopLabelDecorator ArrowDown { get; set; }
		public string ArrowUp { get; set; } = "+"; //"⮝"; //⮝⯅⭡
		public string ArrowDown { get; set; } = "-"; //"⮟"; //⮟⯆
		public IFont UpFont { get; set; }
		public IFont DownFont { get; set; }
		public IFont EvenFont { get; set; }

		//bookkeeping
		private IWatch DeltaWatch; //{ get; set; }
		private LabelStringDecorator Label;
		private double LastSeenValue;
		private int LastSeenTick;
		private double StartValue;
		private bool DeltaIncreased;

		//ILabelDecorator
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
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

        public LabelDeltaDecorator(IController hud, Func<double> func)
        {
			Hud = hud;
			Value = func;
        }
		
        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (Font == null)
			//	return;
			//if (!Enabled)
			//	return;
			if (Font == null || Value == null || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				return;
			}
			
			if (Label == null)
				InitLabel();
			
			//if (DeltaWatch == null)
			//	DeltaWatch = Hud.Time.CreateWatch();
			double valueNow = Value.Invoke();
			if (LastSeenValue != valueNow)
			{
				LastSeenValue = valueNow;
				LastSeenTick = Hud.Game.CurrentGameTick;
			}
			else if (!FixedStartValue.HasValue && LastSeenValue != StartValue)
			{
				int diff = Hud.Game.CurrentGameTick - LastSeenTick;
				if (diff < 0 || diff > DeltaInterval*60)
					StartValue = LastSeenValue;
			}
			
			double delta = DeltaFunc(valueNow, StartValue);
			Label.StaticText = Format(delta); //valueNow.ToString("F4"); //
			if (Label.Width < Width)
				Label.Width = Width;
			if (Label.Height < Height)
				Label.Height = Height;
			Label.Paint(x, y);
			
			ContentWidth = Label.ContentWidth;
			ContentHeight = Label.ContentHeight;
			Width = Label.Width;
			Height = Label.Height;
			Hovered = Label.Hovered;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);

			if (Font == null)
				return;
			
			if (Label == null)
				InitLabel();
			
			//if (DeltaWatch == null)
			//	DeltaWatch = Hud.Time.CreateWatch();
			StartValue = FixedStartValue.HasValue ? FixedStartValue.Value : Value.Invoke();
			
			Label.StaticText = Format(0);
			
			ContentWidth = Label.ContentWidth;
			ContentHeight = Label.ContentHeight;
			Width = Label.Width;
			Height = Label.Height;
		}
		
		private void InitLabel()
		{
			Label = new LabelStringDecorator(Hud) {Font = Font, SpacingTop = SpacingTop, SpacingBottom = SpacingBottom, SpacingLeft = SpacingLeft, SpacingRight = SpacingRight};
		}
	}
}