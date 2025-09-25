/*

IMovable represents a plugin that implements UI elements that can be picked up, dragged or otherwise moved around the screen

*/

//using System.Drawing; //RectangleF
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Movable
{
	public interface IMovable : IPlugin
	{
		//List<MovableArea> MovableAreas { get; set; }
		//MovableController MovableController { get; set; }
		
		void OnRegister(MovableController mover); //OnRegister(MovableController mover);
		void PaintArea(MovableController mover, MovableArea area, float deltaX, float deltaY);
	}
}