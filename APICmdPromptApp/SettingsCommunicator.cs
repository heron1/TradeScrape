using System;
using AppSettings;
using static Helpers.Custom;

namespace APICmdPromptApp
{
	public static class SettingsCommunicator
	{
		public static IAppSettings settingsConnector;

		public static (bool success, string platform, string apiKey, string secretKey, string passphrase)
			GetDefaultCredentialsFromDatabase()
		{
			return settingsConnector.GetDefaultCredentials();
		}

		public static (bool success, string apiKey, string secretKey, string passphrase) PromptSetUserPlatformCredentials(
			string platform)
		{
			var apiKey = "";
			var secretKey = "";
			var passphrase = "";

			var prompter = "Please enter your API Key for " + platform;
			print(prompter);
			Console.Write("API Key: ");
			apiKey = Console.ReadLine();
			Console.Write("Secret Key: ");
			secretKey = Console.ReadLine();
			Console.Write("Passphrase (if applicable): ");
			passphrase = Console.ReadLine();

			var success = SetCredentials(platform, apiKey, secretKey, passphrase);
			if (!success)
				return (false, "", "", "");
			else
				return (true, apiKey, secretKey, passphrase);
		}

		public static (string platform, string apiKey, string secretKey, string passphrase)
			PromptSetUserDefaultCredentials()
		{
			var platform = "";
			var apiKey = "";
			var secretKey = "";
			var passphrase = "";

			var supportedPlatforms = Settings.GetSupportedPlatforms();
			var prompter =
				"Default trading platform details have not been set. You will now be guided to enter your desired platform," +
				" your API key, and your secret key. All three must be entered exactly correct, with case sensitivity.\nHere is a list of available" +
				" platforms: ";
			foreach (var platform_ in supportedPlatforms) prompter += platform_ + " ";

			print(prompter);
			Console.Write("Platform: ");
			platform = Console.ReadLine();
			Console.Write("API Key: ");
			apiKey = Console.ReadLine();
			Console.Write("Secret Key: ");
			secretKey = Console.ReadLine();
			Console.Write("Passphrase (if applicable): ");
			passphrase = Console.ReadLine();

			var success = SetCredentials(platform, apiKey, secretKey, passphrase);
			while (!success)
			{
				print("Platform not accepted. Please try again");
				Console.Write("Platform: ");
				platform = Console.ReadLine();
				Console.Write("API Key: ");
				apiKey = Console.ReadLine();
				Console.Write("Secret Key: ");
				secretKey = Console.ReadLine();
				Console.Write("Passphrase (if applicable): ");
				secretKey = Console.ReadLine();
				success = SetCredentials(platform, apiKey, secretKey, passphrase);
			}

			success = ChangeDefaultPlatform(platform);
			while (!success)
			{
				print("The platform you entered doesn't exist. Please try again");
				Console.Write("Platform: ");
				platform = Console.ReadLine();
				Console.Write("API Key: ");
				apiKey = Console.ReadLine();
				Console.Write("Secret Key: ");
				secretKey = Console.ReadLine();
				Console.Write("Passphrase (if applicable): ");
				secretKey = Console.ReadLine();
				success = ChangeDefaultPlatform(platform);
			}

			(success, platform, apiKey, secretKey, passphrase) = GetDefaultCredentialsFromDatabase();
			while (!success)
			{
				print("Error retrieving the platform. Please ensure entered platform is supported");
				Console.Write("Platform: ");
				platform = Console.ReadLine();
				SetCredentials(platform, apiKey, secretKey, passphrase);
				settingsConnector.SetDefaultPlatform(platform);
				(success, platform, apiKey, secretKey, passphrase) = GetDefaultCredentialsFromDatabase();
			}

			return (platform, apiKey, secretKey, passphrase);
		}

		public static bool ChangeDefaultPlatform(string platform)
		{
			return settingsConnector.SetDefaultPlatform(platform);
		}

		public static bool SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			return settingsConnector.SetCredentials(platform, apiKey, secretKey, passphrase);
		}
	}
}