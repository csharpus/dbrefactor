using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines;

namespace DbRefactor.Providers.Properties
{
	internal class IdentityProvider : PropertyProvider
	{
		public IdentityProvider(IColumnProperties columnProperties) : base(columnProperties)
		{
		}
		
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Identity();
		}

		public override string CreateTableSql()
		{
			return ColumnProperties.Identity();
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