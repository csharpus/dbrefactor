using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public class TextProvider : ColumnProvider
	{
		public TextProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Text(Name);
		}
	}
}