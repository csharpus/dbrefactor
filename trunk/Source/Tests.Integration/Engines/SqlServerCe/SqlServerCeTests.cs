using DbRefactor.Factories;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.SqlServerCe
{
	class SqlServerCeHelper
	{
		public static DbRefactorFactory CreateFactory()
		{
			return DbRefactorFactory.BuildSqlServerCeFactory(
				@"Data Source=..\..\Database\SqlServerCe.sdf");
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
	public class SqlServerCeSqlGenerationVerificationTests : SqlGenerationVerificationTests
	{
		protected override DbRefactorFactory CreateFactory()
		{
			return SqlServerCeHelper.CreateFactory();
		}
	}
}
