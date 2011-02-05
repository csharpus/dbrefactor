using System;
using System.Linq.Expressions;
using DbRefactor.Api;

namespace DbRefactor.Providers.Properties
{
	public abstract class PropertyProvider
	{
		public abstract Expression<Action<NewTable>> Method();

		public string MethodName()
		{
			return ((MethodCallExpression)Method().Body).Method.Name;
		}
	}
}