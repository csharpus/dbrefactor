using System;
using System.Diagnostics;
using DbRefactor.Extended;
using DbRefactor.Providers;
using DbRefactor.Tools;
using DbRefactor.Tools.Loggers;
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
			foreach (var table in Provider.GetTables())
			{
				Provider.DropTable(table);
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
		public void should_create_foreign_key()
		{
			CreateMigration<CreateTableMigration>().Up();
			CreateMigration<CreateForeignKeyMigration>().Up();

			Assert.That(Provider.ConstraintExists("FK_Dependent_Test"), Is.True);
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
			return new TMigration {TransformationProvider = Provider, ColumnPropertyProviderFactory = ProviderFactory.ColumnPropertyProviderFactory, ColumnProviderFactory = ProviderFactory.ColumnProviderFactory};
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
		public void Can_create_unique_column()
		{
			Database.CreateTable("A").Int("B").Unique().Execute();
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
		[Ignore("This value can be inserted - 0xC9CBBBCCCEB9C8CABCCCCEB9C9CBBB, but we need to check order of numbers when converting bytes to hex. It is better to insert and select one pixel png file")]
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
		public void Can_add_unique_constraint()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddUnique();
		}

		[Test]
		public void Can_add_unique_for_several_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddUnique();
		}

		[Test]
		public void Can_add_index()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddIndex();
		}

		[Test]
		public void Can_add_index_for_several_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddIndex();
		}

		[Test]
		public void Can_drop_index_for_column()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").AddIndex();
			Database.Table("A").Column("B").DropIndex();
		}

		[Test]
		public void Can_drop_index_for_several_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").Column("C").AddIndex();
			Database.Table("A").Column("B").Column("C").DropIndex();
		}

		[Test]
		[ExpectedException(typeof(DbRefactorException))]
		public void Should_not_delete_any_indexes_if_they_are_not_for_all_columns()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Column("B").AddIndex();
			Database.Table("A").Column("B").Column("C").DropIndex();
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
		public void Can_change_type()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Column("B").ConvertTo().Long();
		}
	}

	public abstract class UpMigration : Migration
	{
		public override void Down()
		{
		}
	}
}