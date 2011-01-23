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

using System.Reflection;
using DbRefactor.Runner;

namespace DbRefactor.Core
{
	public sealed class Migrator
	{
		private readonly MigrationService migrationService;

		internal Migrator(MigrationService migrationService)
		{
			this.migrationService = migrationService;
		}

		/// <summary>
		/// Migrate the database to a specific version.
		/// Runs all migration between the actual version and the
		/// specified version.
		/// If <c>version</c> is greater then the current version,
		/// the <c>Up()</c> method will be invoked.
		/// If <c>version</c> lower then the current version,
		/// the <c>Down()</c> method of previous migration will be invoked.
		/// </summary>
		/// <param name="assembly">Assembly that contains migrations</param>
		/// <param name="version">The version that must became the current one</param>
		public void MigrateTo(Assembly assembly, int version)
		{
			migrationService.MigrateTo(assembly, version);
		}

		/// <summary>
		/// Run all migrations up to the latest.
		/// </summary>
		public void MigrateToLastVersion(Assembly assembly)
		{
			migrationService.MigrateToLastVersion(assembly);
		}
	}
}