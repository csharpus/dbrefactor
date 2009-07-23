using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	public class LongProvider : ColumnProvider
	{
		private long? defaultValue = null;

		public LongProvider(string name, object defaultValue) : base(name)
		{
			if (defaultValue != null)
				this.defaultValue = Convert.ToInt64(defaultValue);
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Long(Name);
		}
	}
}