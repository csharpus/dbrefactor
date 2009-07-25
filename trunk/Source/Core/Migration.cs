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
using System.IO;
using System.Resources;
using DbRefactor.Compatibility;
using DbRefactor.Providers;
using DbRefactor.Tools.Loggers;
using System.Data;

namespace DbRefactor
{
	///// <summary>
	///// A migration is a group of transformation applied to the database schema
	///// (or sometimes data) to port the database from one version to another.
	///// The <c>Up()</c> method must apply the modifications (eg.: create a table)
	///// and the <c>Down()</c> method must revert, or rollback the modifications
	///// (eg.: delete a table).
	///// <para>
	///// Each migration must be decorated with the <c>[Migration(0)]</c> attribute.
	///// Each migration number (0) must be unique, or else a 
	///// <c>DuplicatedVersionException</c> will be trown.
	///// </para>
	///// <para>
	///// All migrations are executed inside a transaction. If an exception is
	///// thrown, the transaction will be rolledback and transformations wont be
	///// applied.
	///// </para>
	///// <para>
	///// It is best to keep a limited number of transformation inside a migration
	///// so you can easely move from one version of to another with fine grain
	///// modifications.
	///// You should give meaningful name to the migration class and prepend the
	///// migration number to the filename so they keep ordered, eg.: 
	///// <c>002_CreateTableTest.cs</c>.
	///// </para>
	///// <para>
	///// Use the <c>Database</c> property to apply transformation and the
	///// <c>Logger</c> property to output informations in the console (or other).
	///// For more details on transformations see
	///// <see cref="TransformationProvider">TransformationProvider</see>.
	///// </para>
	///// </summary>
	///// <example>
	///// The following migration creates a new Customer table.
	///// (File <c>003_AddCustomerTable.cs</c>)
	///// <code>
	///// [Migration(3)]
	///// public class AddCustomerTable : Migration
	///// {
	/////		public override void Up()
	/////		{
	/////			CreateTable("Customer", Columns
	/////				.String("Name", 50)
	/////				.String("Address", 100));
	///// 	}
	///// 
	///// 	public override void Down()
	///// 	{
	///// 		DropTable("Customer");
	///// 	}
	///// }
	///// </code>
	///// </example>
	
	public abstract class Migration : BaseMigration
	{
		private Providers.TransformationProvider _transformationProvider;

		internal Providers.TransformationProvider TransformationProvider
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
		/// This gets called once on the first migration object.
		/// </summary>
		public virtual void InitializeOnce(string[] args)
		{
			// Console.WriteLine("Migration.InitializeOnce()");
		}

		/// <summary>
		/// Event logger.
		/// </summary>
		[Obsolete("Usage of this logger is obsolete")]
		public ILogger Logger
		{
			get
			{
				return _transformationProvider.Logger;
			}
		}

		[Obsolete("Please, use CreateTable(\"Name\").Int(\"ColumnName\").Execute()")]
		protected void CreateTable(string name, ColumnsCollection columns)
		{
			Database.AddTable(name, columns.ToArray());
		}

		[Obsolete("This collection is obsolete. Perviously used for CreateTable")]
		protected static ColumnsCollection Columns
		{
			get
			{
				return ColumnsCollection.Create();
			}
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

		[Obsolete("Please, use Table(...).AddColumn")]
		protected void AddTo(string table, Column column)
		{
			Database.AddColumn(table, column);
		}

		protected void DropTable(string name)
		{
			Database.DropTable(name);
		}

		protected void DropForeignKey(string foreignKeyTable, string name)
		{
			Database.RemoveForeignKey(foreignKeyTable, name);
		}

		private void DropColumn(string table, string column)
		{
			Database.DropColumn(table, column);
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		protected void ExecuteNonQuery(string sql, params string[] values)
		{
			Database.ExecuteNonQuery(sql, values);
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		protected IDataReader ExecuteQuery(string sql, params string[] values)
		{
			return Database.ExecuteQuery(sql, values);
		}

		protected void ExecuteFile(string filePath)
		{
			Database.ExecuteFile(filePath);
		}

		/// <summary>
		/// Extracts an embedded file out of a given assembly.
		/// </summary>
		/// <param name="assemblyName">The namespace of you assembly.</param>
		/// <param name="filePath">The name of the file to extract.</param>
		/// <returns>A stream containing the file data.</returns>
		protected void ExecuteResource(string assemblyName, string filePath)
		{
			Database.ExecuteResource(assemblyName, filePath);
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		protected object ExecuteScalar(string sql, params string[] values)
		{
			return Database.ExecuteScalar(sql, values);
		}

		[Obsolete("Please, use Table.SelectScalar(...)")]
		protected object SelectScalar(string what, string from, string where)
		{
			return Database.SelectScalar(what, from, where);
		}

		/// <summary>
		/// Example:
		/// <code>Insert("Table", "column1='value1'", "column2=10");</code>
		/// </summary>
		/// <example><code>Insert("Table", "column1='value1'", "column2=10");</code></example>
		/// <returns>Number of rows inserted</returns>
		[Obsolete("Please, use Table.Insert(...)")]
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
		[Obsolete("Please, use Table.Update(...)")]
		protected int Update(string table, params string[] columnValues) 
		{
			return Database.Update(table, columnValues);
		}

		[Obsolete("Please, use Table.AlterColumn(...")]
		protected void AlterColumn(string table, Column column)
		{
			Database.AlterColumn(table, column);
		}

		protected void RemoveColumnConstraints(string table, string column)
		{
			Database.DeleteColumnConstraints(table, column);
		}

		[Obsolete("Please, use Table(...).RenameTo")]
        protected void RenameTable(string oldName, string newName)
        {
            Database.RenameTable(oldName, newName);
        }

		[Obsolete("Please, use Table(...).RenameColumn")]
        protected void RenameColumn(string table, string oldName, string newName)
        {
            Database.RenameColumn(table, oldName, newName);
        }

		public override NewTable CreateTable(string tableName)
		{
			return new NewTable(TransformationProvider.Environment, tableName);
		}

		public override ActionTable Table(string tableName)
		{
			return new ActionTable(TransformationProvider.Environment, tableName);
		}
	}
}