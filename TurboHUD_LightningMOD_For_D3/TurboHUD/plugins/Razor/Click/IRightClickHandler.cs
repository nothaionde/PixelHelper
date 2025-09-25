/*

Standardize left mouse click event handling. 
ClickEventHandler (which will check and propagate mouse click events) will check if a plugin implements this interface and then call the functions named here.

*/

namespace Turbo.Plugins.Razor.Click
{
	public interface IRightClickHandler : IPlugin
	{
		void OnRightMouseDown();		
		void OnRightMouseUp();
	}
}