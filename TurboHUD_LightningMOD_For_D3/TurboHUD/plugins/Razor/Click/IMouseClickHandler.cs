using System.Windows.Forms;

namespace Turbo.Plugins
{
    public interface IMouseClickHandler : IPlugin
    {
        /// <summary>
        /// Called on the main thread, right BEFORE IAfterCollectHandler.AfterCollect()
        /// </summary>
        bool MouseDown(MouseButtons button);

        /// <summary>
        /// Called on the main thread, right after MouseDown() but BEFORE IAfterCollectHandler.AfterCollect()
        /// </summary>
        bool MouseUp(MouseButtons button);
    }
}