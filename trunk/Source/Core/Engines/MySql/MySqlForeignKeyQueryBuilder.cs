using System;
using System.Collections.Generic;
using DbRefactor.Providers;

namespace DbRefactor.Engines.MySql
{
	internal class MySqlForeignKeyQueryBuilder
	{
		private readonly ForeignKeyFilter filter;
		private readonly List<string> restrictions = new List<string>();

		public MySqlForeignKeyQueryBuilder(ForeignKeyFilter filter)
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
			return whereClause != String.Empty ? BaseQuery + " where " + whereClause : BaseQuery;
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
			restrictions.Add(String.Format("constraints.CONSTRAINT_NAME = '{0}'", filter.Name));
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

		private const string BaseQuery =
@"
select
	constraints.CONSTRAINT_NAME as Name, 
	constraints.TABLE_NAME as ForeignTable,
	ForeignKeys.COLUMN_NAME as ForeignColumn,
	constraints.REFERENCED_TABLE_NAME as PrimaryTable,
	PrimaryKeys.COLUMN_NAME as PrimaryColumn,
	Columns.IS_NULLABLE as ForeignNullable
from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS as constraints
inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as ForeignKeys
	on constraints.CONSTRAINT_NAME = ForeignKeys.CONSTRAINT_NAME
inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as PrimaryKeys
	on constraints.UNIQUE_CONSTRAINT_NAME = PrimaryKeys.CONSTRAINT_NAME
inner join INFORMATION_SCHEMA.COLUMNS as Columns
	on ForeignKeys.COLUMN_NAME = Columns.COLUMN_NAME
";

		private const string ForeignTableSql = "constraints.TABLE_NAME";
		private const string ForeignColumnSql = "ForeignKeys.COLUMN_NAME";
		private const string PrimaryTableSql = "constraints.REFERENCED_TABLE_NAME";
		private const string PrimaryColumnSql = "PrimaryKeys.COLUMN_NAME";
	}
}