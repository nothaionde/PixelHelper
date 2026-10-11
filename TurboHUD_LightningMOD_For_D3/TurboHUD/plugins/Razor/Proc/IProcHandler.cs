/*

Standardize proc handling. 
PartyProcTracker will check if a plugin implements this interface and then call the functions named here.

*/

namespace Turbo.Plugins.Razor.Proc
{
	public interface IProcHandler : IPlugin
	{
		void OnProcStart(ProcInfo proc, IPlayer player); //uint sno, IPlayer player, double elapsed, double left);
		void OnProcFinish(ProcInfo proc, IPlayer player);
	}
}