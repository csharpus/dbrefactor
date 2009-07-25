using System;
using System.Linq.Expressions;
using DbRefactor.Extended;

namespace DbRefactor.Providers.Columns
{
	public class FloatProvider : ColumnProvider
	{
		public FloatProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Float(Name);
		}
	}
}