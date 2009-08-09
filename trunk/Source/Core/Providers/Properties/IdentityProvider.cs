using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Providers.TypeToSqlProviders;

namespace DbRefactor.Providers.Properties
{
	public class IdentityProvider : PropertyProvider
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