C# library for versioning and refactoring database structure using Microsoft SQL Server

Project home page: http://dbrefactor.csharpus.com

Quick example:
```
// This migration creates "Role" table.
// Down method exists to roll back a database structure to the previous version.
[Migration(1)]
public class CreateRoleTable: Migration
{
	public override void Up()
	{
		CreateTable("Role")
			.Int("Id").PrimaryKeyWithIdentity()
			.String("Name", 30).Unique().NotNull()
			.Execute();
	}

	public override void Down()
	{
		DropTable("Role");
	}
}
```
For more details use manual http://dbrefactor.csharpus.com/manual