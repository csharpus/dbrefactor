using DbRefactor.Runner;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;

namespace DbRefactor.Tests.Integration.Runner
{
	[TestFixture]
	public class MigrationServiceTests
	{
		[Test]
		public void Should_run_passed_migrations()
		{
			var migrations = new List<BaseMigration>();
			var m1 = MockRepository.GenerateMock<BaseMigration>();
			var m2 = MockRepository.GenerateMock<BaseMigration>();
			migrations.Add(m1);
			migrations.Add(m2);
			var migrationSerivce = new MigrationService();
			migrationSerivce.Migrate(migrations);
			m1.AssertWasCalled(m => m.Up());
			m2.AssertWasCalled(m => m.Up());
		}
	}
}
