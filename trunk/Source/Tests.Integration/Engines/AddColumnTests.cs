using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class AddColumnTests : ProviderTestBase
	{
		[Test]
		public void Can_add_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C"));

			Assert.True(Provider.ColumnExists("A", "C"));
		}

		[Test]
		public void Can_create_column_with_several_properties()
		{
			Database.CreateTable("A").Int("B").Unique().NotNull().Execute();

			Assert.True(Provider.ColumnExists("A", "B"));
		}

		[Test]
		public void Can_add_not_null_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C", 0).NotNull());

			Assert.False(Provider.IsNullable("A", "C"));
		}

		[Test]
		public void Can_add_unique_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C").Unique());

			Assert.True(Provider.UniqueExists("UQ_A_C"));
		}

		[Test]
		public void Can_add_identity_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C").NotNull().Identity());

			Assert.True(Provider.IsIdentity("A", "C"));
		}

		[Test]
		public void Can_add_column_with_default_value()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C", 1));

			Assert.True(Provider.IsDefault("A", "C"));
		}

		[Test]
		public void Can_add_primary_key_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C").PrimaryKey().Identity());

			Assert.That(Provider.PrimaryKeyExists("PK_A_C"));
		}

		[Test]
		public void Can_drop_column()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").DropColumn("C");

			Assert.False(Provider.ColumnExists("A", "C"));
		}
	}
}