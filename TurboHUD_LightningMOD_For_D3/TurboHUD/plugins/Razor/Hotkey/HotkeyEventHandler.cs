/*

This is an event handler that consolidates hotkey validity checks (if tabbed in, when in a game only, if the chat prompt is not active) across all plugins that implement the IHotkeyEventHandler. This way, the validity checks are done once, not once per plugin.

Changelog
- April 7, 2021 - Initial release

*/

using System.Linq;
//using System.Windows.Forms; //Keys

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Hotkey
{
    public class HotkeyEventHandler : BasePlugin, IKeyEventHandler
    {
		public IUiElement ChatPrompt { get; private set; }
		private IPlugin[] HotkeyHandlers;
		
        public HotkeyEventHandler() : base()
        {
            Enabled = true;
			Order = -10001; //run this before some other stuff
        }
		
        public override void Load(IController hud)
        {
            base.Load(hud);
			
			ChatPrompt = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
        }
		
		public void OnKeyEvent(IKeyEvent keyEvent)
		{
			//only process the key event if hud is actively displayed
			//this check is now baked into hud but sometimes I get weird behavior when alt-tabbed anyway
			if (!Hud.Window.IsForeground || !Hud.Game.IsInGame)
				return;
			
			//check if chat window is open, if so, ignore key input
			if (ChatPrompt == null)
			{
				ChatPrompt = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
				if (ChatPrompt == null || ChatPrompt.Visible) //if (ChatPrompt is object && ChatPrompt.Visible)
					return;
			}
			else if (ChatPrompt.Visible)
				return;
			
			if (HotkeyHandlers == null)
			{
				//find all plugins that want to be notified
				HotkeyHandlers = Hud.AllPlugins.Where(p => p is IHotkeyEventHandler).ToArray();
			}
			
			if (HotkeyHandlers.Length > 0)
			{
				foreach (IPlugin plugin in HotkeyHandlers)
					((IHotkeyEventHandler)plugin).OnHotkeyEvent(keyEvent);
			}
		}
    }
}