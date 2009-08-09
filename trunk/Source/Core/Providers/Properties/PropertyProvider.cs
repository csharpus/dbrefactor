using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Providers.TypeToSqlProviders;

namespace DbRefactor.Providers.Properties
{
	public abstract class PropertyProvider
	{
		private readonly IColumnProperties columnProperties;

		protected PropertyProvider(IColumnProperties columnProperties)
		{
			this.columnProperties = columnProperties;
		}

		protected IColumnProperties ColumnProperties
		{
			get { return columnProperties; }
		}

		public abstract Expression<Action<NewTable>> Method();

		public string MethodName()
		{
			return ((MethodCallExpression)Method().Body).Method.Name;
		}

		public abstract string CreateTableSql();
		public abstract string AlterTableSql();
		public abstract string AddTableSql();
	}
}