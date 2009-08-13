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
using System.Reflection;
using DbRefactor.Extensions;
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.ForeignKeys;
using DbRefactor.Providers.Properties;
using DbRefactor.Tools;
using DbRefactor.Tools.DesignByContract;
using System.IO;
using DbRefactor.Tools.Loggers;


namespace DbRefactor.Providers
{
	/// <summary>
	/// A 'tranformation' is an operation that modifies the database.
	/// This class might be changed in feature
	/// </summary>
	public sealed class TransformationProvider
	{
		public IDatabase GetDatabase()
		{
			return new Database(this, ProviderFactory.ColumnProviderFactory, propertyFactory);
		}

		private readonly IDatabaseEnvironment environment;
		private readonly SqlServerColumnMapper sqlServerColumnMapper;
		internal readonly ColumnPropertyProviderFactory propertyFactory;

		internal TransformationProvider(IDatabaseEnvironment environment, ILogger logger,
		                                SqlServerColumnMapper sqlServerColumnMapper,
		                                ColumnPropertyProviderFactory propertyFactory)
		{
			this.environment = environment;
			Logger = logger;
			this.sqlServerColumnMapper = sqlServerColumnMapper;
			this.propertyFactory = propertyFactory;
		}

		internal IDatabaseEnvironment Environment
		{
			get { return environment; }
		}

		/// <summary>
		/// Returns the event logger
		/// </summary>
		private ILogger Logger { get; set; }

		public void AddTable(string name, params ColumnProvider[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.Require(columns.Length > 0, "At least one column should be passed");
			var columnsSql = GetCreateColumnSql(columns);
			AddTable(name, columnsSql);
		}

		private static string GetCreateColumnSql(IEnumerable<ColumnProvider> columns)
		{
			return columns.Select(col => col.GetCreateColumnSql()).ComaSeparated();
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
			DeleteColumnConstraints(table, column);
			ExecuteNonQuery("ALTER TABLE {0} DROP COLUMN {1} ", table, column);
		}

		public void DropUnique(string table, params string[] columnNames)
		{
			Check.RequireNonEmpty(table, "table");

			var uniqueConstraints = new List<List<string>>();
			foreach (var columnName in columnNames)
			{
				uniqueConstraints.Add(GetUniqueConstraints(table, columnName));
			}
			var sharedConstraints = uniqueConstraints[0];
			foreach (var uniqueConstraintList in uniqueConstraints)
			{
				sharedConstraints = uniqueConstraintList.Intersect(sharedConstraints).ToList();
			}
			if (sharedConstraints.Count == 0)
			{
				string message = columnNames.Length == 1
				                 	? String.Format("Could not find any unique constraints for column '{0}' in table '{1}'", 
				                 	                columnNames[0], table)
				                 	: String.Format("Could not find any mutual unique constraints for columns '{0}' in table '{1}'",
				                 	                String.Join("', '", columnNames), table);
				throw new DbRefactorException(message);
			}
			foreach (var constraint in sharedConstraints)
			{
				DropConstraint(table, constraint);
			}
		}

		public void DropPrimaryKey(string table)
		{
			Check.RequireNonEmpty(table, "table");
			string primaryKeyColumn = GetPrimaryKeyColumn(table);
			Check.RequireNonEmpty(primaryKeyColumn, "primary key");

			List<string> indexes = GetConstraints(table, primaryKeyColumn);
			Check.Require(indexes.Count > 0, "Primary index not found.");

			DropConstraint(table, indexes[0]);
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
			ExecuteNonQuery("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'",
			                table, oldColumnName, newColumnName);
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
			ExecuteNonQuery("EXEC sp_rename '{0}', '{1}', 'OBJECT'",
			                oldName, newName);
		}

		/// <summary>
		/// Determines if a table exists.
		/// </summary>
		/// <param name="table">Table name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public bool TableExists(string table)
		{
			return ObjectExists(table);
		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public bool ConstraintExists(string name)
		{
			return ObjectExists(name);
		}

		/// <summary>
		/// Determines if a index exists.
		/// </summary>
		/// <param name="indexName">Index name</param>
		/// <returns><c>true</c> if the index exists.</returns>
		public bool IndexExists(string indexName)
		{
			return ObjectExists(indexName);
		}

		private void AlterColumn(string table, string sqlColumn)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(sqlColumn, "sqlColumn");
			ExecuteNonQuery("ALTER TABLE [{0}] ALTER COLUMN {1}", table, sqlColumn);
		}

		private List<Relation> GetTablesRelations()
		{
			var keys = GetForeignKeys();
			var relations = new List<Relation>();
			foreach (var key in keys)
			{
				relations.Add(new Relation(
				              	key.PrimaryTable,
				              	key.ForeignTable));
			}
			return relations;
		}

		public List<string> GetTablesSortedByDependency()
		{
			return SortTablesByDependency(new List<string>(GetTables()));
		}

		private List<string> SortTablesByDependency(List<string> tables)
		{
			List<Relation> relations = GetTablesRelations();
			return DependencySorter.Run(tables, relations);
		}

		public class DependencySorter
		{
			public static List<string> Run(List<string> tables, List<Relation> relations)
			{
				CheckCyclicDependencyAbsence(relations);
				var sortedTables = new List<string>(tables);
				sortedTables.Sort(delegate(string a, string b)
				                  	{
				                  		if (a == b) return 0;
				                  		return IsChildParent(a, b, relations) ? -1 : 1;
				                  	});
				return sortedTables;
			}

			private static void CheckCyclicDependencyAbsence(List<Relation> relations)
			{
				//TODO: implement
			}

			private static bool IsChildParent(string table1, string table2, IEnumerable<Relation> relations)
			{
				foreach (Relation r in relations)
				{
					if (r.Child == table1 && r.Parent == table2)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void DeleteColumnConstraints(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			var constraints = GetConstraints(table, column);
			foreach (string constraint in constraints)
			{
				DropConstraint(table, constraint);
			}
		}

		private void AddTable(string name, string columns)
		{
			ExecuteNonQuery("CREATE TABLE [{0}] ({1})", name, columns);
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
				new ForeignKeyConstraintMapper().Resolve(constraint));
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
				provider.AddProperty(propertyFactory.CreatePrimaryKey());
			}
			else if (!IsNull(table, provider.Name))
			{
				provider.AddProperty(propertyFactory.CreateNotNull());
			}

			if (IsIdentity(table, provider.Name))
			{
				provider.AddProperty(propertyFactory.CreateIdentity());
			}

			if (IsUnique(table, provider.Name))
			{
				provider.AddProperty(propertyFactory.CreateUnique());
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

		public int Delete(string table, string[] where)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(where.Length > 0, "You have to pass at least one criteria for delete");
			return ExecuteNonQuery("DELETE FROM [{0}] WHERE {1}", table, String.Join(" AND ", where));
		}

		public int Insert(string table, params string[] columnValues)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(columnValues.Length > 0, "You have to pass at least one column value");
			var columns = new string[columnValues.Length];
			var values = new string[columnValues.Length];
			int i = 0;

			foreach (string cs in columnValues)
			{
				columns[i] = cs.Split('=')[0];
				values[i] = cs.Split('=')[1];
				i++;
			}

			return ExecuteNonQuery(
				"INSERT INTO [{0}] ({1}) VALUES ({2})",
				table,
				String.Join(", ", columns),
				String.Join(", ", values));
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
		public void Rollback()
		{
			environment.RollbackTransaction();
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void Commit()
		{
			environment.CommitTransaction();
		}

		private string category;

		/// <summary>
		/// Get or set the current version of the database.
		/// This determines if the migrator should migrate up or down
		/// in the migration numbers.
		/// </summary>
		/// <remark>
		/// This value should not be modified inside a migration.
		/// </remark>
		public int CurrentVersion
		{
			get
			{
				CreateSchemaInfoTable();
				object version = SelectScalar("Version", "SchemaInfo", String.Format("Category='{0}'", category));
				return Convert.ToInt32(version);
			}
			set
			{
				CreateSchemaInfoTable();
				int count = Update("SchemaInfo", new[] {"Version=" + value}, String.Format("Category='{0}'", category));
				if (count == 0)
				{
					Insert("SchemaInfo", "Version=" + value, "Category='" + category + "'");
				}
			}
		}

		public string Category
		{
			get { return category; }
			set { category = value ?? String.Empty; }
		}

		private void CreateSchemaInfoTable()
		{
			if (TableExists("SchemaInfo")) return;
			GetDatabase().CreateTable("SchemaInfo")
				.Int("Version").PrimaryKey()
				.String("Category", 50, String.Empty)
				.Execute();
		}

		public void ExecuteFile(string fileName)
		{
			Check.RequireNonEmpty(fileName, "fileName");
			if (!File.Exists(fileName))
			{
				string migrationScriptPath = String.Format(@"{0}\Scripts\{1:000}\{2}", Directory.GetCurrentDirectory(),
				                                           CurrentVersion, fileName);
				Check.Ensure(File.Exists(migrationScriptPath), String.Format("Script file '{0}' has not found.", fileName));
				fileName = migrationScriptPath;
			}
			string content = File.ReadAllText(fileName);
			environment.ExecuteNonQuery(content);
		}

		public void ExecuteResource(string assemblyName, string resourceName)
		{
			Check.RequireNonEmpty(assemblyName, "assemblyName");
			Check.RequireNonEmpty(resourceName, "resourceName");

			Assembly assembly = Assembly.Load(assemblyName);
			resourceName = GetResource(resourceName, assembly);

			Stream stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
				throw new DbRefactorException(String.Format("Could not locate embedded resource '{0}' in assembly '{1}'",
				                                            resourceName, assemblyName));
			string script;
			using (var streamReader = new StreamReader(stream))
			{
				script = streamReader.ReadToEnd();
			}
			environment.ExecuteNonQuery(script);
		}

		private string GetResource(string resourceName, Assembly assembly)
		{
			foreach (var resource in assembly.GetManifestResourceNames())
			{
				if (resource.Contains(String.Format("._{0:000}.{1}", CurrentVersion, resourceName)))
					return resource;
			}
			return String.Empty;
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
				var indexes = FindIndexes(table, column);
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

		private static List<string> GetIndexesPresentInAllColumns(IList<List<string>> list)
		{
			var startIndexes = list[0];
			foreach (var indexList in list)
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
			if (provider.Properties.Select(p => p.GetType() == typeof (NotNullProvider)).Any())
			{
				// TODO: remove typeof
				provider.Properties.Remove(provider.Properties.Where(p => p.GetType() == typeof (NotNullProvider)).Single());
			}
			AlterColumn(tableName, provider.GetAlterColumnSql());
		}

		public void SetNotNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			// TODO: remove typeof
			bool isNotNull = provider.Properties.Where(t => t.GetType() == typeof (NotNullProvider)).Any();
			if (isNotNull) return;
			provider.AddProperty(propertyFactory.CreateNotNull());
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
			List<string> defaultConstraints = GetConstraintsByType(tableName, columnName, "D");
			foreach (var constraint in defaultConstraints)
			{
				DropConstraint(tableName, constraint);
			}
		}

		public void AlterColumn(string tableName, ColumnProvider columnProvider)
		{
			var provider = GetColumnProvider(tableName, columnProvider.Name);
			CopyProperties(provider, columnProvider);
			AlterColumn(tableName, columnProvider.GetAlterColumnSql());
		}

		private static void CopyProperties(ColumnProvider source, ColumnProvider destination)
		{
			foreach (var property in source.Properties)
			{
				destination.AddProperty(property);
			}
		}

		public IDataReader Select(string tableName, string[] columns, object whereParameters)
		{
			string query = String.Format("SELECT {0} FROM {1}", columns.ComaSeparated(), tableName);
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

		private static string GetOperationValues(IEnumerable<ColumnProvider> providers, object updateObject)
		{
			var updateValues = ParametersHelper.GetPropertyValues(updateObject);
			var sqlUpdatePairs = from p in providers 
			                     join v in updateValues on p.Name equals v.Key
			                     select String.Format("{0} = {1}", p.Name, p.GetValueSql(v.Value));
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
			return String.Format("{0} {1} {2}", name, equalitySign, valueSql);
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
			return ExecuteScalar("SELECT {0} FROM {1} WHERE {2}", column, tableName, whereClause);
		}

		public void DropForeignKey(string foreignKeyTable, string[] foreignKeyColumns, string primaryKeyTable, string[] primaryKeyColumns)
		{
			throw new NotImplementedException();
		}

		private List<string> GetConstraintsByType(string table, string column, string type)
		{
			var filter = new ConstraintFilter {TableName = table, ColumnName = column, ConstraintType = type};
			return GetConstraints(filter).Select(c => c.Name).ToList();
		}

		private List<Constraint> GetConstraints(ConstraintFilter filter)
		{
			var query = new ConstraintQueryBuilder(filter).BuildQuery();
			return ExecuteQuery(query).AsReadable().Select(r => new Constraint
			                                                    	{
			                                                    		Name = r.GetString(0),
                                                                        TableSchema = r.GetString(1),
																		TableName = r.GetString(2),
																		ColumnName = r.GetString(3),
																		ConstraintType = r.GetString(4)
			                                                    	}).ToList();
		}

		private class ConstraintFilter
		{
			public string TableName { get; set; }
			public string ColumnName { get; set; }
			public string ConstraintType { get; set; }
		}

		public class Constraint
		{
			public string Name { get; set; }
			public string TableSchema { get; set; }
			public string TableName { get; set; }
			public string ColumnName { get; set; }
			public string ConstraintType { get; set; }
		}

		private class ConstraintQueryBuilder
		{
			private readonly ConstraintFilter filter;
			private readonly List<string> restrictions = new List<string>();

			public ConstraintQueryBuilder(ConstraintFilter filter)
			{
				this.filter = filter;
			}

			public string BuildQuery()
			{
				// INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE - view that contains all constraints except defaults
				// sys.default_constraints - view for default constraints
				// http://blogs.msdn.com/sqltips/archive/2005/07/05/435882.aspx
				const string baseQuery = @"
WITH AllConstraints
AS (
	SELECT 
		Constraints.CONSTRAINT_NAME AS ConstraintName,
		Constraints.TABLE_SCHEMA AS TableSchema, 
		Constraints.TABLE_NAME AS TableName, 
		Constraints.COLUMN_NAME AS ColumnName, 
		Objects.[type] AS ConstraintType
	FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as Constraints
	JOIN sys.objects AS Objects 
		ON Objects.[Name] = Constraints.CONSTRAINT_NAME
UNION ALL
	SELECT
		DefaultConstraints.[name] AS ConstraintName,
		Schemas.[name] AS TableSchema,
		Objects.[name] As TableName,
		Columns.[name] AS ColumnName,
		Objects.[type] AS ConstraintType
	FROM sys.default_constraints AS DefaultConstraints
	JOIN sys.objects AS Objects
		ON Objects.object_id = DefaultConstraints.parent_object_id
	JOIN sys.columns AS Columns
		ON Columns.object_id = Objects.object_id 
			AND Columns.column_id = DefaultConstraints.parent_column_id
	JOIN sys.schemas AS Schemas
		ON Schemas.schema_id = Objects.schema_id)
SELECT ConstraintName, TableSchema, TableName, ColumnName, ConstraintType
FROM AllConstraints
				";
				AddTableRestriction();
				AddColumnRestriction();
				AddTypeRestriction();
				var whereClause = String.Join(" AND ", restrictions.ToArray());
				return whereClause != string.Empty ? baseQuery + " WHERE " + whereClause : baseQuery;
			}

			private void AddTypeRestriction()
			{
				if (filter.ConstraintType == null) return;
				restrictions.Add(String.Format("ConstraintType = '{0}'", filter.ConstraintType));
			}

			private void AddColumnRestriction()
			{
				if (filter.ColumnName == null) return;
				restrictions.Add(String.Format("ColumnName = '{0}'", filter.ColumnName));
			}

			private void AddTableRestriction()
			{
				if (filter.TableName == null) return;
				restrictions.Add(String.Format("TableName = '{0}'", filter.TableName));
			}
		}

		public List<string> GetConstraints(string table, string column)
		{
			var filter = new ConstraintFilter { TableName = table, ColumnName = column};
			return GetConstraints(filter).Select(c => c.Name).ToList();
		}

		public List<string> GetUniqueConstraints(string table, string column)
		{
			var filter = new ConstraintFilter {TableName = table, ColumnName = column, ConstraintType = "UQ"};
			return GetConstraints(filter).Select(c => c.Name).ToList();
		}

		private string GetPrimaryKeyColumn(string table)
		{
			const string sql =
				@"
						SELECT [name]
						FROM syscolumns 
						 WHERE [id] IN (SELECT [id] 
										  FROM sysobjects 
										 WHERE [name] = '{0}')
						   AND colid IN (SELECT SIK.colid 
										   FROM sysindexkeys SIK 
										   JOIN sysobjects SO ON SIK.[id] = SO.[id]  
										  WHERE SIK.indid = 2
											AND SO.[name] = '{0}')
						";

			return Convert.ToString(ExecuteScalar(sql, table));
		}

		public List<ForeignKey> GetForeignKeys()
		{
			const string query =
				@"
				SELECT f.name AS Name,
				   OBJECT_NAME(f.parent_object_id) AS ForeignTable,
				   COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ForeignColumn,
				   OBJECT_NAME (f.referenced_object_id) AS PrimaryTable,
				   COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS PrimaryColumn
				FROM sys.foreign_keys AS f
				INNER JOIN sys.foreign_key_columns AS fc
				   ON f.OBJECT_ID = fc.constraint_object_id
				";
			return ExecuteQuery(query).AsReadable()
				.Select(r => new ForeignKey
				{
					Name = r["Name"].ToString(),
					ForeignTable = r["ForeignTable"].ToString(),
					ForeignColumn = r["ForeignColumn"].ToString(),
					PrimaryTable = r["PrimaryTable"].ToString(),
					PrimaryColumn = r["PrimaryColumn"].ToString()
				}).ToList();
		}

		private bool IsUnique(string table, string column)
		{
			return
				Convert.ToBoolean(
					ExecuteScalar(
						@"SELECT COUNT(c.CONSTRAINT_NAME) 
				FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as c
				join sys.objects as s on c.CONSTRAINT_NAME = s.Name and type = 'UQ'
				where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'",
						table, column));
		}

		private bool IsPrimaryKey(string table, string column)
		{
			return
				Convert.ToBoolean(
					ExecuteScalar(
						@"SELECT COUNT(c.CONSTRAINT_NAME) 
				FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as c
				join sys.objects as s on c.CONSTRAINT_NAME = s.Name and type = 'PK'
				where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'",
						table, column));
		}

		internal List<ColumnProvider> GetColumnProviders(string table)
		{
			var providers = new List<ColumnProvider>();

			using (
				IDataReader reader =
					ExecuteQuery(
						"SELECT DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, COLUMN_DEFAULT FROM information_schema.columns WHERE table_name = '{0}';",
						table))
			{
				while (reader.Read())
				{
					ColumnProvider provider = GetProvider(reader);
					providers.Add(provider);
				}
			}
			foreach (var provider in providers)
			{
				AddProviderProperties(table, provider);
			}
			return providers;
		}

		private bool IsIdentity(string table, string column)
		{
			return Convert.ToBoolean(ExecuteScalar(@"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'),'{1}','IsIdentity')", table, column));
		}

		private bool IsNull(string table, string column)
		{
			return Convert.ToBoolean(ExecuteScalar(@"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'),'{1}','AllowsNull')", table, column));
		}

		public bool TableHasIdentity(string table)
		{
			Check.RequireNonEmpty(table, "table");
			return Convert.ToInt32(ExecuteScalar("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')", table)) == 1;
		}

		private List<string> FindIndexes(string tableName, string columnName)
		{
			var query =
				String.Format(
					@"SELECT o.name as [TableName], i.name as [IndexName], c.name as [ColumnName]
							FROM sysobjects o
							JOIN sysindexes i ON i.id = o.id
							JOIN sysindexkeys ik ON ik.id = i.id
								AND ik.indid = i.indid
							JOIN syscolumns c ON c.id = ik.id
								AND c.colid = ik.colid
							WHERE i.indid BETWEEN 2 AND 254
								AND indexproperty(o.id, i.name, 'IsStatistics') = 0
								AND indexproperty(o.id, i.name, 'IsHypothetical') = 0
								AND o.Name = '{0}' and c.Name = '{1}'",
					tableName, columnName);
			return ExecuteQuery(query, tableName, columnName).AsReadable()
				.Select(r => r["IndexName"].ToString()).ToList();
		}

		private ColumnProvider GetColumnProvider(string tableName, string columnName)
		{
			ColumnProvider provider;
			using (
				IDataReader reader =
					ExecuteQuery(
						"SELECT DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, COLUMN_DEFAULT FROM information_schema.columns WHERE table_name = '{0}' and column_name = '{1}'",
						tableName, columnName))
			{
				if (!reader.Read())
				{
					throw new DbRefactorException(String.Format("Couldn't find column '{0}' in table '{1}'", columnName, tableName));
				}
				provider = GetProvider(reader);
			}
			AddProviderProperties(tableName, provider);
			return provider;
		}

		public bool ColumnExists(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			if (!TableExists(table))
			{
				Logger.Warn("Table {0} does not exists", table);
				return false;
			}
			var query = String.Format("SELECT TOP 1 * FROM syscolumns WHERE id = object_id('{0}') AND name = '{1}'",
									  table,
									  column);
			return ExecuteQuery(query).AsReadable().Any();
		}

		private bool ObjectExists(string name)
		{
			Check.RequireNonEmpty(name, "name");
			var query = String.Format("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')", name);
			return ExecuteQuery(query).AsReadable().Any();
		}

		public string[] GetTables()
		{
			const string query = "SELECT name FROM sysobjects WHERE xtype = 'U'";
			return ExecuteQuery(query).AsReadable().Select(r => r.GetString(0)).ToArray();
		}

	}
}