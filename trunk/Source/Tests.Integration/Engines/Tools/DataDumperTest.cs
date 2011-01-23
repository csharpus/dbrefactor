using System;
using System.Linq;
using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class DataDumperTest : ProviderTestBase
	{
		// TODO: refactor tests to inherit from another base class with closed connection
		public override void Setup()
		{
			CreateProvider();
			DropAllTables();
			DatabaseEnvironment.CommitTransaction();
			DatabaseEnvironment.CloseConnection();
		}

		public override void TearDown()
		{

		}

		[Test]
		[Ignore]
		public void DumpTest()
		{
			var factory = new SqlServerFactory(@"Data Source=.\SQLExpress;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");

			TransformationProvider provider = factory.GetProvider();
			SchemaHelper schemaHelper = factory.GetSchemaProvider();
			var d = new DataDumper(factory.GetEnvironment(), provider, schemaHelper, false);
			string result = d.Dump(true);
		}

		[Test]
		public void could_generate_delete_statement_for_db_with_cyclic_dependencies()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A")
				.Int("Id").PrimaryKey()
				.Int("BId").NotNull()
				.Execute();
			Database.CreateTable("B")
				.Int("Id").PrimaryKey()
				.Int("CId").NotNull()
				.Execute();
			Database.CreateTable("C")
				.Int("Id").PrimaryKey()
				.Int("AId")
				.Execute();

			Database.Table("A").Column("BId").AddForeignKeyTo("B", "Id");
			Database.Table("B").Column("CId").AddForeignKeyTo("C", "Id");
			Database.Table("C").Column("AId").AddForeignKeyTo("A", "Id");
			DatabaseEnvironment.CloseConnection();

			var factory = CreateFactory();
			var d = new DataDumper(factory.GetEnvironment(), factory.GetProvider(), factory.GetSchemaProvider(), false);
			string result = d.GenerateDeleteStatement();
			Console.Write(result);
		}

		[Test]
		public void should_generate_insert_statements()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").Execute();

			Database.Table("A").Insert(new {B = 100});
			DatabaseEnvironment.CloseConnection();

			var factory = CreateFactory();
			var d = new DataDumper(factory.GetEnvironment(), factory.GetProvider(), factory.GetSchemaProvider(), false);

			var dumper = new DataDumper(factory.GetEnvironment(), factory.GetProvider(), factory.GetSchemaProvider(), false);

			var result = dumper.Dump(false);

			Assert.That(result, Is.EqualTo("insert into [A] ([B]) values (100)\r\n"));
		}

		internal class R : ForeignKey
		{
			public R()
			{
			}

			public string Foreign
			{
				get { return ForeignTable; }
				set { ForeignTable = value; }
			}

			public string Primary
			{
				get { return PrimaryTable; }
				set { PrimaryTable = value; }
			}
		}
	}
}