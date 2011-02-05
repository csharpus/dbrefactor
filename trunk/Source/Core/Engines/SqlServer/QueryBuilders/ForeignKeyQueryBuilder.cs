using System;
using System.Collections.Generic;
using DbRefactor.Providers;
using DbRefactor.Providers.Filters;

namespace DbRefactor.Engines.SqlServer
{
	internal class ForeignKeyQueryBuilder
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
			var whereClause = String.Join(" and ", restrictions.ToArray());
			return whereClause != String.Empty ? baseQuery + " where " + whereClause : baseQuery;
		}

		private void AddForeignTableRestriction()
		{
			if (filter.ForeignKeyTable == null) return;
			restrictions.Add(String.Format("{0} = '{1}'", ForeignTableSql, filter.ForeignKeyTable));
		}

		private void AddForeignColumnRestriction()
		{
			if (filter.ForeignKeyColumns == null) return;
			restrictions.Add(String.Format("{0} in ('{1}')", ForeignColumnSql, String.Join("', '", filter.ForeignKeyColumns)));
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
			restrictions.Add(String.Format("{0} in ('{1}')", PrimaryColumnSql, String.Join("', '", filter.PrimaryKeyColumns)));
		}

		readonly string baseQuery =
			String.Format(
@"
select ForeignKeys.[name] as [Name],
   {0} as ForeignTable,
   {1} as ForeignColumn,
   {2} as PrimaryTable,
   {3} as PrimaryColumn,
   COLUMNPROPERTY(OBJECT_ID({0}), {1},'AllowsNull') as ForeignNullable
from sys.foreign_keys as ForeignKeys
inner join sys.foreign_key_columns as ForeignColumns
   on ForeignKeys.object_id = ForeignColumns.constraint_object_id
				", ForeignTableSql, ForeignColumnSql, PrimaryTableSql, PrimaryColumnSql);

		private const string ForeignTableSql = "OBJECT_NAME(ForeignKeys.parent_object_id)";
		private const string ForeignColumnSql = "COL_NAME(ForeignColumns.parent_object_id, ForeignColumns.parent_column_id)";
		private const string PrimaryTableSql = "OBJECT_NAME (ForeignKeys.referenced_object_id)";
		private const string PrimaryColumnSql =
			"COL_NAME(ForeignColumns.referenced_object_id, ForeignColumns.referenced_column_id)";
	}
}