using System.Collections.Generic;
using System.Linq;
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.Filters;
using DbRefactor.Providers.Model;

namespace DbRefactor.Providers
{
	public sealed class SchemaHelper
	{
		private readonly ISchemaProvider schemaProvider;

		public SchemaHelper(ISchemaProvider schemaProvider)
		{
			this.schemaProvider = schemaProvider;
		}

		public bool TableHasIdentity(string table)
		{
			return schemaProvider.TableHasIdentity(table);
		}
		
		public bool IsNullable(string table, string column)
		{
			return schemaProvider.IsNullable(table, column);
		}

		public bool IsDefault(string table, string column)
		{
			return schemaProvider.IsDefault(table, column);
		}

		public bool IsIdentity(string table, string column)
		{
			return schemaProvider.IsIdentity(table, column);
		}

		public bool IsUnique(string table, string column)
		{
			return GetUniques(new UniqueFilter {TableName = table, ColumnNames = new[] {column}}).Any();
		}

		public IList<string> GetTables(TableFilter filter)
		{
			return schemaProvider.GetTables(filter);
		}

		public IList<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
		{
			return schemaProvider.GetConstraints(filter);
		}

		public IList<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
		{
			return schemaProvider.GetForeignKeys(filter);
		}

		public IList<Unique> GetUniques(UniqueFilter filter)
		{
			return schemaProvider.GetUniques(filter);
		}

		public IList<PrimaryKey> GetPrimaryKeys(PrimaryKeyFilter filter)
		{
			return schemaProvider.GetPrimaryKeys(filter);
		}

		public IList<Index> GetIndexes(IndexFilter filter)
		{
			return schemaProvider.GetIndexes(filter);
		}

		public IList<ColumnProvider> GetColumns(ColumnFilter filter)
		{
			return schemaProvider.GetColumns(filter);
		}

		public bool UniqueExists(string name)
		{
			return GetUniques(new UniqueFilter { Name = name }).Any();
		}

		public bool ForeignKeyExists(string name)
		{
			var filter = new ForeignKeyFilter { Name = name };
			return GetForeignKeys(filter).Any();
		}

		public bool PrimaryKeyExists(string name)
		{
			return GetPrimaryKeys(new PrimaryKeyFilter { Name = name }).Any();
		}

		public bool IndexExists(string name)
		{
			var filter = new IndexFilter { Name = name };
			return GetIndexes(filter).Any();
		}

		public bool TableExists(string table)
		{
			return GetTables(new TableFilter { TableName = table }).Any();
		}

		public bool ColumnExists(string table, string column)
		{
			return GetColumns(new ColumnFilter { TableName = table, ColumnName = column }).Any();
		}

		public IList<ForeignKey> GetForeignKeys()
		{
			return GetForeignKeys(new ForeignKeyFilter());
		}

		public IList<string> GetTables()
		{
			return GetTables(new TableFilter());
			//const string query = "SELECT [name] FROM sysobjects WHERE xtype = 'U'";
			//const string query = "select [TABLE_NAME] as [name] from information_schema.tables";
		}
	}
}