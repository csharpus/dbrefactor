using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines;

namespace DbRefactor.Providers.Properties
{
	internal class PrimaryKeyProvider : PropertyProvider
	{
		private readonly string name;

		public PrimaryKeyProvider(string name, IColumnProperties columnProperties) : base(columnProperties)
		{
			this.name = name;
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.PrimaryKey();
		}

		public override string CreateTableSql()
		{
			return ColumnProperties.PrimaryKey(name);
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