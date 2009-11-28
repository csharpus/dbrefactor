using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines;

namespace DbRefactor.Providers.Properties
{
	internal class NullProvider : PropertyProvider
	{
		public NullProvider(IColumnProperties columnProperties)
			: base(columnProperties)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Null();
		}

		public override string CreateTableSql()
		{
			return ColumnProperties.Null();
		}

		public override string AlterTableSql()
		{
			return ColumnProperties.Null();
		}

		public override string AddTableSql()
		{
			return ColumnProperties.Null();
		}
	}
}