using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	public class NotNullProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.NotNull();
		}
	}
}