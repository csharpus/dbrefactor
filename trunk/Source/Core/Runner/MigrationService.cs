using System;
using System.Collections.Generic;

namespace DbRefactor.Runner
{
	public class MigrationService
	{
		public MigrationService()
		{
		}

		public void Migrate(IEnumerable<BaseMigration> migrations)
		{
			foreach (var migration in migrations)
			{
				migration.Up();
			}
		}
	}
}
