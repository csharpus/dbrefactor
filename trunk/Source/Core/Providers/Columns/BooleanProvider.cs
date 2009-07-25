namespace DbRefactor.Providers.Columns
{
	public class BooleanProvider : ColumnProvider
	{
		public BooleanProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
		}

		public override System.Linq.Expressions.Expression<System.Action<NewTable>> Method()
		{
			return t => t.Boolean(Name);
		}
	}
}