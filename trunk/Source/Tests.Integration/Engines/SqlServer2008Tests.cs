using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class SqlServer2008CreateTableTests : CreateTableTests
	{
		public override string GetConnectionString()
		{
			return @"Data Source=.\SQLEXPRESS2008;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI";
		}

		protected override ProviderFactory CreateProviderFactory()
		{
			return new SqlServerFactory(GetConnectionString(), new ConsoleLogger(), null);
		}
	}
}