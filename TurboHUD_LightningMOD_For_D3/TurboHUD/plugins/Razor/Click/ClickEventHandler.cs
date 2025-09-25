/*

This is an event handler to consolidate left and right mouse click input checks across all plugins that implement the ILeftClickHandler and/or IRightClickHandler interface.

Thanks to Dysfunctional for the tip about how to detect mouse clicks!

Changelog
- July 28, 2020 - optimization: cached the mouse event listeners so that this plugin doesn't have to iterate through the entire list of hud plugins for every click notification
- July 25, 2020 - rewrote code in a more self correcting way
- July 14, 2020 - rewrote the code logic to fix an issue with click detection desync when alt-tabbing
- July 3, 2020 - Initial release

*/

using System.Linq;
using System.Windows.Forms; //Keys

using Turbo.Plugins.Default;

namespace Turbo.Plugins.Razor.Click
{
    public class ClickEventHandler : BasePlugin, IAfterCollectHandler //IInGameTopPainter
    {
		public bool LButtonPressed { get; private set; }
		public bool RButtonPressed { get; private set; }
		
		private IPlugin[] LeftMouseHandlers;
		private IPlugin[] RightMouseHandlers;
		
		private bool IsForeground;
		
        public ClickEventHandler() : base()
        {
            Enabled = true;
			Order = -10001; //run this before some other stuff
        }
		
        public override void Load(IController hud)
        {
            base.Load(hud);
			IsForeground = Hud.Window.IsForeground;
			Init();
        }
		
		public void AfterCollect()
		{
			if (LeftMouseHandlers == null)
			{
				//find all plugins that want to be notified
				LeftMouseHandlers = Hud.AllPlugins.Where(p => p is ILeftClickHandler).ToArray();
			}
			if (RightMouseHandlers == null)
			{
				//find all plugins that want to be notified
				RightMouseHandlers = Hud.AllPlugins.Where(p => p is IRightClickHandler).ToArray();
			}

			if (Hud.Window.IsForeground)
			{
				if (!IsForeground) //tabbed in
				{
					Init(); //reinitialize the button states so that a pressed state when tabbing in doesn't trigger a button down event
					IsForeground = true;
				}

				if (!Hud.Game.IsInGame)
				{
					if (LButtonPressed)
					{
						LButtonPressed = false;

						foreach (IPlugin plugin in LeftMouseHandlers)
						{
							if (plugin.Enabled)
								((ILeftClickHandler)plugin).OnLeftMouseUp();
						}
					}
					if (RButtonPressed)
					{
						RButtonPressed = false;
						
						foreach (IPlugin plugin in RightMouseHandlers)
						{
							if (plugin.Enabled)
								((IRightClickHandler)plugin).OnRightMouseUp();
						}
					}
					
					return;
				}
				
				bool state = Hud.Input.IsKeyDown(Keys.LButton);
				if (state) //down
				{
					if (!LButtonPressed)
					{
						foreach (IPlugin plugin in LeftMouseHandlers)
						{
							if (plugin.Enabled)
								((ILeftClickHandler)plugin).OnLeftMouseDown();
						}
					}
				}
				else if (LButtonPressed) //up
				{
					foreach (IPlugin plugin in LeftMouseHandlers)
					{
						if (plugin.Enabled)
							((ILeftClickHandler)plugin).OnLeftMouseUp();
					}
				}
				LButtonPressed = state;
				
				state = Hud.Input.IsKeyDown(Keys.RButton);
				if (state) //down
				{
					if (!RButtonPressed)
					{
						foreach (IPlugin plugin in RightMouseHandlers)
						{
							if (plugin.Enabled)
								((IRightClickHandler)plugin).OnRightMouseDown();
						}
					}
				}
				else if (RButtonPressed) //up
				{
					foreach (IPlugin plugin in RightMouseHandlers)
					{
						if (plugin.Enabled)
							((IRightClickHandler)plugin).OnRightMouseUp();
					}
				}
				RButtonPressed = state;
			}
			else
			{
				if (IsForeground) //tabbed out
				{
					IsForeground = false;
					
					if (LButtonPressed)
					{
						LButtonPressed = false;

						foreach (IPlugin plugin in LeftMouseHandlers)
						{
							if (plugin.Enabled)
								((ILeftClickHandler)plugin).OnLeftMouseUp();
						}
					}
					if (RButtonPressed)
					{
						RButtonPressed = false;
						
						foreach (IPlugin plugin in RightMouseHandlers)
						{
							if (plugin.Enabled)
								((IRightClickHandler)plugin).OnRightMouseUp();
						}
					}
				}
			}
		}
		
		public void Init()
		{
			LButtonPressed = Hud.Input.IsKeyDown(Keys.LButton);
			RButtonPressed = Hud.Input.IsKeyDown(Keys.RButton);
		}
    }
}