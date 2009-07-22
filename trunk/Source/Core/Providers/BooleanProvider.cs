namespace DbRefactor.Providers
{
	public class BooleanProvider : ColumnProvider
	{
		public BooleanProvider(string name) : base(name)
		{
			
		}

		public override System.Linq.Expressions.Expression<System.Action<NewTable>> Method()
		{
			return t => t.Boolean(Name);
		}
	}
}