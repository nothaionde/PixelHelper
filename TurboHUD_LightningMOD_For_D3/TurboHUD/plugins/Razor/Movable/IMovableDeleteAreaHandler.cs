/*

IMovable plugins that want to receive OnDeleteArea events for their own areas

*/

//using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Movable
{
	public interface IMovableDeleteAreaHandler
	{
		void OnDeleteArea(MovableArea area);
	}
}