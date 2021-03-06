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
using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using NConsoler;
using System.Reflection;

namespace DbRefactor.Cli
{
	/// <summary>
	/// Console application boostrap class.
	/// </summary>
	public class Boot
	{
		[STAThread]
		public static void Main(string[] argv)
		{
//			System.Diagnostics.Debugger.Break();
			Consolery.Run(typeof (Boot), argv);
		}

		[Action]
		public static void Migrate(
			[Required(Description = "The database provider (SqlServer)")] string provider,
			[Required(Description = "Connection string to the database")] string connectionString,
			[Required(Description = "Path to the assembly containing the migrations")] string migrationAssembly,
			[Optional(-1, Description = "To specific version to migrate the database to")] int version,
			[Optional(false, Description = "Show debug information")] bool trace,
			[Optional(null, Description = "To define another set of migrations")] string category)
		{
			Assembly asm = Assembly.LoadFrom(migrationAssembly);
			var logger = trace ? new ConsoleLogger() : Logger.NullLogger;
			var migrator = DbRefactorFactory.SqlServer().CreateMigrator(connectionString, logger, category);
			if (version == -1)
			{
				migrator.MigrateToLastVersion(asm);
			}
			else
			{
				migrator.MigrateTo(asm, version);
			}
		}

		[Action]
		public static void Schema(string provider, string connectionString, string outputFolder)
		{
			var dumper = DbRefactorFactory.SqlServer().CreateSchemaDumper(connectionString);
			dumper.DumpTo(outputFolder + "/M001_LegacyMigration.cs");
		}

		//[Action]
		//public static void Dump(string provider, string connectionString, string outputFolder)
		//{
		//    var logger = new ConsoleLogger();
		//    var dumper = DbRefactorFactory.BuildSqlServerFactory(connectionString, logger, null).CreateSchemaDumper();
		//    dumper.DumpTo(outputFolder);
		//}
	}
}