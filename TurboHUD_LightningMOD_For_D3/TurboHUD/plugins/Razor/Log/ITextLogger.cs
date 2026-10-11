namespace Turbo.Plugins.Razor.Log
{
	//using Turbo.Plugins.Default;
	
	public interface ITextLogger
	{
		void Log(string path); //path = relative path of plugins directory without trailing backslash
	}
}