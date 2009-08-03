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
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.ForeignKeys;
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
		private readonly IDatabaseEnvironment environment;
		private readonly ColumnProviderFactory columnProviderFactory;

		internal TransformationProvider(IDatabaseEnvironment environment, ILogger logger,
		                                ColumnProviderFactory columnProviderFactory)
		{
			this.environment = environment;
			Logger = logger;
			this.columnProviderFactory = columnProviderFactory;
		}

		internal IDatabaseEnvironment Environment
		{
			get { return environment; }
		}

		/// <summary>
		/// Returns the event logger
		/// </summary>
		private ILogger Logger { get; set; }

		public void AddTable(string name, params Column[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.Require(columns.Length > 0, "At least one column should be passed");
			string columnsAndIndexes = ColumnsAndIndexes(columns);
			AddTable(name, columnsAndIndexes);
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
			if (!ColumnExists(table, column))
			{
				Logger.Warn("Column {0} does not exists", column);
				return;
			}
			DeleteColumnConstraints(table, column);
			ExecuteNonQuery("ALTER TABLE {0} DROP COLUMN {1} ", table, column);
		}

		internal void DropUnique(string table, string columnName)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Ensure(IsUnique(table, columnName), "column is not unique");

			List<string> indexes = GetConstraints(table, columnName);
			Check.Require(indexes.Count > 0, "Index not found.");

			DropConstraint(table, indexes[0]);
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

		private List<string> GetConstraints(string table, string column)
		{
			string sql =
				String.Format(
					@"WITH constraint_depends
						AS
						(
							SELECT c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.CONSTRAINT_NAME
							FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as c
							UNION ALL
							SELECT s.name, o.name, c.name, d.name
							FROM sys.default_constraints AS d
							JOIN sys.objects AS o
								ON o.object_id = d.parent_object_id
							JOIN sys.columns AS c
								ON c.object_id = o.object_id AND c.column_id = d.parent_column_id
							JOIN sys.schemas AS s
								ON s.schema_id = o.schema_id)
					SELECT c.CONSTRAINT_NAME
					FROM constraint_depends as c
					WHERE c.TABLE_NAME = '{0}' AND c.COLUMN_NAME = '{1}';",
					table, column);

			var constraints = new List<string>();
			using (IDataReader reader = ExecuteQuery(sql))
			{
				while (reader.Read())
				{
					constraints.Add(reader.GetString(0));
				}
			}
			return constraints;
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

		/// <summary>
		/// Removes an existing table from the database
		/// </summary>
		/// <param name="name">Table name</param>
		public void DropTable(string name)
		{
			Check.RequireNonEmpty(name, "name");
			if (!TableExists(name))
			{
				Logger.Warn("Table {0} does not exist", name);
				return;
			}
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

		public bool ColumnExists(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			if (!TableExists(table))
			{
				Logger.Warn("Table {0} does not exists", table);
				return false;
			}
			using (IDataReader reader =
				ExecuteQuery(
					"SELECT TOP 1 * FROM syscolumns WHERE id = object_id('{0}') AND name = '{1}'",
					table,
					column))
			{
				return reader.Read();
			}
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

		private bool ObjectExists(string name)
		{
			Check.RequireNonEmpty(name, "name");
			using (IDataReader reader = ExecuteQuery("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')", name))
			{
				return reader.Read();
			}
		}

		public void AlterColumn(string table, string sqlColumn)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(sqlColumn, "sqlColumn");
			ExecuteNonQuery("ALTER TABLE [{0}] ALTER COLUMN {1}", table, sqlColumn);
		}

		public List<ForeignKey> GetForeignKeys()
		{
			string query =
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
			var keys = new List<ForeignKey>();
			using (IDataReader reader = ExecuteQuery(query))
			{
				while (reader.Read())
				{
					keys.Add(
						new ForeignKey
							{
								Name = reader["Name"].ToString(),
								ForeignTable = reader["ForeignTable"].ToString(),
								ForeignColumn = reader["ForeignColumn"].ToString(),
								PrimaryTable = reader["PrimaryTable"].ToString(),
								PrimaryColumn = reader["PrimaryColumn"].ToString()
							});
				}
			}
			return keys;
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

		private static string ColumnsAndIndexes(Column[] columns)
		{
			string indexes = JoinIndexes(columns);
			return JoinColumns(columns) + (indexes != null ? "," + indexes : string.Empty);
		}

		private static string JoinIndexes(Column[] columns)
		{
			var indexes = new List<string>(columns.Length);
			foreach (Column column in columns)
			{
				string indexSql = column.IndexSQL();
				if (indexSql != null)
				{
					indexes.Add(indexSql);
				}
			}

			if (indexes.Count == 0)
			{
				return null;
			}

			return String.Join(", ", indexes.ToArray());
		}


		private static string JoinColumns(Column[] columns)
		{
			var columnStrings = new string[columns.Length];
			int i = 0;
			foreach (Column column in columns)
			{
				columnStrings[i++] = column.ColumnSQL();
			}
			return String.Join(", ", columnStrings);
		}

		public void AlterColumn(string table, Column column)
		{
			//if (!ColumnExists(table, column.Name))
			//{
			//    Logger.Warn("Column {0}.{1} does not exists", table, column.Name);
			//    return;
			//}
			AlterColumn(table, column.ColumnSQL());
		}

		public void AddColumn(string table, Column column)
		{
			//if (ColumnExists(table, column.Name))
			//{
			//    Logger.Warn("Column {0}.{1} already exists", table, column.Name);
			//    return;
			//}
			AddColumn(table, column.ColumnSQL());
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public void AddColumn(string table, string column, Type type)
		{
			AddColumn(table, column, type, 0, ColumnProperties.Null, null);
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public void AddColumn(string table, string column, Type type, int size)
		{
			AddColumn(table, column, type, size, ColumnProperties.Null, null);
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public void AddColumn(string table, string column, Type type, ColumnProperties property)
		{
			AddColumn(table, column, type, 0, property, null);
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public void AddColumn(string table, string column, Type type, int size, ColumnProperties property)
		{
			AddColumn(table, column, type, size, property, null);
		}

		/// <summary>
		/// Add a new column to an existing table.
		/// </summary>
		/// <param name="table">Table to which to add the column</param>
		/// <param name="column">Column name</param>
		/// <param name="type">Date type of the column</param>
		/// <param name="size">Max length of the column</param>
		/// <param name="property">Properties of the column, see <see cref="ColumnProperties">ColumnProperties</see>,</param>
		/// <param name="defaultValue">Default value</param>
		public void AddColumn(string table, string column, Type type, int size, ColumnProperties property, object defaultValue)
		{
			AddColumn(table, new Column(column, type, size, property, defaultValue));
		}

		public void AddColumn(string table, string sqlColumn)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(sqlColumn, "sqlColumn");
			ExecuteNonQuery("ALTER TABLE [{0}] ADD {1}", table, sqlColumn);
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
			//if (ConstraintExists(name, table))
			//{
			//    Logger.Warn("Primary key {0} already exists", name);
			//    return;
			//}
			ExecuteNonQuery("ALTER TABLE [{0}] ADD CONSTRAINT {1} PRIMARY KEY ({2}) ",
			                table, name, String.Join(",", columns));
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable,
		                               string refColumn)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn,
			              refTable, refColumn);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </see>
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string[] primaryColumns,
		                               string refTable, string[] refColumns)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns,
			              refTable, refColumns);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable,
		                               string refColumn, OnDelete ondelete)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn,
			              refTable, refColumn, ondelete);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </see>
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string[] primaryColumns,
		                               string refTable, string[] refColumns, OnDelete ondelete)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns,
			              refTable, refColumns, ondelete);
		}

		/// <summary>
		/// Append a foreign key (relation) between two tables.
		/// tables.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="primaryTable">Table name containing the primary key</param>
		/// <param name="primaryColumn">Primary key column name</param>
		/// <param name="refTable">Foreign table name</param>
		/// <param name="refColumn">Foreign column name</param>
		public void AddForeignKey(string name, string primaryTable, string primaryColumn,
		                          string refTable, string refColumn)
		{
			AddForeignKey(name, primaryTable, new[] {primaryColumn}, refTable,
			              new[] {refColumn});
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddForeignKey(string, string, string, string, string)">
		/// AddForeignKey(string, string, string, string, string)
		/// </see>
		/// </summary>
		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns,
		                          string refTable, string[] refColumns)
		{
			AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns,
			              OnDelete.NoAction);
		}

		public void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn,
		                          OnDelete ondelete)
		{
			AddForeignKey(name, primaryTable, new[] {primaryColumn}, refTable,
			              new[] {refColumn}, ondelete);
		}

		// Not sure how SQL server handles ON UPDATRE & ON DELETE
		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns,
		                          string refTable, string[] refColumns, OnDelete constraint)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(primaryTable, "primaryTable");
			Check.RequireNonEmpty(refTable, "refTable");
			Check.Require(primaryColumns.Length > 0, "You have to pass at least one primary column");
			Check.Require(refColumns.Length > 0, "You have to pass at least one ref column");
			//if (ConstraintExists(name, primaryTable))
			//{
			//    Logger.Warn("The contraint {0} already exists", name);
			//    return;
			//}
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

			ExecuteNonQuery("ALTER TABLE [dbo].[{0}] DROP CONSTRAINT [{1}]", table, name);
		}

		public string[] GetTables()
		{
			var tables = new List<string>();

			using (IDataReader reader =
				ExecuteQuery("SELECT name FROM sysobjects WHERE xtype = 'U'"))
			{
				while (reader.Read())
				{
					tables.Add(reader[0].ToString());
				}
			}
			return tables.ToArray();
		}

		private Dictionary<string, Func<ColumnData, ColumnProvider>> GetTypesMap()
		{
			return new Dictionary<string, Func<ColumnData, ColumnProvider>>
			       	{
			       		{"bigint", columnProviderFactory.CreateLong},
			       		{"binary", columnProviderFactory.CreateBinary},
			       		{"bit", columnProviderFactory.CreateBoolean},
			       		{"char", columnProviderFactory.CreateString},
			       		{"datetime", columnProviderFactory.CreateDateTime},
			       		{"decimal", columnProviderFactory.CreateDecimal},
			       		{"float", columnProviderFactory.CreateFloat},
			       		{"image", columnProviderFactory.CreateBinary},
			       		{"int", columnProviderFactory.CreateInt},
			       		{"money", columnProviderFactory.CreateDecimal},
			       		{"nchar", columnProviderFactory.CreateString},
			       		{"ntext", columnProviderFactory.CreateText},
			       		{"numeric", columnProviderFactory.CreateDecimal},
			       		{"nvarchar", columnProviderFactory.CreateString},
			       		{"real", columnProviderFactory.CreateFloat},
			       		{"smalldatetime", columnProviderFactory.CreateDateTime},
			       		{"smallint", columnProviderFactory.CreateInt},
			       		{"smallmoney", columnProviderFactory.CreateDecimal},
			       		{"sql_variant", columnProviderFactory.CreateBinary},
			       		{"text", columnProviderFactory.CreateText},
			       		{"timestamp", columnProviderFactory.CreateDateTime},
			       		{"tinyint", columnProviderFactory.CreateInt},
			       		{"uniqueidentifier", columnProviderFactory.CreateString},
			       		{"varbinary", columnProviderFactory.CreateBinary},
			       		{"varchar", columnProviderFactory.CreateString},
			       		{"xml", columnProviderFactory.CreateString}
			       	};
		}

		private const int GuidLength = 38; // 36 symbols in guid + 2 curly brackets

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
					var data = new ColumnData
					           	{
					           		Name = reader["COLUMN_NAME"].ToString(),
					           		DataType = reader["DATA_TYPE"].ToString(),
					           		Length = NullSafeGet<int>(reader, "CHARACTER_MAXIMUM_LENGTH"),
					           		Precision = NullSafeGet<byte>(reader, "NUMERIC_PRECISION"),
					           		Radix = NullSafeGet<short>(reader, "NUMERIC_PRECISION_RADIX"),
					           		DefaultValue = GetDefaultValue(reader["COLUMN_DEFAULT"])
					           	};
					providers.Add(GetTypesMap()[data.DataType](data));
				}
			}
			foreach (var provider in providers)
			{
				if (IsPrimaryKey(table, provider.Name))
				{
					provider.AddProperty(new PrimaryKeyProvider());
				}
				else if (!IsNull(table, provider.Name))
				{
					provider.AddProperty(new NotNullProvider());
				}

				if (IsIdentity(table, provider.Name))
				{
					provider.AddProperty(new IdentityProvider());
				}

				if (IsUnique(table, provider.Name))
				{
					provider.AddProperty(new UniqueProvider());
				}
			}
			return providers;
		}

		private object GetDefaultValue(object databaseValue)
		{
			return databaseValue == DBNull.Value ? null : databaseValue;
		}

		private bool IsIdentity(string table, string column)
		{
			return Convert.ToBoolean(ExecuteScalar(@"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'),'{1}','IsIdentity')", table, column));
		}

		private bool IsNull(string table, string column)
		{
			return Convert.ToBoolean(ExecuteScalar(@"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'),'{1}','AllowsNull')", table, column));
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

		public Column[] GetColumns(string table)
		{
			Check.RequireNonEmpty(table, "table");
			List<Column> columns = new List<Column>();

			using (
				IDataReader reader =
					ExecuteQuery("SELECT DATA_TYPE, COLUMN_NAME FROM information_schema.columns WHERE table_name = '{0}';", table))
			{
				while (reader.Read())
				{
					Type t = reader["DATA_TYPE"].ToString() == "datetime" ? typeof (DateTime) : typeof (string);
					columns.Add(new Column(reader["COLUMN_NAME"].ToString(), t));
				}
			}
			return columns.ToArray();
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

		public IDataReader Select(string what, string from)
		{
			Check.RequireNonEmpty(what, "what");
			Check.RequireNonEmpty(from, "from");
			return Select(what, from, "1=1");
		}

		public IDataReader Select(string what, string from, string where)
		{
			Check.RequireNonEmpty(what, "what");
			Check.RequireNonEmpty(from, "from");
			Check.RequireNonEmpty(where, "where");
			return ExecuteQuery("SELECT {0} FROM {1} WHERE {2}", what, from, where);
		}

		public object SelectScalar(string what, string from)
		{
			Check.RequireNonEmpty(what, "what");
			Check.RequireNonEmpty(from, "from");
			return SelectScalar(what, from, "1=1");
		}

		public object SelectScalar(string what, string from, string where)
		{
			Check.RequireNonEmpty(what, "what");
			Check.RequireNonEmpty(from, "from");
			Check.RequireNonEmpty(where, "where");
			return ExecuteScalar("SELECT {0} FROM {1} WHERE {2}", what, from, where);
		}


		public int Update(string table, params string[] columnValues)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(columnValues.Length > 0, "You have to pass at least one column value");
			return ExecuteNonQuery("UPDATE [{0}] SET {1}", table, String.Join(", ", columnValues));
		}

		public int Update(string table, string[] columnValues, string[] where)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(columnValues.Length > 0, "You have to pass at least one column value");
			Check.Require(where.Length > 0, "You have to pass at least one criteria for update");
			return ExecuteNonQuery("UPDATE [{0}] SET {1} WHERE {2}", table, String.Join(", ", columnValues),
			                       String.Join(" AND ", where));
		}

		[Obsolete("Old method will be removed in a feature")]
		public int Update(string table, string[] columnValues, string where)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(columnValues.Length > 0, "You have to pass at least one column value");
			Check.RequireNonEmpty(where, "where");
			return ExecuteNonQuery("UPDATE [{0}] SET {1} WHERE {2}", table, String.Join(", ", columnValues), where);
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
				if (version == null)
				{
					return 0;
				}
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
			//EnsureHasConnection();
			if (!TableExists("SchemaInfo"))
			{
				AddTable("SchemaInfo",
				         new Column("Version", typeof (int), ColumnProperties.PrimaryKey));
			}
			if (!ColumnExists("SchemaInfo", "Category"))
			{
				AddColumn("SchemaInfo", new Column("Category", typeof (string), 50, ColumnProperties.Null));
				Update("SchemaInfo", "Category=''");
			}
		}

		public bool TableHasIdentity(string table)
		{
			Check.RequireNonEmpty(table, "table");
			return Convert.ToInt32(ExecuteScalar("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')", table)) == 1;
		}

		public void ExecuteFile(string fileName)
		{
			Check.RequireNonEmpty(fileName, "fileName");
			Check.Ensure(File.Exists(fileName), "Migration file could not be found");

			string content = File.ReadAllText(fileName);
			environment.ExecuteNonQuery(content);
		}

		public void ExecuteResource(string assemblyName, string filePath)
		{
			Check.RequireNonEmpty(assemblyName, "assemblyName");
			Check.RequireNonEmpty(filePath, "filePath");

			System.Reflection.Assembly a = System.Reflection.Assembly.Load(assemblyName);
			Stream stream = a.GetManifestResourceStream(filePath);
			Check.Require(stream != null,
			              String.Format("Could not locate embedded resource '{0}' in assembly '{1}'", filePath, assemblyName));

			string script;
			using (var streamReader = new StreamReader(stream))
			{
				script = streamReader.ReadToEnd();
			}
			environment.ExecuteNonQuery(script);
		}

		#region Obsolete

		[Obsolete("Use DropTable instead")]
		public void RemoveTable(string name)
		{
			DropTable(name);
		}

		[Obsolete("Use DropColumn instead")]
		public void RemoveColumn(string table, string column)
		{
			DropColumn(table, column);
		}

		#endregion
	}

	public class ColumnData
	{
		public string DataType { get; set; }

		public string Name { get; set; }

		public int? Length { get; set; }

		public int? Precision { get; set; }

		public int? Radix { get; set; }

		public object DefaultValue { get; set; }
	}

	public class Relation
	{
		public Relation(string parent, string child)
		{
			Parent = parent;
			Child = child;
		}

		public string Parent { get; set; }

		public string Child { get; set; }
	}

	public class ForeignKey
	{
		public string Name { get; set; }
		public string PrimaryTable { get; set; }
		public string PrimaryColumn { get; set; }
		public string ForeignTable { get; set; }
		public string ForeignColumn { get; set; }
	}
}