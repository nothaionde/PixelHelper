/*

Standardize hotkey event handling.
HotkeyEventHandler (which will check and propagate key events) will check if a plugin implements this interface and then call the function named here.

*/

namespace Turbo.Plugins.Razor.Hotkey
{
	public interface IHotkeyEventHandler : IPlugin
	{
		void OnHotkeyEvent(IKeyEvent keyEvent);
	}
}