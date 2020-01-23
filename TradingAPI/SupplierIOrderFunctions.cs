﻿using Scraper;
 
 namespace TradingAPI
{
	public static class SupplierIOrderFunctions
	{
		public static IOrderFunctions GetIOrderFunctions(string platform, string apiKey, string secretKey,
			string passphrase = null)
		{
			//This is the only part that needs to be changed if calling a different implementation of IOrderFunctions
			return new WebScraper(platform, apiKey, secretKey, passphrase);
		}
	}
}