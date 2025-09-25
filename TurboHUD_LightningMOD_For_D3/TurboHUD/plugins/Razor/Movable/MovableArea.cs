using System.Drawing;
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Movable
{
	public class MovableArea
	{
		public IMovable Owner { get; set; }
		public string Name { get; set; }
		public RectangleF Rectangle { 
			get
			{
				if (RectangleHistory.Count < 1)
					return default(RectangleF);
				else
					return RectangleHistory[RectangleHistory.Count - 1];
			}
			set
			{
				if (RectangleHistory.Count > 0 && value.Equals(RectangleHistory[RectangleHistory.Count - 1]))
					return;
				
				RectangleHistory.Add(value);
			}				
		}
		public bool Enabled { get; set; } = true;
		public bool SaveToConfig { get; set; } = true;
		public bool DeleteOnDisable { get; set; } = false;
		public ResizeMode ResizeMode { get; set; } = ResizeMode.Off;
		public ClipState ClipState { get; set; } = ClipState.BeforeClip;
		
		//private RectangleF OldRectangle;
		private List<RectangleF> RectangleHistory = new List<RectangleF>();
		
		public MovableArea(string s) {
			Name = s;
		}
		
		/*public bool IsHovered(IController Hud)
		{
			return Hud.Window.CursorInsideRect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
		}*/
		
		public void Move(float deltaX, float deltaY)
		{
			if (deltaX != 0 || deltaY != 0)
				Rectangle = new RectangleF(Rectangle.X + deltaX, Rectangle.Y + deltaY, Rectangle.Width, Rectangle.Height);
				//SetRectangle(Rectangle.X + deltaX, Rectangle.Y + deltaY, Rectangle.Width, Rectangle.Height);
		}
		
		public void SetConfig(float x, float y, float w, float h, bool enabled)
		{
			if (Rectangle.X != x || Rectangle.Y != y || Rectangle.Width != w || Rectangle.Height != h)
				Rectangle = new RectangleF(x, y, w, h); //SetRectangle(x, y, w, h);
				
			Enabled = enabled;
		}
		
		/*public void SetRectangle(float x, float y, float w, float h)
		{
			//OldRectangle = Rectangle;
			//RectangleHistory.Add(Rectangle);
			Rectangle = new RectangleF(x, y, w, h);
		}*/
		
		public void Undo()
		{
			//Rectangle = OldRectangle;
			if (RectangleHistory.Count > 1)
				RectangleHistory.RemoveAt(RectangleHistory.Count - 1);
			//Rectangle = RectangleHistory[RectangleHistory.Count - 1];			
		}
		
		public void Reset()
		{
			if (RectangleHistory.Count > 1)
				RectangleHistory.RemoveRange(1, RectangleHistory.Count - 1);
		}
	}
}