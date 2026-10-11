namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using SharpDX.DirectWrite; //TextLayout

	using Turbo.Plugins.Default;

    public class LabelStringDecorator : ILabelDecorator
    {
		public bool StrikeOut { get; set; } = false;
		public IBrush StrikeBrush { get; set; }
		
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public IFont Font { get; set; }
		public IFont HoveredFont { get; set; }
		public float FontSize { get; set; }
		public string StaticText { //get; set; }
			get { return _text; }
			set { 
				if (_text != value)
				{
					_text = value;
					
					//if (AutomaticResizeOnChange)
					//	Resize();
					/*if (Font is object && !string.IsNullOrEmpty(_text))
					{
						var layout = Font.GetTextLayout(_text);
						ContentHeight = layout.Metrics.Height;
						ContentWidth = layout.Metrics.Width;
						//Height = ContentHeight + SpacingTop + SpacingBottom;
						//Width = ContentWidth + SpacingLeft + SpacingRight;
					}
					else
					{
						ContentHeight = 0;
						ContentWidth = 0;
						//Height = 0;
						//Width = 0;
					}*/
				}
			} 
		}
		public Func<string> TextFunc { get; set; }
		public float FuncCacheDuration { get; set; } = 0; //in seconds
		public bool AutomaticResizeOnChange { get; set; } = true;
		private string _text;
		private int LastUpdateTick;

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

        public LabelStringDecorator(IController hud, Func<string> func)
        {
			Hud = hud;
			TextFunc = func;
        }

        public LabelStringDecorator(IController hud, string s = null)
        {
			Hud = hud;
			
			if (s is object)
				StaticText = s;
				//_text = s;
        }
		
        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (Font == null)
			//	return;
			//if (!Enabled)
			//	return;
			if (Font == null || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				return;
			}
			
			if (TextFunc is object)
			{
				var diff = Hud.Game.CurrentGameTick - LastUpdateTick;
				if (FuncCacheDuration == 0 || string.IsNullOrEmpty(StaticText) || diff < 0 || diff > FuncCacheDuration*60f)
				{
					float w = Width;
					StaticText = TextFunc.Invoke();
					
					//prevent flickering from the automatic resize temporarily changing total label width when it is overriden
					if (AutomaticResizeOnChange && Width < w)
						Width = w;
					
					LastUpdateTick = Hud.Game.CurrentGameTick;
				}
			}
			
			var text = StaticText; //!string.IsNullOrEmpty(StaticText) ? StaticText : TextFunc?.Invoke();
			if (!string.IsNullOrEmpty(text))
			{
				Visible = true;
				
				var layout = Font.GetTextLayout(text);
				if (FontSize > 0)
					layout.SetFontSize(FontSize, new TextRange(0, text.Length));
				
				ContentHeight = layout.Metrics.Height;
				ContentWidth = layout.Metrics.Width;

				float h = Height - SpacingTop - SpacingBottom; //ContentHeight + SpacingTop + SpacingBottom;
				float w = Width - SpacingLeft - SpacingRight; //ContentWidth + SpacingLeft + SpacingRight;
				float x2 = x + SpacingLeft;
				//float y2 = y + Height*0.5f - h*0.5f;
				//float y2 = y + SpacingTop + (Height - SpacingTop - SpacingBottom)*0.5f - h*0.5f;
				//float y2 = y + Height*0.5f - h*0.5f + SpacingTop; //vertically centered + skew between top and bottom
				float y2 = y + SpacingTop; 
				if (h > 0)
					y2 += h*0.5f - ContentHeight*0.5f;
				
				if (Alignment == HorizontalAlign.Center)
					x2 += w*0.5f - ContentWidth*0.5f; //Width*0.5f - w*0.5f;
				else if (Alignment == HorizontalAlign.Right)
					x2 += w - ContentWidth; //Width - w;
				
				BackgroundBrush?.DrawRectangle(x, y, Width, Height);
				BorderBrush?.DrawRectangle(x, y, Width, Height);
				LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
				LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
				//LabelDecorator.DebugWrite(y.ToString() + " -> " + (y+Height), x + Width, y);
				
				Hovered = Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
				if (Hovered)
				{
					LabelDecorator.SetHint(this);
					(HoveredFont ?? Font).DrawText(layout, x2, y2);
				}
				else
					Font.DrawText(layout, x2, y2);
				
				if (StrikeOut)
				{
					if (StrikeBrush == null)
						StrikeBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 2);
					
					StrikeBrush.DrawLine(x2, y2 + layout.Metrics.Height*0.6f, x2 + layout.Metrics.Width, y2 + layout.Metrics.Height*0.6f);
				}

				
				//debug
				//if (Hint is object)
				//	((StringLabelDecorator)Hint).StaticText = Width.ToString("F0");
				
				//LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
				//LabelDecorator.DebugBrush2?.DrawRectangle(x2 - SpacingLeft, y2 - SpacingTop, ContentWidth, ContentHeight);

				Height = ContentHeight + SpacingTop + SpacingBottom; //h;
				Width = ContentWidth + SpacingLeft + SpacingRight; //w;
				LastX = x;
				LastY = y;
			}
			else
			{
				//will need a dimension recalculation upon resumption
				Hovered = false;
			}
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				Visible = OnBeforeRender(this);

			if (Font == null)
				return;
			
			if (TextFunc is object)
			{
				var diff = Hud.Game.CurrentGameTick - LastUpdateTick;
				if (FuncCacheDuration == 0 || string.IsNullOrEmpty(StaticText) || diff < 0 || diff > FuncCacheDuration*60f)
				{
					StaticText = TextFunc.Invoke();
					LastUpdateTick = Hud.Game.CurrentGameTick;
				}
			}
			
			//var text = TextFunc is object ? TextFunc?.Invoke() : StaticText; //!string.IsNullOrEmpty(StaticText) ? StaticText : TextFunc?.Invoke();
			var text = StaticText;
			if (!string.IsNullOrEmpty(text))
			{
				var layout = Font.GetTextLayout(text);
				ContentHeight = layout.Metrics.Height;
				ContentWidth = layout.Metrics.Width;
				Height = ContentHeight + SpacingTop + SpacingBottom;
				Width = ContentWidth + SpacingLeft + SpacingRight;
			}
			else
			{
				ContentHeight = 0;
				ContentWidth = 0;
				Height = 0;
				Width = 0;
				//Height = SpacingTop + SpacingBottom;
				//Width = SpacingLeft + SpacingRight;
			}
		}
	}
}