using System;
using System.Linq.Expressions;
using DbRefactor.Api;

namespace DbRefactor.Providers.Properties
{
	public class PrimaryKeyProvider : PropertyProvider
	{
		private readonly string name;

		public PrimaryKeyProvider(string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.PrimaryKey();
		}
	}
}