using DbRefactor;

namespace Database
{
	public class M001_CreateUser : Migration
	{
		public override void Up()
		{
			CreateTable("Users")
				.Int("Id").PrimaryKey()
				.String("Name", 50).NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("Users");
		}
	}
}
