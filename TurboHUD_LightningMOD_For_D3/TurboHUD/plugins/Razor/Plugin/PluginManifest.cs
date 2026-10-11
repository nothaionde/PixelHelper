namespace Turbo.Plugins.Razor.Plugin
{
	//using System.Collections.Generic;
	//using Turbo.Plugins.Default;

    public class PluginManifest : IPluginManifest
    {
		public string Path { get; set; } //full namespace with classname
        public string Name { get; set; } //display name
        public string Description { get; set; }
		public string Version { get; set; } //optional
		public System.Collections.Generic.List<string> Dependencies { get; set; } //the IPlugins that must be enabled for this plugin to work
		
		//DateTime? LastUpdated { get; set; } //optional date to display with the version
		//string Changelog { get; set; } //optional path to changelog text file
    }
}