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

namespace DbRefactor.Core
{
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

            Assembly assembly = Assembly.GetCallingAssembly();
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