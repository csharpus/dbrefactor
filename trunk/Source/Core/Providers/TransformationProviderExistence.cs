using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Providers
{
	internal sealed partial class TransformationProvider
	{
		public bool UniqueExists(string name)
		{
			return schemaProvider.GetUniques(new UniqueFilter {Name = name}).Any();
		}

		public bool ForeignKeyExists(string name)
		{
			var filter = new ForeignKeyFilter {Name = name};
			return schemaProvider.GetForeignKeys(filter).Any();
		}

		public bool PrimaryKeyExists(string name)
		{
			return schemaProvider.GetPrimaryKeys(new PrimaryKeyFilter{Name = name}).Any();
		}

		public bool IndexExists(string name)
		{
			var filter = new IndexFilter {Name = name};
			return GetIndexes(filter).Any();
		}

		public bool TableExists(string table)
		{
			return schemaProvider.GetTables(new TableFilter {TableName = table}).Any();
		}

		public bool ColumnExists(string table, string column)
		{
			return schemaProvider.GetColumns(new ColumnFilter {TableName = table, ColumnName = column}).Any();
		}

		public bool IsIdentity(string table, string column)
		{
			return schemaProvider.IsIdentity(table, column);
		}

		public bool IsNullable(string table, string column)
		{
			return schemaProvider.IsNullable(table, column);
		}

		public bool TableHasIdentity(string table)
		{
			Check.RequireNonEmpty(table, "table");
			return
				Convert.ToInt32(ExecuteScalar("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')", table)) ==
				1;
		}

		public bool IsDefault(string table, string column)
		{
			return schemaProvider.IsDefault(table, column);
		}
	}
}