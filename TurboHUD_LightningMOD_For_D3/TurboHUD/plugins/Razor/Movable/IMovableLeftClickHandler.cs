/*

IMovable plugins that want to receive left click events when they are hovered while not in Modify Mode

*/

//using System.Drawing; //RectangleF
//using System.Collections.Generic;
//using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Movable
{
	public interface IMovableLeftClickHandler
	{
		void OnLeftMouseDown(MovableArea area);
		void OnLeftMouseUp(MovableArea area);
	}
}