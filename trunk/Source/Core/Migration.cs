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
using System.Reflection;
using DbRefactor.Api;
using DbRefactor.Exceptions;
using DbRefactor.Providers;
using DbRefactor.Runner;
using DbRefactor.Tools.DesignByContract;

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


	public abstract class Migration : BaseMigration, IDatabase
	{
		internal IDatabase Database { get; set; }
		internal TransformationProvider Provider { get; set; }

		public void DropTable(string name)
		{
			Database.DropTable(name);
		}

		public void ExecuteFile(string fileName)
		{
			Check.RequireNonEmpty(fileName, "fileName");
			if (!File.Exists(fileName))
			{
				string migrationScriptPath = String.Format(@"{0}\Scripts\{1:000}\{2}", Directory.GetCurrentDirectory(), MigrationHelper.GetMigrationVersion(GetType())
														   , fileName);
				Check.Ensure(File.Exists(migrationScriptPath), String.Format("Script file '{0}' has not found.", fileName));
				fileName = migrationScriptPath;
			}
			string content = File.ReadAllText(fileName);
			Database.Execute().NonQuery(content);
		}

		public void ExecuteResource(string resourceName)
		{
			var assemblyName = Assembly.GetCallingAssembly().FullName;
			Check.RequireNonEmpty(assemblyName, "assemblyName");
			Check.RequireNonEmpty(resourceName, "resourceName");

			Assembly assembly = Assembly.Load(assemblyName);
			resourceName = GetResource(resourceName, assembly, MigrationHelper.GetMigrationVersion(GetType()));

			Stream stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
				throw new DbRefactorException(String.Format("Could not locate embedded resource '{0}' in assembly '{1}'",
															resourceName, assemblyName));
			string script;
			using (var streamReader = new StreamReader(stream))
			{
				script = streamReader.ReadToEnd();
			}
			Database.Execute().NonQuery(script);
		}

		private static string GetResource(string resourceName, Assembly assembly, int version)
		{
			foreach (var resource in assembly.GetManifestResourceNames())
			{
				if (resource.Contains(String.Format("._{0:000}.{1}", version, resourceName)))
					return resource;
			}
			return String.Empty;
		}

		public NewTable CreateTable(string name)
		{
			return Database.CreateTable(name);
		}

		public ActionTable Table(string name)
		{
			return Database.Table(name);
		}

		public ExecuteEngine Execute()
		{
			return Database.Execute();
		}
	}
}