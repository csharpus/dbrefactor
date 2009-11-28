using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.SqlServerCe
{
	class SqlServerCeHelper
	{
		public static DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerCeFactory(
				@"Data Source=..\..\Database\SqlServerCe.sdf",
				null, true);
		}
	}

	[TestFixture]
	public class SqlServerCeAddColumnTests : AddColumnTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServerCeAlterTableTests : AlterTableTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServerCeCreateTableTests : CreateTableTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServerCeCrudTests : CrudTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServerCeDataDumperTests : DataDumperTest
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServerCeObjectManipulationTests : ObjectManipulationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServerCeSqlGenerationVerificationTests : SqlGenerationVerificationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}
}
