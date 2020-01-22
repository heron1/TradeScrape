using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Helpers.Custom;

namespace Scraper
{
	public class KucoinAPI : IOrderFunctions
	{
		private string _platform = "kucoin";
		private string _apiKey;
		private string _secretKey;
		private string _passphrase;
		private bool _debug = false;

		private string stringifyBodyDict(Dictionary<string, string> bodyDict)
		{
			return JsonConvert.SerializeObject(bodyDict);
		}
		
		private T destringifyString<T>(string stringified)
		{
			return JsonConvert.DeserializeObject<T>(stringified);
		}
		
		private async Task<string> getResponseStringFromGet(string apiUri)
		{
			string timestamp = "https://api.kucoin.com/api/v1/timestamp";
			HttpClient httpClient = new HttpClient();
			HttpResponseMessage response = await httpClient.GetAsync(new Uri(timestamp));
			var destr = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
			
			string nonce = destr["data"];
			string preStr = nonce + "GET" + "/api/v1" + apiUri.Split("/v1")[1]; //body at end
			byte[] str_to_sign = Encoding.UTF8.GetBytes(preStr); 
			byte[] encodedSecretKey = Encoding.UTF8.GetBytes(_secretKey);
			var hashSig = new HMACSHA256(encodedSecretKey).ComputeHash(str_to_sign);
			string signature = Convert.ToBase64String(hashSig);

			HttpRequestMessage request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri(apiUri),
				Headers =
				{
					{HttpRequestHeader.Accept.ToString(), "application/json"},
					{"KC-API-SIGN", signature},
					{"KC-API-TIMESTAMP", nonce},
					{"KC-API-KEY", _apiKey},
					{"KC-API-PASSPHRASE", _passphrase}
				},
				Content = new StringContent("", Encoding.UTF8, "application/json")
			};
			
			response = await httpClient.SendAsync(request);
			return await response.Content.ReadAsStringAsync();
		}
		
		private async Task<string> getResponseStringFromPost(string apiUri, Dictionary<string, string> bodyDict = null)
		{
			if (bodyDict is null)
			{
				bodyDict = new Dictionary<string, string>();
			}
			string stringifiedBodyDict = stringifyBodyDict(bodyDict);
			
			string timestamp = "https://api.kucoin.com/api/v1/timestamp";
			HttpClient httpClient = new HttpClient();
			HttpResponseMessage response = await httpClient.GetAsync(new Uri(timestamp));
			var destr = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
			
			string nonce = destr["data"];
			string preStr = nonce + "POST" + "/api/v1" + apiUri.Split("/v1")[1] + stringifiedBodyDict; //body at end
			byte[] str_to_sign = Encoding.UTF8.GetBytes(preStr); 
			byte[] encodedSecretKey = Encoding.UTF8.GetBytes(_secretKey);
			var hashSig = new HMACSHA256(encodedSecretKey).ComputeHash(str_to_sign);
			string signature = Convert.ToBase64String(hashSig);

			HttpRequestMessage request = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = new Uri(apiUri),
				Headers =
				{
					{HttpRequestHeader.Accept.ToString(), "application/json"},
					{"KC-API-SIGN", signature},
					{"KC-API-TIMESTAMP", nonce},
					{"KC-API-KEY", _apiKey},
					{"KC-API-PASSPHRASE", _passphrase}
				},
				Content = new StringContent(stringifiedBodyDict, Encoding.UTF8, "application/json")
			};
			
			response = await httpClient.SendAsync(request);
			return await response.Content.ReadAsStringAsync();
		}
		
		private async Task<string> getResponseStringFromDelete(string apiUri)
		{
			string timestamp = "https://api.kucoin.com/api/v1/timestamp";
			HttpClient httpClient = new HttpClient();
			HttpResponseMessage response = await httpClient.GetAsync(new Uri(timestamp));
			var destr = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
			
			string nonce = destr["data"];
			string preStr = nonce + "DELETE" + "/api/v1" + apiUri.Split("/v1")[1]; //body at end
			byte[] str_to_sign = Encoding.UTF8.GetBytes(preStr); 
			byte[] encodedSecretKey = Encoding.UTF8.GetBytes(_secretKey);
			var hashSig = new HMACSHA256(encodedSecretKey).ComputeHash(str_to_sign);
			string signature = Convert.ToBase64String(hashSig);

			HttpRequestMessage request = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				RequestUri = new Uri(apiUri),
				Headers =
				{
					{HttpRequestHeader.Accept.ToString(), "application/json"},
					{"KC-API-SIGN", signature},
					{"KC-API-TIMESTAMP", nonce},
					{"KC-API-KEY", _apiKey},
					{"KC-API-PASSPHRASE", _passphrase}
				},
				Content = new StringContent("", Encoding.UTF8, "application/json")
			};
			
			response = await httpClient.SendAsync(request);
			return await response.Content.ReadAsStringAsync();
		}
		
		public KucoinAPI(string apiKey, string secretKey, string passphrase)
		{
			_apiKey = apiKey;
			_secretKey = secretKey;
			_passphrase = passphrase;
		}
		
		public async Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			string symbs = symbolPair[0] + '-' + symbolPair[1];

			Dictionary<string, string> bodyDict = new Dictionary<string, string>();
			bodyDict["clientOid"] = Guid.NewGuid().ToString();
			bodyDict["side"] = "buy";
			bodyDict["symbol"] = symbs;
			bodyDict["price"] = price.ToString();
			bodyDict["size"] = amount.ToString();
			
			string apiUri = "https://api.kucoin.com/api/v1/orders";
			
			var strResponse = getResponseStringFromPost(apiUri, bodyDict).Result;
			if (_debug)
				print(strResponse);

			if (strResponse.Contains("Not Exists"))
				return Status.Failure_InvalidSymbol;
			else if (strResponse.Contains("Balance insufficient"))
				return Status.Failure_InvalidOrderBalance;
			else if (strResponse.Contains("\"code\":\"200000\""))
				return Status.Success;

			else return Status.Failure_Undefined;
		}

		public async Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			string symbs = symbolPair[0] + '-' + symbolPair[1];

			Dictionary<string, string> bodyDict = new Dictionary<string, string>();
			bodyDict["clientOid"] = Guid.NewGuid().ToString();
			bodyDict["side"] = "sell";
			bodyDict["symbol"] = symbs;
			bodyDict["price"] = price.ToString();
			bodyDict["size"] = amount.ToString();
			
			string apiUri = "https://api.kucoin.com/api/v1/orders";
			
			var strResponse = getResponseStringFromPost(apiUri, bodyDict).Result;
			if (_debug)
				print(strResponse);

			if (strResponse.Contains("Not Exists"))
				return Status.Failure_InvalidSymbol;
			else if (strResponse.Contains("Balance insufficient"))
				return Status.Failure_InvalidOrderBalance;
			else if (strResponse.Contains("\"code\":\"200000\""))
				return Status.Success;

			else return Status.Failure_Undefined;
		}

		public async Task<Status> CancelOrderByID(string orderId)
		{
			string apiUri = $"https://api.kucoin.com/api/v1/orders/{orderId}";
			
			var strResponse = getResponseStringFromDelete(apiUri).Result;
			if (_debug)
				print(strResponse);
			if (strResponse.Contains("\"code\":\"200000\""))
			{
				return Status.Success;
			}
			else if (strResponse.Contains("order_not_exist_or_not_allow_to_cancel"))
			{
				return Status.Failure_InvalidID;
			}
			else
				return Status.Failure_Undefined;
		}

		public async Task<Status> CancelAll()
		{
			string apiUri = $"https://api.kucoin.com/api/v1/orders";
			
			var strResponse = getResponseStringFromDelete(apiUri).Result;
			if (_debug)
				print(strResponse);
			if (strResponse.Contains("\"code\":\"200000\""))
			{
				return Status.Success;
			}
			else
			{
				return Status.Failure_Undefined;
			}
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> 
			GetBuyOrderHistoryAll()
		{
			var outDict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();
			string apiUri = "https://api.kucoin.com/api/v1/orders?status=done";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			if (_debug)
				print(strResponse);
			
			var output = destringifyString<ResultOutputCode>(strResponse);
			var output2 = JsonConvert.DeserializeObject<ResultOutputListOrders>(JsonConvert.SerializeObject(output.data));
			var outData = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(JsonConvert.SerializeObject(output2.items));
			
			foreach (var order in outData)
			{
				if (order["side"] == "buy" & Convert.ToDecimal(order["dealFunds"]) > 0)
				{
					string[] symbs = order["symbol"].Split("-");
					if (!outDict.ContainsKey((symbs[0], symbs[1])))
					{
						outDict[(symbs[0], symbs[1])] = new List<(decimal amount, decimal price, long timestampUnixEpoch)>();
					}

					outDict[(symbs[0], symbs[1])]
						.Add((Convert.ToDecimal(order["size"]), Convert.ToDecimal(order["dealFunds"]), Convert.ToInt64(order["createdAt"])));
				}
			}

			return outDict;
		}

		public async Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> 
			GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			var outDict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();

			string apiUri = $"https://api.kucoin.com/api/v1/orders?status=done&symbol={symbolPair.symb1.ToUpper()}-{symbolPair.symb2.ToUpper()}";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			if (_debug)
				print(strResponse);
			
			var output = destringifyString<ResultOutputCode>(strResponse);
			var output2 = JsonConvert.DeserializeObject<ResultOutputListOrders>(JsonConvert.SerializeObject(output.data));
			var outData = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(JsonConvert.SerializeObject(output2.items));
			
			foreach (var order in outData)
			{
				if (order["side"] == "buy" && Convert.ToDecimal(order["dealFunds"]) > 0)
				{
					string[] symbs = order["symbol"].Split("-");
					if (!outDict.ContainsKey((symbs[0], symbs[1])))
					{
						outDict[(symbs[0], symbs[1])] = new List<(decimal amount, decimal price, long timestampUnixEpoch)>();
					}

					outDict[(symbs[0], symbs[1])]
						.Add((Convert.ToDecimal(order["size"]), Convert.ToDecimal(order["dealFunds"]), Convert.ToInt64(order["createdAt"])));
				}
			}

			return (Status.Success, outDict);
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			var outDict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();
			string apiUri = "https://api.kucoin.com/api/v1/orders?status=done";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			if (_debug)
				print(strResponse);
			
			var output = destringifyString<ResultOutputCode>(strResponse);
			var output2 = JsonConvert.DeserializeObject<ResultOutputListOrders>(JsonConvert.SerializeObject(output.data));
			var outData = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(JsonConvert.SerializeObject(output2.items));
			
			foreach (var order in outData)
			{
				if (order["side"] == "sell" & Convert.ToDecimal(order["dealFunds"]) > 0)
				{
					string[] symbs = order["symbol"].Split("-");
					if (!outDict.ContainsKey((symbs[0], symbs[1])))
					{
						outDict[(symbs[0], symbs[1])] = new List<(decimal amount, decimal price, long timestampUnixEpoch)>();
					}

					outDict[(symbs[0], symbs[1])]
						.Add((Convert.ToDecimal(order["size"]), Convert.ToDecimal(order["price"]), Convert.ToInt64(order["createdAt"])));
				}
			}

			return outDict;
		}

		public async Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			var outDict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();

			string apiUri = $"https://api.kucoin.com/api/v1/orders?status=done&symbol={symbolPair.symb1.ToUpper()}-{symbolPair.symb2.ToUpper()}";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			if (_debug)
				print(strResponse);
			
			var output = destringifyString<ResultOutputCode>(strResponse);
			var output2 = JsonConvert.DeserializeObject<ResultOutputListOrders>(JsonConvert.SerializeObject(output.data));
			var outData = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(JsonConvert.SerializeObject(output2.items));
			
			foreach (var order in outData)
			{
				if (order["side"] == "sell" && Convert.ToDecimal(order["dealFunds"]) > 0)
				{
					string[] symbs = order["symbol"].Split("-");
					if (!outDict.ContainsKey((symbs[0], symbs[1])))
					{
						outDict[(symbs[0], symbs[1])] = new List<(decimal amount, decimal price, long timestampUnixEpoch)>();
					}

					outDict[(symbs[0], symbs[1])]
						.Add((Convert.ToDecimal(order["size"]), Convert.ToDecimal(order["dealFunds"]), Convert.ToInt64(order["createdAt"])));
				}
			}

			return (Status.Success, outDict);
		}

		class ResultOutputCode
		{
			public int code { get; set; }
			public object data { get; set; }
		}
		
		class ResultOutputListOrders
		{
			public int currentPage { get; set; }
			public int pageSize { get; set; }
			public int totalNum { get; set; }
			public int totalPage { get; set; }
			public object items { get; set; }
		}

		class ResultOutputData
		{
			public List<Dictionary<string, string>> data { get; set; }
		}

		public async Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			Dictionary<string, decimal> ownedStocks = new Dictionary<string, decimal>();

			string apiUri = "https://api.kucoin.com/api/v1/accounts";
			var strResponse = getResponseStringFromGet(apiUri).Result;

			if (_debug)
				print(strResponse);
			
			var output = JsonConvert.DeserializeObject<ResultOutputCode>(strResponse);
			var outputData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(JsonConvert.SerializeObject(output.data));
			foreach (var asset in outputData)
			{
				if (asset["type"] == "trade")
				{
					ownedStocks.Add(asset["currency"], Convert.ToDecimal(asset["available"]));
				}
			}
			
			return ownedStocks;
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			var outDict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>();
			string apiUri = "https://api.kucoin.com/api/v1/orders?status=active";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			var output = destringifyString<ResultOutputCode>(strResponse);
			var output2 = JsonConvert.DeserializeObject<ResultOutputListOrders>(JsonConvert.SerializeObject(output.data));
			var outData = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(JsonConvert.SerializeObject(output2.items));

			foreach (var order in outData)
			{
				if (order["side"] == "buy")
				{
					string[] symbs = order["symbol"].Split("-");
					if (!outDict.ContainsKey((symbs[0], symbs[1])))
					{
						outDict[(symbs[0], symbs[1])] = new List<(decimal amount, decimal price, string id)>();
					}

					outDict[(symbs[0], symbs[1])]
						.Add((Convert.ToDecimal(order["size"]), Convert.ToDecimal(order["price"]), order["id"]));
				}
			}

			return outDict;
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			var outDict = new Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>();
			string apiUri = "https://api.kucoin.com/api/v1/orders?status=active";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			var output = destringifyString<ResultOutputCode>(strResponse);
			var output2 = JsonConvert.DeserializeObject<ResultOutputListOrders>(JsonConvert.SerializeObject(output.data));
			var outData = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(JsonConvert.SerializeObject(output2.items));

			foreach (var order in outData)
			{
				if (order["side"] == "sell")
				{
					string[] symbs = order["symbol"].Split("-");
					if (!outDict.ContainsKey((symbs[0], symbs[1])))
					{
						outDict[(symbs[0], symbs[1])] = new List<(decimal amount, decimal price, string id)>();
					}

					outDict[(symbs[0], symbs[1])]
						.Add((Convert.ToDecimal(order["size"]), Convert.ToDecimal(order["price"]), order["id"]));
				}
			}

			return outDict;
		}

		public async Task<(string, string)[]> GetSymbols()
		{
			string apiUri = "https://api.kucoin.com/api/v1/symbols";
			string strResponse = getResponseStringFromGet(apiUri).Result;
			
			var output = JsonConvert.DeserializeObject<ResultOutputCode>(strResponse);
			var outputData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(JsonConvert.SerializeObject(output.data));
			
			List<(string, string)> tuplesList = new List<(string, string)>();
			foreach (var symbol in outputData)
			{
				string[] symbolPair = symbol["symbol"].Split('-');
				tuplesList.Add((symbolPair[0], symbolPair[1]));
			}
			
			tuplesList.Sort();
			return tuplesList.ToArray();
		}

		public async Task<string[]> SymbolStats(string[] symbolPair)
		{
			string apiUri = $"https://api.kucoin.com/api/v1/market/stats?symbol={symbolPair[0].ToUpper()}-{symbolPair[1].ToUpper()}";
			string strResponse = getResponseStringFromGet(apiUri).Result;

			if (_debug)
				print(strResponse);

			var output = destringifyString<ResultOutputCode>(strResponse);
			var outData = destringifyString<Dictionary<string, string>>(JsonConvert.SerializeObject(output.data));
			
			string[] responseStrArr = new string[9];
			//Returns a string array with the symbolPair, current bid, current bid size, ask, ask size, last price, volume, high, low
			responseStrArr[0] = outData["symbol"];
			responseStrArr[1] = outData["buy"];
			responseStrArr[2] = "NA";
			responseStrArr[3] = outData["sell"];
			responseStrArr[4] = "NA";
			responseStrArr[5] = outData["last"];
			responseStrArr[6] = outData["vol"];
			responseStrArr[7] = outData["high"];
			responseStrArr[8] = outData["low"];
			
			return responseStrArr;
		}

		public string[] GetSupportedPlatforms()
		{
			throw new System.NotSupportedException("Use WebScraper entry point");
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
			throw new System.NotSupportedException("Use WebScraper entry point");
		}

		public async Task<bool> TestCredentials()
		{
			string apiUri = "https://api.kucoin.com/api/v1/accounts";
			var strResponse = getResponseStringFromGet(apiUri).Result;

			if (strResponse.Contains("\"code\":\"200000\""))
				return true;
			else
				return false;
		}
	}
}