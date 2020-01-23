using AppSettings;
using AppSettingsFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

namespace TradeScrape_Unit_Tests.LiveAPIIntegrations_Level1
{
	[TestClass]
	public class KucoinAPITest
	{
		private IOrderFunctions getAPI()
		{
			IAppSettings appSettings = SupplierIAppSettings.GetDatabaseIAppSettings();
			(bool success, string apiKey, string secretKey, string passphrase) = appSettings.GetCredentials("kucoin");
			return new KucoinAPI(apiKey, secretKey, passphrase);
		}
		
		[TestMethod]
		public void CancelAllTest()
		{
			//Arrange
			IOrderFunctions api = getAPI();
			
			//Act
			Status status = api.CancelAll().Result;
			
			//Assert
			Assert.AreEqual(Status.Success, status);

		}
		
		[TestMethod]
		public void GetBuyOrderHistoryAllTest()
		{
			//Arrange
			IOrderFunctions api = getAPI();
			
			//Act
			var output = api.GetBuyOrderHistoryAll().Result;
			
			//Assert
			Assert.IsTrue(output.Count > 0);
		}
		
		[TestMethod]
		public void GetSellOrderHistoryAll()
		{
			//Arrange
			IOrderFunctions api = getAPI();
			
			//Act
			var output = api.GetSellOrderHistoryAll().Result;
			
			//Assert
			Assert.IsTrue(output.Count > 0);
		}
		
		[TestMethod]
		public void GetOwnedStocksTest()
		{
			//Arrange
			IOrderFunctions api = getAPI();
			
			//Act
			var output = api.GetOwnedStocks().Result;
			
			//Assert
			Assert.IsTrue(output.Count > 0);
		}
		
		[TestMethod]
		public void GetSymbols()
		{
			//Arrange
			IOrderFunctions api = getAPI();
			
			//Act
			var output = api.GetSymbols().Result;
			
			//Assert
			Assert.IsTrue(output.Length > 0);
		}
		
		[TestMethod]
		public void TestCredentials()
		{
			//Arrange
			IOrderFunctions api = getAPI();
			
			//Act
			var output = api.TestCredentials().Result;
			
			//Assert
			Assert.IsTrue(output is true);
		}
	}
	
}