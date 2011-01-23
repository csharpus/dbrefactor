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

	public interface IEngineAccessor
	{
		IDatabaseEngine Engine { get; }
	}

	public interface IProviderAccessor
	{
		TransformationProvider Provider { get; }
	}

	public class Database : IDatabase, IEngineAccessor, IProviderAccessor, ISchemaAccessor
	{
		private readonly TransformationProvider transformationProvider;
		private readonly ObjectNameService objectNameService;
		private readonly ApiFactory apiFactory;
		private readonly SchemaHelper schemaHelper;
		private IDatabaseEngine databaseEngine;

		internal Database(TransformationProvider transformationProvider, ObjectNameService objectNameService, ApiFactory apiFactory)
		{
			this.transformationProvider = transformationProvider;
			this.objectNameService = objectNameService;
			this.apiFactory = apiFactory;
		}

		internal Database(TransformationProvider transformationProvider, ObjectNameService objectNameService, ApiFactory apiFactory, SchemaHelper schemaHelper, IDatabaseEngine databaseEngine)
		{
			this.transformationProvider = transformationProvider;
			this.objectNameService = objectNameService;
			this.apiFactory = apiFactory;
			this.schemaHelper = schemaHelper;
			this.databaseEngine = databaseEngine;
		}

		public NewTable CreateTable(string name)
		{
			return new NewTable(transformationProvider, name, objectNameService);
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

		IDatabaseEngine IEngineAccessor.Engine
		{
			get { return databaseEngine; }
		}

		TransformationProvider IProviderAccessor.Provider
		{
			get { return transformationProvider; }
		}

		SchemaHelper ISchemaAccessor.SchemaHelper
		{
			get { return schemaHelper; }
		}
	}
}