using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public class LongProvider : ColumnProvider
	{
		public LongProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Long(Name);
		}
	}
}