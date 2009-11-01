using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure.Loggers;

namespace DbRefactor.Runner
{
	public class MigrationRunner
	{
		private readonly IMigrationTarget migrationTarget;
		private readonly ILogger logger;

		internal MigrationRunner(IMigrationTarget migrationTarget, ILogger logger)
		{
			this.migrationTarget = migrationTarget;
			this.logger = logger;
		}

		internal void MigrateUp(IEnumerable<IVersionedMigration> migrations)
		{
			Migrate(migrations.OrderBy(m => m.Version), m => m.Up());
		}

		internal void MigrateDown(IEnumerable<IVersionedMigration> migrations)
		{
			Migrate(migrations.OrderByDescending(m => m.Version), m => m.Down());
		}

		private void Migrate(IEnumerable<IVersionedMigration> migrations, Action<IVersionedMigration> directionMethod)
		{
			RunMigrations(migrations, directionMethod);
			UpdateVersion(migrations);
		}

		private void UpdateVersion(IEnumerable<IVersionedMigration> migrations)
		{
			migrationTarget.UpdateVersion(migrations.Last().Version);
		}

		private void RunMigrationsInTransaction(IEnumerable<IVersionedMigration> migrations,
		                                        Action<IVersionedMigration> directionMethod)
		{
			try
			{
				migrationTarget.BeginTransaction();
				RunMigrations(migrations, directionMethod);
			}
			catch (Exception)
			{
				migrationTarget.RollbackTransaction();
				throw;
			}
			finally
			{
				migrationTarget.CloseConnection();
			}
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