/*

IMovable plugins that want to receive right click events when they are hovered while not in Modify Mode

*/

//using System.Drawing; //RectangleF
//using System.Collections.Generic;
//using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Movable
{
	public interface IMovableRightClickHandler
	{
		void OnRightMouseDown(MovableArea area);
		void OnRightMouseUp(MovableArea area);
	}
}