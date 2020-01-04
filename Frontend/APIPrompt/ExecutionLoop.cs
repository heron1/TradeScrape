using System;
using System.Collections.Generic;
using System.Threading;
using Helpers;
using TradeScraper.Scraper;
using TradeScraper.SQLite;
using TradeScraper.TradingAPI;

namespace TradeScraper.Frontend.APIPrompt
{
	internal static class ExecutionLoop
	{
		private static ITrade tradingAPI;
		private static DatabaseConnector dbConnector;
		
		public static void Start(ITrade tradingApi, DatabaseConnector dbConnection)
		{
			tradingAPI = tradingApi;
			dbConnector = dbConnection;
			bool exit = false;
			while (!exit)
			{
				try
				{
					Thread.Sleep(10);
					Console.Write("Input (" + GetCurrentPlatform() + "): ");
					string userInput = Console.ReadLine();
					if (userInput == "exit")
						exit = true;

					else if (userInput == "balance")
					{
						ExecuteGetOwnedStocks();
					}
					else if (userInput == "getsymbols")
					{
						ExecuteGetSymbols();
					}
					else if (userInput.StartsWith("symbolstats "))
					{
						ExecuteSymbolStats(userInput);
					}
					else if (userInput.StartsWith("buy ")) //buy symb1 symb2 amount price
					{
						ExecuteBuyOrder(userInput);
					}
					else if (userInput.StartsWith("sell "))
					{
						ExecuteSellOrder(userInput);
					}

					else if (userInput == "buyorders")
					{
						ExecuteGetBuyOrders();
					}
					else if (userInput == "buyordershistall")
					{
						ExecuteGetBuyOrderHistoryAll();
					}
					else if (userInput.Contains("buyordershist "))
					{
						ExecuteGetBuyOrderHistoryBySymbolPair(userInput);
					}
					else if (userInput == "sellordershistall")
					{
						ExecuteGetSellOrderHistoryAll();
					}
					else if (userInput.Contains("sellordershist"))
					{
						ExecuteGetSellOrderHistoryBySymbolPair(userInput);
					}
					else if (userInput == "sellorders")
					{
						ExecuteGetSellOrders();
					}
					else if (userInput == "cancelall")
					{
						ExecuteCancelAll();
					}
					else if (userInput.Contains("cancel"))
					{
						ExecuteCancelOrderByID(userInput);
					}
					else if (userInput == ("getplatforms"))
					{
						GetPlatforms();
					}
					else if (userInput.Contains("setplatform ")) //enter desired platform
					{
						SetPlatform(userInput);
					}
					else if (userInput == ("setcreds"))
					{
						SetCreds();
					}
					else if (userInput == ("getcreds")) //credentials for current platform
					{
						ShowCreds();
					}
					else if (userInput == ("getcredsall")) //credentials for all platforms
					{
						ShowCredsAll();
					}
					else if (userInput.Contains("clearkeysonexit ")) //set to true or false
					{
						ClearKeysOnExit(userInput);
					}
					else if (userInput == "help") //set to true or false
					{
						PrintInstructions();
					}

					else
					{
						Custom.print("Query not recognized");
					}

				}

				catch (Exception e) when (e.InnerException is FormatException || e is FormatException)
				{
					Custom.print("Query is in invalid format");
				}

				catch (Exception e) when (e.InnerException is NullReferenceException || e is NullReferenceException)
				{
					Custom.print("Exception encountered. Type 'setcreds' to reset your API credentials");
				}

				catch (Exception e)
				{
					Custom.print("Need to handle this exception.." + e);
				}
			}
		}

		private static void ClearKeysOnExit(string userInput)
		{
			string clearBool = userInput.Split(" ")[1];
			if (clearBool == "true")
			{
				dbConnector.SetRemoveKeysOnExit(true);
				Custom.print("All API and Secret keys will be automatically removed every time the program closes.");
			}
			else if (clearBool == "false")
			{
				dbConnector.SetRemoveKeysOnExit(false);
				Custom.print("All API and Secrets keys will be remembered every time the program closes.");
			}
			else
			{
				Custom.print("Command in invalid format, bool value required. Eg: 'clearkeysonexit true'");
			}
		}

		private static void ShowCredsAll()
		{
			var keyList = dbConnector.GetAllKeys();
			foreach (var entry in keyList)
			{
				Custom.print("Platform: " + entry.platform);
				Custom.print("API Key: " + entry.apiKey);
				Custom.print("Secret Key: " + entry.secretKey + "\n");
			}
		}

		private static void ShowCreds()
		{
			var creds = tradingAPI.GetCredentials();
			Custom.print("Platform: " + creds.platform);
			Custom.print("API Key: " + creds.apiKey);
			Custom.print("Secret Key: " + creds.secretKey);
			Custom.print("Passphrase: " + creds.passphrase);
		}

		private static void SetCreds()
		{
			var creds = tradingAPI.GetCredentials();
			DatabaseCommunicator.PromptSetUserPlatformCredentials(creds.platform);
			SetPlatform("blah " + creds.platform);

			bool successfulCredentials = false;

			try
			{
				successfulCredentials = tradingAPI.TestCredentials().Result;
			}
			catch
			{
				Custom.print("Error encountered on platform when attempting to test the entered credentials");
			}
			
			if (!successfulCredentials)
			{
				Custom.print("WARNING: The credentials you entered failed upon testing with " + creds.platform +
				             ". Type 'setcreds' to reset your credentials on this platform, or change to another platform. All other" +
				             " commands will likely fail to be accepted by the platform due to the invalid credentials.");
			}
			else
			{
				Custom.print("Successfully updated credentials. Platform accepted new credentials upon testing.");
			}
		}

		private static void GetPlatforms()
		{
			Custom.print("Available platforms are -");
			foreach (var platform in dbConnector.GetSupportedPlatforms())
				Custom.print(platform);
		}

		private static string GetCurrentPlatform()
		{
			var creds = tradingAPI.GetCredentials();
			return creds.platform;
		}

		private static void SetPlatform(string userInput)
		{
			string platform = userInput.Split(" ")[1];
			if (!dbConnector.GetSupportedPlatforms().Contains(platform))
			{
				Custom.print("Specified platform is not known");
				return;
			}

			(bool success, string apiKey, string secretKey, string passphrase) = dbConnector.GetCredentials(platform);
			if (!success)
			{
				bool successRet;
				(successRet, apiKey, secretKey, passphrase) = DatabaseCommunicator.PromptSetUserPlatformCredentials(platform);
				if (!successRet)
				{
					Custom.print("Selected platform did not accept your credentials. Reverting to previous platform");
					return;
				}
			}
			
			tradingAPI = new TradingAPI.TradingAPI(platform, apiKey, secretKey, passphrase);
			DatabaseCommunicator.ChangeDefaultPlatform(platform);
			Custom.print("Succesfully switched to " + platform);
			
		}

		private static void PrintInstructions()
		{
			string printThis = "The following commands are available for all platforms: \n" +
			                   "IMMDIATE TRADING Commands - \n" +
			                   "balance (eg: 'balance'): Retrieve current balance of owned symbols \n" +
			                   "buy symbol1 symbol2 amount price (eg: 'buy btc usd 33 7280): Open a buy order at the specified amount and price. \n" +
			                   "sell symbol1 symbol2 amount price (eg: 'sell btc usd 33 7200): Open a sell order at the specified amount and price \n" +
			                   "cancel orderID (eg: cancel 1234567): Cancels the open buy/sell order for the given orderID \n" +
			                   "cancelall (eg: cancelall): Cancels absolutely all open buy and sell orders on the platform \n" +
			                   "buyorders (eg: 'buyorders'): Retrieve your current, open buy orders for all symbols \n" +
			                   "sellorders (eg: 'sellorders'): Retrieve your current, open sell orders for all symbols \n" +
			                   "GENERAL TRADING Commands - \n" +
			                   "buyordershist symbolPair1 symbolPair2 (eg:'buyordershist btc usd'): Returns the current history of recent buy orders for the symbol pair \n" +
			                   "sellordershist symbolPair1 symbolPair2 (eg:'sellordershist btc usd'): Returns the current history of recent sell orders for the symbol pair \n" +
			                   "buyordershistall (eg: 'buyordershistall'): Retrieves the current history of recent buy orders for all symbols \n" +
			                   "sellordershistall (eg: 'sellordershistall'): Retrieves the current history of recent sell orders for all symbols \n" +
			                   "getsymbols (eg: 'getsymbols'): Retrieves the buy/sell order symbol pairs for all available symbols on the current platform \n" +
			                   "symbolstats symbolPair1 symbolPair2 (eg: 'symbolstats btc usd'): Gives current market statistics for the given symbol pair \n" +
			                   "GENERAL Commands - \n" +
			                   "getplatforms (eg: 'getplatforms'): Returns a list of all currently supported platforms by this program. \n" +
			                   "setplatform platform (eg: 'setplatform bitfinex'): Changes the current platform to the supported platform given \n" +
			                   "getcreds (eg: 'getcreds'): Returns your credentials (API Key, Secret Key) for the current, active platform \n" +
			                   "getcredsall (eg: 'getcredsall'): Returns your credentials (API Key, Secret Key) for all supported platforms \n" +
			                   "setcreds (eg: 'setcreds'): Allows you to re-set the credentials (API Key, Secret Key) for the current, active platform \n" +
			                   "clearkeysonexit bool (eg: clearkeysonexit true): Every time you exit the program, delete all your API Keys and Secret Keys for all platforms. If false, save them (default: false). \n" +
			                   "exit (eg: 'exit'): Exit the program \n" +
			                   "help (eg: 'help'): This";

			Custom.print(printThis);
		}

		private static void ExecuteCancelOrderByID(string userInput)
		{
			try
			{
				string id = userInput.Split(" ")[1];
				var output = tradingAPI.CancelOrderByID(id).Result;
				Custom.print(output);
			}
			catch (Exception e) when (e.InnerException is IndexOutOfRangeException || e is IndexOutOfRangeException)
			{
				Custom.print("Error: Invalid query. Pleasure ensure query is in the format of 'cancel orderID'. " +
				             "Eg: 'cancel 33392093'. Alternatively, simply type 'cancelall' to cancel all open orders.");
			}
		}

		private static void ExecuteCancelAll()
		{
			var output = tradingAPI.CancelAll().Result;
			Custom.print(output);
		}

		private static void ExecuteGetSellOrderHistoryBySymbolPair(string userInput)
		{
			try
			{
				string[] symbs = userInput.Split(" ");
				var output =
					tradingAPI.GetSellOrderHistoryBySymbolPair((symbs[1], symbs[2])).Result;
				if (output.Item1 == Status.Failure_InvalidSymbol)
				{
					Custom.print("Invalid symbol pair entered. Please type 'getsymbols' for a list of valid symbol pairs");
					return;
				}
				else if (output.Item1 != Status.Success)
				{
					Custom.print("Unknown error encountered.");
					return;
				}

				Custom.print("Sell Orders History -");
				foreach (var order in output.Item2)
				{
					foreach (var uniqueOrder in order.Value)
					{
						Custom.print($"Symbol Pair: {order.Key.symb1}|{order.Key.symb2}. " +
						             $"Amount: {uniqueOrder.amount}. Price: {uniqueOrder.avgPrice}. " +
						             $"Date/Time: {DateTimeOffset.FromUnixTimeMilliseconds(uniqueOrder.timestampUnixEpoch)}");
					}
				}
			}
			catch (Exception e) when (e.InnerException is IndexOutOfRangeException || e is IndexOutOfRangeException)
			{
				Custom.print("Error: Invalid query. Pleasure ensure query is in the format of 'sellordershist symbol1 symbol2. " +
				             "Eg: 'sellordershist btc usd'. Use 'getsymbols' for a list of valid symbol pairs. Alternatively use 'sellordershist' " +
				             "alone to get all historical sell orders for all symbol pairs.");
			}
		}

		private static void ExecuteGetSellOrderHistoryAll()
		{
			var output = tradingAPI.GetSellOrderHistoryAll().Result;
			Custom.print("Sell Orders History -");
			foreach (var order in output)
			{
				foreach (var uniqueOrder in order.Value)
				{
					Custom.print($"Symbol Pair: {order.Key.symb1}|{order.Key.symb2}. " +
					             $"Amount: {uniqueOrder.amount}. Price: {uniqueOrder.avgPrice}. " +
					             $"Date/Time: {DateTimeOffset.FromUnixTimeMilliseconds(uniqueOrder.timestampUnixEpoch)}");
				}
			}
		}

		private static void ExecuteGetBuyOrderHistoryBySymbolPair(string userInput)
		{
			try
			{
				string[] symbs = userInput.Split(" ");
				var output =
					tradingAPI.GetBuyOrderHistoryBySymbolPair((symbs[1], symbs[2])).Result;
				if (output.Item1 == Status.Failure_InvalidSymbol)
				{
					Custom.print("Invalid symbol pair entered. Please type 'getsymbols' for a list of valid symbol pairs");
					return;
				}
				else if (output.Item1 != Status.Success)
				{
					Custom.print("Unknown error encountered.");
					return;
				}
				Custom.print("Buy Orders History -");
				foreach (var order in output.Item2)
				{
					foreach (var uniqueOrder in order.Value)
					{
						Custom.print($"Symbol Pair: {order.Key.symb1}|{order.Key.symb2}. " +
						             $"Amount: {uniqueOrder.amount}. Price: {uniqueOrder.avgPrice}. " +
						             $"Date/Time: {DateTimeOffset.FromUnixTimeMilliseconds(uniqueOrder.timestampUnixEpoch)}");
					}
				}
			}
			catch (Exception e) when (e.InnerException is IndexOutOfRangeException || e is IndexOutOfRangeException)
			{
				Custom.print("Error: Invalid query. Pleasure ensure query is in the format of 'buyordershist symbol1 symbol2. " +
				             "Eg: 'buyordershist btc usd'. Use 'getsymbols' for a list of valid symbol pairs. Alternatively use 'buyordershist' " +
				             "alone to get all historical buy orders for all symbol pairs.");
			}
		}

		private static void ExecuteGetBuyOrderHistoryAll()
		{
			var output = tradingAPI.GetBuyOrderHistoryAll().Result;
			Custom.print("Buy Orders History -");
			foreach (var order in output)
			{
				foreach (var uniqueOrder in order.Value)
				{
					Custom.print($"Symbol Pair: {order.Key.symb1}|{order.Key.symb2}. " +
					             $"Amount: {uniqueOrder.amount}. Price: {uniqueOrder.avgPrice}. " +
					             $"Date/Time: {DateTimeOffset.FromUnixTimeMilliseconds(uniqueOrder.timestampUnixEpoch)}");
				}
			}
		}

		private static void ExecuteGetSellOrders()
		{
			var output = tradingAPI.GetSellOrders().Result;
			Custom.print("Sell Orders -");
			foreach (var order in output)
			{
				foreach (var uniqueOrder in order.Value)
				{
					Custom.print($"Symbol Pair: {order.Key.symb1}|{order.Key.symb2}. " +
					             $"Amount: {uniqueOrder.amount}. Price: {uniqueOrder.price}. ID: {uniqueOrder.id}");
				}
			}
		}

		private static void ExecuteGetBuyOrders()
		{
			var output = tradingAPI.GetBuyOrders().Result;
			Custom.print("Buy Orders -");
			foreach (var order in output)
			{
				foreach (var uniqueOrder in order.Value)
				{
					Custom.print($"Symbol Pair: {order.Key.symb1}|{order.Key.symb2}. " +
					             $"Amount: {uniqueOrder.amount}. Price: {uniqueOrder.price}. ID: {uniqueOrder.id}");
				}
			}
		}

		private static void ExecuteBuyOrder(string userInput)
		{
			try
			{
				userInput = userInput.ToUpper();
				string[] inputSplit = userInput.Split(" ");
				var output = tradingAPI.BuyOrder(new string[] {inputSplit[1], inputSplit[2]},
					Convert.ToDecimal(inputSplit[3]), Convert.ToDecimal(inputSplit[4])).Result;
				if (output == Status.Failure_InvalidOrderBalance)
				{
					Custom.print("FAILURE. Insufficient balance for order.");
				}
				else if (output == Status.Failure_InvalidOrderMinimumAmount)
				{
					Custom.print("FAILURE. Invalid minimum amount entered for order");
				}
				else if (output == Status.Success)
					Custom.print("SUCCESS. Order submitted");
				else if (output == Status.Failure_Undefined)
					Custom.print("Unknown error encountered. Order failed.");
				else
				{
					throw new Exception("Critical error: Illegal state entered on ExecuteBuyOrder. Please report to developer");
				}
			}
			catch (Exception e) when (e.InnerException is IndexOutOfRangeException || e is IndexOutOfRangeException)
			{
				Custom.print("Error: Invalid query. Pleasure ensure query is in the format of 'buy symbol1 symbol2 amount price" +
				             "Eg: 'buy btc usd 5 8000'. Use 'getsymbols' for a list of valid symbol pairs.");
			}
		}

		private static void ExecuteSellOrder(string userInput)
		{
			try
			{
				userInput = userInput.ToUpper();
				string[] inputSplit = userInput.Split(" ");
				var output = tradingAPI.SellOrder(new string[] {inputSplit[1], inputSplit[2]},
					Convert.ToDecimal(inputSplit[3]), Convert.ToDecimal(inputSplit[4])).Result;
				if (output == Status.Failure_InvalidOrderBalance)
				{
					Custom.print("FAILURE. Insufficient balance for order.");
				}
				else if (output == Status.Failure_InvalidOrderMinimumAmount)
				{
					Custom.print("FAILURE. Invalid minimum amount entered for order");
				}
				else if (output == Status.Failure_InvalidSymbol)
				{
					Custom.print("FAILURE. Invalid symbol pair");
				}
				else if (output == Status.Success)
					Custom.print("SUCCESS. Order submitted");
				else if (output == Status.Failure_Undefined)
					Custom.print("Unknown error encountered. Order failed.");
				else
				{
					throw new Exception("Critical error: Illegal state entered on ExecuteBuyOrder. Please report to developer");
				}
					
			}
			catch (Exception e) when (e.InnerException is IndexOutOfRangeException || e is IndexOutOfRangeException)
			{
				Custom.print("Error: Invalid query. Pleasure ensure query is in the format of 'sell symbol1 symbol2 amount price" +
				             "Eg: 'sell btc usd 5 8000'. Use 'getsymbols' for a list of valid symbol pairs.");
			}
		}

		private static void ExecuteGetOwnedStocks()
		{
			var output = tradingAPI.GetOwnedStocks().Result;
			Custom.PrintCollection(output);
		}

		private static void ExecuteGetSymbols()
		{
			var output = tradingAPI.GetSymbols().Result;
			Custom.PrintCollection(output);
		}

		private static void ExecuteSymbolStats(string userInput)
		{
			userInput = userInput.ToUpper();
			string[] inputSplit;
			try
			{
				inputSplit = userInput.Split(" ");
			}
			catch (Exception e)
			{
				Custom.print("Error: Query incorrectly formatted.");
				return;
			}
			
			string[] output;
			try
			{
				output = tradingAPI.SymbolStats(new string[] {inputSplit[1], inputSplit[2]}).Result;
				// symbolPair, current bid, current bid size, ask, ask size, last price, volume, high, low
				string[] names = new string[] {"SymbolPair", "Current Bid", "Bid Size", "Current Ask", "Ask Size", "Last Price", 
					"Volume 24h", "High 24h", "Low 24h"};
				for (int i = 0; i < output.Length; i++)
				{
					Custom.print($"{names[i]}: {output[i]}");
				}
			}
			catch (Exception e) when (e.InnerException is IndexOutOfRangeException || e is IndexOutOfRangeException
			                          || e.InnerException is KeyNotFoundException || e is KeyNotFoundException)
			{
				Custom.print("Error: Invalid query. Pleasure ensure two valid symbols are entered. " +
				             "Eg: 'symbolstats btc usd'. Use 'getsymbols' for a list of valid symbol pairs.");
				return;
			}
		}
	}
}