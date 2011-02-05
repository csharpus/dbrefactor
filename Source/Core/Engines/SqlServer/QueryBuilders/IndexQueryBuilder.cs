using System;
using System.Collections.Generic;
using DbRefactor.Providers.Filters;

namespace DbRefactor.Engines.SqlServer.QueryBuilders
{
	internal class IndexQueryBuilder
	{
		private readonly IndexFilter filter;

		public IndexQueryBuilder(IndexFilter filter)
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
			return whereClause != String.Empty ? BaseQuery + " and " + whereClause : BaseQuery;
		}

		private void AddColumnRestriction()
		{
			if (filter.ColumnName == null) return;
			restrictions.Add(String.Format("Columns.[name] = '{0}'", filter.ColumnName));
		}

		private void AddTableRestriction()
		{
			if (filter.TableName == null) return;
			restrictions.Add(String.Format("Objects.[name] = '{0}'", filter.TableName));
		}

		private void AddNameRestrictions()
		{
			if (filter.Name == null) return;
			restrictions.Add(String.Format("Indexes.[Name] = '{0}'", filter.Name));
		}

		private const string BaseQuery =
@"
select Indexes.[name] as [Name], Objects.[name] as TableName, Columns.[name] as ColumnName
from sys.objects as Objects
join sys.indexes as Indexes 
	on Indexes.object_id = Objects.object_id
join sysindexkeys as IndexKeys 
	on IndexKeys.id = Indexes.object_id
		and IndexKeys.indid = Indexes.index_id
join sys.columns as Columns 
	on Columns.object_id = IndexKeys.id
		and Columns.column_id = IndexKeys.colid
where Indexes.index_id between 2 and 254
	and INDEXPROPERTY(Objects.object_id, Indexes.[name], 'IsStatistics') = 0
	and INDEXPROPERTY(Objects.object_id, Indexes.[name], 'IsHypothetical') = 0
";
	}
}