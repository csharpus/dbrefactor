using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class AddColumnTests : ProviderTestBase
	{
		[Test]
		public void Can_add_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").Execute();

			Assert.True(Provider.ColumnExists("A", "C"));
		}

		[Test]
		public void Can_add_not_null_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").NotNull().Execute();

			Assert.False(Provider.IsNullable("A", "C"));
		}

		[Test]
		public void Can_add_unique_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").Unique().Execute();

			Assert.True(Provider.UniqueExists("UQ_A_C"));
		}

		[Test]
		public void Can_add_identity_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").NotNull().Identity().Execute();

			Assert.True(Provider.IsIdentity("A", "C"));
		}

		[Test]
		public void Can_add_column_with_default_value()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C", 1).Execute();
		}

		[Test]
		public void Can_create_column_with_several_properties()
		{
			Database.CreateTable("A").Int("B").Unique().NotNull().Execute();
		}

		[Test]
		public void Can_add_primary_key_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").PrimaryKey().Execute();

			Assert.That(Provider.PrimaryKeyExists("PK_A_C"));
		}
	}
}