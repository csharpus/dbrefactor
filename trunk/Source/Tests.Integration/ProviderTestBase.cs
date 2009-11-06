using System;
using DbRefactor.Api;
using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	public abstract class ProviderTestBase
	{
		internal TransformationProvider Provider;
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

		public virtual string GetConnectionString()
		{
			return ConnectionString;
		}

		protected virtual ProviderFactory CreateProviderFactory()
		{
			return new SqlServerFactory(GetConnectionString(), new ConsoleLogger(), null);
		}

		public const string ConnectionString =
			@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI";

		private void CreateProvider()
		{
			var factory = CreateProviderFactory();
			factory.Init();
			//var info = new DbRefactorFactory().CreateAll(ConnectionString, new ConsoleLogger());
			Provider = factory.GetProvider();
			Database = factory.GetDatabase();
		}
	}
}