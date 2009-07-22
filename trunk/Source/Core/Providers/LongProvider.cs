using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	public class LongProvider : ColumnProvider
	{
		public LongProvider(string name) : base(name)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Long(Name);
		}
	}
}