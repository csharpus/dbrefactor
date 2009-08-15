using System;
using DbRefactor.Exceptions;
using DbRefactor.Extended;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	public class ProviderTests
	{
		protected TransformationProvider Provider;
		protected IDatabase Database;

		[SetUp]
		public void Setup()
		{
			CreateProvider();
			DropAllTables();
		}

		private void DropAllTables()
		{
			var sql = new DataDumper(Provider).GenerateDropStatement();
			if (sql != String.Empty)
			{
				Provider.ExecuteNonQuery(sql);
			}
		}

		private const string ConnectionString =
			@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI";

		private void CreateProvider()
		{
			Provider =
				new ProviderFactory().Create(ConnectionString, new ConsoleLogger());
			Database = Provider.GetDatabase();
		}
	}

	[TestFixture]
	public class SqlServerTests : ProviderTests
	{
		[Test]
		public void should_create_table()
		{
			CreateMigration<CreateTableMigration>().Up();

			Assert.That(Provider.TableExists("Test"), Is.True);
			Assert.That(Provider.ColumnExists("Test", "Id"));
		}

		[Migration(1)]
		public class CreateTableMigration : UpMigration
		{
			public override void Up()
			{
				CreateTable("Test").Int("Id").Execute();
			}
		}

		[Test]
		public void should_drop_table()
		{
			CreateMigration<CreateTableMigration>().Up();
			CreateMigration<DropTableMigration>().Up();
			Assert.That(Provider.TableExists("Test"), Is.False);
		}

		[Migration(1)]
		public class DropTableMigration : UpMigration
		{
			public override void Up()
			{
				DropTable("Test");
			}
		}

		[Test]
		public void should_add_column()
		{
			CreateMigration<CreateTableMigration>().Up();
			CreateMigration<AddColumnMigration>().Up();

			Assert.That(Provider.ColumnExists("Test", "Name"), Is.True);
		}

		[Migration(1)]
		public class AddColumnMigration : UpMigration
		{
			public override void Up()
			{
				Table("Test").AddColumn().String("Name", 5).Execute();
			}
		}


		[Test]
		public void should_create_schema_dump()
		{
			//var dumper = new SchemaDumper(@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			//dumper.Dump();

			#region CreateTable

			Provider.ExecuteNonQuery(
				@"CREATE TABLE [dbo].[Table1](
	[BI] [bigint] NULL,
	[BN] [binary](50) NULL,
	[BT] [bit] NULL,
	[CH] [char](10) COLLATE Cyrillic_General_CI_AS NULL,
	[DT] [datetime] NULL,
	[DC] [decimal](18, 0) NULL,
	[FT] [float] NULL,
	[IM] [image] NULL,
	[IT] [int] NULL,
	[MN] [money] NULL,
	[NC] [nchar](10) COLLATE Cyrillic_General_CI_AS NULL,
	[NT] [ntext] COLLATE Cyrillic_General_CI_AS NULL,
	[NM] [numeric](18, 0) NULL,
	[NV] [nvarchar](50) COLLATE Cyrillic_General_CI_AS NULL,
	[NB] [nvarchar](max) COLLATE Cyrillic_General_CI_AS NULL,
	[RL] [real] NULL,
	[SD] [smalldatetime] NULL,
	[SI] [smallint] NULL,
	[SM] [smallmoney] NULL,
	[SV] [sql_variant] NULL,
	[TX] [text] COLLATE Cyrillic_General_CI_AS NULL,
	[TS] [timestamp] NULL,
	[TI] [tinyint] NULL,
	[UI] [uniqueidentifier] NOT NULL,
	[VB] [varbinary](50) NULL,
	[VM] [varbinary](max) NULL,
	[VC] [varchar](50) COLLATE Cyrillic_General_CI_AS NULL,
	[VR] [varchar](max) COLLATE Cyrillic_General_CI_AS NULL,
	[XL] [xml] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

			#endregion

			string values = new SchemaDumper(Provider).Dump();
			Console.Write(values);
		}


		[Test]
		public void should_generate_method_call_from_lambda()
		{
			//var codeGenerationService = MockRepository.GenerateMock<ICodeGenerationService>();
			//codeGenerationService.Expect(s => s.PrimitiveValue(1)).Return("1");
			//var longProvider = new LongProvider("ColumnName", 1, codeGenerationService);
			//string methodValue = GetMethodValue(longProvider);
			//Assert.That(methodValue, Is.EqualTo("Long(\"ColumnName\")"));
			//Assert.That(ToCsharpString(new DateTime()), Is.EqualTo("4m"));
			//Assert.That(ToCsharpStringComplex(new[] { "1" }), Is.EqualTo("4m"));
		}

		[Migration(1)]
		public class CreateForeignKeyMigration : UpMigration
		{
			public override void Up()
			{
				throw new NotImplementedException();
			}
		}

		private TMigration CreateMigration<TMigration>()
			where TMigration : Migration, new()
		{
			return new TMigration
			       	{
			       		TransformationProvider = Provider,
			       		ColumnPropertyProviderFactory = ProviderFactory.ColumnPropertyProviderFactory,
			       		ColumnProviderFactory = ProviderFactory.ColumnProviderFactory
			       	};
		}

		[Test]
		public void Can_create_table_using_column_providers()
		{
			Assert.False(Provider.TableExists("A"));
			Assert.False(Provider.ColumnExists("A", "B"));

			Database.CreateTable("A").Int("B").Execute();

			Assert.True(Provider.TableExists("A"));
			Assert.True(Provider.ColumnExists("A", "B"));
		}

		[Test]
		public void Can_create_not_null_column()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
		}

		[Test]
		public void Can_create_primary_key_column()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();
		}

		[Test]
		public void Can_add_primary_key_on_two_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
		}

		[Test]
		public void Can_drop_primary_key_on_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
			Database.Table("A").Column("B").Column("C").DropPrimaryKey();
		}

		[Test]
		[Ignore("Doesn't work")]
		public void Can_create_primary_key_on_two_columns()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Int("C").PrimaryKey().Execute();
		}

		[Test]
		public void Can_create_unique_column()
		{
			Database.CreateTable("A").Int("B").Unique().Execute();
		}

		

		[Test]
		[ExpectedException(typeof (DbRefactorException))]
		public void Should_fail_when_dropping_unique_constraint_for_incorrect_columns()
		{
			Database.CreateTable("A")
				.Int("B")
				.Int("C").Unique().Execute();

			Database.Table("A").Column("B").Column("C").DropUnique();
		}

		[Test]
		public void Can_add_two_unique_indexes()
		{
			Database.CreateTable("A")
				.Int("B")
				.Int("C")
				.Int("D").Execute();
			Database.Table("A").Column("B").Column("C").AddUnique();
			Database.Table("A").Column("B").Column("D").AddUnique();
		}

		[Test]
		public void Can_create_identity_column()
		{
			Database.CreateTable("A").Int("B").Identity().Execute();
		}

		[Test]
		public void Can_create_default_value_column()
		{
			Database.CreateTable("A").Int("B", 1).Execute();
		}
	}

	[TestFixture]
	public class SqlGenerationVerificationTests : ProviderTests
	{
		[Test]
		public void Can_generate_boolean_sql()
		{
			Database.CreateTable("A").Boolean("B", true).Execute();
		}

		[Test]
		[Ignore(
			"This value can be inserted - 0xC9CBBBCCCEB9C8CABCCCCEB9C9CBBB, but we need to check order of numbers when converting bytes to hex. It is better to insert and select one pixel png file"
			)]
		public void Can_generate_binary_sql()
		{
			Database.CreateTable("A").Binary("B", new byte[] {1}).Execute();
		}

		[Test]
		public void Can_generate_datetime_sql()
		{
			Database.CreateTable("A").DateTime("B", new DateTime(2000, 1, 2, 12, 34, 56)).Execute();
		}

		[Test]
		public void Can_generate_decimal_sql()
		{
			Database.CreateTable("A").Decimal("B", 1.5M).Execute();
		}

		[Test]
		public void Can_generate_double_sql()
		{
			Database.CreateTable("A").Double("B", 1.5).Execute();
		}

		[Test]
		public void Can_generate_float_sql()
		{
			Database.CreateTable("A").Float("B", 1.5f).Execute();
		}

		[Test]
		public void Can_generate_int_sql()
		{
			Database.CreateTable("A").Int("B", 1).Execute();
		}

		[Test]
		public void Can_generate_long_sql()
		{
			Database.CreateTable("A").Long("B", 1).Execute();
		}

		[Test]
		public void Can_generate_string_sql()
		{
			Database.CreateTable("A").String("B", 5, "hello").Execute();
		}

		[Test]
		public void Can_generate_string_sql_for_empty_string()
		{
			Database.CreateTable("A").String("B", 1, string.Empty).Execute();
		}

		[Test]
		public void Can_generate_text_sql()
		{
			Database.CreateTable("A").Text("B", "hello").Execute();
		}


		[Test]
		public void Can_change_type()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").ConvertTo().Long();
		}

		[Test]
		public void Can_update_table()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Update(new {B = 1}).Where(new {B = 2}).Execute();
		}

		[Test]
		public void Can_use_null_in_where_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Update(new {B = 1}).Where(new {B = DBNull.Value}).Execute();
		}

		[Test]
		public void Can_use_null_in_update_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Update(new {B = DBNull.Value}).Where(new {B = 1}).Execute();
		}

		[Test]
		public void Can_use_where_with_several_items()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Update(new {B = DBNull.Value}).Where(new {B = 1, C = 2}).Execute();
		}

		[Test]
		public void Can_use_insert()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new {B = 1, C = 1});
		}

		[Test]
		public void Can_use_select_scalar()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new {B = 1, C = 1});
			Database.Table("A").Insert(new {B = 2, C = 2});
			Database.Table("A").SelectScalar<int>("B").Where(new {C = 2}).Execute();
		}

		[Test]
		public void Can_delete_record()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Insert(new {B = 1});
			Database.Table("A").Delete().Where(new {B = 1}).Execute();
		}

		[Test]
		public void Can_add_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").Execute();
		}

		[Test]
		public void Can_add_not_null_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").NotNull().Execute();
		}
	}

	public abstract class UpMigration : Migration
	{
		public override void Down()
		{
		}
	}

	[TestFixture]
	public class SqlServerConstraintsTests : ProviderTests
	{
		[Test]
		public void Can_add_unique_constraint()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddUnique();

			Assert.That(Provider.UniqueExists("UQ_A_B"));
		}

		[Test]
		public void Can_drop_unique_constraint()
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
		public void Can_drop_unique_constraint_for_several_columns()
		{
			Database.CreateTable("A")
				.Int("B")
				.Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddUnique();

			Database.Table("A").Column("B").Column("C").DropUnique();
			Assert.False(Provider.UniqueExists("UQ_A_B_C"));
		}

		[Test]
		public void Can_add_index()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddIndex();

			Assert.That(Provider.IndexExists("IX_A_B"));
		}

		[Test]
		public void Can_drop_index_for_column()
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
		public void Can_add_primary_key_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").PrimaryKey().Execute();
		}

		[Test]
		public void Can_add_unique_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").Unique().Execute();
		}

		[Test]
		public void Can_add_identity_column_to_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").AddColumn().Int("C").NotNull().Identity().Execute();
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
		public void Can_make_column_nullable()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").SetNull();
		}

		[Test]
		public void Can_make_column_not_null()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").SetNotNull();
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
		public void should_create_foreign_key()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();
			Database.CreateTable("A1").Int("B1").Execute();
			Database.Table("A1").Column("B1").AddForeignKeyTo("A", "B");

			Assert.That(Provider.ForeignKeyExists("FK_A1_A"));
		}

		[Test]
		public void should_drop_foreign_key()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();
			Database.CreateTable("A1").Int("B1").Execute();
			Database.Table("A1").Column("B1").AddForeignKeyTo("A", "B");
			Database.Table("A1").Column("B1").DropForeignKey("A", "B");

			Assert.False(Provider.ForeignKeyExists("FK_A1_A"));
		}

		[Test]
		public void should_create_foreign_for_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
			Database.CreateTable("A1").Int("B1").Int("C1").Execute();
			Database.Table("A1").Column("B1").Column("C1").AddForeignKeyTo("A", "B", "C");
		}

		[Test]
		public void should_drop_foreign_for_several_columns()
		{
			Database.CreateTable("A").Int("B").NotNull().Int("C").NotNull().Execute();
			Database.Table("A").Column("B").Column("C").AddPrimaryKey();
			Database.CreateTable("A1").Int("B1").Int("C1").Execute();
			Database.Table("A1").Column("B1").Column("C1").AddForeignKeyTo("A", "B", "C");
			// Database.Table("A1").Column("B1").AddForeignKeyTo("A", "B");
			Database.Table("A1").Column("B1").Column("C1").DropForeignKey("A", "B", "C");
		}
	}
}