using System.Collections.Generic;
using AppSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TradeScrape_Unit_Tests.InternalSettingsIntegrations
{
	[TestClass]
	public class SettingsFileSystemTest
	{
		private string platform = "bitfinex";
		
		private IAppSettings getSettings()
		{
			return SupplierIAppSettings.GetIAppSettings();
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