using System;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class ObjectManipulationTests : ProviderTestBase
	{
		[Test]
		public void Can_create_table()
		{
			Database.CreateTable("Test").Int("Id").Execute();

			Assert.That(Provider.TableExists("Test"), Is.True);
			Assert.That(Provider.ColumnExists("Test", "Id"));
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
		public void Can_drop_table()
		{
			Database.CreateTable("Test").Int("Id").Execute();
			Database.DropTable("Test");

			Assert.That(Provider.TableExists("Test"), Is.False);
		}

		[Test]
		public void Should_create_schema_dump()
		{
			//var dumper = new SchemaDumper(@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			//dumper.Dump();

			#region CreateTable

			Provider.ExecuteNonQuery(
				@"CREATE TABLE Table1 (
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
		public void Should_generate_method_call_from_lambda()
		{
			//var codeGenerationService = MockRepository.GenerateMock<ICodeGenerationService>();
			//codeGenerationService.Expect(s => s.PrimitiveValue(1)).Return("1");
			//var longProvider = new LongProvider("ColumnName", 1, codeGenerationService);
			//string methodValue = GetMethodValue(longProvider);
			//Assert.That(methodValue, Is.EqualTo("Long(\"ColumnName\")"));
			//Assert.That(ToCsharpString(new DateTime()), Is.EqualTo("4m"));
			//Assert.That(ToCsharpStringComplex(new[] { "1" }), Is.EqualTo("4m"));
		}

		[Test]
		public void Can_rename_table()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").RenameTo("C");

			Assert.True(Provider.ColumnExists("A", "C"));
		}
	}
}