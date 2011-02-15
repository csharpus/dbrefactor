using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer.Columns
{
	public class NTextProvider : ColumnProvider
	{
		public NTextProvider(string name, object defaultValue)
			: base(name, defaultValue)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.NText(Name, null);
		}
	}
}
