using System;
using System.Linq.Expressions;
using DbRefactor.Api;

namespace DbRefactor.Providers.Properties
{
	public class EmptyProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => Empty(t);
		}

		private static NewTable Empty(NewTable t)
		{
			return t;
		}
	}
}