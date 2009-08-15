using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines.SqlServer;

namespace DbRefactor.Providers.Properties
{
	public class PrimaryKeyProvider : PropertyProvider
	{
		public PrimaryKeyProvider(IColumnProperties columnProperties) : base(columnProperties)
		{
		}
		
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.PrimaryKey();
		}

		public override string CreateTableSql()
		{
			return ColumnProperties.PrimaryKey();
		}

		public override string AlterTableSql()
		{
			return String.Empty;
		}

		public override string AddTableSql()
		{
			return String.Empty;
		}
	}
}