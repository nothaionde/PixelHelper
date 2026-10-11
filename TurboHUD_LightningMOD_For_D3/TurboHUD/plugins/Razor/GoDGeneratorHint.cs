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
    public class GoDGeneratorHint : BasePlugin, IMovable
	{
		public int MinimumGeneratorsEquipped { get; set; } = 2; //only show if there are at least this many generators equipped
		public float IconSizeMultiplier { get; set; } = 0.7f;

		public float StartingPositionX { get; set; }
		public float StartingPositionY { get; set; }
		
		public IFont TextFont { get; set; }
		
		public List<MovableArea> MovableAreas { get; set; }

		private HashSet<uint> GeneratorSkills;
		
        public GoDGeneratorHint() : base()
		{
			Enabled = true;
		}

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//TimerBrush = Hud.Render.CreateBrush(120, 51, 255, 204, 0);
			TextFont = Hud.Render.CreateFont("tahoma", 7, 255, 245, 218, 66, true, false, 242, 0, 0, 0, true); //25, 225, 255

			StartingPositionX = Hud.Window.Size.Width * 0.5f + 175f; 
			StartingPositionY = Hud.Window.Size.Height * 0.6f;
			
			GeneratorSkills = new HashSet<uint>()
			{
				Hud.Sno.SnoPowers.DemonHunter_Bolas.Sno,
				Hud.Sno.SnoPowers.DemonHunter_HungeringArrow.Sno,
				Hud.Sno.SnoPowers.DemonHunter_EntanglingShot.Sno,
				Hud.Sno.SnoPowers.DemonHunter_EvasiveFire.Sno,
				Hud.Sno.SnoPowers.DemonHunter_Grenades.Sno,
			};
		}
		
		//drawing Floater
		public void OnRegister(MovableController mover)
		{
			ITexture icon = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.DemonHunter_HungeringArrow.NormalIconTextureId);
			string name = Hud.Sno.SnoPowers.DemonHunter_HungeringArrow.NameLocalized;
			TextLayout layout = TextFont.GetTextLayout(name);
			float height = icon.Height*IconSizeMultiplier;
			float width = icon.Width*IconSizeMultiplier;

			//initialize position and dimension elements
			mover.CreateArea(
				this,
				"Name", //area name
				new RectangleF(StartingPositionX + width*0.5f - layout.Metrics.Width*0.5f, StartingPositionY - layout.Metrics.Height, layout.Metrics.Width, layout.Metrics.Height), //position + dimensions
				true, //enabled at start?
				true, //save changes to config file?
				ResizeMode.Off //resizable?
			);

			mover.CreateArea(
				this,
				"Icon", //area name
				new RectangleF(StartingPositionX, StartingPositionY, icon.Width*IconSizeMultiplier, height), //position + dimensions
				true, //enabled at start?
				true, //save changes to config file?
				ResizeMode.FixedRatio //resizable?
			);
		}
		
		//public override void PaintFloater(float deltaX = 0, float deltaY = 0)
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			if (Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.DemonHunter || !Hud.Game.Me.Powers.BuffIsActive(484289) || (MinimumGeneratorsEquipped < 1 ? true : MinimumGeneratorsEquipped == 1 ? Hud.Game.Me.Powers.UsedSkills.Any(s => GeneratorSkills.Contains(s.SnoPower.Sno)) : Hud.Game.Me.Powers.UsedSkills.Count(s => GeneratorSkills.Contains(s.SnoPower.Sno)) < MinimumGeneratorsEquipped))
				return;
			
			float x = area.Rectangle.X + deltaX;
			float y = area.Rectangle.Y + deltaY;
			
			//find out which generator the GoD set is using
			IBuff buff = Hud.Game.Me.Powers.GetBuff(484289);
			ISnoPower generator = null;
			
			if (buff.IconCounts[2] > 0)
				generator = Hud.Sno.SnoPowers.DemonHunter_Grenades;
			else if (buff.IconCounts[3] > 0)
				generator = Hud.Sno.SnoPowers.DemonHunter_Bolas;
			else if (buff.IconCounts[4] > 0)
				generator = Hud.Sno.SnoPowers.DemonHunter_HungeringArrow;
			else if (buff.IconCounts[5] > 0)
				generator = Hud.Sno.SnoPowers.DemonHunter_EntanglingShot;
			else if (buff.IconCounts[6] > 0)
				generator = Hud.Sno.SnoPowers.DemonHunter_EvasiveFire;
			
			if (generator is object)
			{
				if (area.Name == "Name")
				{
					TextLayout layout = TextFont.GetTextLayout(generator.NameLocalized);
					TextFont.DrawText(layout, x + area.Rectangle.Width*0.5f - layout.Metrics.Width*0.5f, y); //centered
				}
				else //if (area.Name == "Icon")
				{
					ITexture icon = Hud.Texture.GetTexture(generator.NormalIconTextureId);
					if (icon is object)
						icon.Draw(x, y, icon.Width * (area.Rectangle.Height/icon.Height), area.Rectangle.Height);
				}
			}
		}
    }
}