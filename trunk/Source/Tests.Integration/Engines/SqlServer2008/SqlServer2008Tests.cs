using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Tests.Integration.Engines.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.SqlServer2008
{
	static class SqlServer2008Helper
	{
		public static NewDbRefactorFactory CreateFactory()
		{
			var logger = new ConsoleLogger();
			return NewDbRefactorFactory.SqlServer();

			//.BuildSqlServerFactory(
			//@"Data Source=.;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI", logger,
			//null);
		}
	}

	[TestFixture]
	public class SqlServer2008AddColumnTests : AddColumnTests
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return  SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008AlterTableTests : AlterTableTests
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008CreateTableTests : CreateTableTests
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008CrudTests : CrudTests
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008DataDumperTests : DataDumperTest
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}

	[TestFixture]
	public class SqlServer2008ObjectManipulationTests : ObjectManipulationTests
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}

		protected override string GetCreateTableSql()
		{
			#region create table
			return @"
CREATE TABLE Table1 (
	[BI] [bigint] NULL,
	[BN] [binary](50) NULL,
	[BT] [bit] NULL,
	[CH] [char](10) COLLATE Cyrillic_General_CI_AS NULL,
	[DT] [datetime] NULL,
	[DC] [decimal](18, 0) NULL,
	[FT] [float] NULL,
	[IM] [image] NULL,
	[IT] [int] NULL,
	[MN] [money] NULL,
	[NC] [nchar](10) COLLATE Cyrillic_General_CI_AS NULL,
	[NT] [ntext] COLLATE Cyrillic_General_CI_AS NULL,
	[NM] [numeric](18, 0) NULL,
	[NV] [nvarchar](50) COLLATE Cyrillic_General_CI_AS NULL,
	[NB] [nvarchar](max) COLLATE Cyrillic_General_CI_AS NULL,
	[RL] [real] NULL,
	[SD] [smalldatetime] NULL,
	[SI] [smallint] NULL,
	[SM] [smallmoney] NULL,
	[SV] [sql_variant] NULL,
	[TX] [text] COLLATE Cyrillic_General_CI_AS NULL,
	[TS] [timestamp] NULL,
	[TI] [tinyint] NULL,
	[UI] [uniqueidentifier] NOT NULL,
	[VB] [varbinary](50) NULL,
	[VM] [varbinary](max) NULL,
	[VC] [varchar](50) COLLATE Cyrillic_General_CI_AS NULL,
	[VR] [varchar](max) COLLATE Cyrillic_General_CI_AS NULL,
	[XL] [xml] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
";
			#endregion
		}
	}

	[TestFixture]
	public class SqlServer2008SqlGenerationVerificationTests : SqlGenerationVerificationTests
	{
		protected override NewDbRefactorFactory CreateFactory()
		{
			return SqlServer2008Helper.CreateFactory();
		}
	}
}
