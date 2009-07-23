using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	public class UniqueProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Unique();
		}
	}

	public class IdentityProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Identity();
		}
	}

	public class PrimaryKeyProvider : PropertyProvider
	{
		public override Expression<Action<NewTable>> Method()
		{
			return t => t.PrimaryKey();
		}
	}

	public abstract class PropertyProvider
	{
		public abstract Expression<Action<NewTable>> Method();

		public string MethodName()
		{
			return ((MethodCallExpression)Method().Body).Method.Name;
		}
	}
}