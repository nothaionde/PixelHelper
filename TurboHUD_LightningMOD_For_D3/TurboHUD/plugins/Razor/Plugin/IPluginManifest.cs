namespace Turbo.Plugins.Razor.Plugin
{
	//using System.Collections.Generic;
	//using Turbo.Plugins.Default;

    public interface IPluginManifest
    {
        string Name { get; set; }
        string Description { get; set; }
		string Version { get; set; }
		System.Collections.Generic.List<string> Dependencies { get; set; } //the IPlugins that must be enabled for this plugin to work
		
		//DateTime? LastUpdated { get; set; } //optional date to display with the version
		//string Changelog { get; set; } //optional path to changelog text file
    }
}