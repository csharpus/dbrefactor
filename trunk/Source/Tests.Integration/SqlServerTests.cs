using DbRefactor.Providers;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class SqlServerTests
	{
		[Test]
		public void should_create_table()
		{
			// var m = new DbRefactor.Migrator("sqlserver", "");
			var m = new CreateTableMigration();
			m.TransformationProvider = new TransformationProvider(new SqlServerEnvironment(@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI"));
			m.Up();
			Assert.That(m.TransformationProvider.TableExists("Test"), Is.True);
			Assert.That(m.TransformationProvider.ColumnExists("Test", "Id"));
		}

		[Migration(1)]
		public class CreateTableMigration : UpMigration
		{
			public override void Up()
			{
				CreateTable("Test").Int("Id").Execute();
			}
		}

		// private Migration CreateMigration(Func<)
	}

	public abstract class UpMigration : Migration
	{
		public override void Down()
		{
			
		}
	}
}