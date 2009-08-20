using System;
using DbRefactor.Api;
using DbRefactor.Providers;

namespace DbRefactor.Runner
{
	public abstract class MigrationTarget
	{
		public abstract int GetVersion();
		public abstract void UpdateVersion(int version);
		public abstract BaseMigration CreateMigration(Type type);
		public abstract void BeginTransaction();
		public abstract void CommitTransaction();
		public abstract void RollbackTransaction();
	}

	public class DatabaseMigrationTarget : MigrationTarget
	{
		private readonly TransformationProvider provider;
		private readonly IDatabase database;
		private readonly string category;

		public DatabaseMigrationTarget(TransformationProvider provider, IDatabase database, string category)
		{
			this.provider = provider;
			this.database = database;
			this.category = category;
		}

		public override int GetVersion()
		{
			if (!provider.TableExists("SchemaInfo")) return 0;
			int version = database
				.Table("SchemaInfo").SelectScalar<int>("Version").Where(new { Category = category }).Execute();
			return Convert.ToInt32(version);
		}

		public override void UpdateVersion(int version)
		{
			CreateSchemaInfoTable();
			int count = provider.Update("SchemaInfo", new { Version = version }, new { Category = category });
			if (count == 0)
			{
				provider.Insert("SchemaInfo", new { Version = version, Category = category });
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
			var migration = (Migration)Activator.CreateInstance(type);
			migration.Database = database;
			return migration;
		}

		public override void BeginTransaction()
		{
			provider.BeginTransaction();
		}

		public override void CommitTransaction()
		{
			provider.CommitTransaction();
		}

		public override void RollbackTransaction()
		{
			provider.RollbackTransaction();
		}
	}
}
