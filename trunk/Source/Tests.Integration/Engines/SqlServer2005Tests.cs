using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class SqlServerCreateTableTests : CreateTableTests
	{
		public override string GetConnectionString()
		{
			return ConnectionString;
		}

		protected override ProviderFactory CreateProviderFactory()
		{
			return new SqlServerFactory(GetConnectionString(), new ConsoleLogger(), null);
		}
	}
}