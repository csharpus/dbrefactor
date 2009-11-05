using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure.Loggers;

namespace DbRefactor.Runner
{
	public interface IMigrationRunner
	{
		void MigrateUp(IEnumerable<IVersionedMigration> migrations);
		void MigrateDown(IEnumerable<IVersionedMigration> migrations);
	}

	public class MigrationRunner : IMigrationRunner
	{
		private readonly IMigrationTarget migrationTarget;
		private readonly ILogger logger;

		internal MigrationRunner(IMigrationTarget migrationTarget, ILogger logger)
		{
			this.migrationTarget = migrationTarget;
			this.logger = logger;
		}

		public void MigrateUp(IEnumerable<IVersionedMigration> migrations)
		{
			var orderdMigrations = migrations.OrderBy(m => m.Version).ToList();
			RunMigrations(orderdMigrations, m => m.Up());
			UpdateVersion(orderdMigrations.Last().Version);
		}

		public void MigrateDown(IEnumerable<IVersionedMigration> migrations)
		{
			var orderdMigrations = migrations.OrderByDescending(m => m.Version).ToList();
			RunMigrations(orderdMigrations, m => m.Down());
			UpdateVersion(orderdMigrations.Last().Version - 1);
		}

		private void UpdateVersion(int version)
		{
			migrationTarget.UpdateVersion(version);
		}

		private void RunMigrations(IEnumerable<IVersionedMigration> migrations, Action<IVersionedMigration> directionMethod)
		{
			foreach (var migration in migrations)
			{
				logger.MigrateTo(migration.Version, migration.HumanName);
				try
				{
					migrationTarget.BeginTransaction();
					directionMethod(migration);
					migrationTarget.CommitTransaction();
				}
				catch (Exception exception)
				{
					migrationTarget.RollbackTransaction();
					logger.Exception(migration.Version, migration.HumanName, exception);
					throw new MigrationException(migration.HumanName, migration.Version, exception);
				}
			}
		}
	}
}