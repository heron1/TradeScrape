using AppSettings;
using SettingsDatabase;

namespace AppSettingsFactory
{
	public static class SupplierIAppSettings
	{
		public static IAppSettings GetDatabaseIAppSettings()
		{
			return new DatabaseConnector();
		}
	}
}