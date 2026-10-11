namespace Turbo.Plugins.Razor.Menu
{
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;

	public abstract class BaseMenuAddon : BasePlugin, IMenuAddon
	{
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

        public BaseMenuAddon() : base()
        {
            Enabled = true;
        }
		
		/*public override void Load(IController hud)
        {
            base.Load(hud);
		}*/
		
		public abstract void OnRegister(MenuPlugin plugin);
	}
}