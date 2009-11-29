using DbRefactor.Factories;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public interface IDatabase
	{
		NewTable CreateTable(string name);
		ActionTable Table(string name);
		void DropTable(string name);
		ExecuteEngine Execute();
	}

	public class Database : IDatabase
	{
		private readonly TransformationProvider transformationProvider;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly ObjectNameService objectNameService;
		private readonly ApiFactory apiFactory;

		internal Database(TransformationProvider transformationProvider, ColumnProviderFactory columnProviderFactory,
		                ObjectNameService objectNameService, ApiFactory apiFactory)
		{
			this.transformationProvider = transformationProvider;
			this.columnProviderFactory = columnProviderFactory;
			this.objectNameService = objectNameService;
			this.apiFactory = apiFactory;
		}

		public NewTable CreateTable(string name)
		{
			return new NewTable(transformationProvider, columnProviderFactory, name, objectNameService);
		}

		public ActionTable Table(string name)
		{
			return apiFactory.CreateActionTable(name);
		}

		public void DropTable(string name)
		{
			transformationProvider.DropTable(name);
		}

		public ExecuteEngine Execute()
		{
			return new ExecuteEngine(transformationProvider);
		}
	}
}