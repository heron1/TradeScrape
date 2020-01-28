using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scraper;

namespace TradeScrape_Unit_Tests.LiveAPIIntegrations_Level1.Mocks
{
	internal class MockAPI : IOrderFunctions
	{
		//TODO: Implement a mockAPI which is internal. Useful for Unit Testing and general testing of new concepts
		public async Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return Status.Success;
		}

		public async Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return Status.Success;
		}

		public async Task<Status> CancelOrderByID(string ID)
		{
			return Status.Success;
		}

		public async Task<Status> CancelAll()
		{
			return Status.Success;
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetBuyOrderHistoryAll()
		{
			var dict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>();
			dict.Add(("SYMB1", "SYMB2"), new List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>() {(10, 3, 34342424)});
			return dict;
		}

		public async Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			var dict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>();
			dict.Add(("SYMB1", "SYMB2"), new List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>() {(10, 3, 34342424)});
			return (Status.Success, dict);
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			var dict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>();
			dict.Add(("SYMB1", "SYMB2"), new List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>() {(10, 3, 34342424)});
			return dict;
		}

		public async Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			var dict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>();
			dict.Add(("SYMB1", "SYMB2"), new List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>() {(10, 3, 34342424)});
			return (Status.Success, dict);
		}

		public async Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			var dict = new Dictionary<string, decimal>();
			dict.Add("SYMB1", 0.0032M);
			return dict;
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			var dict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>();
			dict.Add(("SYMB1", "SYMB2"), new List<(decimal amount, decimal avgPrice, string id)>() {(10, 3, "123")});
			return dict;
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			var dict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>();
			dict.Add(("SYMB1", "SYMB2"), new List<(decimal amount, decimal avgPrice, string id)>() {(10, 3, "123")});
			return dict;
		}

		public async Task<(string, string)[]> GetSymbols()
		{
			return new (string, string)[] {("SYMB1", "SYMB2")};
		}

		//Returns a string array with the symbolPair, current bid, current bid size, ask, ask size, last price, volume, high, low
		//TODO: Obviously now with hindsight, this would be better as a struct with valid fields. Redo on project re-creation.
		public async Task<string[]> SymbolStats(string[] symbolPair)
		{
			return new string[] {"SYMB1-SYMB2", "32", "100", "35", "100", "33", "240.2", "1000.0", "24.93"};
		}

		public string[] GetSupportedPlatforms()
		{
			throw new System.NotSupportedException();
		}

		public Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			throw new System.NotSupportedException();
		}

		public (string platform, string apiKey, string secretKey, string passphrase) GetCredentials()
		{
			throw new System.NotSupportedException();
		}

		public async Task<bool> TestCredentials()
		{
			return true;
		}
	}
}