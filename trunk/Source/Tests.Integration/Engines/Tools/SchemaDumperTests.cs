using System;
using System.Text.RegularExpressions;
using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.Tools
{
	[TestFixture]
	public class SchemaDumperTests : ProviderTestBase
	{
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
		public void should_create_schema_dump()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").Execute();
			DatabaseEnvironment.CloseConnection();

			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Console.Write(values);
		}

		[Test]
		public void should_create_schema_dump_for_primary_key()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").PrimaryKey().Execute();
			DatabaseEnvironment.CloseConnection();

			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Assert.That(values.Contains("PrimaryKey()"));
		}

		[Test]
		public void should_create_shema_dump_for_unique()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").Unique().Execute();
			DatabaseEnvironment.CloseConnection();

			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Assert.That(values.Contains("Unique()"));
		}

		[Test]
		public void should_create_schema_dump_for_null()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").Null().Execute();
			DatabaseEnvironment.CloseConnection();

			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Assert.That(!values.Contains(".NotNull()"));	
		}

		[Test]
		public void should_create_dump_for_not_null()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").NotNull().Execute();
			DatabaseEnvironment.CloseConnection();

			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Assert.That(values.Contains(".NotNull()"));
		}

		[Test]
		public void should_create_unique_only_for_target_column()
		{
			DatabaseEnvironment.OpenConnection();
			Database.CreateTable("A").Int("B").Unique().Int("C").Execute();
			DatabaseEnvironment.CloseConnection();

			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Assert.That(values.Contains(".Unique()"));
			Assert.That(new Regex(@"\.Unique\(\)").Matches(values).Count, Is.EqualTo(1));
		}
	}

	[TestFixture]
	public class ExternalDbTests
	{
		[Test, Ignore]
		public void should_create_schema_dump()
		{
			var d = DbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=  ;Integrated Security=SSPI");
			var result = d.Dump();
		}
	}
}