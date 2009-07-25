using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public class StringProvider : ColumnProvider
	{
		private readonly int size;

		public StringProvider(string name, object defaultValue, int size, ICodeGenerationService codeGenerationService)
			: base(name, defaultValue, codeGenerationService)
		{
			this.size = size;
		}

		public int Size
		{
			get { return size; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.String(Name, Size);
		}
	}
}