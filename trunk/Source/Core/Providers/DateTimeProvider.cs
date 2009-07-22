using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	internal class DateTimeProvider : ColumnProvider
	{
		public DateTimeProvider(string name) : base(name)
		{
			
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.DateTime(Name);
		}
	}
}