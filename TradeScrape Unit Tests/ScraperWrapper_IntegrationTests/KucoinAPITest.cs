using System;
using AppSettings;
using AppSettingsFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

namespace TradeScrape_Unit_Tests.ScraperWrapper_IntegrationTests
{
	[TestClass]
	public class KucoinAPITest : ScraperWrapperTests
	{
		public override string GetPlatform()
		{
			return "kucoin";
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullApiKeyFailureTest()
		{
			new ScraperWrapper("kucoin");
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullSecretKeyFailureTest()
		{
			new ScraperWrapper("kucoin", "someValidApiKey");
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullPassphraseFailureTest()
		{
			new ScraperWrapper("kucoin", "someValidApiKey", "someSecretKey");
		}
		
		[TestMethod]
		public void InstantiateScraperWrapper()
		{
			new ScraperWrapper("kucoin", "someValidApiKey", "someValidSecretKey", "somePassphrase");
		}
	}
}