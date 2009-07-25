using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public class IntProvider : ColumnProvider
	{
		public IntProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Int(Name);
		}
	}
}