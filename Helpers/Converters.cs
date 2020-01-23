using System.Linq;

namespace Helpers
{
	public static class Converters
	{
		public static object[] TupleToArray<T>(object tuple)
		{
			var t = tuple.GetType()
				.GetProperties()
				.Select(property => property.GetValue(tuple));
			return t.ToArray();
		}
	}
}