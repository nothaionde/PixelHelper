//using SharpDX.Direct2D1;
//using SharpDX.DirectInput;
using SharpDX.DirectWrite;
//using System; //Math
using System.Collections.Generic;
using System.Drawing; //RectangleF
//using System.Globalization;
using System.Linq;
//using System.Media;
//using System.Threading;

using Turbo.Plugins.Default;
using Turbo.Plugins.Razor.Movable;

namespace Turbo.Plugins.Razor
{
    public class JessethHint : BasePlugin, IMovable
	{
		public bool HideWhenActive { get; set; } = true;
		public float IconSizeMultiplier { get; set; } = 0.7f;

		public float StartingPositionX { get; set; }
		public float StartingPositionY { get; set; }
		
		public IFont TextFont { get; set; }
		public IBrush MissingBrush { get; set; }
		
		public List<MovableArea> MovableAreas { get; set; }

        public JessethHint() : base()
		{
			Enabled = true;
		}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//TimerBrush = Hud.Render.CreateBrush(120, 51, 255, 204, 0);
			TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 245, 218, 66, true, false, 242, 0, 0, 0, true); //25, 225, 255
			MissingBrush = Hud.Render.CreateBrush(100, 255, 0, 0, 0);

			StartingPositionX = Hud.Window.Size.Width * 0.5f + 225f; 
			StartingPositionY = Hud.Window.Size.Height * 0.6f;
		}
		
		//drawing Floater
		public void OnRegister(MovableController mover)
		{
			ITexture icon = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.DemonHunter_HungeringArrow.NormalIconTextureId);
			float height = icon.Height*IconSizeMultiplier;

			//initialize position and dimension elements
			mover.CreateArea(
				this,
				"Icon", //area name
				new RectangleF(StartingPositionX, StartingPositionY, icon.Width*IconSizeMultiplier, height), //position + dimensions
				true, //enabled at start?
				true, //save changes to config file?
				ResizeMode.FixedRatio
			);
		}
		
		//public override void PaintFloater(float deltaX = 0, float deltaY = 0)
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			//MovableArea area = GetArea("Icon");
			//if (area == null || !area.Enabled)
			//	return;

			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Necromancer || !Hud.Game.Me.Powers.BuffIsActive(476048))
				return;
			
			ITexture icon = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Necromancer_CommandSkeletons.NormalIconTextureId);
			if (icon == null)
				return;

			float x = area.Rectangle.X + deltaX;
			float y = area.Rectangle.Y + deltaY;
			
			//draw icon
			bool isBuffActive = Hud.Game.Me.Powers.BuffIsActive(476048, 1);
			if (!isBuffActive || !HideWhenActive)
			{
				float iWidth = icon.Width * (area.Rectangle.Height/icon.Height); //icon.Width*IconSizeMultiplier;
				float iHeight = area.Rectangle.Height; //icon.Height*IconSizeMultiplier;
				icon.Draw(x, y, iWidth, iHeight);

				//draw indication that the buff is missing
				if (!isBuffActive)
				{
					MissingBrush.DrawRectangle(x, y, iWidth, iHeight);
					//MissingBrush.StrokeWidth = 1f;
					//MissingBrush.Opacity = 1f;
					//MissingBrush.DrawRectangle(x, y, iWidth, iHeight);
					Hud.Texture.DebuffFrameTexture.Draw(x, y, iWidth, iHeight, 0.85f);
				}
			}
		}
    }
}