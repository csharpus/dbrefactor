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

		protected virtual string GetCreateTableSql()
		{
			return "override GetCreateTableSql method";
		}

		[Test]
		public void Should_create_schema_dump()
		{
			#region CreateTable

			Provider.ExecuteNonQuery(GetCreateTableSql());

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
			Database.Table("A").RenameTo("C");

			Assert.True(Provider.TableExists("C"));
		}

		[Test]
		public void Can_rename_column()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Column("B").RenameTo("C");

			Assert.True(Provider.ColumnExists("A", "C"));
		}
	}
}