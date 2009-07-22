using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	public class ColumnProvider
	{
		public ColumnProvider(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public virtual Expression<Action<NewTable>> Method()
		{
			return t => t.String(null, default(int));
		}

		public string MethodName()
		{
			return ((MethodCallExpression) Method().Body).Method.Name;
		}
	}
}