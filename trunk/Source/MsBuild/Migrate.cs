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
using System.ComponentModel;
using System.Reflection;
using DbRefactor.Factories;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MsBuild
{
	public class Migrate : Task
	{
		[Required, Description("The database provider (SqlServer)")]
		public string Provider { get; set; }

		[Required, Description("Connection string to the database")]
		public string ConnectionString { get; set; }

		[Required, Description("Path to the assembly containing the migrations")]
		public string MigrationsAssembly { get; set; }

		[DefaultValue(-1), Description("To specific version to migrate the database to")]
		public int Version { get; set; } 
		[DefaultValue(false), Description("Show debug information")]
		public bool Trace { get; set; }

		[DefaultValue(null), Description ("To define another set of migrations")]
		public string Category { get; set; }

		public Migrate()
		{
		}

		public override bool Execute()
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(MigrationsAssembly);
				if (assembly == null)
					return false;

				var migrator = new ProviderFactory().CreateMigrator(
					Provider,
					ConnectionString,
					null,
					Trace);

				if (Version <= 0)
				{
					migrator.MigrateToLastVersion(assembly);
				}
				else
				{
					migrator.MigrateTo(assembly, Version);
				}
			}
			catch(Exception ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}

			return true;

		}
	}
}
