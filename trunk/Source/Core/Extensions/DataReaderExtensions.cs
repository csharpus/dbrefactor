using System.Collections.Generic;
using System.Data;

namespace DbRefactor.Extensions
{
	internal static class DataReaderExtensions
	{
		public static IEnumerable<IDataReader> AsReadable(this IDataReader reader)
		{
			using (reader)
			{
				while (reader.Read())
				{
					yield return reader;
				}
			}
		}
	}
}