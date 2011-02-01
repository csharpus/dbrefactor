using System;
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

			var d = NewDbRefactorFactory.SqlServer().CreateSchemaDumper(@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string values = d.Dump();
			Console.Write(values);
		}

		protected virtual string GetCreateTableSql()
		{
			return "override GetCreateTableSql method";
		}
	}
}