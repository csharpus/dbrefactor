using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DbRefactor.Runner
{
	public class MigrationService
	{
		private readonly IMigrationTarget migrationTarget;
		private readonly IMigrationRunner migrationRunner;
		private readonly IMigrationReader migrationReader;

		public MigrationService(IMigrationTarget migrationTarget, IMigrationRunner migrationRunner, IMigrationReader migrationReader)
		{
			this.migrationTarget = migrationTarget;
			this.migrationRunner = migrationRunner;
			this.migrationReader = migrationReader;
		}

		public void MigrateTo(Assembly assembly, int version)
		{
			if (version < 0)
			{
				throw new ArgumentException("Should be greater than zero", "version");
			}
			var migrations = migrationReader.ReadFrom(assembly);
			MigrateInTransaction(version, migrations);
		}

		public void MigrateToLastVersion(Assembly assembly)
		{
			var migrations = migrationReader.ReadFrom(assembly);
			var maxVersion = migrations.Max(m => m.Version);
			MigrateInTransaction(maxVersion, migrations);
			// TODO: think about case when last version in dll is less than version in database
		}

		private void MigrateInTransaction(int version, IEnumerable<IVersionedMigration> migrations)
		{
			try
			{
				migrationTarget.BeginTransaction();
				Migrate(version, migrations);
				migrationTarget.CommitTransaction();
			}
			catch
			{
				migrationTarget.RollbackTransaction();
				throw;
			}
			finally
			{
				migrationTarget.CloseConnection();
			}
		}

		private void Migrate(int version, IEnumerable<IVersionedMigration> migrations)
		{
			int currentVersion = migrationTarget.GetVersion();
			if (version > currentVersion)
			{
				var filteredMigrations = migrations.Where(m => m.Version > currentVersion 
					&& m.Version <= version).ToList();
				migrationRunner.MigrateUp(filteredMigrations);
			}
			else
			{
				var filteredMigrations = migrations.Where(m => m.Version <= currentVersion).ToList();
				migrationRunner.MigrateDown(filteredMigrations);
			}
		}
	}
}
