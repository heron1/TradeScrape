﻿using Scraper;
using AppSettingsFactory;

namespace AdvancedAPI
{
	public static class SupplierIOrderFunctions
	{
		public static IOrderFunctions GetIOrderFunctions(string platform)
		{
			//This is the only part that needs to be changed if calling a different implementation of IOrderFunctions
			(bool success, string apiKey, string secretKey, string passphrase) = 
				SupplierIAppSettings.GetIAppSettingsStorage().GetCredentials(platform);
			
			return new ScraperWrapper(platform, apiKey, secretKey, passphrase);
		}
	}
}