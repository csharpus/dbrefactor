using System;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class GenericProviderTests : ProviderTestBase
	{
		[Test]
		public void should_create_table()
		{
			Database.CreateTable("Test").Int("Id").Execute();

			Assert.That(Provider.TableExists("Test"), Is.True);
			Assert.That(Provider.ColumnExists("Test", "Id"));
		}

		[Test]
		public void should_drop_table()
		{
			Database.CreateTable("Test").Int("Id").Execute();
			Database.DropTable("Test");

			Assert.That(Provider.TableExists("Test"), Is.False);
		}

		[Test]
		public void should_add_column()
		{
			Database.CreateTable("Test").Int("Id").Execute();
			Database.Table("Test").AddColumn().String("Name", 5).Execute();

			Assert.That(Provider.ColumnExists("Test", "Name"), Is.True);
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
		[Ignore("Doesn't work")]
		public void Can_create_primary_key_on_two_columns()
		{
			Database.CreateTable("A").Int("B").PrimaryKey().Int("C").PrimaryKey().Execute();
		}
	}

	public abstract class UpMigration : Migration
	{
		public override void Down()
		{
		}
	}
}