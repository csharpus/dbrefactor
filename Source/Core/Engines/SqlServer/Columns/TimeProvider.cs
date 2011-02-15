using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer.Columns
{
	public class TimeProvider : ColumnProvider
	{
		private readonly int? scale;

		public TimeProvider(string name, int? scale, object defaultValue) : base(name, defaultValue)
		{
			this.scale = scale;
		}

		public int? Scale
		{
			get { return scale; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Time(Name, scale, null);
		}
	}
}