using DbRefactor.Api;
using DbRefactor.Providers;

namespace DbRefactor.Factories
{
	internal class ApiFactory
	{
		private readonly TransformationProvider transformationProvider;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly ConstraintNameService constraintNameService;

		public ApiFactory(TransformationProvider transformationProvider, ColumnProviderFactory columnProviderFactory,
		                  ConstraintNameService constraintNameService)
		{
			this.transformationProvider = transformationProvider;
			this.columnProviderFactory = columnProviderFactory;
			this.constraintNameService = constraintNameService;
		}

		public OtherTypeColumn CreateOtherTypeColumn(string table, string column)
		{
			return new OtherTypeColumn(table, column, columnProviderFactory, transformationProvider);
		}

		public ActionColumn CreateActionColumn(string table, string column)
		{
			return new ActionColumn(transformationProvider, table, column, this, constraintNameService);
		}

		public ActionTable CreateActionTable(string table)
		{
			return new ActionTable(transformationProvider, table, columnProviderFactory, constraintNameService, this);
		}
	}
}