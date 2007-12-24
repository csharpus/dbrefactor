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
using DbRefactor.Tools;

namespace DbRefactor.Console
{
	/// <summary>
	/// Commande line utility to run the migrations
	/// </summary>
	/// </remarks>
	public class MigratorConsole
	{
		private string _provider;
		private string _connectionString;
		private string _migrationsAssembly;
		private bool _list = false;
		private bool _trace = false;
		private string _dumpTo;
		private int _migrateTo = -1;
		private string[] args;

		/// <summary>
		/// Builds a new console
		/// </summary>
		/// <param name="argv">Command line arguments</param>
		public MigratorConsole(string[] argv)
		{
			args = argv;
			ParseArguments(argv);
		}

		/// <summary>
		/// Run the migrator's console
		/// </summary>
		/// <returns>-1 if error, else 0</returns>
		public int Run()
		{
			try
			{
				if (_list)
					List();
				else if (_dumpTo != null)
					Dump();
				else
					Migrate();
			}
			catch (ArgumentException aex)
			{
				System.Console.WriteLine("Invalid argument '{0}' : {1}", aex.ParamName, aex.Message);
				System.Console.WriteLine();
				PrintUsage();
				return -1;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex);
				return -1;
			}
			return 0;
		}

		/// <summary>
		/// Runs the migrations.
		/// </summary>
		public void Migrate()
		{
			CheckArguments();

			DbRefactor.Migrator mig = GetMigrator();
			if (_migrateTo == -1)
				mig.MigrateToLastVersion();
			else
				mig.MigrateTo(_migrateTo);
		}

		/// <summary>
		/// List migrations.
		/// </summary>
		public void List()
		{
			CheckArguments();

			DbRefactor.Migrator mig = GetMigrator();
			int currentVersion = mig.CurrentVersion;

			System.Console.WriteLine("Available migrations:");
			foreach (Type t in mig.MigrationsTypes)
			{
				int v = DbRefactor.Migrator.GetMigrationVersion(t);
				System.Console.WriteLine("{0} {1} {2}",
								  v == currentVersion ? "=>" : "  ",
								  v.ToString().PadLeft(3),
								  DbRefactor.Migrator.ToHumanName(t.Name)
					);
			}
		}

		public void Dump()
		{
			CheckArguments();

			SchemaDumper dumper = new SchemaDumper(_connectionString);

			dumper.DumpTo(_dumpTo);
		}

		/// <summary>
		/// Show usage information and help.
		/// </summary>
		public void PrintUsage()
		{
			int tab = 17;
			Version ver = Assembly.GetExecutingAssembly().GetName().Version;

			System.Console.WriteLine("Database Refactor - v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
			System.Console.WriteLine();
			System.Console.WriteLine("usage:\nMigrator.Console.exe provider connectionString migrationsAssembly [options]");
			System.Console.WriteLine();
			System.Console.WriteLine("\t{0} {1}", "provider".PadRight(tab), "The database provider (SqlServer, MySql, Postgre)");
			System.Console.WriteLine("\t{0} {1}", "connectionString".PadRight(tab), "Connection string to the database");
			System.Console.WriteLine("\t{0} {1}", "migrationAssembly".PadRight(tab), "Path to the assembly containing the migrations");
			System.Console.WriteLine("Options:");
			System.Console.WriteLine("\t-{0}{1}", "version NO".PadRight(tab), "To specific version to migrate the database to");
			System.Console.WriteLine("\t-{0}{1}", "list".PadRight(tab), "List migrations");
			System.Console.WriteLine("\t-{0}{1}", "trace".PadRight(tab), "Show debug informations");
			System.Console.WriteLine("\t-{0}{1}", "dump FILE".PadRight(tab), "Dump the database schema as migration code");
			System.Console.WriteLine();
		}

		#region Private helper methods
		private void CheckArguments()
		{
			if (_connectionString == null)
				throw new ArgumentException("Connection string is missing", "connectionString");
			if (_migrationsAssembly == null)
				throw new ArgumentException("Migrations assembly is missing", "migrationsAssembly");
		}

		private DbRefactor.Migrator GetMigrator()
		{
			Assembly asm = Assembly.LoadFrom(_migrationsAssembly);

			DbRefactor.Migrator migrator = new DbRefactor.Migrator(_connectionString, asm, _trace);
			migrator.args = args;
			return migrator;
		}

		private void ParseArguments(string[] argv)
		{
			for (int i = 0; i < argv.Length; i++)
			{
				if (argv[i].Equals("-list"))
				{
					_list = true;
				}
				else if (argv[i].Equals("-trace"))
				{
					_trace = true;
				}
				else if (argv[i].Equals("-version"))
				{
					_migrateTo = int.Parse(argv[i + 1]);
					i++;
				}
				else if (argv[i].Equals("-dump"))
				{
					_dumpTo = argv[i + 1];
					i++;
				}
				else
				{
					if (i == 0) _provider = argv[i];
					if (i == 1) _connectionString = argv[i];
					if (i == 2) _migrationsAssembly = argv[i];
				}
			}
		}
		#endregion
	}
}