using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class SqlServerCeCreateTableTests : CreateTableTests
	{
		public override string GetConnectionString()
		{
			return @"Data Source=..\..\Database\SqlServerCe.sdf";
		}

		protected override DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerCeFactory(GetConnectionString(), null, true);
		}
	}

	[TestFixture]
	public class SqlServerCeAlterTableTests : AlterTableTests
	{
		public override string GetConnectionString()
		{
			return @"Data Source=..\..\Database\SqlServerCe.sdf";
		}

		protected override DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerCeFactory(GetConnectionString(), null, true);
		}
	}

	[TestFixture]
	public class SqlServerCeCrudTests : CrudTests
	{
		public override string GetConnectionString()
		{
			return @"Data Source=..\..\Database\SqlServerCe.sdf";
		}

		protected override DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerCeFactory(GetConnectionString(), null, true);
		}
	}

	[TestFixture]
	public class SqlServerCeAddColumnTests : AddColumnTests
	{
		public override string GetConnectionString()
		{
			return @"Data Source=..\..\Database\SqlServerCe.sdf";
		}

		protected override DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerCeFactory(GetConnectionString(), null, true);
		}
	}
}