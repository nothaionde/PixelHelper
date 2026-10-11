using System.Windows.Forms;

namespace Turbo.Plugins
{
    public interface IMouseClickBlocker : IPlugin
    {
        /// <summary>
        /// Called when a mouse click even happens while the game's window is active.
        /// The method is called on a separated thread - do not attempt to modify the main thread's state without adding extra locking.
        /// </summary>
        bool MouseClickShouldBeBlocked(MouseButtons button, int x, int y);
    }
}