using System;
using System.Linq.Expressions;
using DbRefactor.Extended;

namespace DbRefactor.Providers.Columns
{
	public class BinaryProvider : ColumnProvider
	{
		public BinaryProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService) : base(name, defaultValue, codeGenerationService)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Binary(Name);
		}

		protected override string DefaultValueCode()
		{
			return string.Empty;
			//return CodeGenerationService.BinaryValue((byte[]) DefaultValue);
		}
	}
}