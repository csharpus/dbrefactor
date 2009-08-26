using System;
using System.Collections.Generic;

namespace DbRefactor.Engines.SqlServer
{
	internal class Index
	{
		public string Name { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
	}

	internal class IndexFilter
	{
		public string Name { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
	}

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
			var whereClause = String.Join(" AND ", restrictions.ToArray());
			return whereClause != String.Empty ? BaseQuery + " AND " + whereClause : BaseQuery;
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
SELECT Indexes.[name] as [Name], Objects.[name] as TableName, Columns.[name] as ColumnName
FROM sys.objects AS Objects
JOIN sys.indexes AS Indexes 
	ON Indexes.object_id = Objects.object_id
JOIN sysindexkeys AS IndexKeys 
	ON IndexKeys.id = Indexes.object_id
		AND IndexKeys.indid = Indexes.index_id
JOIN sys.columns AS Columns 
	ON Columns.object_id = IndexKeys.id
		AND Columns.column_id = IndexKeys.colid
WHERE Indexes.index_id BETWEEN 2 AND 254
	AND INDEXPROPERTY(Objects.object_id, Indexes.[name], 'IsStatistics') = 0
	AND INDEXPROPERTY(Objects.object_id, Indexes.[name], 'IsHypothetical') = 0
";
	}
}