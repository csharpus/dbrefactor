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
using DbRefactor.Providers.ForeignKeys;
using DbRefactor.Tools.DesignByContract;
using DbRefactor.Tools.Loggers;
using System.IO;

namespace DbRefactor.Providers
{
	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	internal sealed class TransformationProvider
	{
		private ILogger _logger = new Logger(false);

		private readonly IDatabaseEnvironment _environment;

		internal TransformationProvider(IDatabaseEnvironment environment)
		{
			_environment = environment;
		}

		internal IDatabaseEnvironment Environment
		{
			get
			{
				return _environment;
			}
		}

		/// <summary>
		/// Returns the event logger
		/// </summary>
		public ILogger Logger
		{
			get { return _logger; }
			set { _logger = value; }
		}

		/// <summary>
		/// Add a new table
		/// </summary>
		/// <param name="name">Table name</param>
		/// <param name="columns">Columns</param>
		/// <example>
		/// Adds the Test table with two columns:
		/// <code>
		/// Database.AddTable("Test",
		///	                  new Column("Id", typeof(int), ColumnProperties.PrimaryKey),
		///	                  new Column("Title", typeof(string), 100)
		///	                 );
		/// </code>
		/// </example>
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
					"SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}') AND name='{1}'",
					table,
					column))
			{
				return reader.Read();
			}
		}

		public bool TableExists(string table)
		{
			Check.RequireNonEmpty(table, "table");
			using (IDataReader reader =
				ExecuteQuery("SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}')",
				             table))
			{
				return reader.Read();
			}
		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table owning the constraint</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		//		public abstract bool ConstraintExists(string name, string table);
		public bool ConstraintExists(string name, string table)
		{
			using (IDataReader reader =
				ExecuteQuery("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')",
				             name))
			{
				return reader.Read();
			}
		}

		public void AlterColumn(string table, string sqlColumn)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(sqlColumn, "sqlColumn");
			ExecuteNonQuery("ALTER TABLE {0} ALTER COLUMN {1}", table, sqlColumn);
		}

		private class Relation
		{
			public Relation(string parent, string child)
			{
				_parent = parent;
				_child = child;
			}

			private string _parent;

			public string Parent
			{
				get
				{
					return _parent;
				}

				set
				{
					_parent = value;
				}
			}

			private string _child;

			public string Child
			{
				get
				{
					return _child;
				}

				set
				{
					_child = value;
				}
			}
		}

		private List<string> SortTablesByDependency(List<string> tables)
		{
			string query = @"
				SELECT f.name AS ForeignKey,
				   OBJECT_NAME(f.parent_object_id) AS TableName,
				   COL_NAME(fc.parent_object_id, 
				   fc.parent_column_id) AS ColumnName,
				   OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
				   COL_NAME(fc.referenced_object_id, 
				   fc.referenced_column_id) AS ReferenceColumnName
				FROM sys.foreign_keys AS f
				INNER JOIN sys.foreign_key_columns AS fc
				   ON f.OBJECT_ID = fc.constraint_object_id";
			List<Relation> relations = new List<Relation>();
			using (IDataReader reader = ExecuteQuery(query))
			{
				while (reader.Read())
				{
					relations.Add(
						new Relation(
							reader["ReferenceTableName"].ToString(), 
							reader["TableName"].ToString()));
				}
			}
			CheckCyclicDependencyAbsence(relations);
			tables.Sort(delegate(string a, string b) { return IsChildParent(a, b, relations) ? 1 : -1;});
			List<Relation> sortedRelations = new List<Relation>();
			List<string> sortedTables = new List<string>();
			return sortedTables;
		}

		private bool IsChildParent(string table1, string table2, List<Relation> relations)
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

		private void CheckCyclicDependencyAbsence(List<Relation> relations)
		{
			//TODO: implement
		}

		public void DeleteColumnConstraints(string table, string column)
		{
			string sqlContrainte =
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
			List<string> constraints = new List<string>();

			using (IDataReader reader = ExecuteQuery(sqlContrainte))
			{
				while (reader.Read())
				{
					constraints.Add(reader.GetString(0));
				}
			}
			// Can't share the connection so two phase modif
			foreach (string constraint in constraints)
			{
				RemoveForeignKey(table, constraint);
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
			List<string> indexes = new List<string>(columns.Length);
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
			string[] columnStrings = new string[columns.Length];
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
			AddForeignKey(name, primaryTable, new string[] { primaryColumn }, refTable, 
				new string[] { refColumn });
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

		public void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn, OnDelete ondelete)
		{
			AddForeignKey(name, primaryTable, new string[] { primaryColumn }, refTable,
				new string[] { refColumn }, ondelete);
		}

		// Not sure how SQL server handles ON UPDATRE & ON DELETE
		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, 
			string refTable, string[] refColumns, OnDelete constraint)
		{
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
		/// <param name="key">Constraint name</param>
		public void RemoveForeignKey(string table, string key)
		{
			Check.RequireNonEmpty(key, "key");
			Check.RequireNonEmpty(table, "table");
			//if (TableExists(table) && ConstraintExists(name, table))
			//{
			ExecuteNonQuery("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", table, key);
			//}
		}

		public string[] GetTables()
		{
			List<string> tables = new List<string>();

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

		public Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();

			using (IDataReader reader = ExecuteQuery("SELECT DATA_TYPE, COLUMN_NAME FROM information_schema.columns WHERE table_name = '{0}';", table))
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
			return _environment.ExecuteNonQuery(String.Format(sql, values));
		}

		/// <summary>
		/// Execute an SQL query returning results.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <param name="values">Replacements for {d} pattern.</param>
		/// <returns>A data iterator, <see cref="System.Data.IDataReader">IDataReader</see>.</returns>
		public IDataReader ExecuteQuery(string sql, params string[] values)
		{
			return _environment.ExecuteQuery(String.Format(sql, values));
		}

		public object ExecuteScalar(string sql, params string[] values)
		{
			return _environment.ExecuteScalar(String.Format(sql, values));
		}

		public IDataReader Select(string what, string from)
		{
			return Select(what, from, "1=1");
		}

		public IDataReader Select(string what, string from, string where)
		{
			return ExecuteQuery("SELECT {0} FROM {1} WHERE {2}", what, from, where);
		}

		public object SelectScalar(string what, string from)
		{
			return SelectScalar(what, from, "1=1");
		}

		public object SelectScalar(string what, string from, string where)
		{
			return ExecuteScalar("SELECT {0} FROM {1} WHERE {2}", what, from, where);
		}

		public int Update(string table, params string[] columnValues)
		{
			return ExecuteNonQuery("UPDATE [{0}] SET {1}", table, String.Join(", ", columnValues));
		}

		public int Insert(string table, params string[] columnValues)
		{
			string[] columns = new string[columnValues.Length];
			string[] values = new string[columnValues.Length];
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
			_environment.BeginTransaction();
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public void Rollback()
		{
			_environment.RollbackTransaction();
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void Commit()
		{
			_environment.CommitTransaction();
		}

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
				object version = SelectScalar("Version", "SchemaInfo");
				if (version == null)
				{
					return 0;
				}
				else
				{
					return Convert.ToInt32(version);
				}
			}
			set
			{
				CreateSchemaInfoTable();
				int count = Update("SchemaInfo", "Version=" + value);
				if (count == 0)
				{
					Insert("SchemaInfo", "Version=" + value);
				}
			}
		}

		private void CreateSchemaInfoTable()
		{
			//EnsureHasConnection();
			if (!TableExists("SchemaInfo"))
			{
				AddTable("SchemaInfo",
					new Column("Version", typeof(int), ColumnProperties.PrimaryKey));
			}
		}

		public bool TableHasIdentity(string table)
		{
			return Convert.ToInt32(ExecuteScalar("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')", table)) == 1;
		}

		public void ExecuteFile(string fileName)
		{
			using(StreamReader reader = File.OpenText(fileName))
			{
				string content = reader.ReadToEnd();
				ExecuteNonQuery(content);
			}
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
}