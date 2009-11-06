using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	[Ignore]
	public class SqlServerCeCreateTableTests : CreateTableTests
	{
		public override string GetConnectionString()
		{
			return @"Data Source=..\..\Database\SqlServerCe.sdf";
		}

		protected override ProviderFactory CreateProviderFactory()
		{
			return new SqlServerCeFactory(GetConnectionString(), new ConsoleLogger(), null);
		}
	}
}