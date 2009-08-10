using System;
using System.Linq;
using DbRefactor.Extensions;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	public interface ISqlGenerationService
	{
		string GenerateCreateColumnSql(ColumnProvider columnProvider);
		string GenerateAlterColumnSql(ColumnProvider provider);
		string GenerateAddColumnSql(ColumnProvider provider);
	}

	public class SQLGenerationService : ISqlGenerationService
	{
		public string GenerateCreateColumnSql(ColumnProvider columnProvider)
		{
			string propertiesSql = columnProvider.Properties
				.Select(p => p.CreateTableSql()).SpaceSeparated();
			
			string returnValue = String.Format("[{0}] {1} {2}", columnProvider.Name, columnProvider.SqlType(), propertiesSql).TrimEnd();
			if (columnProvider.HasDefaultValue)
			{
				returnValue += String.Format(" default {0}", columnProvider.GetDefaultValueSql());
			}
			return returnValue;
		}

		public string GenerateAlterColumnSql(ColumnProvider columnProvider)
		{
			string propertiesSql = columnProvider.Properties
				.Select(p => p.AlterTableSql()).SpaceSeparated();

			return String.Format("[{0}] {1} {2}", columnProvider.Name, columnProvider.SqlType(), propertiesSql).TrimEnd();
		}

		public string GenerateAddColumnSql(ColumnProvider provider)
		{
			return GenerateCreateColumnSql(provider);
		}
	}
}