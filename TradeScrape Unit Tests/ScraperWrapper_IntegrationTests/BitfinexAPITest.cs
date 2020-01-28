using System;
using AppSettings;
using AppSettingsFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

namespace TradeScrape_Unit_Tests.ScraperWrapper_IntegrationTests
{
	[TestClass]
	public class BitfinexAPITest : ScraperWrapperTests
	{
		public override string GetPlatform()
		{
			return "bitfinex";
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullApiKeyFailureTest()
		{
			new ScraperWrapper("bitfinex");
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullSecretKeyFailureTest()
		{
			new ScraperWrapper("bitfinex", "someValidApiKey");
		}
		
		[TestMethod]
		public void InstantiateScraperWrapper()
		{
			new ScraperWrapper("bitfinex", "someValidApiKey", "someValidSecretKey");
		}
	}
	
}