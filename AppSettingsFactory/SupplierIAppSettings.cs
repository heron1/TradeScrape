using System.IO;
using Helpers;
using AppSettings;
using Newtonsoft.Json;
using SettingsDatabase;
using SettingsFileSystem;

namespace AppSettingsFactory
{
	public static class SupplierIAppSettings
	{
		//TODO General factory retrieval here. Factory itself is responsible for reading the settings file.

		public static IAppSettings GetDatabaseIAppSettings()
		{
			return new DatabaseConnector();
		}

		public static IAppSettings GetFileSystemIAppSettings()
		{
			return new FileSystemConnector();
		}

		public static IAppSettings GetIAppSettingsStorage()
		{
			string storageBackend = Settings.RetrieveBackendSettingsStorage();
			if (storageBackend == "database")
				return GetDatabaseIAppSettings();
			else if (storageBackend == "filesystem")
				return GetFileSystemIAppSettings();
			else
			{
				Settings.ReCreateUserSettings();
				return GetDatabaseIAppSettings();
			}
		}
		
	}
}