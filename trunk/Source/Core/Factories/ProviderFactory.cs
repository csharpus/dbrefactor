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

using System.Reflection;
using DbRefactor.Api;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Infrastructure;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using DbRefactor.Runner;
using DbRefactor.Tools;

namespace DbRefactor.Factories
{
	public class ProviderFactory
	{
		internal TransformationProvider Create(string connectionString)
		{
			return Create(connectionString, Logger.NullLogger);
		}

		internal FactoryInfo CreateAll(string connectionString, ILogger logger)
		{
			var sqlServerEnvironment = new SqlServerEnvironment(connectionString, logger);
			var codeGenerationService = new CodeGenerationService();
			var sqlGenerationService = new SqlGenerationService();
			var sqlServerTypes = new SqlServerTypes();
			var columnPropertyProviderFactory = new ColumnPropertyProviderFactory(new SqlServerColumnProperties());
			var sqlServerColumnMapper = new SqlServerColumnMapper(codeGenerationService, sqlServerTypes, sqlGenerationService, columnPropertyProviderFactory);
			var columnProviderFactory = new ColumnProviderFactory(codeGenerationService, sqlServerTypes, sqlGenerationService, columnPropertyProviderFactory);
			var constraintNameService = new ConstraintNameService();
			var provider = new TransformationProvider(sqlServerEnvironment, sqlServerColumnMapper, constraintNameService);
			var apiFactory = new ApiFactory(provider, columnProviderFactory, constraintNameService);
			var database = new Database(provider, columnProviderFactory, constraintNameService, apiFactory);
			return new FactoryInfo {Provider = provider, Database = database};
		}

		internal TransformationProvider Create(string connectionString, ILogger logger)
		{
			return CreateAll(connectionString, logger).Provider;
		}

		internal class FactoryInfo
		{
			public TransformationProvider Provider { get; set; }
			public IDatabase Database { get; set; }
		}

		private MigrationTarget CreateTarget(string connectionString, ILogger logger, string category)
		{
			var info = CreateAll(connectionString, logger);
			return new DatabaseMigrationTarget(info.Provider, info.Database, category);
		}

		private MigrationService CreateMigrationService(string connectionString, string category, ILogger logger)
		{
			var target = CreateTarget(connectionString, logger, category);
			return new MigrationService(target, new MigrationRunner(target, logger), new MigrationReader(target));
		}

		public Migrator CreateMigrator(string provider, string connectionString, string category, bool trace)
		{
			var logger = new Logger();
			logger.Attach(new ConsoleWriter());
			return new Migrator(CreateMigrationService(connectionString, category, logger));
		}

		public SchemaDumper CreateSchemaDumper(string connectionString, ConsoleLogger logger)
		{
			return new SchemaDumper(Create(connectionString, logger));
		}
	}
}