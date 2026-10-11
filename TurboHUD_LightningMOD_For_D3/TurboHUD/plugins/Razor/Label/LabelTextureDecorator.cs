namespace Turbo.Plugins.Razor.Label
{
	using System; //Func
	using System.Collections.Generic;
	using SharpDX.DirectWrite; //TextLayout

	using Turbo.Plugins.Default;

    public class LabelTextureDecorator : ILabelDecorator
    {
		public bool IgnoreHeight { get; set; } = true; //if containing labels shouldn't size themselves based on these dimensions
		
		public bool Enabled { get; set; } = true;
		public bool Hovered { get; set; }
		public bool Visible { get; set; } = true;
		public ILabelDecorator Hint { get; set; }
		
		public Func<ILabelDecorator, bool> OnBeforeRender { get; set; }
		public Action<ILabelDecorator> OnClick { get; set; }
		
		public ITexture Texture { //get; set; 
			get { return _texture; }
			set { 
				if (_texture != value)
				{
					_texture = value;
					//_resize = true;
					//Resize();
				}
			}
		}
		private ITexture _texture = null;
		//private bool _resize = false;

		public IBrush BackgroundBrush { get; set; }
		public IBrush BorderBrush { get; set; }
		
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center;
		public float SpacingLeft { get; set; }
		public float SpacingRight { get; set; }
		public float SpacingTop { get; set; }
		public float SpacingBottom { get; set; }
		/*public float SpacingLeft { 
			get { return _spacingLeft; }
			set { 
				if (_spacingLeft != value)
				{
					_spacingLeft = value;
					_resize = true;
				}
			}
		}
		private float _spacingLeft;
		public float SpacingRight { //get; set; }
			get { return _spacingRight; }
			set { 
				if (_spacingRight != value)
				{
					_spacingRight = value;
					_resize = true;
				}
			}
		}
		private float _spacingRight;
		public float SpacingTop { //get; set; }
			get { return _spacingTop; }
			set { 
				if (_spacingTop != value)
				{
					_spacingTop = value;
					_resize = true;
				}
			}
		}
		private float _spacingTop;
		public float SpacingBottom { //get; set; }
			get { return _spacingBottom; }
			set { 
				if (_spacingBottom != value)
				{
					_spacingBottom = value;
					_resize = true;
				}
			}
		}
		private float _spacingBottom;
		private bool _resize;*/

		public float TextureWidth { get; set; } //stays the same
		public float TextureHeight { get; set; } //stays the same
		public float ContentWidth { get; set; } //stays the same
		public float ContentHeight { get; set; } //stays the same
		public float Width { get; set; } //dynamic
		public float Height { get; set; } //dynamic
		public float LastX { get; private set; }
		public float LastY { get; private set; }

		public IController Hud { get; private set; }

        public LabelTextureDecorator(IController hud, ITexture texture = null)
        {
			Hud = hud;
			Texture = texture;
        }
		
        public void Paint(float x, float y, IBrush debugBrush = null)
        {
			//if (Font == null)
			//	return;
			//if (!Enabled)
			//	return;
			if (Texture == null || !LabelDecorator.IsVisible(this) || (OnBeforeRender is object && !OnBeforeRender(this)))
			{
				Width = 0;
				Height = 0;
				Visible = false;
				Hovered = false;
				return;
			}
			
			if (TextureHeight > 0)
			{
				if (TextureWidth == 0)
					TextureWidth = TextureHeight * (Texture.Width / Texture.Height);
			}
			else if (TextureWidth > 0)
			{
				if (TextureHeight == 0)
					TextureHeight = TextureWidth * (Texture.Height / Texture.Width);
			}
			else
			{
				TextureWidth = Texture.Width;
				TextureHeight = Texture.Height;
			}

			if (ContentHeight > 0)
			{
				if (ContentWidth == 0)
					ContentWidth = ContentHeight * (TextureWidth / TextureHeight);
			}
			else if (ContentWidth > 0)
			{
				if (ContentHeight == 0)
					ContentHeight = ContentWidth * (TextureHeight / TextureWidth);
			}
			else
			{					
				ContentHeight = TextureHeight;
				ContentWidth = TextureWidth;
			}
			
			float w = Width - SpacingLeft - SpacingRight;
			float h = Height - SpacingTop - SpacingBottom;
			/*if (Width > w)
			{
				cw = Width - SpacingLeft - SpacingRight;
				
			}*/
			
			Visible = true;
			
			BackgroundBrush?.DrawRectangle(x, y, Width, Height);
			BorderBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush?.DrawRectangle(x, y, Width, Height);
			LabelDecorator.DebugBrush2?.DrawRectangle(x + SpacingLeft, y + SpacingTop, Width - SpacingLeft - SpacingRight, Height - SpacingTop - SpacingBottom);
			//LabelDecorator.DebugWrite(TextureWidth.ToString("F0") + " x " + TextureHeight.ToString("F0"), x + Width, y);
			
			/*float w = Width - SpacingLeft - SpacingRight;
			float h = Height - SpacingTop - SpacingBottom;
			float x2 = x + SpacingLeft;
			//float y2 = y + Height*0.5f - h*0.5f;
			//float y2 = y + SpacingTop + (Height - SpacingTop - SpacingBottom)*0.5f - h*0.5f;
			float y2 = y + SpacingTop + h*0.5f - ContentHeight*0.5f; //y + (Height - SpacingTop - SpacingBottom)*0.5f - ContentHeight*0.5f + SpacingTop; //vertically centered + skew between top and bottom
				
			if (Alignment == HorizontalAlign.Center)
				x2 = x + SpacingLeft + w*0.5f - ContentWidth*0.5f; //x + (Width - SpacingLeft - SpacingRight)*0.5f - ContentWidth*0.5f + SpacingLeft;
			else if (Alignment == HorizontalAlign.Right)
				x2 = x + Width - ContentWidth - SpacingRight;*/
			
			float x2 = x + SpacingLeft + w*0.5f - TextureWidth*0.5f; //ContentWidth*0.5f - TextureWidth*0.5f; 
			float y2 = y + SpacingTop + h*0.5f - TextureHeight*0.5f; //ContentHeight*0.5f - TextureHeight*0.5f;
			
			Texture.Draw(x2, y2, TextureWidth, TextureHeight);
			
			Hovered = Hud.Window.CursorInsideRect(x, y, Width, Height); //calculation with old dimensions
			//Width = ContentWidth + SpacingLeft + SpacingRight;
			//Height = IgnoreHeight ? 1f : ContentHeight + SpacingTop + SpacingBottom;
			Height = ContentHeight + SpacingTop + SpacingBottom;
			Width = ContentWidth + SpacingLeft + SpacingRight;
			LastX = x;
			LastY = y;
		}
		
		public void Resize()
		{
			if (OnBeforeRender is object)
				OnBeforeRender(this);
			
			if (Texture is object)
			{
				//ContentHeight = texture.Height;
				//ContentWidth = texture.Width;
			if (TextureHeight > 0)
			{
				if (TextureWidth == 0)
					TextureWidth = TextureHeight * (Texture.Width / Texture.Height);
			}
			else if (TextureWidth > 0)
			{
				if (TextureHeight == 0)
					TextureHeight = TextureWidth * (Texture.Height / Texture.Width);
			}
			else
			{
				TextureWidth = Texture.Width;
				TextureHeight = Texture.Height;
			}

			if (ContentHeight > 0)
			{
				if (ContentWidth == 0)
					ContentWidth = ContentHeight * (TextureWidth / TextureHeight);
			}
			else if (ContentWidth > 0)
			{
				if (ContentHeight == 0)
					ContentHeight = ContentWidth * (TextureHeight / TextureWidth);
			}
			else
			{					
				ContentHeight = TextureHeight;
				ContentWidth = TextureWidth;
			}
				
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
			
			//_resize = false;
		}
	}
}