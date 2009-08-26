using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines;

namespace DbRefactor.Providers.Properties
{
	internal class EmptyProvider : PropertyProvider
	{
		public EmptyProvider(IColumnProperties columnProperties)
			: base(columnProperties)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => Empty(t);
		}

		private static NewTable Empty(NewTable t)
		{
			return t;
		}

		public override string CreateTableSql()
		{
			return String.Empty;
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