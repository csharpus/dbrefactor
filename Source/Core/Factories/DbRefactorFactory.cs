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

using DbRefactor.Api;
using DbRefactor.Core;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Engines.SqlServer.Compact;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Runner;
using DbRefactor.Tools;

namespace DbRefactor.Factories
{
	public class DbRefactorFactory
	{
		private static IDatabaseEngine CreateEngine(string connectionString)
		{
			var databaseEnvironment = new SqlServerEnvironment(connectionString);
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
		}

		public static Database CreateDatabase(string connectionString)
		{
			var engine = CreateEngine(connectionString);

			var wrappedDatabaseEnvironment = new LoggerDecorator(engine.Environment, null);

			var objectNameService = new ObjectNameService();
			var schemaHelper = new SchemaHelper(engine.SchemaProvider);
			var crudProvider = new CrudProvider(wrappedDatabaseEnvironment, engine.CrudGenerator);
			var transformationProvider = new TransformationProvider(wrappedDatabaseEnvironment, engine.DatabaseProvider,
			                                                        crudProvider);
			var apiFactory = new ApiFactory(transformationProvider, objectNameService);
			return new Database(transformationProvider, objectNameService, apiFactory, schemaHelper, engine);
		}

		public static Migrator CreateMigrator(string connectionString)
		{
			var database = CreateDatabase(connectionString);
			IEngineAccessor accessor = database;
			IProviderAccessor providerAccessor = database;
			ISchemaAccessor schemaAccessor = database;
			var databaseMigrationTarget = new DatabaseMigrationTarget(providerAccessor.Provider, database, accessor.Engine.Environment, schemaAccessor.SchemaHelper, null);
			var migrationRunner = new MigrationRunner(databaseMigrationTarget, null);
			var migrationReader = new MigrationReader(databaseMigrationTarget);
			var migrationService = new MigrationService(databaseMigrationTarget, migrationRunner, migrationReader);
			return new Migrator(migrationService);
		}

		public static DataDumper CreateDataDumper(string connectionString)
		{
			var database = CreateDatabase(connectionString);
			IProviderAccessor providerAccessor = database;
			ISchemaAccessor schemaAccessor = database;
			IEngineAccessor accessor = database;
			return new DataDumper(accessor.Engine.Environment, providerAccessor.Provider, schemaAccessor.SchemaHelper, true);
		}

		public static SchemaDumper CreateSchemaDumper(string connectionString)
		{
			var database = CreateDatabase(connectionString);
			ISchemaAccessor schemaAccessor = database;
			IProviderAccessor providerAccessor = database;
			return new SchemaDumper(providerAccessor.Provider, schemaAccessor.SchemaHelper);
		}

		public static DbRefactorFactory BuildSqlServerFactory(string connectionString)
		{
			var logger = new Logger();
			logger.Attach(new ConsoleWriter());
			var providerFactory = new SqlServerFactory(connectionString, logger, null);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

		public static DbRefactorFactory BuildSqlServerFactory(string connectionString, ILogger logger, string category)
		{
			var providerFactory = new SqlServerFactory(connectionString, logger, null);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

		public static DbRefactorFactory BuildSqlServerCeFactory(string connectionString)
		{
			return BuildSqlServerCeFactory(connectionString, null, Logger.NullLogger);
		}

		public static DbRefactorFactory BuildSqlServerCeFactory(string connectionString, string category,
		                                                        ILogger logger)
		{
			//var logger = new Logger();
			//logger.Attach(new ConsoleWriter());
			var providerFactory = new SqlServerCeFactory(connectionString, logger, category);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

		//public static DbRefactorFactory BuildMySqlFactory(string connectionString)
		//{
		//    return BuildMySqlFactory(connectionString, null, Logger.NullLogger);
		//}

		//public static DbRefactorFactory BuildMySqlFactory(string connectionString, string category,
		//                                                  ILogger logger)
		//{
		//    var providerFactory = new MySqlFactory(connectionString, logger, null);
		//    providerFactory.Init();
		//    return new DbRefactorFactory(providerFactory);
		//}

		private readonly ProviderFactory providerFactory;

		private DbRefactorFactory(ProviderFactory providerFactory)
		{
			this.providerFactory = providerFactory;
		}

		public Migrator CreateSqlServerMigrator()
		{
			var migrationService = providerFactory.GetMigrationService();
			return new Migrator(migrationService);
		}

		public SchemaDumper CreateSchemaDumper()
		{
			return new SchemaDumper(providerFactory.GetProvider(), providerFactory.GetSchemaProvider());
		}

		public DataDumper CreateDataDumper()
		{
			return providerFactory.GetDataDumper();
		}

		public Database CreateDatabase()
		{
			return providerFactory.GetDatabase();
		}

		internal TransformationProvider GetProvider()
		{
			return providerFactory.GetProvider();
		}

		public SchemaHelper GetSchemaProvider()
		{
			return providerFactory.GetSchemaProvider();
		}

		public IDatabaseEnvironment GetEnvironment()
		{
			return providerFactory.GetEnvironment();
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

	public abstract class ProviderFactory
	{
		private readonly string connectionString;
		private readonly ILogger logger;
		private readonly string category;
		private IDatabaseEnvironment databaseEnvironment;
		private ObjectNameService objectNameService;
		private TransformationProvider provider;
		private ApiFactory apiFactory;
		private Database database;
		private DatabaseMigrationTarget databaseMigrationTarget;
		private MigrationService migrationService;
		private DataDumper dataDumper;
		private SchemaHelper schemaHelper;

		protected ProviderFactory(string connectionString, ILogger logger, string category)
		{
			this.connectionString = connectionString;
			this.logger = logger;
			this.category = category;
		}

		protected ProviderFactory(string connectionString)
			: this(connectionString, Infrastructure.Loggers.Logger.NullLogger, null)
		{
		}

		protected string ConnectionString
		{
			get { return connectionString; }
		}

		protected ILogger Logger
		{
			get { return logger; }
		}


		internal void Init()
		{
			databaseEnvironment = CreateEnvironment();
			objectNameService = new ObjectNameService();


			schemaHelper = CreateSchemaProvider(databaseEnvironment, objectNameService);
			var schemaProvider = new SqlServerSchemaProvider(databaseEnvironment);
			var databaseProvider = new SqlServerDatabaseProvider(databaseEnvironment, schemaProvider);
			var crudProvider = new CrudProvider(databaseEnvironment, new SqlServerCrudGenerator());
			provider = new TransformationProvider(databaseEnvironment, databaseProvider, crudProvider);
			apiFactory = new ApiFactory(provider, objectNameService);
			database = new Database(provider, objectNameService, apiFactory);
			databaseMigrationTarget = new DatabaseMigrationTarget(provider, database, databaseEnvironment, schemaHelper, category);
			var migrationRunner = new MigrationRunner(databaseMigrationTarget, logger);
			var migrationReader = new MigrationReader(databaseMigrationTarget);
			migrationService = new MigrationService(databaseMigrationTarget, migrationRunner, migrationReader);
			dataDumper = CreateDataDumper(databaseEnvironment, provider, schemaHelper);
		}

		protected abstract IDatabaseEnvironment CreateEnvironment();

		protected abstract SchemaHelper CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                                     ObjectNameService objectNameService);

		internal abstract DataDumper CreateDataDumper(IDatabaseEnvironment databaseEnvironment, TransformationProvider transformationProvider, SchemaHelper schemaHelper);

		internal TransformationProvider GetProvider()
		{
			return provider;
		}

		internal Database GetDatabase()
		{
			return database;
		}

		internal MigrationService GetMigrationService()
		{
			return migrationService;
		}

		internal DataDumper GetDataDumper()
		{
			return dataDumper;
		}

		public SchemaHelper GetSchemaProvider()
		{
			return schemaHelper;
		}

		public IDatabaseEnvironment GetEnvironment()
		{
			return databaseEnvironment;
		}
	}

	internal class SqlServerFactory : ProviderFactory
	{
		public SqlServerFactory(string connectionString) : base(connectionString)
		{
		}

		public SqlServerFactory(string connectionString, ILogger logger, string category)
			: base(connectionString, logger, category)
		{
		}

		protected override IDatabaseEnvironment CreateEnvironment()
		{
			return new SqlServerEnvironment(ConnectionString);
		}

		protected override SchemaHelper CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                                     ObjectNameService objectNameService)
		{
			var schemaProvider = new SqlServerSchemaProvider(databaseEnvironment);
			return new SchemaHelper(schemaProvider);
		}

		internal override DataDumper CreateDataDumper(IDatabaseEnvironment databaseEnvironment, TransformationProvider transformationProvider, SchemaHelper schemaHelper)
		{
			return new DataDumper(databaseEnvironment, transformationProvider, schemaHelper, false);
		}
	}

	internal class SqlServerCeFactory : ProviderFactory
	{
		public SqlServerCeFactory(string connectionString, ILogger logger, string category)
			: base(connectionString, logger, category)
		{
		}

		protected override IDatabaseEnvironment CreateEnvironment()
		{
			return new SqlServerCeEnvironment(ConnectionString, Logger);
		}

		protected override SchemaHelper CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                                     ObjectNameService objectNameService)
		{
			var schemaProvider = new SqlServerCeSchemaProvider(databaseEnvironment);
			return new SchemaHelper(schemaProvider);
		}

		internal override DataDumper CreateDataDumper(IDatabaseEnvironment databaseEnvironment, TransformationProvider transformationProvider, SchemaHelper schemaHelper)
		{
			return new DataDumper(databaseEnvironment, transformationProvider, schemaHelper, true);
		}
	}

	//internal class MySqlFactory : ProviderFactory
	//{
	//    public MySqlFactory(string connectionString, ILogger logger, string category)
	//        : base(connectionString, logger, category)
	//    {
	//    }

	//    internal override ObjectNameService CreateObjectNameService()
	//    {
	//        return new MySqlObjectNameService();
	//    }

	//    internal override IColumnProperties CreateColumnProperties()
	//    {
	//        return new SqlServerColumnProperties();
	//    }

	//    internal override IDatabaseEnvironment CreateEnvironment()
	//    {
	//        return new MySqlEnvironment(ConnectionString, Logger);
	//    }

	//    internal override ISqlTypes CreateSqlTypes()
	//    {
	//        return new SqlServerCeTypes();
	//    }

	//    internal override SchemaProvider CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
	//                                                          ObjectNameService objectNameService,
	//                                                          SqlServerColumnMapper sqlServerColumnMapper)
	//    {
	//        return new MySqlSchemaProvider(databaseEnvironment, objectNameService, sqlServerColumnMapper);
	//    }

	//    internal override DataDumper CreateDataDumper(TransformationProvider transformationProvider)
	//    {
	//        return new DataDumper(transformationProvider, true);
	//    }
	//}
}