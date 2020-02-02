using Scraper;

namespace AdvancedAPI
{
	public interface IAdvanced : IArbitrage, IOrderFunctions
	{
		void ArbitrageSymbolPairContinuously((string symbol1, string symbol2) symbolPair);
	}
}