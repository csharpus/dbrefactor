using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Runner;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;

namespace DbRefactor.Tests.Integration.Runner
{
	[TestFixture]
	public class MigrationRunnerTests
	{
		[Test]
		public void Should_run_passed_migrations()
		{
			var migrations = new List<IVersionedMigration>();
			var m1 = MockRepository.GenerateMock<IVersionedMigration>();
			var m2 = MockRepository.GenerateMock<IVersionedMigration>();
			var migrationTarget = MockRepository.GenerateStub<IMigrationTarget>();
			var logger = MockRepository.GenerateStub<ILogger>();
			migrations.Add(m1);
			migrations.Add(m2);
			var migrationSerivce = new MigrationRunner(migrationTarget, logger);
			migrationSerivce.MigrateUp(migrations);
			m1.AssertWasCalled(m => m.Up());
			m2.AssertWasCalled(m => m.Up());
		}

		[Test]
		public void should_update_version()
		{
			const int lattestVersion = 2;
			var migrations = new List<IVersionedMigration>();
			var m1 = MockRepository.GenerateStub<IVersionedMigration>();
			var m2 = MockRepository.GenerateStub<IVersionedMigration>();
			m2.Expect(m => m.Version).Return(lattestVersion);
			var migrationTarget = MockRepository.GenerateMock<IMigrationTarget>();
			var logger = MockRepository.GenerateStub<ILogger>();
			migrations.Add(m1);
			migrations.Add(m2);
			var migrationSerivce = new MigrationRunner(migrationTarget, logger);
			migrationSerivce.MigrateUp(migrations);
			migrationTarget.AssertWasCalled(t => t.UpdateVersion(lattestVersion));
		}
	}
}
