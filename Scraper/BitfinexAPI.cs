﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Helpers.Custom;


namespace Scraper
{
	internal class BitfinexAPI : IOrderFunctions
	{
		private string _platform = "bitfinex";
		private string _apiKey;
		private string _secretKey;
		
		#if DEBUG
		private bool _debug = false;
		#else
		private bool _debug = false;
		#endif
		

		private async Task<Status> ascertainResponseStatus(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
				return Status.Success;
			else
			{
				string outer = await response.Content.ReadAsStringAsync();
				var outerone = JsonConvert.DeserializeObject<string[]>(outer);
				string errorMessage = outerone[2];
				
				Debug.WriteLine(errorMessage); //TODO: refactor all _debug prints to this
				
				return Status.Failure_Undefined;
			}
		}

		private async Task<HttpResponseMessage> getResponseForPublic(string fullUriPath)
		{
			HttpClient httpClient = new HttpClient();
			HttpResponseMessage response = await httpClient.GetAsync(new Uri(fullUriPath));
			
			return response;
		}

		private async Task<HttpResponseMessage> getResponseForAuthenticated(string fullUriPath, Dictionary<string, string> bodyDict = null)
		{
			if (bodyDict is null)
			{
				bodyDict = new Dictionary<string, string>();
			}
			
			HttpClient httpClient = new HttpClient();
			string nonce = getNonce();
			string apiPart = sigPart(fullUriPath);
			string stringifiedBodyDict = stringifyBodyDict(bodyDict);
			
			Debug.WriteLine(stringifiedBodyDict);

			string signature = getSignature(apiPart, nonce, stringifiedBodyDict);

			HttpRequestMessage requestMessage = buildRequest(new Uri(fullUriPath), nonce, signature, stringifiedBodyDict);
			HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

			return response;
		}

		private HttpRequestMessage buildRequest(Uri fullUriPath, string nonce, string signature, string stringifiedBodyDict)
		{
			HttpRequestMessage request = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = fullUriPath,
				Headers =
				{
					{HttpRequestHeader.Accept.ToString(), "application/json"},
					{"bfx-nonce", nonce},
					{"bfx-apikey", _apiKey},
					{"bfx-signature", signature}
				},
				Content = new StringContent(stringifiedBodyDict, Encoding.UTF8, "application/json")
			};
			return request;
		}

		private string sigPart(string fullUriPath)
		{
			string[] apiSplit = fullUriPath.Split("/v2");
			return "/api/v2" + apiSplit[1];
		}
		
		private string getNonce()
		{
			return (DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000).ToString();
		}

		private string getSignature(string queryString, string nonce, string stringifiedBodyDict)
		{
			string preStr = queryString + nonce + stringifiedBodyDict;
			byte[] preSig = Encoding.UTF8.GetBytes(preStr);
			byte[] encodedSecretKey = Encoding.UTF8.GetBytes(_secretKey);
			string hashSig = BitConverter.ToString(new HMACSHA384(encodedSecretKey).ComputeHash(preSig));
			string signature = hashSig.Replace("-", "").ToLower(); //clean the signature
			
			return signature;
		}

		private string stringifyBodyDict(Dictionary<string, string> bodyDict)
		{
			return JsonConvert.SerializeObject(bodyDict);
		}
		
		private T destringifyString<T>(string stringified)
		{
			return JsonConvert.DeserializeObject<T>(stringified);
		}

		private Dictionary<string, string> formattedReturnDict(string returnNames, string returnString)
		{
			var returnNamesSpliter = returnNames.Replace("[", String.Empty).Replace("]", String.Empty);
			var returnNamesRetTypes = returnNamesSpliter.Split(",");
			
			var returnStringSpliter = returnString.Replace("[", String.Empty).Replace("]", String.Empty);
			var returnStringRetTypes = returnStringSpliter.Split(",");


			Dictionary<string, string> retDict = new Dictionary<string, string>();
			for (var i = 0; i < returnNamesRetTypes.Length; i++)
			{
				if (!retDict.ContainsKey(returnNamesRetTypes[i].Trim()))
					retDict.Add(returnNamesRetTypes[i].Trim(), returnStringRetTypes[i].Trim());
			}

			return retDict;
		}

		private string returnNamesOrder()
		{
			string returnNames = @"MTS,TYPE,MESSAGE_ID,null,[ID,GID,CID,SYMBOL,MTS_CREATE,MTS_UPDATE,AMOUNT,AMOUNT_ORIG,TYPE,
			TYPE_PREV,MTS_TIF,_PLACEHOLDER,FLAGS,ORDER_STATUS,_PLACEHOLDER,_PLACEHOLDER,PRICE,PRICE_AVG,PRICE_TRAILING,
			PRICE_AUX_LIMIT,_PLACEHOLDER,_PLACEHOLDER,_PLACEHOLDER,HIDDEN,PLACED_ID,_PLACEHOLDER,_PLACEHOLDER,_PLACEHOLDER,
			ROUTING,_PLACEHOLDER,_PLACEHOLDER,META],CODE,STATUS,TEXT";
			return returnNames;
		}
		
		private string returnNamesGetOrder()
		{
			string returnNames = @"ID,GID,CID,SYMBOL,MTS_CREATE,MTS_UPDATE,AMOUNT,AMOUNT_ORIG,TYPE,TYPE_PREV,_PLACEHOLDER,
			_PLACEHOLDER,FLAGS,STATUS,_PLACEHOLDER,_PLACEHOLDER,PRICE,PRICE_AVG,PRICE_TRAILING,PRICE_AUX_LIMIT,_PLACEHOLDER,
			_PLACEHOLDER,_PLACEHOLDER,HIDDEN,PLACED_ID";
			return returnNames;
		}

		private string returnNamesGetorderHistory()
		{
			string returnNames = @"ID,GID,CID,SYMBOL,MTS_CREATE,MTS_UPDATE,AMOUNT,AMOUNT_ORIG,TYPE,TYPE_PREV,MTS_TIF,_PLACEHOLDER,
			FLAGS,ORDER_STATUS,_PLACEHOLDER,_PLACEHOLDER,PRICE,PRICE_AVG,PRICE_TRAILING,PRICE_AUX_LIMIT,_PLACEHOLDER,_PLACEHOLDER,
			_PLACEHOLDER,_PLACEHOLDER,HIDDEN,PLACED_ID,_PLACEHOLDER,_PLACEHOLDER,ROUTING,_PLACEHOLDER,_PLACEHOLDER,META";
			return returnNames;
		}

		public BitfinexAPI(string apiKey, string secretKey)
		{
			_apiKey = apiKey;
			_secretKey = secretKey;
		}

		public async Task<Status> BuyOrder(string[] symbolPair, decimal amount, decimal price)
		{
			Dictionary<string, string> orderBody = new Dictionary<string, string>();
			orderBody["type"] = "EXCHANGE LIMIT";
			orderBody["symbol"] = $"t{symbolPair[0]}{symbolPair[1]}";
			orderBody["price"] = price.ToString();
			orderBody["amount"] = amount.ToString();
			
			string apiUri = "https://api.bitfinex.com/v2/auth/w/order/submit";
			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, orderBody);
			string responseString = await response.Content.ReadAsStringAsync();
			if (_debug)
				print(responseString);
			
			string[] invalidOrderBalanceStrings = new string[]
			{
				"Invalid order: not enough tradable balance",
				"Invalid order: not enough exchange balance"
			};

			if (invalidOrderBalanceStrings.Any(responseString.Contains))
			{
				return Status.Failure_InvalidOrderBalance;
			}
			
			if (responseString.Contains("Invalid order: minimum size for"))
			{
				return Status.Failure_InvalidOrderMinimumAmount;
			}
			
			var returnNames = returnNamesOrder();

			Dictionary<string, string> formattedBuyOrderReturnDict = formattedReturnDict(returnNames, responseString);

			if (_debug)
			{
				foreach (var a in formattedBuyOrderReturnDict)
					print(a.ToString());
			}

			// throw new System.NotImplementedException(); //TODO: Unit Test
			if (formattedBuyOrderReturnDict["STATUS"] == "\"SUCCESS\"")
				return Status.Success;
			else
				return Status.Failure_Undefined;
		}

		public async Task<Status> SellOrder(string[] symbolPair, decimal amount, decimal price)
		{
			Dictionary<string, string> orderBody = new Dictionary<string, string>();
			orderBody["type"] = "EXCHANGE LIMIT";
			orderBody["symbol"] = $"t{symbolPair[0]}{symbolPair[1]}";
			orderBody["price"] = price.ToString();
			orderBody["amount"] = "-" + amount.ToString(); //Order with negative amount is a sell order on Bitfinex API
			
			string apiUri = "https://api.bitfinex.com/v2/auth/w/order/submit";
			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, orderBody);
			string responseString = await response.Content.ReadAsStringAsync();
			if (_debug)
				print(responseString);
			
			string[] invalidOrderBalanceStrings = new string[]
			{
				"Invalid order: not enough tradable balance",
				"Invalid order: not enough exchange balance"
			};
			
			if (invalidOrderBalanceStrings.Any(responseString.Contains))
			{
				return Status.Failure_InvalidOrderBalance;
			}
			if (responseString.Contains("Invalid order: minimum size for"))
			{
				return Status.Failure_InvalidOrderMinimumAmount;
			}

			var returnNames = returnNamesOrder();

			Dictionary<string, string> formattedBuyOrderReturnDict = formattedReturnDict(returnNames, responseString);

			if (_debug)
			{
				foreach (var a in formattedBuyOrderReturnDict)
					print(a.ToString());
			}

			// throw new System.NotImplementedException(); //TODO: Unit Test
			if (formattedBuyOrderReturnDict["STATUS"] == "\"SUCCESS\"")
				return Status.Success;
			else
				return Status.Failure_Undefined;
		}

		public async Task<Status> CancelOrderByID(string ID)
		{
			Dictionary<string, long> orderBody = new Dictionary<string, long>();
			orderBody["id"] = Convert.ToInt64(ID);
			
			string apiUri = "https://api.bitfinex.com/v2/auth/w/order/cancel";

			HttpClient httpClient = new HttpClient();
			string nonce = getNonce();
			string apiPart = sigPart(apiUri);
			string stringifiedBodyDict = JsonConvert.SerializeObject(orderBody);
			if (_debug)
				print(stringifiedBodyDict);
			string signature = getSignature(apiPart, nonce, stringifiedBodyDict);

			HttpRequestMessage requestMessage = buildRequest(new Uri(apiUri), nonce, signature, stringifiedBodyDict);
			HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

			string returnString = await response.Content.ReadAsStringAsync();

			if (_debug)
				print(returnString);

			if (returnString.Contains("Order not found"))
				return Status.Failure_InvalidID;
			
			if (returnString.Contains("SUCCESS"))
				return Status.Success;
			else
			{
				return Status.Failure_Undefined;
			}
		}

		public async Task<Status> CancelAll()
		{
			Dictionary<string, long> orderBody = new Dictionary<string, long>();
			orderBody["all"] = 1;
			
			string apiUri = "https://api.bitfinex.com/v2/auth/w/order/cancel/multi";

			HttpClient httpClient = new HttpClient();
			string nonce = getNonce();
			string apiPart = sigPart(apiUri);
			string stringifiedBodyDict = JsonConvert.SerializeObject(orderBody);
			if (_debug)
				print(stringifiedBodyDict);
			string signature = getSignature(apiPart, nonce, stringifiedBodyDict);

			HttpRequestMessage requestMessage = buildRequest(new Uri(apiUri), nonce, signature, stringifiedBodyDict);
			HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

			string returnString = await response.Content.ReadAsStringAsync();

			if (_debug)
				print(returnString);

			if (returnString.Contains("Order not found"))
				return Status.Failure_InvalidID;
			
			if (returnString.Contains("SUCCESS"))
				return Status.Success;
			else
			{
				return Status.Failure_Undefined;
			}
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetBuyOrderHistoryAll()
		{
			string apiUri = "https://api.bitfinex.com/v2/auth/r/orders/hist";
			var ordersDict = new Dictionary<(string, string), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();

			var returnNames = returnNamesGetorderHistory();
			var retNames = returnNames.Split(',');

			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();
			var ordersBodyReturn = destringifyString<string[][]>(returnString);

			if (_debug)
				print(returnString);

			List<Dictionary<string, string>> orders = new List<Dictionary<string, string>>();
			
			foreach (var order in ordersBodyReturn)
			{
				Dictionary<string, string> orderBody = new Dictionary<string, string>();
				for (int i = 0; i < retNames.Length; i++)
				{
					if (!orderBody.ContainsKey(retNames[i]))
						orderBody.Add(retNames[i], order[i]);
				}

				orders.Add(orderBody);
			}
			
			foreach (var order in orders)
			{
				if (order["ORDER_STATUS"].Contains("EXECUTED") && Convert.ToDecimal(order["AMOUNT_ORIG"]) > 0)
				{
					if (_debug)
						print("Order Status: " + order["ORDER_STATUS"]);
					string symb1 = order["SYMBOL"].Substring(1, 3);
					string symb2 = order["SYMBOL"].Substring(4, 3);
					decimal amount = Convert.ToDecimal(order["AMOUNT_ORIG"]);
					decimal price = Convert.ToDecimal(order["PRICE_AVG"]);
					long orderTimestamp = Convert.ToInt64(order["MTS_CREATE"]);
					if (!ordersDict.ContainsKey((symb1, symb2)))
					{
						ordersDict[(symb1, symb2)] = new List<(decimal, decimal, long)>();
					}

					ordersDict[(symb1, symb2)].Add((amount, price, orderTimestamp));
				}
			}
			
			// throw new System.NotImplementedException(); //TODO: Unit testing
			return ordersDict;
		}

		public async Task<(Status, Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> 
			GetBuyOrderHistoryBySymbolPair((string symb1, string symb2) symbolPair)
		{
			string apiUri = $"https://api.bitfinex.com/v2/auth/r/orders/t{symbolPair.symb1.ToUpper()}{symbolPair.symb2.ToUpper()}/hist";
			if (_debug)
				print(apiUri);
			var ordersDict = new Dictionary<(string, string), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();
			
			var returnNames = returnNamesGetorderHistory();
			var retNames = returnNames.Split(',');

			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();

			if (_debug)
				print(returnString);
			
			if (returnString.Contains("symbol: invalid"))
				return (Status.Failure_InvalidSymbol, ordersDict);
			
			var ordersBodyReturn = destringifyString<string[][]>(returnString);

			List<Dictionary<string, string>> orders = new List<Dictionary<string, string>>();
			
			foreach (var order in ordersBodyReturn)
			{
				Dictionary<string, string> orderBody = new Dictionary<string, string>();
				for (int i = 0; i < retNames.Length; i++)
				{
					if (!orderBody.ContainsKey(retNames[i]))
						orderBody.Add(retNames[i], order[i]);
				}

				orders.Add(orderBody);
			}
			
			foreach (var order in orders)
			{
				if (order["ORDER_STATUS"].Contains("EXECUTED") && Convert.ToDecimal(order["AMOUNT_ORIG"]) > 0)
				{
					if (_debug)
						print("Order Status: " + order["ORDER_STATUS"]);
					string symb1 = order["SYMBOL"].Substring(1, 3);
					string symb2 = order["SYMBOL"].Substring(4, 3);
					decimal amount = Convert.ToDecimal(order["AMOUNT_ORIG"]);
					decimal avgPrice = Convert.ToDecimal(order["PRICE_AVG"]);
					long orderTimestamp = Convert.ToInt64(order["MTS_CREATE"]);
					if (!ordersDict.ContainsKey((symb1, symb2)))
					{
						ordersDict[(symb1, symb2)] = new List<(decimal, decimal, long)>();
					}

					ordersDict[(symb1, symb2)].Add((amount, avgPrice, orderTimestamp));
				}
			}
			
			// throw new System.NotImplementedException(); //TODO: Unit testing
			return (Status.Success, ordersDict);
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>> GetSellOrderHistoryAll()
		{
			string apiUri = "https://api.bitfinex.com/v2/auth/r/orders/hist";
			var ordersDict = new Dictionary<(string, string), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();
			
			var returnNames = returnNamesGetorderHistory();
			var retNames = returnNames.Split(',');

			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();
			var ordersBodyReturn = destringifyString<string[][]>(returnString);

			if (_debug)
				print(returnString);

			List<Dictionary<string, string>> orders = new List<Dictionary<string, string>>();
			
			foreach (var order in ordersBodyReturn)
			{
				Dictionary<string, string> orderBody = new Dictionary<string, string>();
				for (int i = 0; i < retNames.Length; i++)
				{
					if (!orderBody.ContainsKey(retNames[i]))
						orderBody.Add(retNames[i], order[i]);
				}

				orders.Add(orderBody);
			}
			
			foreach (var order in orders)
			{
				if (order["ORDER_STATUS"].Contains("EXECUTED") && Convert.ToDecimal(order["AMOUNT_ORIG"]) < 0)
				{
					if (_debug)
						print("Order Status: " + order["ORDER_STATUS"]);
					string symb1 = order["SYMBOL"].Substring(1, 3);
					string symb2 = order["SYMBOL"].Substring(4, 3);
					decimal amount = Convert.ToDecimal(order["AMOUNT_ORIG"]) * -1;
					decimal price = Convert.ToDecimal(order["PRICE_AVG"]);
					long orderTimestamp = Convert.ToInt64(order["MTS_CREATE"]);
					if (!ordersDict.ContainsKey((symb1, symb2)))
					{
						ordersDict[(symb1, symb2)] = new List<(decimal, decimal, long)>();
					}

					ordersDict[(symb1, symb2)].Add((amount, price, orderTimestamp));
				}
			}
			
			// throw new System.NotImplementedException(); //TODO: Unit testing
			return ordersDict;
		}

		public async Task<(Status, Dictionary<(string symb1, string symb2),
			List<(decimal amount, decimal avgPrice, long timestampUnixEpoch)>>)> GetSellOrderHistoryBySymbolPair(
			(string symb1, string symb2) symbolPair)
		{
			string apiUri = $"https://api.bitfinex.com/v2/auth/r/orders/t{symbolPair.symb1.ToUpper()}{symbolPair.symb2.ToUpper()}/hist";
			var ordersDict = new Dictionary<(string, string), List<(decimal amount, decimal price, long timestampUnixEpoch)>>();
			
			var returnNames = returnNamesGetorderHistory();
			var retNames = returnNames.Split(',');

			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();
			
			if (returnString.Contains("symbol: invalid"))
				return (Status.Failure_InvalidSymbol, ordersDict);
			
			var ordersBodyReturn = destringifyString<string[][]>(returnString);

			if (_debug)
				print(returnString);

			List<Dictionary<string, string>> orders = new List<Dictionary<string, string>>();
			
			foreach (var order in ordersBodyReturn)
			{
				Dictionary<string, string> orderBody = new Dictionary<string, string>();
				for (int i = 0; i < retNames.Length; i++)
				{
					if (!orderBody.ContainsKey(retNames[i]))
						orderBody.Add(retNames[i], order[i]);
				}

				orders.Add(orderBody);
			}
			
			foreach (var order in orders)
			{
				if (order["ORDER_STATUS"].Contains("EXECUTED") && Convert.ToDecimal(order["AMOUNT_ORIG"]) < 0)
				{
					if (_debug)
						print("Order Status: " + order["ORDER_STATUS"]);
					string symb1 = order["SYMBOL"].Substring(1, 3);
					string symb2 = order["SYMBOL"].Substring(4, 3);
					decimal amount = Convert.ToDecimal(order["AMOUNT_ORIG"]) * -1;
					decimal price = Convert.ToDecimal(order["PRICE_AVG"]);
					long orderTimestamp = Convert.ToInt64(order["MTS_CREATE"]);
					if (!ordersDict.ContainsKey((symb1, symb2)))
					{
						ordersDict[(symb1, symb2)] = new List<(decimal, decimal, long)>();
					}

					ordersDict[(symb1, symb2)].Add((amount, price, orderTimestamp));
				}
			}
			
			// throw new System.NotImplementedException(); //TODO: Unit testing
			return (Status.Success, ordersDict);
		}

		public async Task<Dictionary<string, decimal>> GetOwnedStocks()
		{
			Dictionary<string, decimal> ownedStocks = new Dictionary<string, decimal>();
			string apiUri = "https://api.bitfinex.com/v2/auth/r/wallets";
			HttpResponseMessage response = await getResponseForAuthenticated(apiUri);
			string[][] output = destringifyString<string[][]>(await response.Content.ReadAsStringAsync());

			foreach (var stock in output)
			{
				if (stock[0] == "exchange") //this app will only consider active, tradeable assets
				{
					ownedStocks[stock[1]] = Convert.ToDecimal(stock[2]);
				}
			}

			return ownedStocks;
			// throw new System.NotImplementedException(); //TODO unit test this method first before assuming it's complete
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetBuyOrders()
		{
			var ordersDict = new Dictionary<(string, string), List<(decimal amount, decimal price, string id)>>();
			
			var returnNames = returnNamesGetOrder();
			var retNames = returnNames.Split(',');

			string apiUri = "https://api.bitfinex.com/v2/auth/r/orders/";
			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();
			var ordersBodyReturn = destringifyString<string[][]>(returnString);
			//maybe do custom method to join a 2x string[] as each a key, value pair

			List<Dictionary<string, string>> orders = new List<Dictionary<string, string>>();
			
			foreach (var order in ordersBodyReturn)
			{
				Dictionary<string, string> orderBody = new Dictionary<string, string>();
				for (int i = 0; i < retNames.Length; i++)
				{
					if (!orderBody.ContainsKey(retNames[i]))
						orderBody.Add(retNames[i], order[i]);
				}

				orders.Add(orderBody);
			}
			
			foreach (var order in orders)
			{
				if (Convert.ToDecimal(order["AMOUNT"]) > 0)
				{
					string symb1 = order["SYMBOL"].Substring(1, 3);
					string symb2 = order["SYMBOL"].Substring(4, 3);
					decimal amount = Convert.ToDecimal(order["AMOUNT"]);
					decimal price = Convert.ToDecimal(order["PRICE"]);
					string id = order["ID"];
					if (!ordersDict.ContainsKey((symb1, symb2)))
					{
						ordersDict[(symb1, symb2)] = new List<(decimal, decimal, string)>();
					}

					ordersDict[(symb1, symb2)].Add((amount, price, id));
				}
			}
			
			// throw new System.NotImplementedException(); //TODO: Unit testing
			return ordersDict;
		}

		public async Task<Dictionary<(string symb1, string symb2), List<(decimal amount, decimal price, string id)>>> GetSellOrders()
		{
			var ordersDict = new Dictionary<(string, string), List<(decimal amount, decimal price, string id)>>();
			
			var returnNames = returnNamesGetOrder();
			var retNames = returnNames.Split(',');

			string apiUri = "https://api.bitfinex.com/v2/auth/r/orders/";
			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();
			var ordersBodyReturn = destringifyString<string[][]>(returnString);
			//maybe do custom method to join a 2x string[] as each a key, value pair

			List<Dictionary<string, string>> orders = new List<Dictionary<string, string>>();
			
			foreach (var order in ordersBodyReturn)
			{
				Dictionary<string, string> orderBody = new Dictionary<string, string>();
				for (int i = 0; i < retNames.Length; i++)
				{
					if (!orderBody.ContainsKey(retNames[i]))
						orderBody.Add(retNames[i], order[i]);
				}

				orders.Add(orderBody);
			}
			
			foreach (var order in orders)
			{
				if (Convert.ToDecimal(order["AMOUNT"]) < 0)
				{
					string symb1 = order["SYMBOL"].Substring(1, 3);
					string symb2 = order["SYMBOL"].Substring(4, 3);
					decimal amount = Convert.ToDecimal(order["AMOUNT"]) * -1; //cancel BitFinex API minus sign
					decimal price = Convert.ToDecimal(order["PRICE"]);
					string id = order["ID"];
					if (!ordersDict.ContainsKey((symb1, symb2)))
					{
						ordersDict[(symb1, symb2)] = new List<(decimal, decimal, string)>();
					}

					ordersDict[(symb1, symb2)].Add((amount, price, id));
				}
			}
			
			// throw new System.NotImplementedException(); //TODO: Unit testing
			return ordersDict; //TODO proper return
		}

		public async Task<(string, string)[]> GetSymbols()
		{
			HttpResponseMessage response = await getResponseForPublic("https://api-pub.bitfinex.com/v2/tickers?symbols=ALL");
			string[][] output = destringifyString<string[][]>(await response.Content.ReadAsStringAsync());
			
			List<(string, string)> tuplesList = new List<(string, string)>();

			for (var i = 0; i < output.Length; i++)
			{
				if (output[i][0].StartsWith('t') && output[i][0].Length == 7)
				{
					string symb1 = output[i][0].Substring(1, 3);
					string symb2 = output[i][0].Substring(4, 3);
					var a = (1, 2);
					tuplesList.Add((symb1, symb2));
				}
			}
			tuplesList.Sort();
			return tuplesList.ToArray();
			// throw new System.NotImplementedException(); //TODO: Test
		}

		public async Task<string[]> SymbolStats(string[] symbolPair)
		{
			var timeout = Task.Delay(2000);
			var response = getResponseForPublic($"https://api-pub.bitfinex.com/v2/tickers?symbols=" +
			                                    $"t{symbolPair[0].ToUpper()}{symbolPair[1].ToUpper()}");
			var completeTask = await Task.WhenAny(timeout, response);
			if (completeTask == timeout)
				throw new Exception("API timeout (BitFinexAPI)");
			else if (completeTask != response)
				throw new Exception("Unknown exception (BitFinexAPI)");

			HttpResponseMessage res = response.Result;

			var timeout2 = Task.Delay(2000);
			var response2 = res.Content.ReadAsStringAsync();
			var completeTask2 = await Task.WhenAny(timeout2, response2);
			if (completeTask2 == timeout2)
				throw new Exception("API timeout (BitFinexAPI)");
			else if (completeTask2 != response2)
			{
				throw new Exception("Unknown exception (BitFinexAPI)");
			}

			string[] output = destringifyString<string[][]>(response2.Result)[0];
			string[] symbolStats = new string[9];
			//symbol, current bid, current bid size, ask, ask size, last price, volume, high, low
			symbolStats[0] = output[0].Substring(1);
			symbolStats[1] = output[1];
			symbolStats[2] = output[2];
			symbolStats[3] = output[3];
			symbolStats[4] = output[4];
			symbolStats[5] = output[7];
			symbolStats[6] = output[8];
			symbolStats[7] = output[9];
			symbolStats[8] = output[10];

			return symbolStats;
			// throw new System.NotImplementedException(); //TODO: Test
		}

		public string[] GetSupportedPlatforms()
		{
			throw new System.NotSupportedException("Use ScraperWrapper entry point");
		}

		public Status SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			throw new NotSupportedException("Wrong overload used");
		}

		public (string platform, string apiKey, string secretKey, string passphrase) GetCredentials()
		{
			throw new System.NotSupportedException("Use ScraperWrapper entry point");
		}

		public async Task<bool> TestCredentials()
		{
			string apiUri = "https://api.bitfinex.com/v2/auth/r/orders/hist";
			HttpResponseMessage response = await getResponseForAuthenticated(apiUri, null);
			string returnString = await response.Content.ReadAsStringAsync();
			var ordersBodyReturn = destringifyString<string[][]>(returnString);
			//TODO: Test this more elegantly rather than just catching an error. Analyze the respone code cleanly.

			return true;
		}
	}
}