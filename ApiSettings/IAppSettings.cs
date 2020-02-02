using System.Collections.Generic;

namespace AppSettings
{
	public interface IAppSettings
	{
		public (bool success, string apiKey, string secretKey, string passphrase) GetCredentials(string platform);
		public (bool success, string platform, string apiKey, string secretKey, string passphrase) GetDefaultCredentials();

		public bool SetCredentials(string platform, string apiKey, string secretKey,
			string passphrase);

		public bool SetDefaultPlatform(string platform);
		public List<(string platform, string apiKey, string secretKey, string passphrase)> GetAllKeys();
		public bool SetRemoveKeysOnExit(bool value);
		public bool GetRemoveKeysOnExitValue();
		public void ClearAllPlatformKeys();
		


	}
}