using System;

namespace DbRefactor
{
	public sealed class SetUpMigrationAttribute : Attribute
	{
	}

	public sealed class MigrationSetUp : Attribute
	{
	}

	public sealed class MigrationTearDown : Attribute
	{
	}

	public sealed class MigratorSetUp : Attribute
	{
	}

	public sealed class MigratorTearDown : Attribute
	{
	}
}