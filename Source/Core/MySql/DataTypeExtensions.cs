using DbRefactor.Api;

namespace DbRefactor.MySql
{
	public static class DataTypeExtensions
	{
		public static NewTable Enum(this NewTable newTable, params string[] values)
		{

			return newTable;
		}
	}
}
