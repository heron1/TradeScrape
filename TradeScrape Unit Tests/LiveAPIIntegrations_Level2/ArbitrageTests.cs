using System.Collections.Generic;
using AdvancedAPI;
using AppSettings;
using AppSettingsFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TradeScrape_Unit_Tests.LiveAPIIntegrations_Level2
{
	[TestClass]
	public class ArbitrageTests
	{
		// IAdvanced advancedAPI = new AdvancedAPI.AdvancedAPI();
		public IAdvanced getAdvancedAPI(string platform)
		{
			IAppSettings appSettings = SupplierIAppSettings.GetDatabaseIAppSettings();
			(bool success, string apiKey, string secretKey, string passphrase) = appSettings.GetCredentials(platform);
			IAdvanced advancedAPI = new AdvancedAPI.AdvancedAPI(platform, apiKey, secretKey, passphrase);
			return advancedAPI;
		}

		[TestMethod]
		public void TestFindArbitrageOpportunities()
		{
			//Arrange
			IAdvanced api = getAdvancedAPI("bitfinex");
			var platformApis = (SupplierIOrderFunctions.GetIOrderFunctions("bitfinex"),
				SupplierIOrderFunctions.GetIOrderFunctions("kucoin"));
			
			// api.AttemptArbitrageSingle(("AGI", "BTC"), platformApis, 1.0M, 0, 1);

			//Act
			var output = api.FindArbitrageOpportunities(("bitfinex", "kucoin"), 0,
				0, 10000, int.MaxValue);
			
			//Assert
			//TODO: A good assertion
		}

		[TestMethod]
		public void FindMatchingPlatformSymbolPairsTest()
		{
			//Arrange
			IAdvanced api = getAdvancedAPI("bitfinex");

			//Act
			List<(string symb1, string symb2)> output = api.FindMatchingPlatformSymbolPairs(("bitfinex", "kucoin"));
			
			//Assert
			Assert.IsTrue(output.Count > 0);
		}
	}
}