/*

IMovable plugins that want to receive IKeyEvents when they are hovered or picked up in Modify Mode

*/

//using System.Drawing; //RectangleF
using System.Collections.Generic;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Movable
{
	public interface IMovableKeyEventHandler
	{
		//CursorPluginArea == selected : selected = true 
		//HoveredPluginArea == selected : selected = false
		void OnKeyEvent(MovableController mover, IKeyEvent keyEvent, MovableArea area); //, CursorPluginArea == selected
	}
}