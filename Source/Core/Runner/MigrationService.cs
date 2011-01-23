using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DbRefactor.Runner
{
	public class MigrationService
	{
		private readonly IMigrationTarget target;
		private readonly IMigrationRunner runner;
		private readonly IMigrationReader reader;

		public MigrationService(IMigrationTarget target, IMigrationRunner runner, IMigrationReader reader)
		{
			this.target = target;
			this.runner = runner;
			this.reader = reader;
		}

		public void MigrateTo(Assembly assembly, int version)
		{
			if (version < 0)
			{
				throw new ArgumentException("Should be greater than zero", "version");
			}
			var migrations = reader.ReadFrom(assembly);
			MigrateInTransaction(version, migrations);
		}

		public void MigrateToLastVersion(Assembly assembly)
		{
			var migrations = reader.ReadFrom(assembly);
			var maxVersion = migrations.Max(m => m.Version);
			MigrateInTransaction(maxVersion, migrations);
			// TODO: think about case when last version in dll is less than version in database
		}

		private void MigrateInTransaction(int version, IEnumerable<IVersionedMigration> migrations)
		{
			try
			{
				target.OpenConnection();
				Migrate(version, migrations);
			}
			finally
			{
				target.CloseConnection();
			}
		}

		private void Migrate(int version, IEnumerable<IVersionedMigration> migrations)
		{
			int currentVersion = target.GetVersion();
			if (version == currentVersion) return; // nothing to migrate
			if (version > currentVersion)
			{
				var filteredMigrations = migrations.Where(m => m.Version > currentVersion 
					&& m.Version <= version).ToList();
				runner.MigrateUp(filteredMigrations);
			}
			else
			{
				var filteredMigrations = migrations.Where(m => m.Version <= currentVersion).ToList();
				runner.MigrateDown(filteredMigrations);
			}
		}
	}
}
