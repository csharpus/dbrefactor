using DbRefactor.Exceptions;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class AlterTableTests : ProviderTestBase
	{
		[Test]
		public void Can_change_type()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").ConvertTo().Long();
		}

		[Test]
		public void Can_add_unique()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddUnique();

			Assert.That(Provider.UniqueExists("UQ_A_B"));
		}

		[Test]
		public void Can_drop_unique()
		{
			Database.CreateTable("A").Int("B").Unique().Execute();
			Database.Table("A").Column("B").DropUnique();

			Assert.False(Provider.UniqueExists("UQ_A_B"));
		}

		[Test]
		public void Can_add_unique_for_several_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddUnique();

			Assert.That(Provider.UniqueExists("UQ_A_B_C"));
		}

		[Test]
		public void Can_drop_unique_for_several_columns()
		{
			Database.CreateTable("A")
				.Int("B")
				.Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddUnique();
			Database.Table("A").Column("B").Column("C").DropUnique();

			Assert.False(Provider.UniqueExists("UQ_A_B_C"));
		}

		[Test]
		[Ignore("Do we need this test (at least here)? It is actually testing target database")]
		public void Can_add_complex_unique_for_the_same_column_several_times()
		{
			Database.CreateTable("A")
				.Int("B")
				.Int("C")
				.Int("D").Execute();
			Database.Table("A").Column("B").Column("C").AddUnique();
			Database.Table("A").Column("B").Column("D").AddUnique();
		}

		[Test]
		[ExpectedException(typeof(DbRefactorException))]
		public void Should_fail_when_dropping_unique_constraint_for_incorrect_columns()
		{
			Database.CreateTable("A")
				.Int("B")
				.Int("C").Unique().Execute();

			Database.Table("A").Column("B").Column("C").DropUnique();
		}

		[Test]
		public void Can_add_index()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddIndex();

			Assert.That(Provider.IndexExists("IX_A_B"));
		}

		[Test]
		public void Can_drop_index()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddIndex();
			Database.Table("A").Column("B").DropIndex();

			Assert.False(Provider.IndexExists("IX_A_B"));
		}

		[Test]
		public void Can_add_index_for_several_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddIndex();

			Assert.That(Provider.IndexExists("IX_A_B_C"));
		}

		[Test]
		public void Can_drop_index_for_several_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddIndex();
			Database.Table("A").Column("B").Column("C").DropIndex();

			Assert.False(Provider.IndexExists("IX_A_B_C"));
		}

		[Test]
		[ExpectedException(typeof (DbRefactorException))]
		public void Should_not_delete_any_indexes_if_they_are_not_for_all_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").AddIndex();
			Database.Table("A").Column("B").Column("C").DropIndex();
		}

		[Test]
		public void Can_make_column_nullable()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").SetNull();

			Assert.True(Provider.IsNullable("A", "B"));
		}

		[Test]
		public void Can_make_column_not_null()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").SetNotNull();

			Assert.False(Provider.IsNullable("A", "B"));
		}

		[Test]
		public void Can_set_default_value()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").SetDefault(1);
		}

		[Test]
		public void Can_drop_default_value()
		{
			Database.CreateTable("A").Int("B", 1).Execute();
			Database.Table("A").Column("B").DropDefault();
		}

		[Test]
		public void Can_add_foreign_key()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();
			Database.CreateTable("A1").Int("B1").Execute();
			Database.Table("A1").Column("B1").AddForeignKeyTo("A", "B");

			Assert.That(Provider.ForeignKeyExists("FK_A1_A"));
		}

		[Test]
		public void Can_drop_foreign_key()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();
			Database.CreateTable("A1").Int("B1").Execute();
			Database.Table("A1").Column("B1").AddForeignKeyTo("A", "B");
			Database.Table("A1").Column("B1").DropForeignKey("A", "B");

			Assert.False(Provider.ForeignKeyExists("FK_A1_A"));
		}

		[Test]
		public void Can_add_foreign_key_for_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
			Database.CreateTable("A1").Int("B1").Int("C1").Execute();
			Database.Table("A1").Column("B1").Column("C1").AddForeignKeyTo("A", "B", "C");

			Assert.That(Provider.ForeignKeyExists("FK_A1_A"));
		}

		[Test]
		public void Can_drop_foreign_key_for_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
			Database.CreateTable("A1").Int("B1").Int("C1").Execute();
			Database.Table("A1").Column("B1").Column("C1").AddForeignKeyTo("A", "B", "C");
			Database.Table("A1").Column("B1").Column("C1").DropForeignKey("A", "B", "C");

			Assert.False(Provider.ForeignKeyExists("FK_A1_A"));
		}

		[Test]
		public void Can_add_primary_key()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").AddPrimaryKey();

			Assert.True(Provider.PrimaryKeyExists("PK_A_B"));
		}

		[Test]
		public void Can_drop_primary_key()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").AddPrimaryKey();
			Database.Table("A").Column("B").DropPrimaryKey();

			Assert.False(Provider.PrimaryKeyExists("PK_A_B"));
		}

		[Test]
		public void Can_add_primary_key_for_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();

			Assert.That(Provider.PrimaryKeyExists("PK_A_B_C"));
		}

		[Test]
		public void Can_drop_primary_key_for_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
			Database.Table("A").Column("B").Column("C").DropPrimaryKey();

			Assert.False(Provider.PrimaryKeyExists("PK_A_B_C"));
		}

		// TODO: Add fail verification tests for:
		// - using incorrect columns
		// - inconsistent number of columns in foreign key for several columns
		// Guess it is just verification test that databases provide needed information for resolving errors
	}
}