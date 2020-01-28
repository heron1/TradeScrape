using System.Collections.Generic;
using AppSettings;
using AppSettingsFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TradeScrape_Unit_Tests.InternalSettingsIntegrations
{
	[TestClass]
	public class SettingsDatabaseTest
	{
		//bug: Some of the "set" unit tests may fail (only 1 will pass). This seems to be a bug with Sqlite. Refactoring with "using" everywhere in
		//DatabaseConnector.cs, and using locks, didn't change anything. Apparently this is a common issue. If it's important, fix it, but for now
		//don't waste anymore time on it. Database functionality seems to work fine in live environment outside integration testing.
		
		private string platform = "bitfinex";
		
		private IAppSettings getSettings()
		{
			return SupplierIAppSettings.GetDatabaseIAppSettings();
		}
		
		[TestMethod]
		public void GetCredentialsTest()
		{
			//Arrange
			IAppSettings settings = getSettings();
			
			//Act
			(bool success, string apiKey, string secretKey, string passphrase) = settings.GetCredentials(platform);
			
			//Assert
			Assert.IsTrue(success);
		}
		
		[TestMethod]
		public void GetDefaultCredentialsTest()
		{
			//Arrange
			IAppSettings settings = getSettings();
			
			//Act
			(bool success, string platform, string apiKey, string secretKey, string passphrase) = settings.GetDefaultCredentials();
			
			//Assert
			Assert.IsTrue(success);
		}
		
		[TestMethod]
		public void SetDefaultPlatformTest()
		{
			//Arrange
			IAppSettings settings = getSettings();
			
			//Act
			bool success = settings.SetDefaultPlatform(platform);
			
			//Assert
			Assert.IsTrue(success);
		}
		
		[TestMethod]
		public void GetAllKeysTest()
		{
			//Arrange
			IAppSettings settings = getSettings();
			
			//Act
			List<(string platform, string apiKey, string secretKey, string passphrase)> list = settings.GetAllKeys();
			
			//Assert
			Assert.IsTrue(list.Count > 0);
		}
		
		[TestMethod]
		public void SetRemoveKeysOnExitTrueTest()
		{
			//Arrange
			IAppSettings settings = getSettings();
			
			//Act
			settings.SetRemoveKeysOnExit(true);
			
			//Assert
			Assert.IsTrue(settings.GetRemoveKeysOnExitValue());
		}
		
		[TestMethod]
		public void SetRemoveKeysOnExitFalseTest()
		{
			//Arrange
			IAppSettings settings = getSettings();
			
			//Act
			settings.SetRemoveKeysOnExit(false);
			
			//Assert
			Assert.IsFalse(settings.GetRemoveKeysOnExitValue());
		}
	}
}