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
		internal TransformationProvider Provider;
		protected IDatabase Database;
		private DataDumper dataDumper;

		[SetUp]
		public void Setup()
		{
			CreateProvider();
			DropAllTables();
		}

		private void DropAllTables()
		{
			var sql = dataDumper.GenerateDropStatement();
			var lines = Regex.Split(sql, "GO")
				.Where(l => l.Trim() != String.Empty).ToList();

			foreach (var line in lines)
			{
				Provider.ExecuteNonQuery(line);
			}
		}

		public virtual string GetConnectionString()
		{
			return ConnectionString;
		}

		protected virtual DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerFactory(GetConnectionString(), null, true);
		}

		public const string ConnectionString =
			@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI";

		private void CreateProvider()
		{
			var factory = CreateFactory();
			//var info = new DbRefactorFactory().CreateAll(ConnectionString, new ConsoleLogger());
			Provider = factory.GetProvider();
			Database = factory.CreateDatabase();
			dataDumper = factory.CreateDataDumper();
		}
	}
}