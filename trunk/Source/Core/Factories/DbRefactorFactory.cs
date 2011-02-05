#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using DbRefactor.Api;
using DbRefactor.Core;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Runner;
using DbRefactor.Tools;

namespace DbRefactor.Factories
{
	public class DbRefactorFactory
	{
		private Func<string, DatabaseEngine> createEngine;

		private DbRefactorFactory(Func<string, DatabaseEngine> createEngine)
		{
			this.createEngine = createEngine;
		}

		public static DbRefactorFactory SqlServer()
		{
			Func<string, DatabaseEngine> createEngine = cs =>
				{
					var databaseEnvironment = new SqlServerEnvironment(cs);
					var schemaProvider = new SqlServerSchemaProvider(databaseEnvironment);
					var sqlServerCrudGenerator = new SqlServerCrudGenerator();
					var databaseProvider = new SqlServerDatabaseProvider(databaseEnvironment, schemaProvider);

					return new DatabaseEngine
					             	{
					             		Environment = databaseEnvironment,
					             		SchemaProvider = schemaProvider,
					             		CrudGenerator = sqlServerCrudGenerator,
					             		DatabaseProvider = databaseProvider
					             	};
				};

			return new DbRefactorFactory(createEngine);
		}

		public Database CreateDatabase(string connectionString)
		{
			var engine = createEngine(connectionString);

			var logger = new NullLogger();

			var wrappedDatabaseEnvironment = new LoggerDecorator(engine.Environment, logger);

			var objectNameService = new ObjectNameService();
			var schemaHelper = new SchemaHelper(engine.SchemaProvider);
			var crudProvider = new CrudProvider(wrappedDatabaseEnvironment, engine.CrudGenerator);
			var transformationProvider = new TransformationProvider(wrappedDatabaseEnvironment, engine.DatabaseProvider,
																	crudProvider);
			var apiFactory = new ApiFactory(transformationProvider, objectNameService);
			return new Database(transformationProvider, objectNameService, apiFactory, schemaHelper, engine);
		}

		public Migrator CreateMigrator(string connectionString, ILogger logger = null, string category = null)
		{
			if (logger == null)
			{
				logger = new NullLogger();
			}


			var database = CreateDatabase(connectionString);
			IEngineAccessor accessor = database;
			IProviderAccessor providerAccessor = database;
			ISchemaAccessor schemaAccessor = database;
			var databaseMigrationTarget = new DatabaseMigrationTarget(providerAccessor.Provider, database, accessor.Engine.Environment, schemaAccessor.SchemaHelper, category);


			var migrationRunner = new MigrationRunner(databaseMigrationTarget, logger);
			var migrationReader = new MigrationReader(databaseMigrationTarget);
			var migrationService = new MigrationService(databaseMigrationTarget, migrationRunner, migrationReader);
			return new Migrator(migrationService);
		}

		public DataDumper CreateDataDumper(string connectionString)
		{
			var database = CreateDatabase(connectionString);
			IProviderAccessor providerAccessor = database;
			ISchemaAccessor schemaAccessor = database;
			IEngineAccessor accessor = database;
			return new DataDumper(accessor.Engine.Environment, providerAccessor.Provider, schemaAccessor.SchemaHelper, true);
		}

		public SchemaDumper CreateSchemaDumper(string connectionString)
		{
			var database = CreateDatabase(connectionString);
			ISchemaAccessor schemaAccessor = database;
			IProviderAccessor providerAccessor = database;
			IEngineAccessor accessor = database;
			return new SchemaDumper(accessor.Engine.Environment, providerAccessor.Provider, schemaAccessor.SchemaHelper);
		}
	}

	public interface ISchemaAccessor
	{
		SchemaHelper SchemaHelper { get; }
	}

	public class DatabaseEngine : IDatabaseEngine
	{
		public IDatabaseEnvironment Environment { get; set; }
		public ISchemaProvider SchemaProvider { get; set; }
		public ICrudGenerator CrudGenerator { get; set; }
		public IDatabaseProvider DatabaseProvider { get; set; }
	}

	public interface IDatabaseEngine
	{
		IDatabaseEnvironment Environment { get; set; }
		ISchemaProvider SchemaProvider { get; set; }
		ICrudGenerator CrudGenerator { get; set; }
		IDatabaseProvider DatabaseProvider { get; set; }
	}
}