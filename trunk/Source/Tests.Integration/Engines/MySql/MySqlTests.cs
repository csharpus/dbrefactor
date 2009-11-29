using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.MySql
{
	class MySqlHelper
	{
		public static DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildMySqlFactory(
				@"Server=localhost;Uid=root;Pwd=1;Database=dbrefactor_tests;",
				null, true);
		}
	}

	[TestFixture]
	public class MySqlAddColumnTests : AddColumnTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class MySqlAlterTableTests : AlterTableTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class MySqlCreateTableTests : CreateTableTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class MySqlCrudTests : CrudTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class MySqlDataDumperTests : DataDumperTest
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}
	}

	[TestFixture]
	public class MySqlObjectManipulationTests : ObjectManipulationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}

		protected override string GetCreateTableSql()
		{
			#region create table
			return @"
CREATE TABLE Table1 (
	[BI] [bigint] NULL,
	[BN] [binary](50) NULL,
	[BT] [bit] NULL,
	[DT] [datetime] NULL,
	[FT] [float] NULL,
	[IM] [image] NULL,
	[IT] [int] NULL,
	[MN] [money] NULL,
	[NC] [nchar](10) NULL,
	[NT] [ntext] NULL,
	[NM] [numeric](18, 0) NULL,
	[NV] [nvarchar](50) NULL,
	[RL] [real] NULL,
	
	[SI] [smallint] NULL,
	[TI] [tinyint] NULL,
	[UI] [uniqueidentifier] NOT NULL,
	[VB] [varbinary](50) NULL	
)
";
			// [RW] [rowversion] NULL,
			#endregion
		}
	}

	[TestFixture]
	public class MySqlSqlGenerationVerificationTests : SqlGenerationVerificationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return MySqlHelper.CreateFactory();
		}
	}
}
