using System.Collections.Generic;
using System.Linq;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	internal sealed partial class TransformationProvider
	{
		private ColumnProvider GetColumnProvider(string tableName, string columnName)
		{
			return schemaProvider.GetColumnProvider(tableName, columnName);
		}

		public string[] GetTables()
		{
			return schemaProvider.GetTables();
			//const string query = "SELECT [name] FROM sysobjects WHERE xtype = 'U'";
			//const string query = "select [TABLE_NAME] as [name] from information_schema.tables";
		}

		private List<string> GetIndexes(string tableName, string columnName)
		{
			var filter = new IndexFilter {TableName = tableName, ColumnName = columnName};
			return GetIndexes(filter).Select(i => i.Name).ToList();
		}

		internal List<ColumnProvider> GetColumnProviders(string table)
		{
			return schemaProvider.GetColumnProviders(table);
		}

		private List<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
		{
			return schemaProvider.GetConstraints(filter);
		}

		public List<string> GetConstraints(string table, string[] columns)
		{
			var filter = new ConstraintFilter {TableName = table, ColumnNames = columns};
			return GetConstraints(filter).Select(c => c.Name).Distinct().ToList();
		}

		public List<string> GetConstraintNames(string table)
		{
			var filter = new ConstraintFilter {TableName = table};
			return GetConstraints(filter).Select(c => c.Name).Distinct().ToList();
		}

		public List<DatabaseConstraint> GetConstraints(string table)
		{
			var filter = new ConstraintFilter {TableName = table};
			return GetConstraints(filter);
		}

		internal List<DatabaseConstraint> GetUniqueConstraints(string table, string[] columns)
		{
			var filter = new ConstraintFilter
			             	{TableName = table, ColumnNames = columns, ConstraintType = ConstraintType.Unique};
			return GetConstraints(filter).ToList();
		}

		private List<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
		{
			return schemaProvider.GetForeignKeys(filter);
		}

		private List<Index> GetIndexes(IndexFilter filter)
		{
			return schemaProvider.GetIndexes(filter);
		}

		public List<ForeignKey> GetForeignKeys()
		{
			return GetForeignKeys(new ForeignKeyFilter());
		}
	}
}