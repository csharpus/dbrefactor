using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;

namespace DbRefactor.Providers.Columns
{
	public class SmallintProvider : ColumnProvider
	{
		public SmallintProvider(string name, object defaultValue)
			: base(name, defaultValue)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Smallint(Name);
		}
	}
}