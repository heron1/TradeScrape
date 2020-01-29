using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Scraper
{
	//public access point API for Scraper namespace, acts as a wrapper
	public class ScraperWrapper : IOrderFunctions
	{
		private string _platform;
		private string _apiKey;
		private string _secretKey;
		private string _passphrase;

		private readonly IOrderFunctions _innerType;

		public ScraperWrapper(string platform, string apiKey = null, string secretKey = null, string passphrase = null)
		{
			//Assert state
			if (platform == null) throw new ArgumentNullException("platform");

			//Constructor simply calls SetCredentials.
			SetCredentials(platform, apiKey, secretKey, passphrase);
			if (platform == "bitfinex")
			{
				//Assert state
				if (apiKey == null) throw new ArgumentNullException("apiKey");
				if (secretKey == null) throw new ArgumentNullException("secretKey");
				
				_innerType = new BitfinexAPI(apiKey, secretKey);
			}
			else if (platform == "kucoin")
			{
				//Assert state
				if (apiKey == null) throw new ArgumentNullException("apiKey");
				if (secretKey == null) throw new ArgumentNullException("secretKey");
				if (passphrase == null) throw new ArgumentNullException("passphrase");
				
				_innerType = new KucoinAPI(apiKey, secretKey, passphrase);
			}
			else if (platform == "mock")
			{
				_innerType = new MockAPI();
			}
			else
			{
				throw new Exception("Invalid platform selected");
			}
		}

		//IOrderFunctions interface implementations below
		public Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return _innerType.BuyOrder(symbolPair, amount, price);
		}

		public Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			return _innerType.SellOrder(symbolPair, amount, price);
		}

		public Task<Status> CancelOrderByID(string ID)
		{
			return _innerType.CancelOrderByID(ID);
		}

		public Task<Status> CancelAll()
		{
			return _innerType.CancelAll();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetBuyOrderHistoryAll()
		{
			return _innerType.GetBuyOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetBuyOrderHistoryBySymbolPair(
			(string symb1, string symb2) symbolPair)
		{
			return _innerType.GetBuyOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			return _innerType.GetSellOrderHistoryAll();
		}

		public Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair(
			(string symb1, string symb2) symbolPair)
		{
			return _innerType.GetSellOrderHistoryBySymbolPair(symbolPair);
		}

		public Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			return _innerType.GetOwnedStocks();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			return _innerType.GetBuyOrders();
		}

		public Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			return _innerType.GetSellOrders();
		}

		public Task<(string, string)[]> GetSymbols()
		{
			return _innerType.GetSymbols();
		}

		public Task<string[]> SymbolStats(string[] symbolPair)
		{
			 var output = _innerType.SymbolStats(symbolPair).Result;
			 output[1] = JsonConvert.DeserializeObject<Decimal>(output[1]).ToString();
			 output[2] = JsonConvert.DeserializeObject<Decimal>(output[2]).ToString();
			 output[3] = JsonConvert.DeserializeObject<Decimal>(output[3]).ToString();
			 output[4] = JsonConvert.DeserializeObject<Decimal>(output[4]).ToString();
			 output[5] = JsonConvert.DeserializeObject<Decimal>(output[5]).ToString();
			 output[6] = JsonConvert.DeserializeObject<Decimal>(output[6]).ToString();
			 output[7] = JsonConvert.DeserializeObject<Decimal>(output[7]).ToString();
			 output[8] = JsonConvert.DeserializeObject<Decimal>(output[8]).ToString();

			 var aFix = Task.Run<string[]>(() => output);
			 return aFix;
		}

		public string[] GetSupportedPlatforms()
		{
			return new string[] {"bitfinex", "kucoin"}; //TODO: Pull from central, lower depenedency source
		}

		public Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			_platform = platform;
			_apiKey = apiKey;
			_secretKey = secretKey;
			_passphrase = passphrase;
			
			return Status.Success;
		}

		public (string platform, string apiKey, string secretKey, string passphrase) GetCredentials()
		{
			
			return (_platform, _apiKey, _secretKey, _passphrase);
		}

		public Task<bool> TestCredentials()
		{
			return _innerType.TestCredentials();
		}
	}
}