using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;

namespace DbRefactor.Providers.Columns
{
	public class MoneyProvider : ColumnProvider
	{
		public MoneyProvider(string name, object defaultValue)
			: base(name, defaultValue)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Money(Name);
		}
	}
}