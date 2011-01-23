using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Runner
{
	[TestFixture]
	public class AcceptanceTests : ProviderTestBase
	{
		public override void Setup()
		{
			CreateProvider();
			DropAllTables();
			DatabaseEnvironment.CommitTransaction();
			DatabaseEnvironment.CloseConnection();
		}

		public override void TearDown()
		{
			
		}

		[Test]
		public void Should_run_all_migrations_to_specified_version()
		{
			DbRefactor.Cli.Boot.Migrate(
				"sqlserver",
				ConnectionString,
				"../../../AcceptanceDatabase/bin/Debug/AcceptanceDatabase.dll",
				2,
				false,
				null);
		}

		[Test]
		public void Should_run_all_migrations_to_last_version()
		{
			DbRefactor.Cli.Boot.Migrate(
				"sqlserver",
				ConnectionString,
				"../../../AcceptanceDatabase/bin/Debug/AcceptanceDatabase.dll",
				-1,
				false,
				null);
		}

		[Test]
		public void Should_run_all_migrations_from_current_to_last_version()
		{
			DbRefactor.Cli.Boot.Migrate(
				"sqlserver",
				ConnectionString,
				"../../../AcceptanceDatabase/bin/Debug/AcceptanceDatabase.dll",
				2,
				false,
				null);

			DbRefactor.Cli.Boot.Migrate(
				"sqlserver",
				ConnectionString,
				"../../../AcceptanceDatabase/bin/Debug/AcceptanceDatabase.dll",
				-1,
				false,
				null);
		}

		[Test]
		public void Should_run_all_migrations_down_to_zero_version()
		{
			DbRefactor.Cli.Boot.Migrate(
				"sqlserver",
				ConnectionString,
				"../../../AcceptanceDatabase/bin/Debug/AcceptanceDatabase.dll",
				-1,
				false,
				null);

			DbRefactor.Cli.Boot.Migrate(
				"sqlserver",
				ConnectionString,
				"../../../AcceptanceDatabase/bin/Debug/AcceptanceDatabase.dll",
				0,
				false,
				null);
		}
	}
}
