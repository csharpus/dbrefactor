using System;
using System.Linq.Expressions;

namespace DbRefactor.Providers
{
	internal class StringProvider : ColumnProvider
	{
		public int Size { get; private set; }

		public StringProvider(string name, int size) : base(name)
		{
			Size = size;
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.String(Name, Size);
		}
	}
}