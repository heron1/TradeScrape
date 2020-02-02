using AppSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

namespace TradeScrape_Unit_Tests.LiveAPIIntegrations_Level2
{
	[TestClass]
	public class API_Intermediate_UnitTests
	{
		private string testPlatform = "kucoin";
		
		private IOrderFunctions getWebScraperApi(string platform)
		{
			var settings = SupplierIAppSettings.GetIAppSettings();
			var creds = settings.GetCredentials(testPlatform);
			return new ScraperWrapper(testPlatform, creds.apiKey, creds.secretKey, creds.passphrase);
		}
		
		[TestMethod]
		public void WebScraperTest()
		{
			//Arrange
			var api = getWebScraperApi(testPlatform);
			
			//Act
			var output = api.SymbolStats(new string[] {"AGI", "BTC"}).Result;
			
			//Assert
			Assert.IsTrue(output != null);
		}
	}
}