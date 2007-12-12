using System;

namespace Migrator
{
	public class SetUpMigrationAttribute : Attribute
	{
	}

	public class MigrationSetUp : Attribute
	{
	}

	public class MigrationTearDown : Attribute
	{
	}

	public class MigratorSetUp : Attribute
	{
	}

	public class MigratorTearDown : Attribute
	{
	}
}
