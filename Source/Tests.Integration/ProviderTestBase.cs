using System;
using System.Linq;
using System.Text.RegularExpressions;
using DbRefactor.Api;
using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	public abstract class ProviderTestBase
	{
		private TransformationProvider provider;
		protected IDatabase Database;
		private DataDumper dataDumper;
		protected SchemaHelper SchemaHelper;
		protected IDatabaseEnvironment DatabaseEnvironment;

		[SetUp]
		public virtual void Setup()
		{
			CreateProvider();
			DatabaseEnvironment.OpenConnection();
			DatabaseEnvironment.BeginTransaction();
			DropAllTables();
		}
		
		[TearDown]
		public virtual void TearDown()
		{
			DatabaseEnvironment.CommitTransaction();
			DatabaseEnvironment.CloseConnection();
		}

		protected void DropAllTables()
		{
			
			var sql = dataDumper.GenerateDropStatement();
			var lines = Regex.Split(sql, "GO")
				.Where(l => l.Trim() != String.Empty).ToList();

			
			foreach (var line in lines)
			{
				provider.ExecuteNonQuery(line);
			}
			
		}

		private static string GetConnectionString()
		{
			return ConnectionString;
		}

		protected virtual DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.SqlServer(); // .BuildSqlServerFactory(GetConnectionString(), logger, null)
		}

		protected const string ConnectionString =
			@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI";

		protected void CreateProvider()
		{
			var factory = CreateFactory();
			Database = factory.CreateDatabase(GetConnectionString());

			SchemaHelper = ((ISchemaAccessor)Database).SchemaHelper;

			provider = ((IProviderAccessor) Database).Provider;

			
			dataDumper = factory.CreateDataDumper(GetConnectionString());
			DatabaseEnvironment = ((IEngineAccessor) Database).Engine.Environment;
		}
	}
}