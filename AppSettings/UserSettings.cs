using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helpers;
using Newtonsoft.Json;

namespace AppSettings
{
	public static class UserSettings
	{
		public static string userSettingsFilename = "settings.json";
		
		private static UserSettingsStruct retrieveUserSettingsStruct()
		{
			Custom.SwitchToAppDataFolder(GetAppDataFolderName());
			if (!File.Exists(userSettingsFilename))
			{
				ReCreateUserSettings();
			}

			if (!File.Exists(userSettingsFilename))
			{
				//impossible state entered into, since prior method call should have created this. Throw exception
				throw new Exception("Unable to create and then access user settings via ReCreateUserSettings()");
			}
			
			string userSettingsJson = File.ReadAllText(userSettingsFilename);
			UserSettingsStruct userSettingsStruct = JsonConvert.DeserializeObject<UserSettingsStruct>(userSettingsJson);
			return userSettingsStruct;
		}
		
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
			UserSettingsStruct userSettingsStruct = retrieveUserSettingsStruct();
				
			if (new[] {"filesystem", "database"}.Contains(userSettingsStruct.storageBackend))
				return userSettingsStruct.storageBackend;
			else
			{
				ReCreateUserSettings();
				if (new[] {"filesystem", "database"}.Contains(userSettingsStruct.storageBackend))
					return userSettingsStruct.storageBackend;
				else
					throw new Exception("User Settings doesn't contain valid storageBackend even after ReCreateUserSettings()");
			}
		}

		public static void ReCreateUserSettings()
		{
			Custom.SwitchToAppDataFolder(GetAppDataFolderName());
			UserSettingsStruct newUserSettingsStruct = new UserSettingsStruct();
			newUserSettingsStruct.storageBackend = "database";
			File.WriteAllText(userSettingsFilename, JsonConvert.SerializeObject(newUserSettingsStruct));
		}
	}

	public struct UserSettingsStruct
	{
		public string storageBackend { get; set; }
	}
}