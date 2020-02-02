using System;
using System.Collections.Generic;
using Scraper;

namespace AdvancedAPI
{
	public interface IArbitrage
	{
		ExecutedArbitrageResults FindAndExecuteFirstValidSymbolPairForArbitrage();
		
		public List<(string symb1, string symb2)> FindMatchingPlatformSymbolPairs(
			(string platform1, string platform2) platformsTuple);

		/// <summary>
		/// Given a symbolPair common to two platforms, perform arbitrage of buying on 1st platform, selling on 2nd platform,
		///  iff the priceDifferenceThreshold is met, the potential arbitrage spend is higher than minSpend (for symb1), and lower than maxSpend
		/// </summary>
		/// <param name="symbolPair"></param>
		/// <param name="platforms"></param>
		/// <param name="priceDifferenceThreshold"></param>
		/// <param name="minSpend"></param>
		/// <param name="maxSpend"></param>
		/// <returns></returns>
		public (StatusAdv status, Decimal amountExecuted, string msg) AttemptArbitrageSingle((string symbol1, string symbol2) symbolPair,
			(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal priceDifferenceThreshold, 
			decimal minSpend, decimal maxSpend);
		
		public List<(string platform1, string platform2, (string symb1, string symb2) symbPair, Decimal size, Decimal
				priceDifference)>
			FindArbitrageOpportunities((string platform1, string platform2) platforms, decimal priceDifferenceThreshold,
				decimal minSpend, decimal maxSpend, int callLimit = 100);

		(StatusAdv status, Decimal platform1Ask, Decimal platform1AskSize, Decimal platform2Bid, Decimal platform2BidSize,
			Decimal actualArbitrageAmount)
			CheckArbitrageOpportunity((string symbol1, string symbol2) symbolPair,
				(IOrderFunctions platform1Api, IOrderFunctions platform2Api) platforms, decimal priceDifferenceThreshold,
				decimal minSpend, decimal maxSpend);
	}
}