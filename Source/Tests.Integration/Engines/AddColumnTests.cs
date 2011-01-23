using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class AddColumnTests : ProviderTestBase
	{
		[Test]
		public void can_add_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C"));

			Assert.True(SchemaHelper.ColumnExists("A", "C"));
		}

		[Test]
		public void can_create_column_with_several_properties()
		{
			Database.CreateTable("A").Int("B").Unique().NotNull().Execute();

			Assert.True(SchemaHelper.ColumnExists("A", "B"));
		}

		[Test]
		public void can_add_not_null_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C", 0).NotNull());

			Assert.False(SchemaHelper.IsNullable("A", "C"));
		}

		[Test]
		public void can_add_unique_column_to_table()
		{
	//        public enum DbType
	//{
	//    AnsiString = 0, -- varchar
	//    Binary = 1, -- binary
	//    Byte = 2, -- byte
	//    Boolean = 3, -- bit
	//    Currency = 4, -- currenty
	//    Date = 5, --date
	//    DateTime = 6, --datetime
	//    Decimal = 7, --decimal
	//    Double = 8,--double
	//    Guid = 9,--guid
	//    Int16 = 10, --int
	//    Int32 = 11,
	//    Int64 = 12,
	//    Object = 13, ???
	//    SByte = 14, ???
	//    Single = 15, ???
	//    String = 16,
	//    Time = 17,
	//    UInt16 = 18,
	//    UInt32 = 19,
	//    UInt64 = 20,
	//    VarNumeric = 21, ???
	//    AnsiStringFixedLength = 22,
	//    StringFixedLength = 23,
	//    Xml = 25,
	//    DateTime2 = 26,
	//    DateTimeOffset = 27, ???
	//}
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C").Unique());

			Assert.True(SchemaHelper.UniqueExists("UQ_A_C"));
		}

		[Test]
		public void can_add_identity_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C").NotNull().Identity());

			Assert.True(SchemaHelper.IsIdentity("A", "C"));
		}

		[Test]
		public void can_add_column_with_default_value()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C", 1));

			Assert.True(SchemaHelper.IsDefault("A", "C"));
		}

		[Test]
		public void can_add_primary_key_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn(c => c.Int("C").PrimaryKey().Identity());

			Assert.That(SchemaHelper.PrimaryKeyExists("PK_A_C"));
		}

		[Test]
		public void can_drop_column()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").DropColumn("C");

			Assert.False(SchemaHelper.ColumnExists("A", "C"));
		}

		[Test]
		public void can_drop_column_with_foreign_key()
		{
			Database.CreateTable("Related").Int("K").PrimaryKey().Execute();
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("C").AddForeignKeyTo("Related", "K");
			Database.Table("A").DropColumn("C");
		}

		[Test]
		public void can_drop_column_with_unique()
		{
			Database.CreateTable("A").Int("B").Int("C").Unique().Execute();
			Database.Table("A").DropColumn("C");
		}

		[Test]
		public void can_drop_column_with_default()
		{
			Database.CreateTable("A").Int("B").Int("C", 1).Execute();
			Database.Table("A").DropColumn("C");
		}

		[Test, Ignore("not supported")]
		public void can_drop_column_with_index()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("C").AddIndex();

			Database.Table("A").DropColumn("C");
		}

		[Test]
		public void can_drop_column_with_identity()
		{
			Database.CreateTable("A").Int("B").Int("C").Identity().Execute();
			Database.Table("A").DropColumn("C");
		}

		[Test]
		public void can_drop_primary_key_column()
		{
			Database.CreateTable("A").Int("B").Int("C").PrimaryKey().Execute();
			Database.Table("A").DropColumn("C");
		}

		[Test]
		public void test()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Int("C").Unique().Int("D", 1).Int("E").Execute();
			Database.Table("A").Column("C").AddIndex();
		}
	}
}