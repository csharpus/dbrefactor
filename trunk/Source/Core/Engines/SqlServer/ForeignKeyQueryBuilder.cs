using System;
using System.Collections.Generic;

namespace DbRefactor.Engines.SqlServer
{
	public class ForeignKeyFilter
	{
		public string Name { get; set; }
		public string ForeignKeyTable { get; set; }
		public string[] ForeignKeyColumns { get; set; }
		public string PrimaryKeyTable { get; set; }
		public string[] PrimaryKeyColumns { get; set; }
	}

	public class ForeignKeyQueryBuilder
	{
		private readonly ForeignKeyFilter filter;
		private readonly List<string> restrictions = new List<string>();

		public ForeignKeyQueryBuilder(ForeignKeyFilter filter)
		{
			this.filter = filter;
		}

		public string BuildQuery()
		{
			AddForeignTableRestriction();
			AddForeignColumnRestriction();
			AddNameRestriction();
			AddPrimaryTableRestriction();
			AddPrimaryColumnRestriction();
			var whereClause = String.Join(" AND ", restrictions.ToArray());
			return whereClause != String.Empty ? baseQuery + " WHERE " + whereClause : baseQuery;
		}

		private void AddForeignTableRestriction()
		{
			if (filter.ForeignKeyTable == null) return;
			restrictions.Add(String.Format("{0} = '{1}'", ForeignTableSql, filter.ForeignKeyTable));
		}

		private void AddForeignColumnRestriction()
		{
			if (filter.ForeignKeyColumns == null) return;
			restrictions.Add(String.Format("{0} IN ('{1}')", ForeignColumnSql, String.Join("', '", filter.ForeignKeyColumns)));
		}

		private void AddNameRestriction()
		{
			if (filter.Name == null) return;
			restrictions.Add(String.Format("Name = '{0}'", filter.Name));
		}

		private void AddPrimaryTableRestriction()
		{
			if (filter.PrimaryKeyTable == null) return;
			restrictions.Add(String.Format("{0} = '{1}'", PrimaryTableSql, filter.PrimaryKeyTable));
		}

		private void AddPrimaryColumnRestriction()
		{
			if (filter.PrimaryKeyColumns == null) return;
			restrictions.Add(String.Format("{0} IN ('{1}')", PrimaryColumnSql, String.Join("', '", filter.PrimaryKeyColumns)));
		}

		readonly string baseQuery =
			String.Format(@"
SELECT ForeignKeys.[name] AS [Name],
   {0} AS ForeignTable,
   {1} AS ForeignColumn,
   {2} AS PrimaryTable,
   {3} AS PrimaryColumn
FROM sys.foreign_keys AS ForeignKeys
INNER JOIN sys.foreign_key_columns AS ForeignColumns
   ON ForeignKeys.object_id = ForeignColumns.constraint_object_id
				", ForeignTableSql, ForeignColumnSql, PrimaryTableSql, PrimaryColumnSql);

		private const string ForeignTableSql = "OBJECT_NAME(ForeignKeys.parent_object_id)";
		private const string ForeignColumnSql = "COL_NAME(ForeignColumns.parent_object_id, ForeignColumns.parent_column_id)";
		private const string PrimaryTableSql = "OBJECT_NAME (ForeignKeys.referenced_object_id)";
		private const string PrimaryColumnSql =
			"COL_NAME(ForeignColumns.referenced_object_id, ForeignColumns.referenced_column_id)";
	}
}