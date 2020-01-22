using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scraper
{
	public interface IOrderFunctions
	{
		/*
		 * Desired functionality to implement here, is of course to launch buy/sell orders from
		 * a higher level object, regardless of platform.
		 */

		//Returns whether the order was valid (0) or not (1+)
		Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price);
		
		//Returns whether the order was valid (0) or not (1+)
		Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price);
		
		//TODO CancelOrderByID, CancelAll
		//Every outstanding buy order should have an ID also returned. This can be cancelled directly.
		Task<Status> CancelOrderByID(string ID);

		Task<Status> CancelAll();
		
		Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> 
			GetBuyOrderHistoryAll();
			
		Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair);
			
		Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> 
			GetSellOrderHistoryAll();
			
		Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair);

		//Returns a dictionary of the string symbol of each owned stock, along with the amount held
		Task<Dictionary<string, decimal>> GetOwnedStocks();

		//Returns a dictionary of the string symbol pair (tuple) of each outstanding buy order,
		//and an multidimensional decimal tuple of outstanding orders ([amount, price] per entry)
		Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders();
		
		//Returns a dictionary of the string symbol of each stock with outstanding sell orders, and an int array [amount, price]
		Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders();

		//Returns a string array of all the available stock symbols
		Task<(string, string)[]> GetSymbols();
		
		//Returns a string array with the symbolPair, current bid, current bid size, ask, ask size, last price, volume, high, low
		Task<string[]> SymbolStats(string[] symbolPair);

		//Returns a string array of all supported platforms
		string[] GetSupportedPlatforms();

		//Same arguments should always be passed into the constructor. May be manually reset to
		//handle re-connection, although if a new token is needed this will be requested directly in the returned Status
		//Will return Failure_InvalidLogin or Failure_InvalidPlatform if error encountered.
		Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase);

		(string platform, string apiKey, string secretKey, string passphrase) GetCredentials();

		public Task<bool> TestCredentials();
	}
}