using DbRefactor;

namespace DbRefactor.Tests
{
	/// <summary>
	/// This is first migration that create first table "User".
	/// Down method exists for roll back database structure to previos version.
	/// </summary>
	[Migration(1)]
	public class CreateUserTable: Migration
	{
		public override void Up()
		{

			CreateTable("User")
				.Int("Id").PrimaryKeyWithIdentity().NotNull()
				.String("Role", 30)
				.String("FirstName", 70).NotNull()
				.String("LastName", 70).NotNull()
				.Text("Description")
				.Execute();
		}

		public override void Down()
		{
			DropTable("User");
		}
	}

	[Migration(2)]
	public class CreateRoleTable : Migration
	{
		public override void Up()
		{
			CreateTable("Role")
				.Int("Id").PrimaryKeyWithIdentity().NotNull()
				.String("Name", 30).Unique().NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("Role");
		}
	}

	[Migration(3)]
	public class AlterUserTable : Migration
	{
		public override void Up()
		{
			AlterTable("User").DropColumn("Role");
		}

		public override void Down()
		{
			AlterTable("User").AlterColumn(new Column("Role", typeof(string)));
		}
	}

	[Migration(4)]
	public class AddColumnToUserTable : Migration
	{
		public override void Up()
		{
			AlterTable("User").AlterColumn(new Column("RoleId", typeof(int)));
			
		}

		public override void Down()
		{
			AlterTable("User").DropColumn("RoleId");
		}
	}

	[Migration(5)]
	public class AddForeignKeyToUserTable: Migration
	{
		public override void Up()
		{
			AlterTable("User").AddForeignKey("RoleId", "Role", "Id", OnDelete.SetDefault);

		}

		public override void Down()
		{
			AlterTable("User").DropForeignKey("FK_User_Id");
		}
	}

	[Migration(6)]
	public class InsertToRoleTable : Migration
	{
		public override void Up()
		{
			Table("Role")
				.Insert().AddParameter("Name", "Administrator")
				.Insert().AddParameter("Name", "Coordinator")
				.Insert().AddParameter("Name", "Manager")
				.Insert().AddParameter("Name", "User");
		}

		public override void Down()
		{
		}
	}

	[Migration(6)]
	public class InsertToUserTable : Migration
	{
		public override void Up()
		{
			Table("User")
				.Insert().AddParameter("RoleId", 1)
				.AddParameter("FirstName", "Rob")
				.AddParameter("LastName", "Tompson")

				.Insert().AddParameter("RoleId", 4)
				.AddParameter("FirstName", "User1")
				.AddParameter("LastName", "User2")
				.AddParameter("Description", "some text from user ...");
		}

		public override void Down()
		{
		}
	}
}
