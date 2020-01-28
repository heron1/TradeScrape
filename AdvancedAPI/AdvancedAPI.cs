﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
 using System.Threading;
 using System.Threading.Tasks;
using AppSettings;
using AppSettingsFactory;
using Scraper;

namespace AdvancedAPI
{
	public class AdvancedAPI : IAdvanced
	{
		private readonly IOrderFunctions scraperInstance;
		private readonly IAppSettings settings = SupplierIAppSettings.GetIAppSettingsStorage();

		public AdvancedAPI(string platform, string apiKey, string secretKey, string passphrase = null)
		{
			scraperInstance = SupplierIOrderFunctions.GetIOrderFunctions(platform);
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

		//Level 1 API Below
		
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

		public List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)> 
		FindArbitrageOpportunities((string platform1, string platform2) platforms, decimal priceDifferenceThreshold, 
			decimal minSpend, decimal maxSpend, int callLimit = 100)
		{
			List<(string symb1, string symb2)> symbolPairs = FindMatchingPlatformSymbolPairs((platforms.platform1, platforms.platform2));

			var ArbitrageOpportunitiesList =
				new List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal priceDifference)>();
			
			(IOrderFunctions platform1, IOrderFunctions platform2) platformApis =
				(SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform1),
					SupplierIOrderFunctions.GetIOrderFunctions(platforms.platform2));

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
			if (!(maxSpend > 0M)) throw new ArgumentException("maxSpend !> 0M");
			if (!(minSpend <= maxSpend)) throw new ArgumentException("minSpend <= maxSpend");
			
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

			return (StatusAdv.Success, platform1Ask, platform1AskSize, platform2Bid, platform2BidSize, actualArbitrageAmount);
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
}