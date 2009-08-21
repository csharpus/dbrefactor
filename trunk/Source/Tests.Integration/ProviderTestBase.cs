using System;
using DbRefactor.Api;
using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	public class ProviderTestBase
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
			var info = new ProviderFactory().CreateAll(ConnectionString, new ConsoleLogger());
			Provider = info.Provider;
			Database = info.Database;
		}
	}
}