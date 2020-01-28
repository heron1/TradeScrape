using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

namespace TradeScrape_Unit_Tests.ScraperWrapper_IntegrationTests
{
	[TestClass]
	public class MockAPITest : ScraperWrapperTests
	{
		public override string GetPlatform()
		{
			return "mock";
		}
		
		[TestMethod]
		public void InstantiateScraperWrapper()
		{
			new ScraperWrapper("mock");
		}
	}
}