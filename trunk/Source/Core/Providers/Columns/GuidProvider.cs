using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;

namespace DbRefactor.Providers.Columns
{
	public class GuidProvider : ColumnProvider
	{
		public GuidProvider(string name, object defaultValue)
			: base(name, defaultValue)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Guid(Name);
		}
	}
}