#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Infrastructure;
using DbRefactor.Providers.Columns;
using DbRefactor.Tools.DesignByContract;


namespace DbRefactor.Providers
{
	public sealed partial class TransformationProvider
	{
		private readonly IDatabaseEnvironment environment;
		private readonly SqlServerColumnMapper sqlServerColumnMapper;
		private readonly ConstraintNameService constraintNameService;

		internal TransformationProvider(IDatabaseEnvironment environment, SqlServerColumnMapper sqlServerColumnMapper,
		                                ConstraintNameService constraintNameService)
		{
			this.environment = environment;
			this.sqlServerColumnMapper = sqlServerColumnMapper;
			this.constraintNameService = constraintNameService;
		}

		internal IDatabaseEnvironment Environment
		{
			get { return environment; }
		}


		public void CreateTable(string name, params ColumnProvider[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.Require(columns.Length > 0, "At least one column should be passed");
			var columnsSql = GetCreateColumnsSql(columns);
			ExecuteNonQuery("CREATE TABLE [{0}] ({1})", name, columnsSql);
		}

		/// <summary>
		/// Removes an existing table from the database
		/// </summary>
		/// <param name="name">Table name</param>
		public void DropTable(string name)
		{
			Check.RequireNonEmpty(name, "name");
			ExecuteNonQuery("DROP TABLE [{0}]", name);
		}

		/// <summary>
		/// Remove a column from an existing table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		public void DropColumn(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			DropColumnConstraints(table, column);
			ExecuteNonQuery("ALTER TABLE {0} DROP COLUMN {1} ", table, column);
		}

		private static string GetCreateColumnsSql(IEnumerable<ColumnProvider> columns)
		{
			return columns.Select(col => col.GetCreateColumnSql()).ComaSeparated();
		}

		public void DropUnique(string table, params string[] columnNames)
		{
			Check.RequireNonEmpty(table, "table");

			var uniqueConstraints = GetUniqueConstraints(table, columnNames);
			var constraintsSharedBetweenAllColumns = uniqueConstraints.GroupBy(c => c.Name)
				.Where(group => !columnNames.Except(group.Select(c => c.ColumnName)).Any())
				.Select(g => g.Key).ToList();
			if (constraintsSharedBetweenAllColumns.Count == 0)
			{
				string message = columnNames.Length == 1
				                 	? String.Format("Could not find any unique constraints for column '{0}' in table '{1}'",
				                 	                columnNames[0], table)
				                 	: String.Format("Could not find any mutual unique constraints for columns '{0}' in table '{1}'",
				                 	                String.Join("', '", columnNames), table);
				throw new DbRefactorException(message);
			}
			foreach (var constraint in constraintsSharedBetweenAllColumns)
			{
				DropConstraint(table, constraint);
			}
		}

		public void DropPrimaryKey(string table)
		{
			Check.RequireNonEmpty(table, "table");

			string constraintName = GetPrimaryKeyConstraintName(table);

			DropConstraint(table, constraintName);
		}

		private string GetPrimaryKeyConstraintName(string table)
		{
			var filter = new ConstraintFilter {TableName = table, ConstraintType = "PK"};
			var constraints = GetConstraints(filter).Select(c => c.Name).Distinct().ToList();
			if (constraints.Count == 0)
				throw new DbRefactorException(String.Format("Could not find primary key constraint on table '{0}'", table));
			return constraints[0];
		}

		/// <summary>
		/// Renames an existing column
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldColumnName">Old column name</param>
		/// <param name="newColumnName">New column name</param>
		public void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(oldColumnName, "oldColumnName");
			Check.RequireNonEmpty(newColumnName, "newColumnName");
			ExecuteNonQuery("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", table, oldColumnName, newColumnName);
		}

		/// <summary>
		/// Renames an existing table
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		public void RenameTable(string oldName, string newName)
		{
			Check.RequireNonEmpty(oldName, "oldName");
			Check.RequireNonEmpty(newName, "newName");
			ExecuteNonQuery("EXEC sp_rename '{0}', '{1}', 'OBJECT'", oldName, newName);
		}

		private void AlterColumn(string table, string sqlColumn)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(sqlColumn, "sqlColumn");
			ExecuteNonQuery("ALTER TABLE [{0}] ALTER COLUMN {1}", table, sqlColumn);
		}

		public void DropColumnConstraints(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			var constraints = GetConstraints(table, new[] {column});
			foreach (string constraint in constraints)
			{
				DropConstraint(table, constraint);
			}
		}

		/// <summary>
		/// Append a primary key to a table.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table name</param>
		/// <param name="columns">Primary column names</param>
		public void AddPrimaryKey(string name, string table, params string[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			ExecuteNonQuery("ALTER TABLE [{0}] ADD CONSTRAINT {1} PRIMARY KEY ({2}) ",
			                table, name, String.Join(",", columns));
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns,
		                          string refTable, string[] refColumns, OnDelete constraint)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(primaryTable, "primaryTable");
			Check.RequireNonEmpty(refTable, "refTable");
			Check.Require(primaryColumns.Length > 0, "You have to pass at least one primary column");
			Check.Require(refColumns.Length > 0, "You have to pass at least one ref column");
			ExecuteNonQuery(
				"ALTER TABLE [{0}] ADD CONSTRAINT [{1}] FOREIGN KEY ({2}) REFERENCES {3} ({4}) ON DELETE {5}",
				primaryTable, name, String.Join(",", primaryColumns),
				refTable, String.Join(",", refColumns),
				new SqlServerForeignKeyConstraintMapper().Resolve(constraint));
		}

		/// <summary>
		/// Removes a constraint.
		/// </summary>
		/// <param name="table">Table owning the constraint</param>
		/// <param name="name">Constraint name</param>
		public void DropConstraint(string table, string name)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(name, "name");

			ExecuteNonQuery("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", table, name);
		}

		private Dictionary<string, Func<ColumnData, ColumnProvider>> GetTypesMap()
		{
			return new Dictionary<string, Func<ColumnData, ColumnProvider>>
			       	{
			       		{"bigint", sqlServerColumnMapper.CreateLong},
			       		{"binary", sqlServerColumnMapper.CreateBinary},
			       		{"bit", sqlServerColumnMapper.CreateBoolean},
			       		{"char", sqlServerColumnMapper.CreateString},
			       		{"datetime", sqlServerColumnMapper.CreateDateTime},
			       		{"decimal", sqlServerColumnMapper.CreateDecimal},
			       		{"float", sqlServerColumnMapper.CreateFloat},
			       		{"image", sqlServerColumnMapper.CreateBinary},
			       		{"int", sqlServerColumnMapper.CreateInt},
			       		{"money", sqlServerColumnMapper.CreateDecimal},
			       		{"nchar", sqlServerColumnMapper.CreateString},
			       		{"ntext", sqlServerColumnMapper.CreateText},
			       		{"numeric", sqlServerColumnMapper.CreateDecimal},
			       		{"nvarchar", sqlServerColumnMapper.CreateString},
			       		{"real", sqlServerColumnMapper.CreateFloat},
			       		{"smalldatetime", sqlServerColumnMapper.CreateDateTime},
			       		{"smallint", sqlServerColumnMapper.CreateInt},
			       		{"smallmoney", sqlServerColumnMapper.CreateDecimal},
			       		{"sql_variant", sqlServerColumnMapper.CreateBinary},
			       		{"text", sqlServerColumnMapper.CreateText},
			       		{"timestamp", sqlServerColumnMapper.CreateDateTime},
			       		{"tinyint", sqlServerColumnMapper.CreateInt},
			       		{"uniqueidentifier", sqlServerColumnMapper.CreateString},
			       		{"varbinary", sqlServerColumnMapper.CreateBinary},
			       		{"varchar", sqlServerColumnMapper.CreateString},
			       		{"xml", sqlServerColumnMapper.CreateString}
			       	};
		}

		private const int GuidLength = 38; // 36 symbols in guid + 2 curly brackets

		private ColumnProvider GetProvider(IDataRecord reader)
		{
			var data = new ColumnData
			           	{
			           		Name = reader["COLUMN_NAME"].ToString(),
			           		DataType = reader["DATA_TYPE"].ToString(),
			           		Length = NullSafeGet<int>(reader, "CHARACTER_MAXIMUM_LENGTH"),
			           		Precision = NullSafeGet<byte>(reader, "NUMERIC_PRECISION"),
			           		Radix = NullSafeGet<short>(reader, "NUMERIC_PRECISION_RADIX"),
			           		DefaultValue = GetDefaultValue(reader["COLUMN_DEFAULT"])
			           	};
			return GetTypesMap()[data.DataType](data);
		}

		private void AddProviderProperties(string table, ColumnProvider provider)
		{
			if (IsPrimaryKey(table, provider.Name))
			{
				provider.AddPrimaryKey(constraintNameService.PrimaryKeyName(table, provider.Name));
			}
			else if (!IsNullable(table, provider.Name))
			{
				provider.AddNotNull();
			}

			if (IsIdentity(table, provider.Name))
			{
				provider.AddIdentity();
			}

			if (IsUnique(table, provider.Name))
			{
				provider.AddUnique(constraintNameService.UniqueName(table, provider.Name));
			}
		}

		private static object GetDefaultValue(object databaseValue)
		{
			return databaseValue == DBNull.Value ? null : databaseValue;
		}

		private static T? NullSafeGet<T>(IDataRecord reader, string name)
			where T : struct
		{
			object value = reader[name];
			if (value == DBNull.Value)
			{
				return null;
			}
			return (T) value;
		}

		public int ExecuteNonQuery(string sql, params string[] values)
		{
			Check.RequireNonEmpty(sql, "sql");
			return environment.ExecuteNonQuery(String.Format(sql, values));
		}

		/// <summary>
		/// Execute an SQL query returning results.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <param name="values">Replacements for {d} pattern.</param>
		/// <returns>A data iterator, <see cref="System.Data.IDataReader">IDataReader</see>.</returns>
		public IDataReader ExecuteQuery(string sql, params string[] values)
		{
			Check.RequireNonEmpty(sql, "sql");
			return environment.ExecuteQuery(String.Format(sql, values));
		}


		public object ExecuteScalar(string sql, params string[] values)
		{
			return environment.ExecuteScalar(String.Format(sql, values));
		}

		public int Insert(string table, object insertObject)
		{
			var providers = GetColumnProviders(table);
			string operation = GetValues(providers, insertObject);
			var columns = String.Join("], [", GetColumnNames(insertObject));

			return ExecuteNonQuery(
				"INSERT INTO [{0}] ([{1}]) VALUES ({2})",
				table,
				columns,
				operation);
		}

		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			environment.BeginTransaction();
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public void RollbackTransaction()
		{
			environment.RollbackTransaction();
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void CommitTransaction()
		{
			environment.CommitTransaction();
		}

		public void AddUnique(string name, string table, params string[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			ExecuteNonQuery("ALTER TABLE [{0}] ADD CONSTRAINT {1} UNIQUE ({2}) ",
			                table, name, String.Join(",", columns));
		}

		public void AddIndex(string name, string table, params string[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			ExecuteNonQuery("CREATE NONCLUSTERED INDEX {0} ON [{1}] ({2}) ",
			                name, table, String.Join(",", columns));
		}

		public void DropIndex(string table, params string[] columns)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			var indexesList = new List<List<string>>();
			foreach (var column in columns)
			{
				var indexes = GetIndexes(table, column);
				indexesList.Add(indexes);
			}
			var indexesPresentInAllColumns = GetIndexesPresentInAllColumns(indexesList);
			if (indexesPresentInAllColumns.Count == 0)
			{
				throw new DbRefactorException("Couldn't find any indexes mutual for all passed columns");
			}
			foreach (var index in indexesPresentInAllColumns)
			{
				ExecuteNonQuery("DROP INDEX {0}.{1}", table, index);
			}
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

		public void AddColumn(string table, ColumnProvider columnProvider)
		{
			ExecuteNonQuery("ALTER TABLE [{0}] ADD {1}", table, columnProvider.GetAddColumnSql());
		}

		public void SetNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.RemoveNotNull();
			AlterColumn(tableName, provider.GetAlterColumnSql());
		}

		public void SetNotNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.AddNotNull();
			AlterColumn(tableName, provider.GetAlterColumnSql());
		}

		public void SetDefault(string constraintName, string tableName, string columnName, object value)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.DefaultValue = value;
			var query = String.Format("ALTER TABLE [{0}] ADD CONSTRAINT {1} DEFAULT {2} FOR [{3}]", tableName, constraintName,
			                          provider.GetDefaultValueSql(), columnName);
			ExecuteNonQuery(query);
		}

		public void DropDefault(string tableName, string columnName)
		{
			List<string> defaultConstraints = GetConstraintsByType(tableName, new[] {columnName}, "D");
			foreach (var constraint in defaultConstraints)
			{
				DropConstraint(tableName, constraint);
			}
		}

		public void AlterColumn(string tableName, ColumnProvider columnProvider)
		{
			var provider = GetColumnProvider(tableName, columnProvider.Name);
			// TODO: IS it correct just copy properties?
			columnProvider.CopyPropertiesFrom(provider);
			AlterColumn(tableName, columnProvider.GetAlterColumnSql());
		}

		public IDataReader Select(string tableName, string[] columns, object whereParameters)
		{
			string query = String.Format("SELECT {0} FROM [{1}]", columns.ComaSeparated(), tableName);
			var providers = GetColumnProviders(tableName);
			string whereClause = GetWhereClauseValues(providers, whereParameters);
			if (whereClause != String.Empty)
			{
				query += String.Format(" WHERE {0}", whereClause);
			}
			return ExecuteQuery(query);
		}

		public int Update(string tableName, object updateObject, object whereParameters)
		{
			var providers = GetColumnProviders(tableName);
			string operation = GetOperationValues(providers, updateObject);
			var query = String.Format("UPDATE [{0}] SET {1}", tableName, operation);

			string whereClause = GetWhereClauseValues(providers, whereParameters);
			if (whereClause != String.Empty)
			{
				query += String.Format(" WHERE {0}", whereClause);
			}
			return ExecuteNonQuery(query);
		}

		private static string[] GetColumnNames(object insertObject)
		{
			return ParametersHelper.GetPropertyValues(insertObject).Select(v => v.Key).ToArray();
		}
		
		private static string GetValues(IEnumerable<ColumnProvider> providers, object updateObject)
		{
			var updateValues = ParametersHelper.GetPropertyValues(updateObject);
			var sqlUpdatePairs = from p in providers
			                     join v in updateValues on p.Name equals v.Key
			                     select String.Format("{0}", p.GetValueSql(v.Value));
			return String.Join(", ", sqlUpdatePairs.ToArray());
		}

		private static string GetOperationValues(IEnumerable<ColumnProvider> providers, object updateObject)
		{
			var updateValues = ParametersHelper.GetPropertyValues(updateObject);
			var sqlUpdatePairs = from p in providers
			                     join v in updateValues on p.Name equals v.Key
			                     select String.Format("[{0}] = {1}", p.Name, p.GetValueSql(v.Value));
			return String.Join(", ", sqlUpdatePairs.ToArray());
		}

		private static string GetWhereClauseValues(IEnumerable<ColumnProvider> providers, object whereParameters)
		{
			var whereValues = ParametersHelper.GetPropertyValues(whereParameters);
			var sqlWherePairs = from p in providers
			                    join v in whereValues on p.Name equals v.Key
			                    select EqualitySql(p.Name, p.GetValueSql(v.Value));
			return String.Join(" AND ", sqlWherePairs.ToArray());
		}

		private static string EqualitySql(string name, string valueSql)
		{
			string equalitySign = valueSql != "null" ? "=" : "is";
			return String.Format("[{0}] {1} {2}", name, equalitySign, valueSql);
		}

		public int Delete(string tableName, object whereParameters)
		{
			var providers = GetColumnProviders(tableName);
			string whereClause = GetWhereClauseValues(providers, whereParameters);
			if (whereClause == String.Empty)
			{
				throw new DbRefactorException("Couldn't execute delete without where clause");
			}
			var query = String.Format("DELETE FROM [{0}] WHERE {1}", tableName, whereClause);
			return ExecuteNonQuery(query);
		}

		public object SelectScalar(string column, string tableName, object whereParameters)
		{
			var providers = GetColumnProviders(tableName);
			string whereClause = GetWhereClauseValues(providers, whereParameters);
			return ExecuteScalar("SELECT {0} FROM [{1}] WHERE {2}", column, tableName, whereClause);
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
			var foreignKeys = GetForeignKeys(filter);
			var keysSharedBetweenAllColumns = foreignKeys.GroupBy(key => key.Name)
				.Where(group => !foreignKeyColumns.Except(group.Select(k => k.ForeignColumn)).Any())
				.Select(g => g.Key).ToList();
			foreach (var key in keysSharedBetweenAllColumns)
			{
				DropConstraint(foreignKeyTable, key);
			}
		}
	}
}