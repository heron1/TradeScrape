using System.Collections.Generic;
using System.Threading.Tasks;
using Scraper;

namespace TradingAPI
{
	public class TradingAPI : ITrade
	{
		private readonly IOrderFunctions scraperInstance;

		public TradingAPI(string platform, string apiKey, string secretKey, string passphrase = null)
		{
			scraperInstance = SupplierIOrderFunctions.GetIOrderFunctions(platform, apiKey, secretKey, passphrase);
		}
		
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