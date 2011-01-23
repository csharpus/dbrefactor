using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines.SqlServer;

namespace DbRefactor.Providers.Properties
{
	public class IndexProvider : PropertyProvider
	{
		private readonly string name;

		public IndexProvider(string name, IColumnProperties properties) : base(properties)
		{
			this.name = name;
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Index();
		}

		public override string CreateTableSql()
		{
			return String.Empty;
		}

		public override string AlterTableSql()
		{
			return String.Empty;
		}

		public override string AddTableSql()
		{
			return String.Empty;
		}
	}
}