namespace Turbo.Plugins.Razor.Util
{
	//using Turbo.Plugins.Default;
 
	public static class XpInfo
	{
		public static uint[] LevelXpTable { get; private set; } = new uint[70] 
		{ //xp table data from https://www.diablowiki.net/Experience_level_chart
			0, //1
			280, 
			2700,
			4500,
			6600,
			9000,
			11700,
			14000,
			16500,
			19200,
			22100,
			25200,
			28500,
			32000,
			35700,
			39600,
			43700,
			48000,
			52500,
			57200, //20
			62100,
			67200,
			72500,
			78000,
			83700,
			89600,
			95700,
			102000,
			108500,
			115200,
			122100,
			150000,
			157500,
			180000,
			203500,
			228000,
			273000,
			320000,
			369000,
			420000, //40
			473000,
			528000,
			585000,
			644000,
			705000,
			768000,
			833000,
			900000,
			1453500,
			2080000,
			3180000,
			4050000,
			5005000,
			6048000,
			7980000,
			10092000,
			12390000,
			14880000,
			17019000,
			20150000, //60
			14586000,
			27000000,
			29400000,
			31900000,
			39100000,
			46800000,
			55000000,
			63700000,
			72900000,
			82600000 //70
		};
		
		public static uint GetExpToNextLevel(this ISnoController Sno, IPlayer player)
		{
			//if (player.CurrentLevelNormal < player.CurrentLevelNormalCap)
				return player.GetAttributeValueAsUInt(Sno.Attributes.Experience_Next_Lo, uint.MaxValue, 0);
			//return 0;
		}
	}
}