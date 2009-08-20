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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Runner;

namespace DbRefactor
{
	public sealed class Migrator
	{
		private readonly List<Type> migrationsTypes = new List<Type>();
		private ILogger logger;
		private readonly MigrationTarget migrationTarget;

		public string Category { get; set; }

		internal Migrator(MigrationTarget migrationTarget, string category, Assembly migrationAssembly, ILogger logger)
		{
			this.logger = logger;
			this.migrationTarget = migrationTarget;
			Category = category;

			migrationsTypes.AddRange(GetMigrationTypes(Assembly.GetExecutingAssembly()));

			if (migrationAssembly != null)
			{
				migrationsTypes.AddRange(GetMigrationTypes(migrationAssembly));
				setUpMigration = GetSetUpMigrationType(migrationAssembly);
			}

			this.logger.Trace("Loaded migrations:");
			foreach (Type t in migrationsTypes)
			{
				this.logger.Trace("{0} {1}", GetMigrationVersion(t).ToString().PadLeft(5), ToHumanName(t.Name));
			}
			CheckForDuplicatedVersion();
		}

		private void BeginTransaction()
		{
			migrationTarget.BeginTransaction();
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
		/// <param name="version">The version that must became the current one</param>
		public void MigrateTo(int version)
		{
			BeginTransaction();

			var currentVersion = migrationTarget.GetVersion();

			if (currentVersion == version)
			{
				RunGlobalSetUp();
				if (version == LastVersion)
				{
					RunGlobalTearDown();
				}
				return;
			}
			int originalVersion = currentVersion;
			bool goingUp = originalVersion < version;
			BaseMigration migration;
			int v; // the currently running migration number

			if (goingUp)
			{
				// When we migrate to an upper version,
				// tranformations of the current version are
				// already applied, so we started at the next version.
				v = currentVersion + 1;
			}
			else
			{
				v = currentVersion;
			}

			logger.Started(originalVersion, version);
			RunGlobalSetUp();

			while (true)
			{
				migration = GetMigration(v);

				if (migration != null)
				{
					string migrationName = ToHumanName(migration.GetType().Name);

//					migration.Database = provider.GetDatabase();
						
					try
					{
						RunSetUp();
						if (goingUp)
						{
							logger.MigrateUp(v, migrationName);
							migration.Up();
						}
						else
						{
							logger.MigrateDown(v, migrationName);
							migration.Down();
						}
						RunTearDown();
					}
					catch (Exception ex)
					{
						logger.Exception(v, migrationName, ex);

						// Oho! error! We rollback changes.
						logger.RollingBack(originalVersion);
						migrationTarget.RollbackTransaction();

						throw;
					}
				}
				else
				{
					// The migration number is not found
					logger.Skipping(v);
				}

				if (goingUp)
				{
					if (v == version) break;
					v++;
				}
				else
				{
					v--;
					// When we go back to previous versions
					// we don't invoke Down() of the current
					// version.
					if (v == version) break;
				}
				migrationTarget.UpdateVersion(v);
			}

			// Update and commit all changes
			migrationTarget.UpdateVersion(version);


			migrationTarget.CommitTransaction();
			logger.Finished(originalVersion, version);

			try
			{
				if (version == LastVersion)
				{
					RunGlobalTearDown();
				}
			}
			catch (Exception ex)
			{
				logger.Exception(v, "Global Tear down", ex);
				//throw;
			}
		}

		/// <summary>
		/// Run all migrations up to the latest.
		/// </summary>
		public void MigrateToLastVersion()
		{
			MigrateTo(LastVersion);
		}

		/// <summary>
		/// Returns the last version of the migrations.
		/// </summary>
		public int LastVersion
		{
			get
			{
				if (migrationsTypes.Count == 0)
				{
					return 0;
				}
				return GetMigrationVersion(migrationsTypes[migrationsTypes.Count - 1]);
			}
		}

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<Type> MigrationsTypes
		{
			get { return migrationsTypes; }
		}

		/// <summary>
		/// Get or set the event logger.
		/// </summary>
		public ILogger Logger
		{
			set { logger = value; }
		}


		/// <summary>
		/// Returns the version of the migration
		/// <see cref="MigrationAttribute">MigrationAttribute</see>.
		/// </summary>
		/// <param name="t">Migration type.</param>
		/// <returns>Version number sepcified in the attribute</returns>
		public static int GetMigrationVersion(Type t)
		{
			return MigrationHelper.GetMigrationVersion(t);
		}

		private void CheckForDuplicatedVersion()
		{
			var versions = new List<int>();

			foreach (Type t in migrationsTypes)
			{
				int version = GetMigrationVersion(t);

				if (versions.Contains(version))
					throw new DbRefactorException(String.Format("Migration version #{0} is duplicated", version));

				versions.Add(version);
			}
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		/// <returns>The migrations collection</returns>
		private static List<Type> GetMigrationTypes(Assembly asm)
		{
			return asm.GetTypes().Where(MigrationHelper.IsMigration)
				.OrderBy(t => t, new MigrationTypeComparer(true)).ToList();
		}

		/// <summary>
		/// Convert a classname to something more readable.
		/// ex.: CreateATable => Create a table
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		internal static string ToHumanName(string className)
		{
			string name = Regex.Replace(className, "([A-Z])", " $1").Substring(1);
			return name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
		}

		#region Helper methods

		private BaseMigration GetMigration(int version)
		{
			foreach (Type t in migrationsTypes)
			{
				if (GetMigrationVersion(t) == version)
				{
					return migrationTarget.CreateMigration(t);
				}
			}
			throw new DbRefactorException(String.Format("Migration {0} was not found", version));
		}

		#endregion

		private static Type GetSetUpMigrationType(Assembly asm)
		{
			var setupList = new List<Type>();

			foreach (Type t in asm.GetTypes())
			{
				var attrib = (SetUpMigrationAttribute)
				             Attribute.GetCustomAttribute(t, typeof (SetUpMigrationAttribute));
				if (attrib != null)
				{
					setupList.Add(t);
				}
			}

			if (setupList.Count > 1)
			{
				throw new DbRefactorException("Found more than one classes with SetUpMigrationAttribute");
			}

			if (setupList.Count == 0)
			{
				return null;
			}

			return setupList[0];
		}

		private readonly Type setUpMigration;

		private void RunGlobalSetUp()
		{
			ExecuteSetupMethodWith(typeof (MigratorSetUp));
		}

		private void RunSetUp()
		{
			ExecuteSetupMethodWith(typeof (MigrationSetUp));
		}

		private void RunGlobalTearDown()
		{
			ExecuteSetupMethodWith(typeof (MigratorTearDown));
		}

		private void RunTearDown()
		{
			ExecuteSetupMethodWith(typeof (MigrationTearDown));
		}

		private void ExecuteSetupMethodWith(Type attributeType)
		{
			if (setUpMigration == null)
			{
				return;
			}
			object setupObject = Activator.CreateInstance(setUpMigration);
			MethodInfo[] methods = setUpMigration.GetMethods();
			MethodInfo globalSetupMethod = null;
			foreach (MethodInfo method in methods)
			{
				if (method.GetCustomAttributes(attributeType, false).Length > 0)
				{
					globalSetupMethod = method;
				}
			}
			if (globalSetupMethod != null)
			{
				globalSetupMethod.Invoke(setupObject, new object[] {});
			}
		}
	}
}