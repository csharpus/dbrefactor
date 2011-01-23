using DbRefactor.Runner;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace DbRefactor.Tests.Integration.Runner
{
	[TestFixture]
	public class MigrationServiceTests
	{
		private IMigrationRunner runner;
		private IMigrationTarget target;
		private IMigrationReader reader;
		private List<IVersionedMigration> migrations;
		private MigrationService migrationService;
		private IVersionedMigration migration1;
		private IVersionedMigration migration2;
		private IVersionedMigration migration3;

		[SetUp]
		public void Setup()
		{
			runner = MockRepository.GenerateMock<IMigrationRunner>();
			target = MockRepository.GenerateStub<IMigrationTarget>();
			reader = MockRepository.GenerateStub<IMigrationReader>();

			migration1 = FakeMigration(1);
			migration2 = FakeMigration(2);
			migration3 = FakeMigration(3);

			migrations = new List<IVersionedMigration>
			             	{
			             		migration1,
			             		migration2,
			             		migration3
			             	};

			reader.Expect(r => r.ReadFrom(null)).IgnoreArguments().Return(migrations);

			migrationService = new MigrationService(target, runner, reader);
		}

		[Test]
		public void Should_run_migrations_from_1_to_specified_when_executing_on_empty_db()
		{
			migrationService.MigrateTo(null, 2);

			var migrationCollection = (IEnumerable<IVersionedMigration>)runner.GetArgumentsForCallsMadeOn(r => r.MigrateUp(null))[0][0];

			CollectionAssert.AreEqual(new [] {migration1, migration2}, migrationCollection);
		}

		[Test]
		public void Should_run_migrations_from_current_to_specified_when_executing_on_non_empty_db()
		{
			target.Expect(t => t.GetVersion()).Return(1);
			migrationService.MigrateTo(null, 2);

			var migrationCollection = (IEnumerable<IVersionedMigration>)runner.GetArgumentsForCallsMadeOn(r => r.MigrateUp(null))[0][0];

			CollectionAssert.AreEqual(new[] { migration2 }, migrationCollection);
		}

		[Test]
		public void Should_run_migrations_down_from_current_to_specified_when_current_is_greater_than_specified()
		{
			target.Expect(t => t.GetVersion()).Return(2);
			migrationService.MigrateTo(null, 0);

			var migrationCollection = (IEnumerable<IVersionedMigration>)runner.GetArgumentsForCallsMadeOn(r => r.MigrateDown(null))[0][0];

			CollectionAssert.AreEqual(new[] { migration1, migration2 }, migrationCollection);
		}

		[Test]
		public void Should_run_migrations_from_current_to_lattest()
		{
			target.Expect(t => t.GetVersion()).Return(1);
			migrationService.MigrateToLastVersion(null);

			var migrationCollection = (IEnumerable<IVersionedMigration>)runner.GetArgumentsForCallsMadeOn(r => r.MigrateUp(null))[0][0];

			CollectionAssert.AreEqual(new[] { migration2, migration3 }, migrationCollection);
		}

		[Test]
		public void Should_not_run_migrations_when_current_is_lattest()
		{
			target.Expect(t => t.GetVersion()).Return(3);
			runner.Expect(r => r.MigrateUp(null)).IgnoreArguments().Repeat.Never();
			runner.Expect(r => r.MigrateDown(null)).IgnoreArguments().Repeat.Never();
			migrationService.MigrateToLastVersion(null);
			runner.VerifyAllExpectations();
		}

		private static IVersionedMigration FakeMigration(int version)
		{
			var migration = MockRepository.GenerateStub<IVersionedMigration>();
			migration.Expect(m => m.Version).Return(version);
			return migration;
		}
	}	
}
