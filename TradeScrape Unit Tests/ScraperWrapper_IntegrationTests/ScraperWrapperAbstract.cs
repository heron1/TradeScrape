using System;
using AppSettingsFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

namespace TradeScrape_Unit_Tests.ScraperWrapper_IntegrationTests
{
	[TestClass]
	public abstract class ScraperWrapperTests
	{
		public abstract string GetPlatform();
		
		private IOrderFunctions _api;
		public IOrderFunctions GetApi => _api;

		//Little difference whether a constructor or [TestInitialize] is used for preperation, although [TestInitialize]
		//allows other pre-condition attributes from MSTest to be used, so may as well get into the habbit.
		[TestInitialize]
		public void Initialize()
		{
			if (GetPlatform() == "mock")
			{
				_api = new ScraperWrapper(GetPlatform());
				return;
			}
			
			(bool success, string apiKey, string secretKey, string passphrase) = 
				SupplierIAppSettings.GetIAppSettingsStorage().GetCredentials(GetPlatform());
			
			_api = new ScraperWrapper(GetPlatform(), apiKey, secretKey, passphrase);
		}

		[TestMethod]
		public void CancelAllTest()
		{
			//Arrange
			IOrderFunctions api = GetApi;

			//Act
			Status status = api.CancelAll().Result;

			//Assert
			Assert.AreEqual(Status.Success, status);

		}

		[TestMethod]
		public void GetBuyOrderHistoryAllTest()
		{
			//Arrange
			IOrderFunctions api = GetApi;

			//Act
			var output = api.GetBuyOrderHistoryAll().Result;

			//Assert
			Assert.IsTrue(output.Count >= 0);
		}

		[TestMethod]
		public void GetSellOrderHistoryAll()
		{
			//Arrange
			IOrderFunctions api = GetApi;

			//Act
			var output = api.GetSellOrderHistoryAll().Result;

			//Assert
			Assert.IsTrue(output.Count >= 0);
		}

		[TestMethod]
		public void GetOwnedStocksTest()
		{
			//Arrange
			IOrderFunctions api = GetApi;

			//Act
			var output = api.GetOwnedStocks().Result;

			//Assert
			Assert.IsTrue(output.Count > 0);
		}

		[TestMethod]
		public void GetSymbols()
		{
			//Arrange
			IOrderFunctions api = GetApi;

			//Act
			var output = api.GetSymbols().Result;

			//Assert
			Assert.IsTrue(output.Length > 0);
		}

		[TestMethod]
		public void TestCredentials()
		{
			//Arrange
			IOrderFunctions api = GetApi;

			//Act
			var output = api.TestCredentials().Result;

			//Assert
			Assert.IsTrue(output is true);
		}
	}
}