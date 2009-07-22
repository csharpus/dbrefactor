using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	internal class IntProvider : ColumnProvider
	{
		public IntProvider(string name) : base(name)
		{
			
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Int(Name);
		}
	}
}