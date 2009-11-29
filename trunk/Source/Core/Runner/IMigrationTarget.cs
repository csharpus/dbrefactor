using System;
using System.Linq;
using DbRefactor.Api;
using DbRefactor.Extensions;
using DbRefactor.Providers;

namespace DbRefactor.Runner
{
	internal abstract class MigrationTarget : IMigrationTarget
	{
		public abstract int GetVersion();
		public abstract void UpdateVersion(int version);
		public abstract BaseMigration CreateMigration(Type type);
		public abstract void BeginTransaction();
		public abstract void CommitTransaction();
		public abstract void RollbackTransaction();

		public abstract void CloseConnection();
	}

	public interface IMigrationTarget
	{
		int GetVersion();
		void UpdateVersion(int version);
		BaseMigration CreateMigration(Type type);
		void BeginTransaction();
		void CommitTransaction();
		void RollbackTransaction();
		void CloseConnection();
	}

	internal class DatabaseMigrationTarget : MigrationTarget
	{
		private readonly TransformationProvider provider;
		private readonly IDatabase database;
		private readonly IDatabaseEnvironment databaseEnvironment;
		private readonly string category;

		public DatabaseMigrationTarget(TransformationProvider provider, IDatabase database,
		                               IDatabaseEnvironment databaseEnvironment, string category)
		{
			this.provider = provider;
			this.database = database;
			this.databaseEnvironment = databaseEnvironment;
			this.category = category;
		}

		public override int GetVersion()
		{
			if (!provider.TableExists("SchemaInfo")) return 0;
			var version = database
				.Table("SchemaInfo").Where(new {Category = category}).SelectScalar<int>("Version");
			return Convert.ToInt32(version);
		}

		public override void UpdateVersion(int version)
		{
			CreateSchemaInfoTable();
			bool recordExists = database.Table("SchemaInfo")
				.Where(new {Category = DBNull.Value})
				.Select("Version")
				.AsReadable()
				.Any();
			if (recordExists)
			{
				provider.Update("SchemaInfo", new {Version = version}, new {Category = category});
			}
			else
			{
				provider.Insert("SchemaInfo", new {Version = version, Category = category});
			}
		}

		private void CreateSchemaInfoTable()
		{
			if (provider.TableExists("SchemaInfo")) return;
			database.CreateTable("SchemaInfo")
				.Int("Version").PrimaryKey()
				.String("Category", 50, String.Empty)
				.Execute();
		}

		public override BaseMigration CreateMigration(Type type)
		{
			var migration = (Migration) Activator.CreateInstance(type);
			migration.Database = database;
			return migration;
		}

		public override void BeginTransaction()
		{
			databaseEnvironment.BeginTransaction();
		}

		public override void CommitTransaction()
		{
			databaseEnvironment.CommitTransaction();
		}

		public override void RollbackTransaction()
		{
			databaseEnvironment.RollbackTransaction();
		}

		public override void CloseConnection()
		{
			databaseEnvironment.CloseConnection();
		}
	}
}