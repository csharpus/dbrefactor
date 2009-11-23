using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	public abstract class CreateTableTests : ProviderTestBase
	{
		[Test]
		public void Can_create_not_null_column()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();

			Assert.False(Provider.IsNullable("A", "B"));
		}

		[Test]
		public void Can_create_primary_key_column()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();

			Assert.That(Provider.PrimaryKeyExists("PK_A_B"));
		}

		[Test]
		public void Can_create_identity_column()
		{
			Database.CreateTable("A").Int("B").Identity().Execute();

			Assert.That(Provider.IsIdentity("A", "B"));
		}

		[Test]
		public void Can_create_default_value_column()
		{
			Database.CreateTable("A").Int("B", 1).Execute();
		}

		[Test]
		public void Can_create_unique_column()
		{
			Database.CreateTable("A").Int("B").Unique().Execute();

			Assert.That(Provider.UniqueExists("UQ_A_B"));
		}

		[Test]
		[Ignore("Doesn't work")]
		public void Can_create_primary_key_on_two_columns()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Int("C").PrimaryKey().Execute();
		}
	}
}