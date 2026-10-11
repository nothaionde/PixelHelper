/*

a structure for MenuPlugin to govern save to file operations

*/

namespace Turbo.Plugins.Razor.Menu
{
	//using System;
	//using Turbo.Plugins.Default;

	public interface IMenuSaveHandler
	{
		bool SaveMode { get; set; }
		System.Func<bool> SaveCondition { get; set; }
		//string SavePath { get; set; }
		//string SaveFileName { get; set; }
		
		void SaveToFile();
	}
}