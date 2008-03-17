﻿#region License
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
using NConsoler;
using System.Reflection;

namespace DbRefactor.Console
{

	public class Program
	{
[STAThread]
public static void Main(string[] args) {
	Consolery.Run(typeof(Program), args);
}

[Action]
public static void Migrate(
	[Required] string provider,
	[Optional(-1)] int version) {
	// ...
}

C:\>program.exe SqlServer /version:10
	}

	/// <summary>
	/// Console application boostrap class.
	/// </summary>
	public class Boot
	{
		[STAThread]
		public static void Main(string[] argv)
		{
			Consolery.Run(typeof(Boot), argv);
		}

		[Action]
		public static void Migrate(
			[Required(Description = "The database provider (SqlServer)")]
			string provider,
			[Required(Description = "Connection string to the database")]
			string connectionString,
			[Required(Description = "Path to the assembly containing the migrations")]
			string migrationAssembly,
			[Optional(-1, Description = "To specific version to migrate the database to")]
			int version,
			[Optional(false, Description = "Show debug informations")]
			bool trace)
		{
			Assembly asm = Assembly.LoadFrom(migrationAssembly);

			Migrator migrator = new Migrator(connectionString, asm, trace);
			if (version == -1)
			{
				migrator.MigrateToLastVersion();
			}
			else
			{
				migrator.MigrateTo(version);
			}
		}
	}
}