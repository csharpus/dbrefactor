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
using System.Data;
using Migrator.Loggers;
using Migrator.Providers.ForeignKeys;
using ForeignKeyConstraint = Migrator.Providers.ForeignKeys.ForeignKeyConstraint;
using Migrator.Providers.TypeToSqlProviders;
using System.Collections;
using Migrator.Providers.ColumnPropertiesMappers;
using Migrator.Columns;
using System.Collections.Generic;

namespace Migrator.Providers
{
	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	public sealed class TransformationProvider
	{
		private ILogger _logger = new Logger(false);

		private IDatabaseEnvironment _environment;

		public TransformationProvider(IDatabaseEnvironment environment)
		{
			_environment = environment;
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
			#region Parameter validation
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (columns.Length == 0)
			{
				throw new ArgumentException("You should pass at least one column");
			}
			#endregion
			if (TableExists(name))
			{
				Logger.Warn("Table {0} already exists", name);
				return;
			}

			List<IColumnPropertiesMapper> columnProviders = new List<IColumnPropertiesMapper>(columns.Length);
			foreach (Column column in columns)
			{
				IColumnPropertiesMapper mapper = GetAndMapColumnProperties(column);
				columnProviders.Add(mapper);
			}

			IColumnPropertiesMapper[] columnArray = columnProviders.ToArray();
			string columnsAndIndexes = JoinColumnsAndIndexes(columnArray);
			AddTable(name, columnsAndIndexes);
		}

		public void RemoveTable(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (TableExists(name))
			{
				ExecuteNonQuery("DROP TABLE {0}", name);
			}
		}

		public void AddColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery("ALTER TABLE {0} ADD {1}", table, sqlColumn);
		}

		public void AlterColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery("ALTER TABLE {0} ALTER COLUMN {1}", table, sqlColumn);
		}

		private void DeleteColumnConstraints(string table, string column)
		{
			string sqlContrainte =
				String.Format(
					"SELECT cont.name FROM SYSOBJECTS cont, SYSCOLUMNS col, SYSCONSTRAINTS cnt "
					+ "WHERE cont.parent_obj = col.id AND cnt.constid = cont.id AND cnt.colid=col.colid "
					+ "AND col.name = '{1}' AND col.id = object_id('{0}')",
					table, column);
			ArrayList constraints = new ArrayList();

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
				RemoveForeignKey(constraint, table);
			}
		}

		public void RemoveColumn(string table, string column)
		{
			DeleteColumnConstraints(table, column);
			if (ColumnExists(table, column))
			{
				ExecuteNonQuery("ALTER TABLE {0} DROP COLUMN {1} ", table, column);
			}
		}

		public void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			ExecuteNonQuery("EXEC sp_rename '{0}.[{1}]', '{2}', 'COLUMN'",
				table, oldColumnName, newColumnName);
		}

		public void RenameTable(string oldTableName, string newTableName)
		{
			ExecuteNonQuery(
				"EXEC sp_rename '{0}', '{1}', 'OBJECT'",
					oldTableName, newTableName);
		}

		public bool ColumnExists(string table, string column)
		{
			#region Parameter validation
			if (table == null)
			{
				throw new ArgumentNullException("table");
			}
			if (column == null)
			{
				throw new ArgumentNullException("column");
			}
			if (table == String.Empty)
			{
				throw new ArgumentException("Should not be empty", "table");
			}
			if (column == String.Empty)
			{
				throw new ArgumentException("Should not be empty", "column");
			}
			#endregion
			if (!TableExists(table))
				return false;

			using (IDataReader reader =
				ExecuteQuery(
					"SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}') and name='{1}'", 
					table, 
					column))
			{
				return reader.Read();
			}
		}

		public bool TableExists(string table)
		{
			using (IDataReader reader =
				ExecuteQuery(
					"SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}')",
					table))
			{
				return reader.Read();
			}
		}

		private string JoinColumnsAndIndexes(IColumnPropertiesMapper[] columns)
		{
			string indexes = JoinIndexes(columns);
			return JoinColumns(columns) + (indexes != null ? "," + indexes : string.Empty);
		}

		private void AddTable(string name, string columns)
		{
			ExecuteNonQuery("CREATE TABLE {0} ({1})", name, columns);
		}

		private IColumnPropertiesMapper GetAndMapColumnProperties(Column column)
		{
			IColumnPropertiesMapper mapper = GetColumnMapper(column);
			MapColumnProperties(mapper, column);
			return mapper;
		}

		private void MapColumnProperties(IColumnPropertiesMapper mapper, Column column)
		{
			mapper.Name = column.Name;
			ColumnProperties properties = column.ColumnProperty;
			if ((properties & ColumnProperties.NotNull) == ColumnProperties.NotNull)
			{
				mapper.NotNull();
			}
			if ((properties & ColumnProperties.PrimaryKey) == ColumnProperties.PrimaryKey)
			{
				mapper.PrimaryKey();
			}
			if ((properties & ColumnProperties.Identity) == ColumnProperties.Identity)
			{
				mapper.Identity();
			}
			if ((properties & ColumnProperties.Unique) == ColumnProperties.Unique)
			{
				mapper.Unique();
			}
			if ((properties & ColumnProperties.Indexed) == ColumnProperties.Indexed)
			{
				mapper.Indexed();
			}
			if ((properties & ColumnProperties.Unsigned) == ColumnProperties.Unsigned)
			{
				mapper.Unsigned();
			}
			if (column.DefaultValue != null)
			{
				if (column.Type == typeof(char) || column.Type == typeof(string))
				{
					mapper.Default(String.Format("'{0}'", column.DefaultValue));
				}
				if (column.Type == typeof(bool))
				{
					mapper.Default(Convert.ToBoolean(column.DefaultValue) ? "1" : "0");
				}
				else
				{
					mapper.Default(column.DefaultValue.ToString());
				}
			}
		}

		private IColumnPropertiesMapper GetColumnMapper(Column column)
		{
			if (column.Type == typeof(char))
			{
				if (column.Size <= Convert.ToInt32(byte.MaxValue))
					return TypeToSqlProvider.Char(Convert.ToByte(column.Size));
				else if (column.Size <= Convert.ToInt32(ushort.MaxValue))
					return TypeToSqlProvider.Text;
				else
					return TypeToSqlProvider.LongText;
			}

			if (column.Type == typeof(string))
			{
				if (column.Size <= 255)
					return TypeToSqlProvider.String(Convert.ToUInt16(column.Size));
				else if (column.Size <= Convert.ToInt32(ushort.MaxValue))
					return TypeToSqlProvider.Text;
				else
					return TypeToSqlProvider.LongText;
			}

			if (column.Type == typeof(int))
			{
				if ((column.ColumnProperty & ColumnProperties.PrimaryKey) == ColumnProperties.PrimaryKey)
					return TypeToSqlProvider.PrimaryKey;
				else
					return TypeToSqlProvider.Integer;
			}
			if (column.Type == typeof(long))
				return TypeToSqlProvider.Long;

			if (column.Type == typeof(float))
				return TypeToSqlProvider.Float;

			if (column.Type == typeof(double))
			{
				if (column.Size == 0)
					return TypeToSqlProvider.Double;
				else
					return TypeToSqlProvider.Decimal(column.Size);
			}

			if (column.Type == typeof(decimal))
			{
				if (typeof(DecimalColumn).IsAssignableFrom(column.GetType()))
				{
					return TypeToSqlProvider.Decimal(column.Size, (column as DecimalColumn).Remainder);
				}
				else
				{
					return TypeToSqlProvider.Decimal(column.Size);
				}
			}

			if (column.Type == typeof(bool))
				return TypeToSqlProvider.Bool;

			if (column.Type == typeof(DateTime))
				return TypeToSqlProvider.DateTime;

			if (column.Type == typeof(byte[]))
			{
				if (column.Size <= Convert.ToInt32(byte.MaxValue))
					return TypeToSqlProvider.Binary(Convert.ToByte(column.Size));
				else if (column.Size <= Convert.ToInt32(ushort.MaxValue))
					return TypeToSqlProvider.Blob;
				else
					return TypeToSqlProvider.LongBlob;
			}

			throw new ArgumentOutOfRangeException("column.Type", "The " + column.Type.ToString() + " type is not supported");
		}

		private string JoinIndexes(IColumnPropertiesMapper[] columns)
		{
			ArrayList indexes = new ArrayList(columns.Length);
			foreach (IColumnPropertiesMapper column in columns)
			{
				string indexSql = column.IndexSql;
				if (indexSql != null)
					indexes.Add(indexSql);
			}

			if (indexes.Count == 0)
				return null;

			return String.Join(", ", (string[])indexes.ToArray(typeof(string)));
		}


		private string JoinColumns(IColumnPropertiesMapper[] columns)
		{
			string[] columnStrings = new string[columns.Length];
			int i = 0;
			foreach (IColumnPropertiesMapper column in columns)
			{
				columnStrings[i++] = column.ColumnSql;
			}
			return String.Join(", ", columnStrings);
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

		public void AddColumn(string table, Column column)
		{
			if (ColumnExists(table, column.Name))
			{
				Logger.Warn("Column {0}.{1} already exists", table, column.Name);
				return;
			}

			IColumnPropertiesMapper mapper = GetAndMapColumnProperties(column);

			AddColumn(table, mapper.ColumnSql);
		}

		public void AlterColumn(string table, Column column)
		{
			if (!ColumnExists(table, column.Name))
			{
				Logger.Warn("Column {0}.{1} does not exists", table, column.Name);
				return;
			}
			IColumnPropertiesMapper mapper = GetAndMapColumnProperties(column);
			AlterColumn(table, mapper.ColumnSql);
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
		/// Append a primary key to a table.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table name</param>
		/// <param name="columns">Primary column names</param>
		public void AddPrimaryKey(string name, string table, params string[] columns)
		{
			if (ConstraintExists(name, table))
			{
				Logger.Warn("Primary key {0} already exists", name);
				return;
			}
			ExecuteNonQuery("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2}) ", table, name, String.Join(",", columns));
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn, refTable, refColumn);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </see>
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns, refTable, refColumns);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn, ForeignKeyConstraint constraint)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn, refTable, refColumn, constraint);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </see>
		/// </summary>
		public void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, ForeignKeyConstraint constraint)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns, refTable, refColumns, constraint);
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
		public void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			AddForeignKey(name, primaryTable, new string[] { primaryColumn }, refTable, new string[] { refColumn });
		}
		/// <summary>
		/// <see cref="TransformationProvider.AddForeignKey(string, string, string, string, string)">
		/// AddForeignKey(string, string, string, string, string)
		/// </see>
		/// </summary>
		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, ForeignKeyConstraint.NoAction);
		}

		public void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn, ForeignKeyConstraint constraint)
		{
			AddForeignKey(name, primaryTable, new string[] { primaryColumn }, refTable, new string[] { refColumn }, constraint);

		}

		// Not sure how SQL server handles ON UPDATRE & ON DELETE
		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, ForeignKeyConstraint constraint)
		{
			if (ConstraintExists(name, primaryTable))
			{
				Logger.Warn("The contraint {0} already exists", name);
				return;
			}
			ExecuteNonQuery(
				"ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})",
				primaryTable, name, String.Join(",", primaryColumns),
				refTable, String.Join(",", refColumns));
		}


		/// <summary>
		/// Removes a constraint.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table owning the constraint</param>
		public void RemoveForeignKey(string name, string table)
		{
			if (TableExists(table) && ConstraintExists(name, table))
			{
				ExecuteNonQuery("ALTER TABLE {0} DROP CONSTRAINT {1}", table, name);
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

		public ForeignKeys.IForeignKeyConstraintMapper ForeignKeyMapper
		{
			get { return new ForeignKeys.SQLServerForeignKeyConstraintMapper(); }
		}

		public global::Migrator.Providers.TypeToSqlProviders.ITypeToSqlProvider TypeToSqlProvider
		{
			get { return new SQLServerTypeToSqlProvider(); }
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

			using (IDataReader reader = ExecuteQuery(String.Format("select COLUMN_NAME from information_schema.columns where table_name = '{0}';", table)))
			{
				while (reader.Read())
				{
					columns.Add(new Column(reader[0].ToString(), typeof(string)));
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
			return ExecuteNonQuery("UPDATE {0} SET {1}", table, String.Join(", ", columnValues));
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
				"INSERT INTO {0} ({1}) VALUES ({2})", 
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
					return (int)Convert.ToInt32(version);
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

		#region ITransformationProvider Members

		public void GenerateForeignKey(string primaryTable, string refTable)
		{
			GenerateForeignKey(primaryTable, refTable, ForeignKeyConstraint.NoAction);
		}

		public void GenerateForeignKey(string primaryTable, string refTable, ForeignKeyConstraint constraint)
		{
			GenerateForeignKey(primaryTable, refTable, refTable, "Id");
		}

		#endregion

		#region Helper methods

		private string Quote(string text)
		{
			return String.Format("`{0}`", text);
		}

		#endregion

	}
}
