using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	public class ColumnProvider
	{
		private readonly List<PropertyProvider> properties = new List<PropertyProvider>();

		public ColumnProvider(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public List<PropertyProvider> Properties
		{
			get { return properties; }
		}

		public virtual Expression<Action<NewTable>> Method()
		{
			return t => t.String(null, default(int));
		}

		public string MethodName()
		{
			return ((MethodCallExpression) Method().Body).Method.Name;
		}

		public void AddProperty(PropertyProvider provider)
		{
			Properties.Add(provider);
		}
	}
}