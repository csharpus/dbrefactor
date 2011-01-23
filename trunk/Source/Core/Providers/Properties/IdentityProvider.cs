using System;
using System.Linq.Expressions;
using DbRefactor.Api;

namespace DbRefactor.Providers.Properties
{
	public class IdentityProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Identity();
		}
	}
}