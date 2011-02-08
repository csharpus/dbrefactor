using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer.Columns
{
	public class BigintProvider : ColumnProvider
	{
		public BigintProvider(string name, object defaultValue) : base(name, defaultValue)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Bigint(Name);
		}
	}
}