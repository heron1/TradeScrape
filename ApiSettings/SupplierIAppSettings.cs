using AppSettings._AppSettingsDatabase;
using AppSettings._AppSettingsFileSystem;

namespace AppSettings
{
	public static class SupplierIAppSettings
	{
		//TODO General factory retrieval here. Factory itself is responsible for reading the settings file.

		private static IAppSettings GetDatabaseIAppSettings()
		{
			return new DatabaseConnector();
		}

		private static IAppSettings GetFileSystemIAppSettings()
		{
			return new FileSystemConnector();
		}

		public static IAppSettings GetIAppSettings()
		{
			string storageBackend = UserSettings.RetrieveBackendSettingsStorage();
			if (storageBackend == "database")
				return GetDatabaseIAppSettings();
			else if (storageBackend == "filesystem")
				return GetFileSystemIAppSettings();
			else
			{
				UserSettings.ReCreateUserSettings();
				return GetDatabaseIAppSettings();
			}
		}
		
	}
}