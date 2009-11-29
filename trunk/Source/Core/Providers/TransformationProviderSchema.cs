using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	internal sealed partial class TransformationProvider
	{
		private ColumnProvider GetColumnProvider(string table, string column)
		{
			var filter = new ColumnFilter { TableName = table, ColumnName = column};
			var columns = schemaProvider.GetColumns(filter);
			if (columns.Count == 0)
			{
				throw new DbRefactorException(String.Format("Column '{0}' in table '{1}' was not found", column, table));
			}
			return columns[0];
		}

		public string[] GetTables()
		{
			return schemaProvider.GetTables(new TableFilter());
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
			var filter = new ColumnFilter {TableName = table};
			return schemaProvider.GetColumns(filter);
		}

		private List<Unique> GetUniqueConstraints(string table, string[] columns)
		{
			return schemaProvider.GetUniques(new UniqueFilter {TableName = table, ColumnNames = columns});
		}

		private List<Index> GetIndexes(IndexFilter filter)
		{
			return schemaProvider.GetIndexes(filter);
		}

		public List<ForeignKey> GetForeignKeys()
		{
			return schemaProvider.GetForeignKeys(new ForeignKeyFilter());
		}
	}
}