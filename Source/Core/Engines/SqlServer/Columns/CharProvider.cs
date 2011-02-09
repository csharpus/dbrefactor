using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer.Columns
{
	public class CharProvider : ColumnProvider
	{
		private readonly int size;
		private readonly string collation;

		public CharProvider(string name, object defaultValue, int size, string collation) : base(name, defaultValue)
		{
			this.size = size;
			this.collation = collation;
		}

		public int Size
		{
			get { return size; }
		}

		public string Collation
		{
			get { return collation; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Char(Name, Size, null, null);
		}
	}
}
