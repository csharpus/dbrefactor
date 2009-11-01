using System.Collections.Generic;
using System.Linq;
using DbRefactor.Runner;
using NUnit.Framework;
using Rhino.Mocks;

namespace DbRefactor.Tests.Integration.Runner
{
	[TestFixture]
	public class MigrationReaderTests
	{
		[Test]
		public void Should_load_all_migrations_from_specified_assembly()
		{
			var migrationTarget = MockRepository.GenerateMock<IMigrationTarget>();
			migrationTarget.Expect(t => t.CreateMigration(null)).Return(null).IgnoreArguments();
			var reader = new MigrationReader(migrationTarget);

			IEnumerable<IVersionedMigration> migrations = reader.ReadFrom(typeof (Example.CreateRoleTable).Assembly);

			Assert.That(migrations.Count(), Is.EqualTo(17));
		}

		[Test]
		public void Should_assign_version_number_to_loaded_migrations()
		{
			var migrationTarget = MockRepository.GenerateMock<IMigrationTarget>();
			migrationTarget.Expect(t => t.CreateMigration(null)).Return(null).IgnoreArguments();
			var reader = new MigrationReader(migrationTarget);

			IEnumerable<IVersionedMigration> migrations = reader.ReadFrom(typeof(Example.CreateRoleTable).Assembly);

			Assert.That(migrations.ElementAt(0).Version, Is.EqualTo(1));
		}
	}
}