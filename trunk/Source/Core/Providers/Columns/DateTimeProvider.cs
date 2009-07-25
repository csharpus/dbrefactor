using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public class DateTimeProvider : ColumnProvider
	{
		public DateTimeProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.DateTime(Name);
		}

		protected override string DefaultValueCode()
		{
			return CodeGenerationService.DateTimeValue((DateTime) DefaultValue);
		}
	}
}