namespace AdvancedAPI
{
	public enum StatusAdv
	{
		Success,
		Failure_Undefined,
		Failure_SafeLowerThanMinAmount,
		Failure_SafeHigherThanMaxAmount,
		Failure_InsufficientArbitrageConditions,
		Failure_InsufficientBalance
	}
}