using System.Collections.Generic;

namespace AppSettings
{
	public static class Settings
	{
		public static List<string> GetSupportedPlatforms()
		{
			return new List<string>()
			{
				"bitfinex", "kucoin"
			};
		}

		public static string GetAppDataFolderName()
		{
			return "TradeScrape";
		}
	}
}