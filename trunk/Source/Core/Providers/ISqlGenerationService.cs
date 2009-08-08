using System;
using System.Linq;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	public interface ISqlGenerationService
	{
		string GenerateColumnSql(ColumnProvider columnProvider);
	}

	public class SQLGenerationService : ISqlGenerationService
	{
		public string GenerateColumnSql(ColumnProvider columnProvider)
		{
			foreach (var property in columnProvider.Properties)
			{
				property.PropertySql();
			}
			var propertiesSql = columnProvider.Properties
				.Select(p => p.PropertySql()).ComaSeparated();
			string returnValue = String.Format("[{0}] {1} {2}", columnProvider.Name, columnProvider.SqlType(), propertiesSql).TrimEnd();
			if (columnProvider.HasDefaultValue)
			{
				returnValue += String.Format(" default {0}", columnProvider.GetDefaultValueSql());
			}
			return returnValue;
		}
	}
}