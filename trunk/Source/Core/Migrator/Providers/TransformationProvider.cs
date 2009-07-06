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

namespace Migrator.Providers
{
	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	public abstract class TransformationProvider
	{
		private ILogger _logger = new NullLogger();
		protected IDbConnection _connection;
		private IDbTransaction _transaction;

		internal IDbTransaction Transaction
	{
		get
		{
			return _transaction;
		}

			set
			{
				_transaction = value;
			}
	}

		/// <summary>
		/// Returns the event logger
		/// </summary>
		virtual public ILogger Logger
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
		public abstract void AddTable(string name, params Column[] columns);
		
		/// <summary>
		/// Remove a table from the database.
		/// </summary>
		/// <param name="name">Table name</param>
		public abstract void RemoveTable(string name);

		public virtual void DeleteColumnConstraints(string table, string column)
		{
			
		}
		
		/// <summary>
		/// Add a new column to an existing table.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// Thrown if the <c>type</c> is not supported by the provider
		/// </exception>
		/// <param name="table">Table to which to add the column</param>
		/// <param name="column">Column name</param>
		/// <param name="type">Date type of the column</param>
		/// <param name="size">Max length of the column</param>
		/// <param name="property">Properties of the column, see <see cref="ColumnProperties">ColumnProperties</see>,</param>
		/// <param name="defaultValue">Default value</param>
		public abstract void AddColumn(string table, string column, Type type, int size, ColumnProperties property, object defaultValue);
		
		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, Type type)
		{
			AddColumn(table, column, type, 0, ColumnProperties.Null, null);
		}
		
		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, Type type, int size)
		{
			AddColumn(table, column, type, size, ColumnProperties.Null, null);
		}
		
		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, Type type, ColumnProperties property)
		{
			AddColumn(table, column, type, 0, property, null);
		}
		
		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, Type, int, ColumnProperties, object)">
		/// AddColumn(string, string, Type, int, ColumnProperties, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, Type type, int size, ColumnProperties property)
		{
			AddColumn(table, column, type, size, property, null);
		}

		public abstract void AddColumn(string table, Column column);
		
		/// <summary>
		/// Removes a column from a table
		/// </summary>
		/// <param name="table">table containing the column</param>
		/// <param name="column">column name</param>
		public abstract void RemoveColumn(string table, string column);
		
		/// <summary>
		/// Determines of a column exists.
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		/// <returns><c>true</c> if the column exists</returns>
		public abstract bool ColumnExists(string table, string column);
		
		/// <summary>
		/// Determines if a table exists.
		/// </summary>
		/// <param name="table">Table name</param>
		/// <returns><c>true</c> if the table exists</returns>
		public abstract bool TableExists(string table);
		
		/// <summary>
		/// Append a primary key to a table.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table name</param>
		/// <param name="columns">Primary column names</param>
		public abstract void AddPrimaryKey(string name, string table, params string[] columns);
		
		/// <summary>
		/// Append a foreign key (relation) between two tables.
		/// tables.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="primaryTable">Table name containing the primary key</param>
		/// <param name="primaryColumn">Primary key column name</param>
		/// <param name="refTable">Foreign table name</param>
		/// <param name="refColumn">Foreign column name</param>
		public virtual void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			AddForeignKey(name, primaryTable, new string[] {primaryColumn}, refTable, new string[] {refColumn});
		}
		/// <summary>
		/// <see cref="TransformationProvider.AddForeignKey(string, string, string, string, string)">
		/// AddForeignKey(string, string, string, string, string)
		/// </see>
		/// </summary>
		public abstract void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns);

		public virtual void AddUnique(string name, string table, string column)
		{
			AddUnique(name, table, new string[] { column });
		}

		public abstract void AddUnique(string name, string table, string[] columns);

		public abstract void RemoveUnique(string name, string table);
		/// <summary>
		/// Removes a constraint.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table owning the constraint</param>
		public abstract void RemoveForeignKey(string name, string table);
		
		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table owning the constraint</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public abstract bool ConstraintExists(string name, string table);
		
		public abstract string[] GetTables();
		
		public abstract Column[] GetColumns(string table);
		
		public int ExecuteNonQuery( string sql )
		{
			IDbCommand cmd = BuildCommand( sql );
			return cmd.ExecuteNonQuery();
		}

		private IDbCommand BuildCommand( string sql )
		{
			IDbCommand cmd = _connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;
			if( _transaction != null )
			{
				cmd.Transaction = _transaction;
			}
			return cmd;
		}

		/// <summary>
		/// Execute an SQL query returning results.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <returns>A data iterator, <see cref="System.Data.IDataReader">IDataReader</see>.</returns>
		public IDataReader ExecuteQuery( string sql )
		{
			IDbCommand cmd = BuildCommand( sql );
			return cmd.ExecuteReader();
		}

		public object ExecuteScalar( string sql )
		{
			IDbCommand cmd = BuildCommand( sql );
			return cmd.ExecuteScalar();
		}
		
		public IDataReader Select(string what, string from)
		{
			return Select(what, from, "1=1");
		}
		public virtual IDataReader Select(string what, string from, string where)
		{
			return ExecuteQuery(string.Format("SELECT {0} FROM {1} WHERE {2}", what, from, where));
		}
		
		public object SelectScalar(string what, string from)
		{
			return SelectScalar(what, from, "1=1");
		}
		public virtual object SelectScalar(string what, string from, string where)
		{
			return ExecuteScalar(string.Format("SELECT {0} FROM [{1}] WHERE {2}", what, from, where));
		}
		
		public virtual int Update(string table, params string[] columnValues)
		{
			return ExecuteNonQuery(string.Format("UPDATE {0} SET {1}", table, string.Join(", ", columnValues)));
		}
		
		public virtual int Insert(string table, params string[] columnValues)
		{
			string[] columns = new string[columnValues.Length];
			string[] values = new string[columnValues.Length];
			int i = 0;
			
			foreach (string cs in columnValues)
			{
				columns[i] = cs.Split('=')[0];
				values[i] = cs.Split('=')[1];
				i ++;
			}
			
			return ExecuteNonQuery(string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, string.Join(", ", columns), string.Join(", ", values)));
		}
        
		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			if( _transaction == null && _connection != null )
			{
				EnsureHasConnection();
				_transaction = _connection.BeginTransaction( IsolationLevel.ReadCommitted );
			}
		}
		
		protected void EnsureHasConnection()
		{
			if(_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
		}
		
		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public virtual void Rollback()
		{
			if( _transaction != null && _connection != null && _connection.State == ConnectionState.Open )
			{
				try
				{
					_transaction.Rollback();
				}
				finally
				{
					_connection.Close();
				}
			}
			_transaction = null;
		}
		
		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void Commit()
		{
			if( _transaction != null && _connection != null && _connection.State == ConnectionState.Open )
			{
				try
				{
					_transaction.Commit();
				}
				finally
				{
					_connection.Close();
				}
			}
			_transaction = null;
		}
		
		/// <summary>
		/// Get or set the current version of the database.
		/// This determines if the migrator should migrate up or down
		/// in the migration numbers.
		/// </summary>
		/// <remark>
		/// This value should not be modified inside a migration.
		/// </remark>
		public virtual int CurrentVersion
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
					return (int) version;
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

		public string Category { get; set;}

		protected void CreateSchemaInfoTable()
 		{
 			EnsureHasConnection();
			if (!TableExists("SchemaInfo"))
			{
 				AddTable("SchemaInfo",
		                 new Column("Version", typeof(int), ColumnProperties.PrimaryKey)
		                );
			}
 		}
	}
}
