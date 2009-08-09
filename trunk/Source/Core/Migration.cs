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
using System.Reflection;
using DbRefactor.Api;
using DbRefactor.Compatibility;
using DbRefactor.Providers;
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
		internal TransformationProvider TransformationProvider { get; set; }
		internal ColumnProviderFactory ColumnProviderFactory { get; set; }
		internal ColumnPropertyProviderFactory ColumnPropertyProviderFactory { get; set; }

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

		protected void DropForeignKey(string foreignKeyTable, string name)
		{
			Database.DropConstraint(foreignKeyTable, name);
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
		/// <param name="resourcePath">The name of the file to extract.</param>
		/// <returns>A stream containing the file data.</returns>
		protected void ExecuteResource(string resourcePath)
		{
			Database.ExecuteResource(Assembly.GetCallingAssembly().FullName, resourcePath);
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		protected object ExecuteScalar(string sql, params string[] values)
		{
			return Database.ExecuteScalar(sql, values);
		}

		protected void RemoveColumnConstraints(string table, string column)
		{
			Database.DeleteColumnConstraints(table, column);
		}

		public override NewTable CreateTable(string tableName)
		{
			return new NewTable(TransformationProvider, ColumnProviderFactory, ColumnPropertyProviderFactory, tableName);
		}

		public override ActionTable Table(string tableName)
		{
			return new ActionTable(TransformationProvider, tableName, ColumnProviderFactory, ColumnPropertyProviderFactory);
		}
	}
}