namespace Turbo.Plugins.LightningMod
{
    using SharpDX.DirectInput;
    using Turbo.Plugins.Default;

    public class BaseInventoryManagementPlugin : BasePlugin
    {
        public bool TurnedOn { get; set; }
        public IKeyEvent ToggleKeyEvent { get; set; }
        public bool Running { get; protected set; }
        public IFont HeaderFont { get; set; }
        public IFont InfoFont { get; set; }
        public IBrush ItemHighlighBrush { get; set; }
        public bool InventoryLockForUpgradeToAncient { get; set; }
        public BaseInventoryManagementPlugin()
        {
            Enabled = true;
            TurnedOn = false;
            Running = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            ToggleKeyEvent = Hud.Input.CreateKeyEvent(true, Key.R, false, false, false);

            HeaderFont = Hud.Render.CreateFont("tahoma", 12, 255, 200, 200, 100, true, false, 255, 0, 0, 0, true);
            InfoFont = Hud.Render.CreateFont("tahoma", 7, 255, 200, 200, 0, true, false, 255, 0, 0, 0, true);
            ItemHighlighBrush = Hud.Render.CreateBrush(255, 200, 200, 100, -1.6f);
        }
    }
}