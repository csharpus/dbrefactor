using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	internal class DecimalProvider : ColumnProvider
	{
		public int Precision { get; private set; }
		public int Radix { get; private set; }

		public DecimalProvider(string name, int precision, int radix) : base(name)
		{
			Precision = precision;
			Radix = radix;
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Decimal(Name, Precision, Radix);
		}
	}
}