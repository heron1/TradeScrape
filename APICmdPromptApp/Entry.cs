using System;
using AppSettings;
using TradingAPI;

namespace APICmdPromptApp
{
	public static class Entry
	{
		private static string _platform = "bitfinex";
		private static string _apiKey = "";
		private static string _secretKey = "";
		private static string _passphrase = "";

		private static ITrade tradingAPI;
		private static IAppSettings settingsConnector;

		public static void Start()
		{
			InitializeServices();
			ExecutionLoop.Start(tradingAPI, settingsConnector);
			HandleExitKeys(); //ensure this method is applied everywhere, such as on a crash or other exit paths
		}

		public static void HandleExitKeys()
		{
			if (settingsConnector.GetRemoveKeysOnExitValue())
				settingsConnector.ClearAllPlatformKeys();
		}

		private static void InitializeServices()
		{
			DatabaseLink(); //AppSettingsDatabase database initialization and connection
			TradingAPILink(); //initialize the default trading API
		}

		private static void DatabaseLink()
		{
			settingsConnector = SupplierIAppSettings.GetIAppSettings();
			
			SettingsCommunicator.settingsConnector = settingsConnector;
			InitializeCredentials();
		}

		private static void TradingAPILink()
		{
			tradingAPI =
				new TradingAPI.TradingAPI(_platform, _apiKey, _secretKey, _passphrase); //level 2 Trading API
		}

		private static void InitializeCredentials()
		{
			(var success, var platform, var apiKey, var secretKey, var passphrase) =
				SettingsCommunicator.GetDefaultCredentialsFromSettings();
			if (!success) (platform, apiKey, secretKey, passphrase) = SettingsCommunicator.PromptSetUserDefaultCredentials();

			_platform = platform;
			_apiKey = apiKey;
			_secretKey = secretKey;
			_passphrase = passphrase;
		}
	}
}