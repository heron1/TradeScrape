using System.Collections.Generic;
using System.IO;
using Helpers;
using Newtonsoft.Json;

namespace AppSettings
{
	public static class Settings
	{
		public static string userSettingsFilename = "settings.json";
		
		public static List<string> GetSupportedPlatforms()
		{
			return new List<string>()
			{
				"bitfinex", "kucoin"
			};
		}

		public static string GetAppDataFolderName()
		{
			return "TradeScrape";
		}
		
		public static string RetrieveBackendSettingsStorage()
		{
			Custom.SwitchToAppDataFolder(GetAppDataFolderName());
			if (File.Exists(userSettingsFilename))
			{
				string userSettingsJson = File.ReadAllText(Settings.userSettingsFilename);
				UserSettings userSettings = JsonConvert.DeserializeObject<UserSettings>(userSettingsJson);
				
				if (userSettings.storageBackend == "filesystem")
				{
					return "filesystem";
				}
				else if (userSettings.storageBackend == "database")
				{
					return "database";
				}
				else
				{
					ReCreateUserSettings();
					return "database";
				}
			}

			ReCreateUserSettings();
			return "database";
		}

		public static void ReCreateUserSettings()
		{
			Custom.SwitchToAppDataFolder(GetAppDataFolderName());
			UserSettings newUserSettings = new UserSettings();
			newUserSettings.storageBackend = "database";
			File.WriteAllText(userSettingsFilename, JsonConvert.SerializeObject(newUserSettings));
		}
	}
	
	public struct UserSettings
	{
		public string storageBackend { get; set; }
	}
}