using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppSettings;
using Helpers;
using Newtonsoft.Json;
using Scraper;

namespace AdvancedAPI
{
	public class AdvancedAPI : IAdvanced
	{
		private readonly IOrderFunctions scraperInstance;
		private readonly IAppSettings settings = SupplierIAppSettings.GetIAppSettings();

		public AdvancedAPI(string platform, string apiKey, string secretKey, string passphrase = null)
		{
			scraperInstance = SupplierIOrderFunctions.GetIOrderFunctions(platform);
		}
		
		public static (IOrderFunctions platform1, IOrderFunctions platform2) GetPlatformApis(
			(string platform1, string platform2) platforms)
		{
			(IOrderFunctions platform1, IOrderFunctions platform2) platformApis =
				(SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform1),
					SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform2));
			return platformApis;
		}
		
		//Level 2 Arbitrage API below
		
		//TODO: Expand this to arbtirary number of platforms later. For now simply check against 2 given APIs 
		// public void FindMatchingPlatformSymbolPairs(params string[] platforms)
		// {
		// 	Debug.Assert(platforms.Length > 1);
		// 	
		// 	var scraperApiConnectors = new List<IOrderFunctions>();
		// 	foreach (var platform in platforms)
		// 	{
		// 		(bool success, string apiKey, string secretKey, string passphrase) = settings.GetCredentials(platform);
		// 		IOrderFunctions api = new ScraperWrapper(platform, apiKey, secretKey, passphrase);
		// 		scraperApiConnectors.Add(api);
		// 	}
		//
		// 	var symbolPairs = new List<(string symb1, string symb2)>();
		// 	var symbolPairsMatched = new List<(string symb1, string symb2)>();
		// 	
		// 	//fill symbolPairs with first platforms symbol pairs
		// 	(string, string)[] output = scraperApiConnectors[0].GetSymbols().Result;
		// 	foreach (var symbPair in output)
		// 		symbolPairs.Add(symbPair);
		//
		// 	//now check remaining platforms against this, add matches to symbolPairsMatched
		// 	for (int i = 1; i < platforms.Length; i++)
		// 	{
		// 		output = scraperApiConnectors[i].GetSymbols().Result;
		// 		foreach (var symbPair in output)
		// 		{
		// 			if (symbolPairs.Contains(symbPair))
		// 				symbolPairsMatched.Add(symbPair);
		// 		}
		// 	}
		// 	
		// }

		public ExecutedArbitrageResults ExecuteCrossPlatformTrade((string symbol1, string symbol2) symbolPair,
			(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal platform1BuyPrice, decimal platform2SellPrice,
			decimal quantityToBuyOnPlatform1)
		{
			ExecutedArbitrageResults executedArbitrageResults = new ExecutedArbitrageResults();

			var statusBuyOrderTask = Task.Run(() =>
				platforms.platform1Api.BuyOrder(new string[] {symbolPair.symbol1, symbolPair.symbol2}, quantityToBuyOnPlatform1,
					platform1BuyPrice));

			var statusSellOrderTask = Task.Run(() =>
				platforms.platform2Api.SellOrder(new string[] {symbolPair.symbol1, symbolPair.symbol2},
					quantityToBuyOnPlatform1, platform2SellPrice));

			var statusBuyOrder = statusBuyOrderTask.Result;
			var statusSellOrder = statusSellOrderTask.Result;

			//BUG: The implementation of all current IOrderFunction APIs returns Success regardless of outcome. Ensure that response code
			//correlates to what happened. Also make separate buy orders that either execute on the exact quantity & price or fail. 
			//Ensure that the returned object also includes the response string for debugging purposes. In fact a DEBUG symbol could
			//even be used to return one of two preconditions, one leading to a type WITH the responseStr, one without
			if (statusBuyOrder == Status.Success && statusSellOrder == Status.Success)
			{
				executedArbitrageResults.Success = true;
			}
			else
			{
				executedArbitrageResults.Success = false;
			}
			
			executedArbitrageResults.QuantityBoughtPlatform1 = quantityToBuyOnPlatform1;
			executedArbitrageResults.PricePaidPlatform1 = platform1BuyPrice;
			executedArbitrageResults.QuantitySoldPlatform2 = quantityToBuyOnPlatform1;
			executedArbitrageResults.PriceSoldPlatform2 = platform2SellPrice;

			executedArbitrageResults.Platform1Ask = platform1BuyPrice;
			executedArbitrageResults.Platform1AskSize = default;
			executedArbitrageResults.Platform2Bid = platform2SellPrice;
			executedArbitrageResults.Platform2BidSize = default;

			executedArbitrageResults.PriceDifferenceThreshold = default;
			executedArbitrageResults.IntendedArbitrageAmount = quantityToBuyOnPlatform1;

			executedArbitrageResults.ActualExecutionTimeForSymbolPair = default;
			
			return executedArbitrageResults;
		}
		
		public void ArbitrageSymbolPairContinuously((string symbol1, string symbol2) symbolPair)
		{
			Custom.SwitchToAppDataFolder(UserSettings.GetAppDataFolderName());
			string logFileName = "arbitrageTradeLog.txt";
			
			//Refactor these fields into signature
			(string platform1, string platform2) platforms = ("bitfinex", "kucoin");
			decimal priceDifferenceThreshold = 0;
			decimal minSpend = 0;
			decimal maxSpend = int.MaxValue;

			int arbitragesBeforeQuit = 2; //if this is set to -1, make it uncapped until exit
			//End Refactor these fields into signature

			int arbitragesBeforeQuitCounter = 0;
			
			ExecutedArbitrageResults executedArbitrageResults = new ExecutedArbitrageResults();

			var platformApis = GetPlatformApis(platforms);

			bool exitKeyPressed = false;
			Task.Run(() =>
			{
				Console.Read();
				exitKeyPressed = true;
			});
			
			
			//temp
			int secCounter = 0;
			
			while (!exitKeyPressed)
			{
				Thread.Sleep(100);
				secCounter++;
				if (secCounter >= 10)
				{
					secCounter = 0;
					Console.WriteLine("Still checking..");
				}

				var stopwatch = Stopwatch.StartNew();
				(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid,
					Decimal platform2BidSize,
					Decimal actualArbitrageAmount) = 
					CheckArbitrageOpportunity(symbolPair, platformApis, priceDifferenceThreshold, minSpend, maxSpend);
				
				stopwatch.Stop();
				long checkArbitrateOpportunity_TimeElapsed = stopwatch.ElapsedMilliseconds;

				if (status == StatusAdv.Success)
				{
					executedArbitrageResults = ExecuteCrossPlatformTrade(symbolPair, platformApis, platform1Ask,
						platform2Bid, actualArbitrageAmount);
					
					//log the trade
					using (StreamWriter logFile = File.AppendText(logFileName))
					{
						logFile.WriteLine("====");
						var serializedObj = JsonConvert.SerializeObject(executedArbitrageResults);
						logFile.WriteLine(serializedObj);

						Console.WriteLine($"Trade executed at ({DateTime.Now}) for {executedArbitrageResults.SymbolPair.ToString()}.\n" +
						                  $"Platform1 Buy Price: {executedArbitrageResults.Platform1Ask}\n" +
						                  $"Platform2 Sell Price: {executedArbitrageResults.Platform2Bid}\n" +
						                  $"Intended Quantity Bought/Sold: {executedArbitrageResults.QuantityBoughtPlatform1}");
					}

					arbitragesBeforeQuitCounter++;
					if (arbitragesBeforeQuitCounter >= arbitragesBeforeQuit)
					{
						Console.WriteLine("Trade threshold reached: " + arbitragesBeforeQuitCounter);
						return;
					}
				}
				else
				{
					executedArbitrageResults.Success = false;
				}
			}

		}

		//Level 1 API Below

			public ExecutedArbitrageResults FindAndExecuteFirstValidSymbolPairForArbitrage()
			{
				ExecutedArbitrageResults executedArbitrageResults;

				//Refactor these fields into signature
				(string platform1, string platform2) platforms = ("bitfinex", "kucoin");
				decimal priceDifferenceThreshold = 0;
				decimal minSpend = 0;
				decimal maxSpend = int.MaxValue;
				//End Refactor these fields into signature

				var symbolPairs = FindMatchingPlatformSymbolPairs((platforms.platform1, platforms.platform2));

				var platformApis = GetPlatformApis(platforms);

				foreach (var symbPair in symbolPairs)
				{

					var stopwatch = Stopwatch.StartNew();

					(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid,
							Decimal platform2BidSize,
							Decimal actualArbitrageAmount) =
						CheckArbitrageOpportunity(symbPair, platformApis, priceDifferenceThreshold, minSpend, maxSpend);

					stopwatch.Stop();
					long checkArbitrateOpportunity_TimeElapsed = stopwatch.ElapsedMilliseconds;

					if (status != StatusAdv.Success)
						continue;

					Decimal priceDifference = platform2Bid - (platform1Ask + priceDifferenceThreshold);

					if (priceDifference > 0)
					{
						var stopwatchExecuteArbitrage = Stopwatch.StartNew();
						//potential spend should be actualArbitrageAmount * priceDifference

						//Execute in parallel
						var statusBuyOrderTask = Task.Run(() => platformApis.platform1.BuyOrder(new string[]
							{symbPair.symb1, symbPair.symb2}, actualArbitrageAmount, platform1Ask));

						var statusSellOrderTask = Task.Run(() => platformApis.platform2.SellOrder(new string[]
							{symbPair.symb1, symbPair.symb2}, actualArbitrageAmount, platform2Bid));

						var statusBuyOrder = statusBuyOrderTask.Result;
						var statusSellOrder = statusSellOrderTask.Result;

						stopwatchExecuteArbitrage.Stop();

						executedArbitrageResults = new ExecutedArbitrageResults();

						//BUG: The implementation of all current IOrderFunction APIs returns Success regardless of outcome. Ensure that response code
						//correlates to what happened. Also make separate buy orders that either execute on the exact quantity & price or fail. 
						//Ensure that the returned object also includes the response string for debugging purposes. In fact a DEBUG symbol could
						//even be used to return one of two preconditions, one leading to a type WITH the responseStr, one without
						if (statusBuyOrder == Status.Success && statusSellOrder == Status.Success)
						{
							executedArbitrageResults.Success = true;
						}
						else
						{
							executedArbitrageResults.Success = false;
						}

						executedArbitrageResults.QuantityBoughtPlatform1 = actualArbitrageAmount;
						executedArbitrageResults.PricePaidPlatform1 = platform1Ask;
						executedArbitrageResults.QuantitySoldPlatform2 = actualArbitrageAmount;
						executedArbitrageResults.PriceSoldPlatform2 = platform2Bid;

						executedArbitrageResults.Platform1Ask = platform1Ask;
						executedArbitrageResults.Platform1AskSize = platform1AskSize;
						executedArbitrageResults.Platform2Bid = platform2Bid;
						executedArbitrageResults.Platform2BidSize = platform2BidSize;

						executedArbitrageResults.PriceDifferenceThreshold = priceDifferenceThreshold;
						executedArbitrageResults.IntendedArbitrageAmount = actualArbitrageAmount;

						executedArbitrageResults.ActualExecutionTimeForSymbolPair = stopwatchExecuteArbitrage.ElapsedMilliseconds;

						return executedArbitrageResults;
						//This method should be coded in such a manner so that
						// a loop that executes all found arbitrage opportunities could simply remove this return
						//In practicality, such a loop would never be used since its unlikely that ownership of all available arbitrage
						//stocks would exist, as there could be hundreds..
						//but then again it might be wise to do this via a stockpile (incl. a max amount that re-buys itself). Assess later.
						//The most important thing is to flesh out the core functionality with integration tests to ensure it's 100% functional
						//before even thinking of an abstraction at this level
					}

				}

				executedArbitrageResults = new ExecutedArbitrageResults();
				executedArbitrageResults.Success = false;
				return executedArbitrageResults;
			}
		

		public List<(string symb1, string symb2)> FindMatchingPlatformSymbolPairs((string platform1, string platform2) platformsTuple)
		{
			List<string> platforms = new List<string>(2) {platformsTuple.platform1, platformsTuple.platform2};
			
			var scraperApiConnectors = new List<IOrderFunctions>();
			foreach (var platform in platforms)
			{
				(bool success, string apiKey, string secretKey, string passphrase) = settings.GetCredentials(platform);
				IOrderFunctions api = SupplierIOrderFunctions.GetIOrderFunctions(platform);
				scraperApiConnectors.Add(api);
			}
		
			var symbolPairs = new List<(string symb1, string symb2)>();
			var symbolPairsMatched = new List<(string symb1, string symb2)>();
			
			//fill symbolPairs with first platforms symbol pairs
			(string, string)[] output = scraperApiConnectors[0].GetSymbols().Result;
			foreach (var symbPair in output)
				symbolPairs.Add(symbPair);
		
			//now check remaining platforms against this, add matches to symbolPairsMatched
			for (int i = 1; i < platforms.Count; i++)
			{
				output = scraperApiConnectors[i].GetSymbols().Result;
				foreach (var symbPair in output)
				{
					if (symbolPairs.Contains(symbPair))
						symbolPairsMatched.Add(symbPair);
				}
			}

			return symbolPairsMatched;
		}

		
		//TODO: Refactor all Level 2 api methods so that the input and outputs can be a struct Type as opposed to many variables.
		//If this works nicely, make it a future to-do to do the same thing for level 1
		//Also see if this functionality can be encapsulated and not dependent upon other requirements. Eg: The IOrderFunctions supplier
		//could call a method within this class that supplies an IOrderFunctions, then the implementations of this method can be changed
		public List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)> 
			FindArbitrageOpportunities((string platform1, string platform2) platforms, decimal priceDifferenceThreshold, 
				decimal minSpend, decimal maxSpend, int callLimit = 100)
		{
			List<(string symb1, string symb2)> symbolPairs;

			symbolPairs = FindMatchingPlatformSymbolPairs((platforms.platform1, platforms.platform2));

			var ArbitrageOpportunitiesList =
				new List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)>();
			
			var platformApis = GetPlatformApis(platforms);

			var stopwatch = Stopwatch.StartNew();
			long timerAdder = 0;
			
			int counter = 0;
			foreach (var symbPair in symbolPairs)
			{
				if (counter++ > callLimit)
					break;

				(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid,
						Decimal platform2BidSize,
						Decimal actualArbitrageAmount) =
					CheckArbitrageOpportunity(symbPair, platformApis, priceDifferenceThreshold, minSpend, maxSpend);

				if (status != StatusAdv.Success)
					continue;

				Decimal priceDifference = platform2Bid - (platform1Ask + priceDifferenceThreshold);
				
				if (priceDifference > 0)
				{
					//potential spend should be actualArbitrageAmount * priceDifference
					ArbitrageOpportunitiesList.Add((platforms.platform1, platforms.platform2, symbPair, actualArbitrageAmount,
						priceDifference));
				}

			}

			
			return ArbitrageOpportunitiesList;
		}

		

		//depending upon the requirements of the unique program. This way this class can be used by itself, so long as


		//the IOrderFunctions interface is implemented in new code somewhere else


		//Like the above but will accept a list of symbol pairs to monitor.
		public List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)> 
			FindArbitrageOpportunitiesBySmbolList(List<(string symb1, string symb2)> symbolList, 
				(string platform1, string platform2) platforms, decimal priceDifferenceThreshold, 
				decimal minSpend, decimal maxSpend, int callLimit = 100)
		{
			var ArbitrageOpportunitiesList =
				new List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)>();
			
			(IOrderFunctions platform1, IOrderFunctions platform2) platformApis =
				(SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform1),
					SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform2));

			int counter = 0;
			foreach (var symbPair in symbolList)
			{
				if (counter++ > callLimit)
					break;

				(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid,
						Decimal platform2BidSize,
						Decimal actualArbitrageAmount) =
					CheckArbitrageOpportunity(symbPair, platformApis, priceDifferenceThreshold, minSpend, maxSpend);

				Decimal priceDifference = platform2Bid - (platform1Ask + priceDifferenceThreshold);
				
				if (priceDifference > 0)
				{
					//potential spend should be actualArbitrageAmount * priceDifference
					ArbitrageOpportunitiesList.Add((platforms.platform1, platforms.platform2, symbPair, actualArbitrageAmount,
						priceDifference));
				}

			}
			return ArbitrageOpportunitiesList;
		}

		public (StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid, Decimal platform2BidSize, 
			Decimal actualArbitrageAmount)
			CheckArbitrageOpportunity((string symbol1, string symbol2) symbolPair,
			(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal priceDifferenceThreshold, 
			decimal minSpend, decimal maxSpend)
		{
			var sw = Stopwatch.StartNew();
			
			if (!(maxSpend > 0M)) throw new ArgumentException("maxSpend !> 0M");
			if (!(minSpend <= maxSpend)) throw new ArgumentException("minSpend !<= maxSpend");
			
			Debug.Assert(maxSpend > 0M);
			Debug.Assert(minSpend <= maxSpend);

			string[] symbolPairArr = new string[2] {symbolPair.symbol1, symbolPair.symbol2};

			string[] platform1SymbolStats;
			string[] platform2SymbolStats;
			//symbolstats in the form of symbolPair, current bid, current bid size, ask, ask size, last price, volume, high, low
			try
			{
				platform1SymbolStats = platforms.platform1Api.SymbolStats(symbolPairArr).Result;
				platform2SymbolStats = platforms.platform2Api.SymbolStats(symbolPairArr).Result;
			}
			catch (Exception e)
			{
				return (StatusAdv.Failure_Undefined, 0, 0, 0, 0, 0);
			}
			
			Decimal platform1Ask = Convert.ToDecimal(platform1SymbolStats[3]);
			Decimal platform2Bid = Convert.ToDecimal(platform2SymbolStats[1]);

			//calculate max safe amount to arbitrage, if lower than minSpend, exit function
			#region safeArbitrageAmount
			
			Decimal platform1AskSize = Convert.ToDecimal(platform1SymbolStats[4]);
			Decimal platform2BidSize = Convert.ToDecimal(platform2SymbolStats[2]);

			//Safe arbitrage amount is the lowest of the BidSize, AskSize
			Decimal safeArbitrageAmount = Math.Min(platform1AskSize, platform2BidSize);

			Decimal buySpendTotal = safeArbitrageAmount * platform1Ask;
			if (buySpendTotal < minSpend)
				return (StatusAdv.Failure_SafeLowerThanMinAmount, 0, 0, 0, 0, 0);

			#endregion

			//calculate actual amount to arbitrage, considering both the safeArbitrageAmount and the maxSpend
			#region actualArbitrageAmount
			
			Decimal actualArbitrageAmount;
			if (safeArbitrageAmount * platform1Ask <= maxSpend)
				actualArbitrageAmount = safeArbitrageAmount;
			else
			{
				actualArbitrageAmount = maxSpend / platform1Ask;
			}
				
			#endregion

			sw.Stop();
			var tim = sw.ElapsedMilliseconds;
			
			if (platform1Ask + priceDifferenceThreshold < platform2Bid)
				return (StatusAdv.Success, platform1Ask, platform1AskSize, platform2Bid, platform2BidSize, actualArbitrageAmount);
			else
				return (StatusAdv.Failure_InsufficientArbitrageConditions, platform1Ask, platform1AskSize, platform2Bid, platform2BidSize, actualArbitrageAmount);
		}
		
		public (StatusAdv status, Decimal amountExecuted, string msg) AttemptArbitrageSingle((string symbol1, string symbol2) symbolPair,
			(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal priceDifferenceThreshold, 
			decimal minSpend, decimal maxSpend)
		{
			(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid, Decimal platform2BidSize, 
				Decimal actualArbitrageAmount) = CheckArbitrageOpportunity(symbolPair, platforms, priceDifferenceThreshold, minSpend, maxSpend);
			string[] symbolPairArr = new string[2] {symbolPair.symbol1, symbolPair.symbol2};
			
			if (platform1Ask + priceDifferenceThreshold < platform2Bid)
			{
				//TODO KNOWN BUG: This is a bit dangerous as it's assuming the full amount is executed. When re-doing the programs, ensure that the
				//actual amount immediately executed is retrieved directly from the platform API. Also make a rigorous 'failing buy order' option
				//whereby the buy order fails unless the exact amount is executed, or ensures no outstanding buy order exists if lower than this amount
				
				//buy on platform 1, sell on platform 2, but keep within confines of maxSpend
				//TODO: Platform 2 sell order
				Status result = platforms.platform1Api.BuyOrder(symbolPairArr, actualArbitrageAmount, platform1Ask).Result;
				if (result == Status.Success)
				{
					return (StatusAdv.Success, actualArbitrageAmount, "");
				}
				else if (result == Status.Failure_InvalidOrderBalance)
				{
					return (StatusAdv.Failure_InsufficientBalance, 0, "");
				}
				else
				{
					return (StatusAdv.Failure_Undefined, 0, "");
				}
			}
			else
			{
				string insufficientOpportunityMessage = $"platform1 Ask: {platform1Ask}. platform2 Bid: {platform2Bid}. " +
				                                        $"Difference: {Math.Abs(platform2Bid - platform1Ask)}";
				
				return (StatusAdv.Failure_InsufficientArbitrageConditions, 0, insufficientOpportunityMessage);
			}
		}
		
		//Level 1 API below
		public Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return scraperInstance.BuyOrder(symbolPair, amount, price);
		}

		public Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return scraperInstance.SellOrder(symbolPair, amount, price);
		}

		public Task<Status> CancelOrderByID(string ID)
		{
			return scraperInstance.CancelOrderByID(ID);
		}

		public Task<Status> CancelAll()
		{
			return scraperInstance.CancelAll();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetBuyOrderHistoryAll()
		{
			return scraperInstance.GetBuyOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			return scraperInstance.GetBuyOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			return scraperInstance.GetSellOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			return scraperInstance.GetSellOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			return scraperInstance.GetOwnedStocks();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			return scraperInstance.GetBuyOrders();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			return scraperInstance.GetSellOrders();
		}

		public Task<(string, string)[]> GetSymbols()
		{
			return scraperInstance.GetSymbols();
		}

		public Task<string[]> SymbolStats(string[] symbolPair)
		{
			return scraperInstance.SymbolStats(symbolPair);
		}

		public string[] GetSupportedPlatforms()
		{
			return scraperInstance.GetSupportedPlatforms();
		}

		public Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			return scraperInstance.SetCredentials(platform, apiKey, secretKey, passphrase);
		}

		public (string platform, string apiKey, string secretKey, string passphrase) GetCredentials()
		{
			return scraperInstance.GetCredentials();
		}

		public Task<bool> TestCredentials()
		{
			return scraperInstance.TestCredentials();
		}

		
	}
	
	public struct ExecutedArbitrageResults
	{
		public (string symbol1, string symbol2) SymbolPair;
		
		//Note: In the current implementation, these are NOT confirmed values from the API, only internal. They are almost certainly
		//wrong and are only an initial first step in development to get something working.
		public bool Success;
		
		public decimal QuantityBoughtPlatform1;
		public decimal PricePaidPlatform1;
		public decimal QuantitySoldPlatform2;
		public decimal PriceSoldPlatform2;

		public decimal Platform1Ask;
		public decimal Platform1AskSize;
		public decimal Platform2Bid;
		public decimal Platform2BidSize;

		public decimal PriceDifferenceThreshold;
		public decimal IntendedArbitrageAmount;

		public long ActualExecutionTimeForSymbolPair;
	}
}