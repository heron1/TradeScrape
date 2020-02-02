using System;
using System.Collections.Generic;
using System.Diagnostics;
using AdvancedAPI;
using AppSettings;
using Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scraper;

//TODO: Not yet finished
namespace TradeScrape_Unit_Tests.LiveAPIIntegrations_Level2
{
	[TestClass]
	public class ArbitrageTests
	{
		// IAdvanced advancedAPI = new AdvancedAPI.AdvancedAPI();
		public IAdvanced getAdvancedAPI(string platform)
		{
			IAppSettings appSettings = SupplierIAppSettings.GetIAppSettings();
			(bool success, string apiKey, string secretKey, string passphrase) = appSettings.GetCredentials(platform);
			IAdvanced advancedAPI = new AdvancedAPI.AdvancedAPI(platform, apiKey, secretKey, passphrase);
			return advancedAPI;
		}

		[TestMethod]
		public void TempTests()
		{
			
		}

		[TestMethod]
		public void TestFindArbitrageOpportunities()
		{
			//Arrange
			IAdvanced api = getAdvancedAPI("bitfinex");
			var platformApis = (SupplierIOrderFunctions.GetIOrderFunctions("bitfinex"),
				SupplierIOrderFunctions.GetIOrderFunctions("kucoin"));
			
			// api.AttemptArbitrageSingle(("AGI", "BTC"), GetPlatformApis, 1.0M, 0, 1);

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

		/* TODO: Direct arbitrage given a specific symbolPair, with a monitor that's left open and a time check interval
		such as once per second. This could be used to test given symbol pairs whereby a trade balance exists. 
		Have also an "auto exiter" after an "n" number of arbitrage executions, so that it can be used
		for testing purposes without going out of control and performing an uncapped number of trades. 
		Log or print the outcome after each arbitrage. Give the "auto exiter" an option for "infinite"
		whereby it's stopped only by manually returning a keyboard key. Do this before refactoring the IOrderFunctions Buy/Sell order
		implementations just to see if the entire concept even works. If it does (even if trading fees lead to a negative gain) then it
		nevertheless demonstrates a good proof of concept to show that serious refactoring implementations should be considered. */
		[TestMethod]
		public void ArbitrageSymbolPairContinuouslyTest()
		{
			//this method will currently incur technical debt as it writes directly to the cmd prompt, bypassing encapsulation. Restrict
			//the technical debt to only this function, so that it alone can be refactored and it doesn't influence anything else in the
			//program
			
			//Arrange
			IAdvanced api = getAdvancedAPI("bitfinex");
			
			//Act
			api.ArbitrageSymbolPairContinuously(("ETH", "BTC"));
		}
		
		
		//Search symbolpairs until the first arbitrage is found, then immediately execute it with minimal code required.
		[TestMethod]
		public void ArbitrageThenBreakTest()
		{
			//Arrange
			IAdvanced api = getAdvancedAPI("bitfinex");
			
			//Act
			ExecutedArbitrageResults executedArbitrageResults = api.FindAndExecuteFirstValidSymbolPairForArbitrage();
			
			//Assert
			
		}

		[TestMethod]
		public void Confirm_That_Platform1_Ask_Plus_Threshold_Is_Less_Than_Platform_2_Bid_From_SymbolPair_List()
		{
			//Arrange
			IAdvanced api = getAdvancedAPI("bitfinex");
			
			(string platform1, string platform2) platforms = ("bitfinex", "kucoin");
			Decimal priceDifferenceThreshold = 0;
			
			(IOrderFunctions platform1, IOrderFunctions platform2) platformApis =
				(SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform1),
					SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform2));

			int trueCounter = 0;
			
			//Act
			//First get list of potential arbitrage symbolpairs
			var foundOpportunitiesList = new 
					List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)>();
			
			var stopwatch = Stopwatch.StartNew();
			
			foundOpportunitiesList = api.FindArbitrageOpportunities(platforms, priceDifferenceThreshold, 0, int.MaxValue, int.MaxValue);
			
			var ms = stopwatch.ElapsedMilliseconds;
			
			//Assert
			//Then confirm at least one arbitrage condition was found, otherwise this test cannot be completed
			Assert.IsTrue(foundOpportunitiesList.Count > 0);
			
			//Now confirm the arbitrage conditions are met
			foreach (var listElement in foundOpportunitiesList)
			{
				var symbolPair = listElement.symbPair;

				//time the time this takes, view it in debugger
				stopwatch = Stopwatch.StartNew();
				
				(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid,
					Decimal platform2BidSize,
					Decimal actualArbitrageAmount) = api.CheckArbitrageOpportunity(symbolPair,
					platformApis, priceDifferenceThreshold, 0, int.MaxValue);
				
				stopwatch.Stop();
				ms = stopwatch.ElapsedMilliseconds;
				
				//Assert conditions are met for each symbolPair
				if (status == StatusAdv.Success)
				{
					//This test fails because the methods execute too slow (they take 1 minute). KuCoin seems to be the bottleneck.
					//Find out why the async operation from KuCoin suddenly becomes very slow after too many queries. Could it be an external
					//rate limit, or something wrong internally with this code? Regardless of what the answer is, surely it could be optimized
					//to perform with far fewer or smarter API calls to make the process fast enough that the arbitrage opportunity doesn't
					//disappear by the time it's re-checked
					Assert.IsTrue(platform1Ask + priceDifferenceThreshold - platform2Bid < 0);
					trueCounter++;
				}
				
				//Assert that at least one arbitrage condition was found
				Assert.IsTrue(trueCounter > 0);
			}
		}
	}
}