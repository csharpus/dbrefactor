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
using Migrator.Providers;
using Migrator.Loggers;
using DbRefactor.Compatibility;

namespace Migrator
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
	/// 	public override void Up()
	/// 	{
	/// 		Database.AddTable("Customer",
	///		                  new Column("Name", typeof(string), 50),
	///		                  new Column("Address", typeof(string), 100)
	///		                 );
	/// 	}
	/// 	public override void Down()
	/// 	{
	/// 		Database.RemoveTable("Customer");
	/// 	}
	/// }
	/// </code>
	/// </example>
	public abstract class Migration : BaseMigration
	{
		private TransformationProvider _transformationProvider;
		
		
		
		/// <summary>
		/// Represents the database.
		/// <see cref="TransformationProvider"></see>.
		/// </summary>
		/// <seealso cref="Migration.Transformations">Migration.Transformations</seealso>
		public TransformationProvider Database
		{
			get {
				return _transformationProvider;
			}
		}
		
		/// <summary>
		/// Alias to the <c>Database</c> property.
		/// </summary>
		public TransformationProvider TransformationProvider
		{
			get {
				return _transformationProvider;
			}
			set {
				_transformationProvider = value;
			}
		}
		
		/// <summary>
		/// Event logger.
		/// </summary>
		public ILogger Logger {
			get {
				return _transformationProvider.Logger;
			}
		}
	}
}
