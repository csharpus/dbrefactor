using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer.Columns
{
	public class DateTimeOffsetProvider : ColumnProvider
	{
		public DateTimeOffsetProvider(string name, object defaultValue) : base(name, defaultValue)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.DateTimeOffset(Name);
		}
	}
}