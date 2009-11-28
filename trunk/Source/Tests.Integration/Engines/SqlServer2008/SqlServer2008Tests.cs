using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.SqlServer2008
{
	class SqlServer2008Helper
	{
		public static DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerFactory(
				@"Data Source=.\SQLEXPRESS2008;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI",
				null, true);
		}
	}

	[TestFixture]
	public class SqlServer2008AddColumnTests : AddColumnTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008AlterTableTests : AlterTableTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008CreateTableTests : CreateTableTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008CrudTests : CrudTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008DataDumperTests : DataDumperTest
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008ObjectManipulationTests : ObjectManipulationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008SqlGenerationVerificationTests : SqlGenerationVerificationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}
}
