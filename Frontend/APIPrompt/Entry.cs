using System;
using System.Collections.Generic;
using System.Threading;
using TradeScraper.Scraper;
using TradeScraper.SQLite;
using TradeScraper.TradingAPI;
using static Helpers.Custom;

namespace TradeScraper.Frontend.APIPrompt
{
	public static class Entry
	{
		private static string _platform = "bitfinex";
		private static string _apiKey = "";
		private static string _secretKey = "";
		private static string _passphrase = "";
		//TODO: Get and set these in config file as input from configSet method
		
		private static ITrade tradingAPI;
		private static DatabaseConnector dbConnector;
		
		public static void Start()
		{
			InitializeServices();
			ExecutionLoop.Start(tradingAPI, dbConnector);
			HandleExitKeys(); //ensure this method is applied everywhere, such as on a crash or other exit paths
		}

		public static void HandleExitKeys()
		{
			if (dbConnector.GetRemoveKeysOnExitValue())
				dbConnector.ClearAllPlatformKeys();
		}

		private static void InitializeServices()
		{
			DatabaseLink(); //SQLite database initialization and connection
			TradingAPILink(); //initialize the default trading API

		}

		private static void DatabaseLink()
		{
			dbConnector = new DatabaseConnector();
			dbConnector.InitializeDatabase();
			DatabaseCommunicator.dbConnector = dbConnector;
			InitializeCredentials();
		}

		private static void TradingAPILink()
		{
			tradingAPI = new TradingAPI.TradingAPI(_platform, _apiKey, _secretKey, _passphrase); //level 2 Trading API
		}

		private static void InitializeCredentials()
		{
			(bool success, string platform, string apiKey, string secretKey, string passphrase) = DatabaseCommunicator.GetDefaultCredentialsFromDatabase();
			if (!success)
			{
				(platform, apiKey, secretKey, passphrase) = DatabaseCommunicator.PromptSetUserDefaultCredentials();
			}

			_platform = platform;
			_apiKey = apiKey;
			_secretKey = secretKey;
			_passphrase = passphrase;

		}
		
	}
}