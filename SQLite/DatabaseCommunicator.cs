using System;
using System.Collections.Generic;
using System.Data.SQLite;
using static Helpers.Custom;

namespace TradeScraper.SQLite
{
	public static class DatabaseCommunicator
	{
		public static DatabaseConnector dbConnector;

		public static (bool success, string platform, string apiKey, string secretKey, string passphrase) GetDefaultCredentialsFromDatabase()
		{
			return dbConnector.GetDefaultCredentials();
		}

		public static (bool success, string apiKey, string secretKey, string passphrase) PromptSetUserPlatformCredentials(string platform)
		{
			string apiKey = "";
			string secretKey = "";
			string passphrase = "";

			string prompter = "Please enter your API Key for " + platform;
			print(prompter);
			Console.Write("API Key: ");
			apiKey = Console.ReadLine();
			Console.Write("Secret Key: ");
			secretKey = Console.ReadLine();
			Console.Write("Passphrase (if applicable): ");
			passphrase = Console.ReadLine();
			
			bool success = SetCredentials(platform, apiKey, secretKey, passphrase);
			if (!success)
			{
				return (false, "", "", "");
			}
			else
			{
				return (true, apiKey, secretKey, passphrase);
			}

		}

		public static (string platform, string apiKey, string secretKey, string passphrase) PromptSetUserDefaultCredentials()
		{
			string platform = "";
			string apiKey = "";
			string secretKey = "";
			string passphrase = "";

			List<string> supportedPlatforms = dbConnector.GetSupportedPlatforms();
			string prompter =
				"Default trading platform details have not been set. You will now be guided to enter your desired platform," +
				" your API key, and your secret key. All three must be entered exactly correct, with case sensitivity.\nHere is a list of available" +
				" platforms: ";
			foreach (var platform_ in supportedPlatforms)
			{
				prompter += platform_ + " ";
			}

			print(prompter);
			Console.Write("Platform: ");
			platform = Console.ReadLine();
			Console.Write("API Key: ");
			apiKey = Console.ReadLine();
			Console.Write("Secret Key: ");
			secretKey = Console.ReadLine();
			Console.Write("Passphrase (if applicable): ");
			passphrase = Console.ReadLine();

			bool success = SetCredentials(platform, apiKey, secretKey, passphrase);
			while (!success)
			{
				print($"Platform not accepted. Please try again");
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
				dbConnector.SetDefaultPlatform(platform);
				(success, platform, apiKey, secretKey, passphrase) = GetDefaultCredentialsFromDatabase();
			}

			return (platform, apiKey, secretKey, passphrase);
		}

		public static bool DatabaseInconsistentError()
		{
			throw new Exception("Unable to properly work with the database. Please report to developer.");
		}

		public static bool ChangeDefaultPlatform(string platform)
		{
			return dbConnector.SetDefaultPlatform(platform);
		}

		public static bool SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			return dbConnector.SetCredentials(platform, apiKey, secretKey, passphrase);
		}
		
		
		
		/*
		 Credentials(platform PK, apiKey, secretKey)
		 Meta(default_platform, removeKeysOnExit) //RESTRICT TO ONE ROW ONLY TO STORE APPLICATION SETTINGS. Ensure table columns easily extendable
		 
		 default_platform: Here, when a platform is changed, it's automatically moved to the default_platform. 
		 When the application starts, it checks the default_platform, then applies its credentials. A GUI may allow more advanced settings 
		 one day, but for now this will suffice.
		 
		 removeKeysOnExit: If true, clear all database Credentials information upon exit, including keys. 
		 Does not remove Meta information, including this setting. Must be manually turned off again to remember keys on exit.
		 Ensure this is done in the Program "finally" clause for exceptions, and also upon exit.
		 See also: https://social.msdn.microsoft.com/Forums/vstudio/en-US/2bfc28cf-56d1-4cdf-bbcc-1f4210a480cc/cleanup-on-application-exit?forum=clr
		 Test multiple scenarios to ensure key clear 
		 */
	}
}