using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.Filters;
using DbRefactor.Providers.Model;

namespace DbRefactor.Engines.SqlServer
{
	public class SqlServerDatabaseProvider : IDatabaseProvider
	{
		private readonly IDatabaseEnvironment environment;
		private readonly ISchemaProvider schemaProvider;

		public SqlServerDatabaseProvider(IDatabaseEnvironment environment,
		                                 ISchemaProvider schemaProvider)
		{
			this.environment = environment;
			this.schemaProvider = schemaProvider;
		}

		public void CreateTable(string name, ColumnProvider[] columns)
		{
			var columnsSql = GetCreateColumnsSql(name, columns);
			ExecuteNonQuery("create table {0} ({1})", Name(name), columnsSql);
		}

		public void DropTable(string name)
		{
			ExecuteNonQuery("drop table {0}", Name(name));
		}

		public void AddColumn(string table, ColumnProvider columnProvider)
		{
			ExecuteNonQuery("alter table {0} add {1}",
			                Name(table), GenerateAddColumnSql(table, columnProvider));
		}

		private string GenerateAddColumnSql(string table, ColumnProvider columnProvider)
		{
			return GetCreateColumnSql(table, columnProvider);
		}

		public void DropColumn(string table, string column)
		{
			DropColumnConstraints(table, column);
			ExecuteNonQuery("alter table {0} drop column {1}",
			                Name(table),
			                Name(column));
		}

		public void AlterColumn(string tableName, ColumnProvider columnProvider)
		{
			var provider = GetColumnProvider(tableName, columnProvider.Name);
			// TODO: IS it correct just copy properties?
			columnProvider.CopyPropertiesFrom(provider);
			AlterColumn(tableName, GenerateAlterColumnSql(tableName, columnProvider));
		}

		private string GenerateAlterColumnSql(string tableName, ColumnProvider columnProvider)
		{
			string propertiesSql = new TemplateParser()
				.Add("NULL", columnProvider.IsNull)
				.Add("NOT NULL", columnProvider.IsNotNull)
				.Add("CONSTRAINT {0} PRIMARY KEY", columnProvider.PrimaryKeyName, columnProvider.IsPrimaryKey)
				.Add("IDENTITY", columnProvider.IsIdentity)
				.Add("CONSTRAINT {0} UNIQUE", columnProvider.UniqueName, columnProvider.IsUnique)
				//.Add("DEFAULT {0}", columnProvider.DefaultValue.ToString().Replace("((", "").Replace("))", ""))
				.Apply();

			return
				String.Format("{0} {1} {2}", Name(columnProvider.Name), GetSqlType(columnProvider), propertiesSql).TrimEnd();
		}

		public void SetDefault(string constraintName, string tableName, string columnName, object value)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.DefaultValue = value;
			var query = String.Format("alter table {0} add constraint {1} default {2} for {3}",
			                          Name(tableName),
			                          constraintName,
			                          GetDefaultValueSql(provider), Name(columnName));
			ExecuteNonQuery(query);
		}

		public void AddUnique(string name, string table, string[] columns)
		{
			ExecuteNonQuery("alter table {0} add constraint {1} unique ({2}) ",
			                Name(table), name, String.Join(",", columns));
		}

		public void DropIndex(string table, string[] columns)
		{
			var indexesList = columns.Select(column => GetIndexes(table, column)).ToList();
			var indexesPresentInAllColumns = GetIndexesPresentInAllColumns(indexesList);
			if (indexesPresentInAllColumns.Count == 0)
			{
				throw new DbRefactorException("Couldn't find any indexes mutual for all passed columns");
			}
			foreach (var index in indexesPresentInAllColumns)
			{
				ExecuteNonQuery("drop index {0}.{1}", table, index);
			}
		}

		public void AddIndex(string name, string table, string[] columns)
		{
			ExecuteNonQuery("create nonclustered index {0} on {1} ({2}) ",
			                name, Name(table), String.Join(",", columns));
		}

		public void AddPrimaryKey(string name, string table, string[] columns)
		{
			ExecuteNonQuery("alter table {0} add constraint {1} primary key ({2}) ",
			                Name(table), name, String.Join(",", columns));
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
		                          string[] refColumns, OnDelete constraint)
		{
			ExecuteNonQuery(
				@"
alter table {0} 
add constraint {1} 
foreign key ({2}) 
references {3} ({4}) 
	on delete {5}
",
				Name(primaryTable), name, String.Join(",", primaryColumns),
				Name(refTable), String.Join(",", refColumns),
				ResolveForeignKeyConstraint(constraint));
		}

		public void SetNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.RemoveNotNull();
			AlterColumn(tableName, GenerateAlterColumnSql(tableName, provider));
		}

		public void SetNotNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.AddNotNull();
			AlterColumn(tableName, GenerateAlterColumnSql(tableName, provider));
		}

		private string GetDefaultValueSql(ColumnProvider columnProvider)
		{
			if (!columnProvider.HasDefaultValue)
			{
				throw new DbRefactorException("could not generate code because default value is null");
			}
			return new SqlServerTypeHelper().GetValueSql(columnProvider, columnProvider.DefaultValue);
		}

		private string ResolveForeignKeyConstraint(OnDelete constraint)
		{
			switch (constraint)
			{
				case OnDelete.Cascade:
					return "CASCADE";
				case OnDelete.SetDefault:
					return "SET DEFAULT";
				case OnDelete.SetNull:
					return "SET NULL";
				case OnDelete.NoAction:
					return "NO ACTION";
				default:
					throw new ArgumentOutOfRangeException("constraint");
			}
		}

		private string GetCreateColumnSql(string table, ColumnProvider columnProvider)
		{
			string defaultValue = columnProvider.DefaultValue != null ? GetDefaultValueSql(columnProvider) : null;
			string propertiesSql = new TemplateParser()
				.Add("NULL", columnProvider.IsNull)
				.Add("NOT NULL", columnProvider.IsNotNull)
				.Add("CONSTRAINT {0} PRIMARY KEY", columnProvider.PrimaryKeyName, columnProvider.IsPrimaryKey)
				.Add("IDENTITY", columnProvider.IsIdentity)
				.Add("CONSTRAINT {0} UNIQUE", columnProvider.UniqueName, columnProvider.IsUnique)
				.Add("DEFAULT {0}", defaultValue)
				.Apply();
			string returnValue =
				String.Format("{0} {1} {2}", Name(columnProvider.Name),
				              GetSqlType(columnProvider), propertiesSql).TrimEnd();
			return returnValue;
		}

		private void AlterColumn(string table, string sqlColumn)
		{
			Contract.Requires(table != null);
			Contract.Requires(sqlColumn != null);
			ExecuteNonQuery("alter table {0} alter column {1}",
			                Name(table),
			                sqlColumn);
		}

		private void DropConstraint(string table, string name)
		{
			Contract.Requires(table != null);
			Contract.Requires(name != null);

			ExecuteNonQuery("alter table {0} drop constraint {1}", Name(table), name);
		}

		public void RenameTable(string oldName, string newName)
		{
			Contract.Requires(oldName != null);
			Contract.Requires(newName != null);
			ExecuteNonQuery(String.Format("EXEC sp_rename '{0}', '{1}', 'OBJECT'", oldName,
			                              newName));
		}

		public void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			ExecuteNonQuery(String.Format("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", table,
			                              oldColumnName, newColumnName));
		}

		public void DropUnique(string table, string[] columnNames)
		{
			var uniqueConstraints = GetUniqueConstraints(table, columnNames);
			var constraintsSharedBetweenAllColumns = uniqueConstraints.GroupBy(c => c.Name)
				.Where(group => !columnNames.Except(group.Select(c => c.ColumnName)).Any())
				.Select(g => g.Key).ToList();
			if (constraintsSharedBetweenAllColumns.Count == 0)
			{
				string message = columnNames.Length == 1
				                 	? String.Format(
				                 		"Could not find any unique constraints for column '{0}' in table '{1}'",
				                 		columnNames[0], table)
				                 	: String.Format(
				                 		"Could not find any mutual unique constraints for columns '{0}' in table '{1}'",
				                 		String.Join("', '", columnNames), table);
				throw new DbRefactorException(message);
			}
			foreach (var constraint in constraintsSharedBetweenAllColumns)
			{
				DropConstraint(table, constraint);
			}
		}

		public void DropDefault(string tableName, string columnName)
		{
			IEnumerable<string> defaultConstraints = GetConstraintsByType(tableName, new[] {columnName},
			                                                              ConstraintType.Default);
			foreach (var constraint in defaultConstraints)
			{
				DropConstraint(tableName, constraint);
			}
		}

		private string GetCreateColumnsSql(string table, IEnumerable<ColumnProvider> columns)
		{
			return columns.Select(c => GetCreateColumnSql(table, c))
				.WithTabsOnStart(2)
				.WithNewLinesOnStart()
				.ComaSeparated();
		}

		private void DropColumnConstraints(string table, string column)
		{
			Contract.Requires(table != null);
			Contract.Requires(column != null);
			var constraints = schemaProvider.GetConstraints(new ConstraintFilter());

			if (constraints.Any(c => c.ConstraintType == ConstraintType.Default))
			{
				environment.ExecuteNonQuery(
					String.Format(
						@"
declare @sql nvarchar(255)
while exists(select * from sys.default_constraints d
	inner join sys.columns c on c.column_id = d.parent_column_id and c.object_id = d.parent_object_id
	where c.name = '{1}' and OBJECT_NAME(d.parent_object_id) = '{0}')
begin
    select @sql = 'alter table {0} drop constraint ' + d.name from sys.default_constraints d
	inner join sys.columns c on c.column_id = d.parent_column_id and c.object_id = d.parent_object_id
	where c.name = '{1}' and OBJECT_NAME(d.parent_object_id) = '{0}'
    exec sp_executesql @sql
end
",
						table, column));
			}

			if (constraints.Any(c => c.ConstraintType != ConstraintType.Default && c.ConstraintType != ConstraintType.ForeignKey))
			{
				environment.ExecuteNonQuery(
					String.Format(
						@"
declare @sql nvarchar(255)
while exists(select * from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE where COLUMN_NAME = '{1}' and TABLE_NAME = '{0}')
begin
    select @sql = 'alter table {0} drop constraint ' + CONSTRAINT_NAME from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
	where COLUMN_NAME = '{1}' and TABLE_NAME = '{0}'
    exec sp_executesql @sql
end
",
						table, column));
			}
		}

		private string GetSqlType(ColumnProvider columnProvider)
		{
			return new SqlServerTypeHelper().GetSqlType(columnProvider);
		}

		public string GetValueSql(ColumnProvider provider, object value)
		{
			return new SqlServerTypeHelper().GetValueSql(provider, value);
		}

		public int ExecuteNonQuery(string sql, params string[] values)
		{
			Contract.Requires(sql != null);
			return environment.ExecuteNonQuery(String.Format(sql, values));
		}

		public void DropForeignKey(string foreignKeyTable, string[] foreignKeyColumns, string primaryKeyTable,
		                           string[] primaryKeyColumns)
		{
			if (foreignKeyColumns.Length != primaryKeyColumns.Length)
				throw new DbRefactorException(
					"The number of foreign key columns should be the same as the number of primary key columns");
			var filter = new ForeignKeyFilter
			             	{
			             		ForeignKeyTable = foreignKeyTable,
			             		ForeignKeyColumns = foreignKeyColumns,
			             		PrimaryKeyTable = primaryKeyTable,
			             		PrimaryKeyColumns = primaryKeyColumns
			             	};
			var foreignKeys = schemaProvider.GetForeignKeys(filter);
			var keysSharedBetweenAllColumns = foreignKeys.GroupBy(key => key.Name)
				.Where(group => !foreignKeyColumns.Except(group.Select(k => k.ForeignColumn)).Any())
				.Select(g => g.Key).ToList();
			foreach (var key in keysSharedBetweenAllColumns)
			{
				DropConstraint(foreignKeyTable, key);
			}
		}

		public void DropPrimaryKey(string table)
		{
			string constraintName = GetPrimaryKeyConstraintName(table);

			DropConstraint(table, constraintName);
		}

		private void DropForeignKey(string table, string name)
		{
			Contract.Requires(table != null);
			Contract.Requires(name != null);

			ExecuteNonQuery("alter table {0} drop constraint {1}", Name(table), name);
		}

		private ColumnProvider GetColumnProvider(string table, string column)
		{
			var filter = new ColumnFilter {TableName = table, ColumnName = column};
			var columns = schemaProvider.GetColumns(filter);
			if (columns.Count == 0)
			{
				throw new DbRefactorException(String.Format("Column '{0}' in table '{1}' was not found", column, table));
			}
			return columns[0];
		}

		private List<string> GetIndexes(string tableName, string columnName)
		{
			var filter = new IndexFilter {TableName = tableName, ColumnName = columnName};
			return GetIndexes(filter).Select(i => i.Name).ToList();
		}

		private IEnumerable<Index> GetIndexes(IndexFilter filter)
		{
			return schemaProvider.GetIndexes(filter);
		}

		private static List<string> GetIndexesPresentInAllColumns(IList<List<string>> indexesList)
		{
			var startIndexes = indexesList[0];
			foreach (var indexList in indexesList)
			{
				var indexesThatExists = indexList.Intersect(startIndexes).ToList();
				startIndexes = indexesThatExists;
			}
			return startIndexes;
		}

		private IEnumerable<Unique> GetUniqueConstraints(string table, string[] columns)
		{
			return schemaProvider.GetUniques(new UniqueFilter {TableName = table, ColumnNames = columns});
		}

		private IEnumerable<string> GetConstraintsByType(string tableName, string[] columns, ConstraintType constraintType)
		{
			return schemaProvider.GetConstraints(new ConstraintFilter
			                                     	{
			                                     		TableName = tableName,
			                                     		ColumnNames = columns,
			                                     		ConstraintType = constraintType
			                                     	}).
				Select(c => c.Name).ToList();
		}

		private string GetPrimaryKeyConstraintName(string table)
		{
			var keys = schemaProvider.GetPrimaryKeys(new PrimaryKeyFilter {TableName = table});
			if (keys.Count == 0)
				throw new DbRefactorException(String.Format("Could not find primary key constraint on table '{0}'",
				                                            table));
			return keys[0].Name;
		}

		private string Name(string objectName)
		{
			return NameEncoderHelper.Encode(objectName);
		}
	}
}