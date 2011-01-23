using System;
using System.Linq.Expressions;
using DbRefactor.Api;

namespace DbRefactor.Providers.Properties
{
	public class NullProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Null();
		}
	}
}