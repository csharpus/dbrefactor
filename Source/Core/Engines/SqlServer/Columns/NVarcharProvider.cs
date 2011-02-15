using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer.Columns
{
	public class NVarcharProvider : ColumnProvider
	{
		private readonly int size;

		public NVarcharProvider(string name, object defaultValue, int size)
			: base(name, defaultValue)
		{
			this.size = size;
		}

		public int Size
		{
			get { return size; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.NVarchar(Name, Size, null);
		}
	}
}
