using DbRefactor.Core;

namespace AcceptanceDatabase
{
	[Migration(1)]
	public class CreateRoleTable : Migration
	{
		public override void Up()
		{
			CreateTable("Role")
				.Int("Id").PrimaryKey().Identity()
				.String("Name", 30).Unique().NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("Role");
		}
	}

	[Migration(2)]
	public class CreateUserTable : Migration
	{
		public override void Up()
		{
			CreateTable("User")
				.Int("Id").PrimaryKey().Identity()
				.String("Name", 30).Unique().NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("User");
		}
	}

	[Migration(3)]
	public class CreateGroupTable : Migration
	{
		public override void Up()
		{
			CreateTable("Group")
				.Int("Id").PrimaryKey().Identity()
				.String("Name", 30).Unique().NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("Group");
		}
	}
}
