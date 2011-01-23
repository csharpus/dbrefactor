namespace DbRefactor.Runner
{
	internal class VersionedMigration : IVersionedMigration
	{
		private readonly BaseMigration migration;
		private readonly int version;
		private readonly string humanName;

		public VersionedMigration(BaseMigration migration, int version, string humanName)
		{
			this.migration = migration;
			this.version = version;
			this.humanName = humanName;
		}

		public void Up()
		{
			migration.Up();
		}

		public void Down()
		{
			migration.Down();
		}

		public int Version
		{
			get
			{
				return version;
			}
		}

		public string HumanName
		{
			get
			{
				return humanName;
			}
		}
	}
}
