using System.Collections.Generic;
using System.Threading.Tasks;
using TradeScraper.Scraper;

namespace TradeScraper.TradingAPI
{
	public class TradingAPI : ITrade
	{
		private IOrderFunctions webScraper;

		public TradingAPI(string platform, string apiKey, string secretKey, string passphrase = null)
		{
			webScraper = new WebScraper(platform, apiKey, secretKey, passphrase);
		}
		
		public Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return webScraper.BuyOrder(symbolPair, amount, price);
		}

		public Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return webScraper.SellOrder(symbolPair, amount, price);
		}

		public Task<Status> CancelOrderByID(string ID)
		{
			return webScraper.CancelOrderByID(ID);
		}

		public Task<Status> CancelAll()
		{
			return webScraper.CancelAll();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetBuyOrderHistoryAll()
		{
			return webScraper.GetBuyOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			return webScraper.GetBuyOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			return webScraper.GetSellOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			return webScraper.GetSellOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			return webScraper.GetOwnedStocks();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			return webScraper.GetBuyOrders();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			return webScraper.GetSellOrders();
		}

		public Task<(string, string)[]> GetSymbols()
		{
			return webScraper.GetSymbols();
		}

		public Task<string[]> SymbolStats(string[] symbolPair)
		{
			return webScraper.SymbolStats(symbolPair);
		}

		public string[] GetSupportedPlatforms()
		{
			return webScraper.GetSupportedPlatforms();
		}

		public Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			return webScraper.SetCredentials(platform, apiKey, secretKey, passphrase);
		}

		public (string platform, string apiKey, string secretKey, string passphrase) GetCredentials()
		{
			return webScraper.GetCredentials();
		}

		public Task<bool> TestCredentials()
		{
			return webScraper.TestCredentials();
		}
	}
}