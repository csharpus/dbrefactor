using System;
using DbRefactor.Factories;
using DbRefactor.Providers.Model;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.Tools
{
	[TestFixture]
	public class DataDumperTest : ProviderTestBase
	{
		// TODO: refactor tests to inherit from another base class with closed connection
		public override void Setup()
		{
			CreateProvider();
			DatabaseEnvironment.OpenConnection();
			DatabaseEnvironment.BeginTransaction();
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
			var d = DbRefactorFactory.SqlServer().CreateDataDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");

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

			var d = DbRefactorFactory.SqlServer().CreateDataDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
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

			var d = DbRefactorFactory.SqlServer().CreateDataDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");

			string result = d.Dump(true);

			Assert.That(result, Is.EqualTo("insert into [A] ([B]) values (100)\r\n"));
		}

		internal class R : ForeignKey
		{
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