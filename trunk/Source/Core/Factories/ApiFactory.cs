using DbRefactor.Api;
using DbRefactor.Providers;

namespace DbRefactor.Factories
{
	internal class ApiFactory
	{
		private readonly TransformationProvider transformationProvider;
		private readonly ObjectNameService objectNameService;

		public ApiFactory(TransformationProvider transformationProvider, 
		                  ObjectNameService objectNameService)
		{
			this.transformationProvider = transformationProvider;
			this.objectNameService = objectNameService;
		}

		public OtherTypeColumn CreateOtherTypeColumn(string table, string column)
		{
			return new OtherTypeColumn(table, column, transformationProvider);
		}

		public ActionColumn CreateActionColumn(string table, string column)
		{
			return new ActionColumn(transformationProvider, table, column, this, objectNameService);
		}

		public ActionTable CreateActionTable(string table)
		{
			return new ActionTable(transformationProvider, table, objectNameService, this);
		}
	}
}