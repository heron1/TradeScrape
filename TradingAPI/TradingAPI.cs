using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvancedAPI;
using Scraper;

namespace TradingAPI
{
	public class TradingAPI : ITrade
	{
		private readonly IAdvanced advancedAPIInstance;

		public TradingAPI(string platform, string apiKey, string secretKey, string passphrase = null)
		{
			advancedAPIInstance = new AdvancedAPI.AdvancedAPI(platform, apiKey, secretKey, passphrase);
		}
		
		//Level 2 API Below
		public ExecutedArbitrageResults FindAndExecuteFirstValidSymbolPairForArbitrage()
		{
			return advancedAPIInstance.FindAndExecuteFirstValidSymbolPairForArbitrage();
		}

		public List<(string symb1, string symb2)> FindMatchingPlatformSymbolPairs((string platform1, string platform2) platformsTuple)
		{
			return advancedAPIInstance.FindMatchingPlatformSymbolPairs((platformsTuple.platform1, platformsTuple.platform2));
		}

		public (StatusAdv status, Decimal amountExecuted, string msg) AttemptArbitrageSingle((string symbol1, string symbol2) symbolPair,
			(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal priceDifferenceThreshold, 
			decimal minSpend, decimal maxSpend)
		{
			return advancedAPIInstance.AttemptArbitrageSingle((symbolPair.symbol1, symbolPair.symbol2),
			(platforms.platform1Api, platforms.platform2Api), priceDifferenceThreshold, minSpend, maxSpend);
		}

		public List<(string platform1, string platform2, (string symb1, string symb2) symbPair, decimal size, decimal priceDifference)> FindArbitrageOpportunities((string platform1, string platform2) platforms, decimal priceDifferenceThreshold,
			decimal minSpend, decimal maxSpend, int callLimit = 100)
		{
			throw new NotImplementedException();
		}

		public (StatusAdv status, decimal platform1Ask, decimal platform1AskSize, decimal platform2Bid, decimal platform2BidSize,
			decimal actualArbitrageAmount) CheckArbitrageOpportunity((string symbol1, string symbol2) symbolPair,
				(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal priceDifferenceThreshold,
				decimal minSpend, decimal maxSpend)
		{
			throw new NotImplementedException();
		}


		//Level 1 API Below
		public Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return advancedAPIInstance.BuyOrder(symbolPair, amount, price);
		}

		public Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return advancedAPIInstance.SellOrder(symbolPair, amount, price);
		}

		public Task<Status> CancelOrderByID(string ID)
		{
			return advancedAPIInstance.CancelOrderByID(ID);
		}

		public Task<Status> CancelAll()
		{
			return advancedAPIInstance.CancelAll();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetBuyOrderHistoryAll()
		{
			return advancedAPIInstance.GetBuyOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			return advancedAPIInstance.GetBuyOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			return advancedAPIInstance.GetSellOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			return advancedAPIInstance.GetSellOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			return advancedAPIInstance.GetOwnedStocks();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			return advancedAPIInstance.GetBuyOrders();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			return advancedAPIInstance.GetSellOrders();
		}

		public Task<(string, string)[]> GetSymbols()
		{
			return advancedAPIInstance.GetSymbols();
		}

		public Task<string[]> SymbolStats(string[] symbolPair)
		{
			return advancedAPIInstance.SymbolStats(symbolPair);
		}

		public string[] GetSupportedPlatforms()
		{
			return advancedAPIInstance.GetSupportedPlatforms();
		}

		public Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			return advancedAPIInstance.SetCredentials(platform, apiKey, secretKey, passphrase);
		}

		public (string platform, string apiKey, string secretKey, string passphrase) GetCredentials()
		{
			return advancedAPIInstance.GetCredentials();
		}

		public Task<bool> TestCredentials()
		{
			return advancedAPIInstance.TestCredentials();
		}

		public void ArbitrageSymbolPairContinuously((string symbol1, string symbol2) symbolPair)
		{
		}
	}
}