using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.SqlServer2005
{
	[TestFixture]
	public class SqlServer2005CreateTableTests : CreateTableTests
	{
		public override string GetConnectionString()
		{
			return ConnectionString;
		}

		protected override DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerFactory(GetConnectionString(), null, true);
		}
	}
}