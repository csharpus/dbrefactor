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
using DbRefactor.Engines;
using DbRefactor.Engines.MySql;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Engines.SqlServer.Compact;
using DbRefactor.Infrastructure;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Runner;
using DbRefactor.Tools;

namespace DbRefactor.Factories
{
	public class DbRefactorFactory
	{
		public static DbRefactorFactory BuildSqlServerFactory(string connectionString)
		{
			var logger = new Logger();
			logger.Attach(new ConsoleWriter());
			var providerFactory = new SqlServerFactory(connectionString, logger, null);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

		public static DbRefactorFactory BuildSqlServerFactory(string connectionString, string category,
		                                                      bool trace)
		{
			var logger = new Logger();
			logger.Attach(new ConsoleWriter());
			var providerFactory = new SqlServerFactory(connectionString, logger, null);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

		public static DbRefactorFactory BuildSqlServerCeFactory(string connectionString, string category,
		                                                        bool trace)
		{
			var logger = new Logger();
			logger.Attach(new ConsoleWriter());
			var providerFactory = new SqlServerCeFactory(connectionString, logger, null);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

		public static DbRefactorFactory BuildMySqlFactory(string connectionString, string category,
																bool trace)
		{
			var logger = new Logger();
			logger.Attach(new ConsoleWriter());
			var providerFactory = new MySqlFactory(connectionString, logger, null);
			providerFactory.Init();
			return new DbRefactorFactory(providerFactory);
		}

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
			return new SchemaDumper(providerFactory.GetProvider());
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
	}

	internal class FactoryInfo
	{
		public TransformationProvider Provider { get; set; }
		public IDatabase Database { get; set; }
	}

	public abstract class ProviderFactory
	{
		private readonly string connectionString;
		private readonly ILogger logger;
		private readonly string category;
		private IDatabaseEnvironment databaseEnvironment;
		private ISqlTypes sqlTypes;
		private CodeGenerationService codeGenerationService;
		private SqlGenerationService sqlGenerationService;
		private ColumnPropertyProviderFactory columnPropertyProviderFactory;
		private IColumnProperties columnProperties;
		private SqlServerColumnMapper sqlServerColumnMapper;
		private ColumnProviderFactory columnProviderFactory;
		private ObjectNameService objectNameService;
		private TransformationProvider provider;
		private ApiFactory apiFactory;
		private Database database;
		private DatabaseMigrationTarget databaseMigrationTarget;
		private MigrationService migrationService;
		private DataDumper dataDumper;

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
			sqlTypes = CreateSqlTypes();
			databaseEnvironment = CreateEnvironment();
			codeGenerationService = new CodeGenerationService();
			objectNameService = CreateObjectNameService();
			sqlGenerationService = new SqlGenerationService(objectNameService);

			columnProperties = CreateColumnProperties();
			columnPropertyProviderFactory = new ColumnPropertyProviderFactory(columnProperties);
			sqlServerColumnMapper = new SqlServerColumnMapper(codeGenerationService, sqlTypes,
			                                                  sqlGenerationService,
			                                                  columnPropertyProviderFactory);
			columnProviderFactory = new ColumnProviderFactory(codeGenerationService, sqlTypes,
			                                                  sqlGenerationService,
			                                                  columnPropertyProviderFactory);
			var schemaProvider = CreateSchemaProvider(databaseEnvironment, objectNameService,
			                                          sqlServerColumnMapper);
			provider = new TransformationProvider(databaseEnvironment, schemaProvider, objectNameService);
			apiFactory = new ApiFactory(provider, columnProviderFactory, objectNameService);
			database = new Database(provider, columnProviderFactory, objectNameService, apiFactory);
			databaseMigrationTarget = new DatabaseMigrationTarget(provider, database, databaseEnvironment, category);
			var migrationRunner = new MigrationRunner(databaseMigrationTarget, logger);
			var migrationReader = new MigrationReader(databaseMigrationTarget);
			migrationService = new MigrationService(databaseMigrationTarget, migrationRunner, migrationReader);
			dataDumper = CreateDataDumper(provider);
		}

		internal virtual ObjectNameService CreateObjectNameService()
		{
			return new ObjectNameService();
		}

		internal abstract IColumnProperties CreateColumnProperties();

		internal abstract IDatabaseEnvironment CreateEnvironment();

		internal abstract ISqlTypes CreateSqlTypes();

		internal abstract SchemaProvider CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                                      ObjectNameService objectNameService,
		                                                      SqlServerColumnMapper sqlServerColumnMapper);

		internal abstract DataDumper CreateDataDumper(TransformationProvider transformationProvider);

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

		internal override IColumnProperties CreateColumnProperties()
		{
			return new SqlServerColumnProperties();
		}

		internal override IDatabaseEnvironment CreateEnvironment()
		{
			return new SqlServerEnvironment(ConnectionString, Logger);
		}

		internal override ISqlTypes CreateSqlTypes()
		{
			return new SqlServerTypes();
		}

		internal override SchemaProvider CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                                      ObjectNameService objectNameService,
		                                                      SqlServerColumnMapper sqlServerColumnMapper)
		{
			return new SqlServerSchemaProvider(databaseEnvironment, objectNameService, sqlServerColumnMapper);
		}

		internal override DataDumper CreateDataDumper(TransformationProvider transformationProvider)
		{
			return new DataDumper(transformationProvider, false);
		}
	}

	internal class SqlServerCeFactory : ProviderFactory
	{
		public SqlServerCeFactory(string connectionString, ILogger logger, string category)
			: base(connectionString, logger, category)
		{
		}

		internal override IColumnProperties CreateColumnProperties()
		{
			return new SqlServerColumnProperties();
		}

		internal override IDatabaseEnvironment CreateEnvironment()
		{
			return new SqlServerCeEnvironment(ConnectionString, Logger);
		}

		internal override ISqlTypes CreateSqlTypes()
		{
			return new SqlServerCeTypes();
		}

		internal override SchemaProvider CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                                      ObjectNameService objectNameService,
		                                                      SqlServerColumnMapper sqlServerColumnMapper)
		{
			return new SqlServerCeSchemaProvider(databaseEnvironment, objectNameService, sqlServerColumnMapper);
		}

		internal override DataDumper CreateDataDumper(TransformationProvider transformationProvider)
		{
			return new DataDumper(transformationProvider, true);
		}
	}

	internal class MySqlFactory : ProviderFactory
	{
		public MySqlFactory(string connectionString, ILogger logger, string category)
			: base(connectionString, logger, category)
		{
		}

		internal override ObjectNameService CreateObjectNameService()
		{
			return new MySqlObjectNameService();
		}

		internal override IColumnProperties CreateColumnProperties()
		{
			return new SqlServerColumnProperties();
		}

		internal override IDatabaseEnvironment CreateEnvironment()
		{
			return new MySqlEnvironment(ConnectionString, Logger);
		}

		internal override ISqlTypes CreateSqlTypes()
		{
			return new SqlServerCeTypes();
		}

		internal override SchemaProvider CreateSchemaProvider(IDatabaseEnvironment databaseEnvironment,
															  ObjectNameService objectNameService,
															  SqlServerColumnMapper sqlServerColumnMapper)
		{
			return new MySqlSchemaProvider(databaseEnvironment, objectNameService, sqlServerColumnMapper);
		}

		internal override DataDumper CreateDataDumper(TransformationProvider transformationProvider)
		{
			return new DataDumper(transformationProvider, true);
		}
	}
}