using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public class DecimalProvider : ColumnProvider
	{
		private readonly int precision;
		private readonly int radix;
		
		public DecimalProvider(string name, object defaultValue, int precision, int radix,	ICodeGenerationService codeGenerationService) : base(name, defaultValue, codeGenerationService)
		{
			this.precision = precision;
			this.radix = radix;
		}

		public int Radix
		{
			get { return radix; }
		}

		public int Precision
		{
			get { return precision; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Decimal(Name, Precision, Radix);
		}
	}
}