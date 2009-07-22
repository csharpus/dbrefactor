using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	internal class TextProvider : ColumnProvider
	{
		public TextProvider(string name) : base(name)
		{
			
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Text(Name);
		}
	}
}