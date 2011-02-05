using System;
using System.Collections.Generic;
using DbRefactor.Providers;
using DbRefactor.Providers.Filters;

namespace DbRefactor.Engines.SqlServer.QueryBuilders
{
	public class SqlServerColumnQueryBuilder
	{
		private readonly ColumnFilter filter;
		private readonly List<string> restrictions = new List<string>();

		public SqlServerColumnQueryBuilder(ColumnFilter filter)
		{
			this.filter = filter;
		}

		public string BuildQuery()
		{
			AddTableNameRestirctions();
			AddColumnNameRestrictions();
			var whereClause = String.Join(" and ", restrictions.ToArray());
			return whereClause != String.Empty ? BaseQuery + " where " + whereClause : BaseQuery;
		}

		private void AddTableNameRestirctions()
		{
			if (filter.TableName == null) return;
			restrictions.Add(String.Format("TABLE_NAME like '{0}'", filter.TableName));
		}

		private void AddColumnNameRestrictions()
		{
			if (filter.ColumnName == null) return;
			restrictions.Add(String.Format("COLUMN_NAME like '{0}'", filter.ColumnName));
		}

		private const string BaseQuery =
			@"
select DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE,
	COLUMN_DEFAULT 
from INFORMATION_SCHEMA.COLUMNS
";
	}
}