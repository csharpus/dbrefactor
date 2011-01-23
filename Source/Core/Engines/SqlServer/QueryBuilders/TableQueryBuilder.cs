using System;
using System.Collections.Generic;

namespace DbRefactor.Providers
{
	public class TableQueryBuilder
	{
		private readonly TableFilter filter;
		private readonly List<string> restrictions = new List<string>();

		public TableQueryBuilder(TableFilter filter)
		{
			this.filter = filter;
		}

		public string BuildQuery()
		{
			AddTableNameRestrictions();
			string whereClause = String.Join(" and ", restrictions.ToArray());
			return whereClause != String.Empty ? BaseQuery + " where " + whereClause : BaseQuery;
		}

		private void AddTableNameRestrictions()
		{
			if (filter.TableName == null) return;
			restrictions.Add(String.Format("TABLE_NAME = '{0}'", filter.TableName));
		}

		private const string BaseQuery =
@"
select TABLE_NAME as name
from information_schema.tables
";
	}
}
