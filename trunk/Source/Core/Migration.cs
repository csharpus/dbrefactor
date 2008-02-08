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
using DbRefactor.Providers;
using DbRefactor.Tools.Loggers;
using System.Data;

namespace DbRefactor
{
	/// <summary>
	/// A migration is a group of transformation applied to the database schema
	/// (or sometimes data) to port the database from one version to another.
	/// The <c>Up()</c> method must apply the modifications (eg.: create a table)
	/// and the <c>Down()</c> method must revert, or rollback the modifications
	/// (eg.: delete a table).
	/// <para>
	/// Each migration must be decorated with the <c>[Migration(0)]</c> attribute.
	/// Each migration number (0) must be unique, or else a 
	/// <c>DuplicatedVersionException</c> will be trown.
	/// </para>
	/// <para>
	/// All migrations are executed inside a transaction. If an exception is
	/// thrown, the transaction will be rolledback and transformations wont be
	/// applied.
	/// </para>
	/// <para>
	/// It is best to keep a limited number of transformation inside a migration
	/// so you can easely move from one version of to another with fine grain
	/// modifications.
	/// You should give meaningful name to the migration class and prepend the
	/// migration number to the filename so they keep ordered, eg.: 
	/// <c>002_CreateTableTest.cs</c>.
	/// </para>
	/// <para>
	/// Use the <c>Database</c> property to apply transformation and the
	/// <c>Logger</c> property to output informations in the console (or other).
	/// For more details on transformations see
	/// <see cref="TransformationProvider">TransformationProvider</see>.
	/// </para>
	/// </summary>
	/// <example>
	/// The following migration creates a new Customer table.
	/// (File <c>003_AddCustomerTable.cs</c>)
	/// <code>
	/// [Migration(3)]
	/// public class AddCustomerTable : Migration
	/// {
	///		public override void Up()
	///		{
	///			CreateTable("Customer",
	///				String("Name", 50),
	///				String("Address", 100));
	/// 	}
	/// 
	/// 	public override void Down()
	/// 	{
	/// 		DropTable("Customer");
	/// 	}
	/// }
	/// </code>
	/// </example>
	public abstract class Migration
	{
		private TransformationProvider _transformationProvider;

		internal TransformationProvider TransformationProvider
		{
			get
			{
				return _transformationProvider;
			}

			set
			{
				_transformationProvider = value;
			}
		}

		private TransformationProvider Database
		{
			get
			{
				return TransformationProvider;
			}
		}

		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public abstract void Up();

		/// <summary>
		/// Defines transformations to revert things done in <c>Up</c>.
		/// </summary>
		public abstract void Down();

		/// <summary>
		/// This gets called once on the first migration object.
		/// </summary>
		public virtual void InitializeOnce(string[] args)
		{
			Console.WriteLine("Migration.InitializeOnce()");
		}

		/// <summary>
		/// Event logger.
		/// </summary>
		public ILogger Logger
		{
			get
			{
				return _transformationProvider.Logger;
			}
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		protected Column String(string name, int size)
		{
			return new Column(name, typeof(string), size);
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		protected Column String(string name, int size, ColumnProperties properties)
		{
			return new Column(name, typeof(string), size, properties);
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		protected Column String(string name, int size, string defaultValue)
		{
			return new Column(name, typeof(string), size, ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		protected Column String(string name, int size, ColumnProperties properties, string defaultValue)
		{
			return new Column(name, typeof(string), size, properties, defaultValue);
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddString(string table, string name, int size, ColumnProperties properties)
		{
			Database.AddColumn(table, String(name, size, properties));
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddString(string table, string name, int size)
		{
			Database.AddColumn(table, String(name, size));
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddString(string table, string name, int size, ColumnProperties properties,
			string defaultValue)
		{
			Database.AddColumn(table, String(name, size, properties, defaultValue));
		}

		private const int defaultTextLength = 1024;

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		protected Column Text(string name)
		{
			return new Column(name, typeof(string), defaultTextLength);
		}

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		protected Column Text(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(string), defaultTextLength, properties);
		}

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		protected Column Text(string name, string defaultValue)
		{
			return new Column(name, typeof(string), defaultTextLength,
				ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		protected Column Text(string name, ColumnProperties properties, string defaultValue)
		{
			return new Column(name, typeof(string), defaultTextLength, properties, defaultValue);
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddText(string table, string name)
		{
			Database.AddColumn(table, Text(name));
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddText(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, Text(name, properties));
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddText(string table, string name, string defaultValue)
		{
			Database.AddColumn(table, Text(name, ColumnProperties.Null, defaultValue));
		}

		/// <summary>
		/// Adds a string column to the specified table
		/// </summary>
		protected void AddText(string table, string name, ColumnProperties properties, string defaultValue)
		{
			Database.AddColumn(table, Text(name, properties, defaultValue));
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		protected Column Int(string name)
		{
			return new Column(name, typeof(int));
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		protected Column Int(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(int), properties);
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		protected Column Int(string name, int defaultValue)
		{
			return new Column(name, typeof(int), ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		protected Column Int(string name, ColumnProperties properties, int defaultValue)
		{
			return new Column(name, typeof(int), properties, defaultValue);
		}

		/// <summary>
		/// Adds an integer column to the specified table
		/// </summary>
		protected void AddInt(string table, string name)
		{
			Database.AddColumn(table, Int(name));
		}

		/// <summary>
		/// Adds an integer column to the specified table
		/// </summary>
		protected void AddInt(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, Int(name, properties));
		}

		/// <summary>
		/// Adds an integer column to the specified table
		/// </summary>
		protected void AddInt(string table, string name, int defaultValue)
		{
			Database.AddColumn(table, Int(name, ColumnProperties.Null, defaultValue));
		}

		/// <summary>
		/// Adds an integer column to the specified table
		/// </summary>
		protected void AddInt(string table, string name, ColumnProperties properties, int defaultValue)
		{
			Database.AddColumn(table, Int(name, properties, defaultValue));
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		protected Column Long(string name)
		{
			return new Column(name, typeof(long));
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		protected Column Long(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(long), properties);
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		protected Column Long(string name, long defaultValue)
		{
			return new Column(name, typeof(long), ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		protected Column Long(string name, ColumnProperties properties, long defaultValue)
		{
			return new Column(name, typeof(long), properties, defaultValue);
		}

		/// <summary>
		/// Adds a long column to the specified table
		/// </summary>
		protected void AddLong(string table, string name)
		{
			Database.AddColumn(table, Long(name));
		}

		/// <summary>
		/// Adds a long column to the specified table
		/// </summary>
		protected void AddLong(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, Long(name, properties));
		}

		/// <summary>
		/// Adds a long column to the specified table
		/// </summary>
		protected void AddLong(string table, string name, long defaultValue)
		{
			Database.AddColumn(table, Long(name, defaultValue));
		}

		/// <summary>
		/// Adds a long column to the specified table
		/// </summary>
		protected void AddLong(string table, string name, ColumnProperties properties, long defaultValue)
		{
			Database.AddColumn(table, Long(name, properties, defaultValue));
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		protected Column DateTime(string name)
		{
			return new Column(name, typeof(DateTime));
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		protected Column DateTime(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(DateTime), properties);
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		protected Column DateTime(string name, DateTime defaultValue)
		{
			return new Column(name, typeof(DateTime), ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		protected Column DateTime(string name, ColumnProperties properties, DateTime defaultValue)
		{
			return new Column(name, typeof(DateTime), properties, defaultValue);
		}

		/// <summary>
		/// Adds a date/time column to the specified table
		/// </summary>
		protected void AddDateTime(string table, string name)
		{
			Database.AddColumn(table, DateTime(name));
		}

		/// <summary>
		/// Adds a date/time column to the specified table
		/// </summary>
		protected void AddDateTime(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, DateTime(name, properties));
		}

		/// <summary>
		/// Adds a date/time column to the specified table
		/// </summary>
		protected void AddDateTime(string table, string name, DateTime defaultValue)
		{
			Database.AddColumn(table, DateTime(name, defaultValue));
		}

		/// <summary>
		/// Adds a date/time column to the specified table
		/// </summary>
		protected void AddDateTime(string table, string name, ColumnProperties properties,
			DateTime defaultValue)
		{
			Database.AddColumn(table, DateTime(name, properties, defaultValue));
		}

		private const int defaultWhole = 18;
		private const int defaultRemainder = 0;

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name)
		{
			return new DecimalColumn(name, defaultWhole, defaultRemainder);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, ColumnProperties properties)
		{
			return new DecimalColumn(name, defaultWhole, defaultRemainder, properties);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, decimal defaultValue)
		{
			return new DecimalColumn(name, defaultWhole, defaultRemainder,
				ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, ColumnProperties properties, decimal defaultValue)
		{
			return new DecimalColumn(name, defaultWhole, defaultRemainder, properties, defaultValue);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, int whole, int remainder)
		{
			return new DecimalColumn(name, whole, remainder);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, int whole, int remainder, ColumnProperties properties)
		{
			return new DecimalColumn(name, whole, remainder, properties);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, int whole, int remainder, decimal defaultValue)
		{
			return new DecimalColumn(name, whole, remainder, ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		protected Column Decimal(string name, int whole, int remainder,
			ColumnProperties properties, decimal defaultValue)
		{
			return new DecimalColumn(name, whole, remainder, properties, defaultValue);
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name)
		{
			Database.AddColumn(table, Decimal(name, defaultWhole, defaultRemainder));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, Decimal(name, defaultWhole, defaultRemainder, properties));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, decimal defaultValue)
		{
			Database.AddColumn(table, Decimal(name, defaultWhole, defaultRemainder,
				ColumnProperties.Null, defaultValue));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, ColumnProperties properties,
			decimal defaultValue)
		{
			Database.AddColumn(table, Decimal(name, defaultWhole, defaultRemainder,
				properties, defaultValue));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, int whole, int remainder)
		{
			Database.AddColumn(table, Decimal(name, whole, remainder));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, int whole, int remainder,
			ColumnProperties properties)
		{
			Database.AddColumn(table, Decimal(name, whole, remainder, properties));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, int whole, int remainder, decimal defaultValue)
		{
			Database.AddColumn(table, Decimal(name, whole, remainder,
				ColumnProperties.Null, defaultValue));
		}

		/// <summary>
		/// Adds a decimal column to the specified table
		/// </summary>
		protected void AddDecimal(string table, string name, int whole, int remainder,
			ColumnProperties properties, decimal defaultValue)
		{
			Database.AddColumn(table, Decimal(name, whole, remainder,
				properties, defaultValue));
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		protected Column Boolean(string name)
		{
			return new Column(name, typeof(bool));
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		protected Column Boolean(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(bool), properties);
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		protected Column Boolean(string name, bool defaultValue)
		{
			return new Column(name, typeof(bool), ColumnProperties.Null, defaultValue);
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		protected Column Boolean(string name, ColumnProperties properties, bool defaultValue)
		{
			return new Column(name, typeof(bool), properties, defaultValue);
		}

		/// <summary>
		/// Adds a boolean column to the specified table
		/// </summary>
		protected void AddBoolean(string table, string name)
		{
			Database.AddColumn(table, Boolean(name));
		}

		/// <summary>
		/// Adds a boolean column to the specified table
		/// </summary>
		protected void AddBoolean(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, Boolean(name, properties));
		}

		/// <summary>
		/// Adds a boolean column to the specified table
		/// </summary>
		protected void AddBoolean(string table, string name, bool defaultValue)
		{
			Database.AddColumn(table, Boolean(name, ColumnProperties.Null, defaultValue));
		}

		/// <summary>
		/// Adds a boolean column to the specified table
		/// </summary>
		protected void AddBoolean(string table, string name, ColumnProperties properties,
			bool defaultValue)
		{
			Database.AddColumn(table, Boolean(name, properties, defaultValue));
		}

		protected void CreateTable(string name, params Column[] columns)
		{
			Database.AddTable(name, columns);
		}

		protected void AddForeignKey(string name, string foreignKeyTable,
			string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn)
		{
			Database.AddForeignKey(name, foreignKeyTable, foreignKeyColumn,
				primaryKeyTable, primaryKeyColumn);
		}

		protected void AddForeignKey(string name, string foreignKeyTable,
			string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn, OnDelete ondelete)
		{
			Database.AddForeignKey(name, foreignKeyTable, foreignKeyColumn, primaryKeyTable, primaryKeyColumn, ondelete);
		}

		protected void DropTable(string name)
		{
			Database.DropTable(name);
		}

		protected void DropForeignKey(string table, string key)
		{
			Database.RemoveForeignKey(table, key);
		}

		protected void DropColumn(string table, string column)
		{
			Database.DropColumn(table, column);
		}

		protected void ExecuteNonQuery(string sql, params string[] values)
		{
			Database.ExecuteNonQuery(sql, values);
		}

		protected IDataReader ExecuteQuery(string sql, params string[] values)
		{
			return Database.ExecuteQuery(sql, values);
		}

		protected object ExecuteScalar(string sql, params string[] values)
		{
			return Database.ExecuteScalar(sql, values);
		}

		/// <summary>
		/// Example:
		/// <code>Insert("Table", "column1='value1'", "column2=10");</code>
		/// </summary>
		/// <example><code>Insert("Table", "column1='value1'", "column2=10");</code></example>
		/// <returns>Number of rows inserted</returns>
		protected int Insert(string table, params string[] columnValues)
		{
			return Database.Insert(table, columnValues);
		}

		/// <summary>
		/// Example:
		/// <code>Update("Table", "column1='value1'");</code> - updates all rows in the table,
		/// sets value of the column1 to 'value1'
		/// </summary>
		/// <returns>Number of rows updated</returns>
		protected int Update(string table, params string[] columnValues) 
		{
			return Database.Update(table, columnValues);
		}
	}
}