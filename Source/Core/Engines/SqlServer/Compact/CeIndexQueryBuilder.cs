using System;
using System.Collections.Generic;
using DbRefactor.Providers.Filters;

namespace DbRefactor.Engines.SqlServer.Compact
{
	internal class CeIndexQueryBuilder
	{
		private readonly IndexFilter filter;

		public CeIndexQueryBuilder(IndexFilter filter)
		{
			this.filter = filter;
		}

		private readonly List<string> restrictions = new List<string>();

		public string BuildQuery()
		{
			AddNameRestrictions();
			AddTableRestriction();
			AddColumnRestriction();
			var whereClause = String.Join(" and ", restrictions.ToArray());
			return whereClause != String.Empty ? BaseQuery + " where " + whereClause : BaseQuery;
		}

		private void AddColumnRestriction()
		{
			if (filter.ColumnName == null) return;
			restrictions.Add(String.Format("COLUMN_NAME = '{0}'", filter.ColumnName));
		}

		private void AddTableRestriction()
		{
			if (filter.TableName == null) return;
			restrictions.Add(String.Format("TABLE_NAME = '{0}'", filter.TableName));
		}

		private void AddNameRestrictions()
		{
			if (filter.Name == null) return;
			restrictions.Add(String.Format("INDEX_NAME = '{0}'", filter.Name));
		}

		private const string BaseQuery =
@"
select INDEX_NAME as [Name], TABLE_NAME as TableName, COLUMN_NAME as ColumnName from INFORMATION_SCHEMA.INDEXES
";
	}
}