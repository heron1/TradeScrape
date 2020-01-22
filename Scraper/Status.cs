﻿namespace Scraper
{
	public enum Status
	{
		Success, 
		Failure_InvalidLogin, 
		Failure_InvalidSymbol, 
		Failure_InvalidPlatform,
		Failure_InvalidID,
		Failure_InvalidOrderBalance,
		Failure_InvalidOrderMinimumAmount,
		Failure_InvalidOrderGeneral,
		Failure_Undefined
	}
}