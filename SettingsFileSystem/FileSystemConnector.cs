using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Helpers.Custom;
using AppSettings;
using Newtonsoft.Json;

namespace SettingsFileSystem
{
	//TODO Dynamically check for or if not exist, create, a JSON file with desired settings
	public class FileSystemConnector : IAppSettings
	{
		private string filename = "settingsInternal.json";
		private const int version = 1;
		
		public FileSystemConnector()
		{
			SwitchToAppDataFolder(Settings.GetAppDataFolderName());

			bool fileIntegrityPassed = false;
			
			if (File.Exists(filename))
			{
				using (var fs = File.Open(filename, FileMode.Open))
				{
					if (ensureJsonIntegrity(fs))
					{
						fileIntegrityPassed = true;
					}
				}
			}
			
			if (!fileIntegrityPassed)
			{
				createOrOverWriteSettingsFile();
			}
		}

		private bool ensureJsonIntegrity(FileStream fs)
		{
			using (var sr = new StreamReader(fs))
			{
				string jsonContent = sr.ReadToEnd();
				try
				{
					JsonConvert.DeserializeObject<JsonSettingsFormat>(jsonContent);
					return true;
				}
				catch (JsonReaderException e)
				{
					LogException(e);
				}
				
			}
			
			return false;
		}

		private void createOrOverWriteSettingsFile()
		{
			JsonSettingsFormat jsonSettingsFormat = new JsonSettingsFormat();
			
			jsonSettingsFormat.credentials = new Dictionary<string, List<string>>();
			foreach (var platform in Settings.GetSupportedPlatforms())
			{
				List<string> creds = new List<string>(3) {"", "", ""}; //apikey, secretkey, passphrase
				jsonSettingsFormat.credentials.Add(platform, creds);
			}
			
			List<string> meta = new List<string>(3) {"", "0", version.ToString()}; //default_platform, remove_keys_on_exit, version
			jsonSettingsFormat.meta = meta;

			string jsonFileString = JsonConvert.SerializeObject(jsonSettingsFormat);
			File.WriteAllText(filename, jsonFileString);
		}

		private JsonSettingsFormat getJsonSettings()
		{
			string jsonFile = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<JsonSettingsFormat>(jsonFile);
		}

		private void setJsonSettings(JsonSettingsFormat settings)
		{
			string jsonSettings = JsonConvert.SerializeObject(settings);
			File.WriteAllText(filename, jsonSettings);
		}
		
		public (bool success, string apiKey, string secretKey, string passphrase) GetCredentials(string platform)
		{
			JsonSettingsFormat settings = getJsonSettings();
			Dictionary<string, List<string>> credentials = settings.credentials;
			string apiKey = credentials[platform][0];
			string secretKey = credentials[platform][1];
			string passphrase = credentials[platform][2];

			return (true, apiKey, secretKey, passphrase);
		}

		public (bool success, string platform, string apiKey, string secretKey, string passphrase) GetDefaultCredentials()
		{
			JsonSettingsFormat settings = getJsonSettings();
			string platform = settings.meta[0]; //default platform is located in 1st column
			List<string> creds = settings.credentials[platform];
			
			return (true, platform, creds[0], creds[1], creds[2]);
		}

		public bool SetCredentials(string platform, string apiKey, string secretKey, string passphrase)
		{
			JsonSettingsFormat settings = getJsonSettings();
			settings.credentials[platform] = new List<string>() {apiKey, secretKey, passphrase};
			setJsonSettings(settings);

			return true;
		}

		public bool SetDefaultPlatform(string platform)
		{
			JsonSettingsFormat settings = getJsonSettings();
			settings.meta[0] = platform;
			setJsonSettings(settings);

			return true;
		}

		public List<(string platform, string apiKey, string secretKey, string passphrase)> GetAllKeys()
		{
			var keyList = new List<(string platform, string apiKey, string secretKey, string passphrase)>();
			
			JsonSettingsFormat settings = getJsonSettings();
			foreach (var platform in settings.credentials)
			{
				keyList.Add((platform.Key, platform.Value[0], platform.Value[1], platform.Value[2]));
			}

			return keyList;
		}

		public bool SetRemoveKeysOnExit(bool value)
		{
			JsonSettingsFormat settings = getJsonSettings();
			settings.meta[1] = value.ToString();
			setJsonSettings(settings);

			return true;
		}

		public bool GetRemoveKeysOnExitValue()
		{
			JsonSettingsFormat settings = getJsonSettings();
			return Convert.ToBoolean(settings.meta[1]);
		}

		public void ClearAllPlatformKeys()
		{
			JsonSettingsFormat settings = getJsonSettings();
			foreach (var platform in settings.credentials.ToList())
			{
				settings.credentials[platform.Key] = new List<string>() {"", "", ""};
			}
			setJsonSettings(settings);
		}
	}

	public struct JsonSettingsFormat
	{
		public Dictionary<string, List<string>> credentials { get; set; }
		public List<string> meta { get; set; }
		
	}
}