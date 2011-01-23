using DbRefactor;

namespace Database
{
	public class M002_CreateRole : Migration
	{
		public override void Up()
		{
			CreateTable("Roles")
				.Int("Id").PrimaryKey()
				.String("Name", 50).NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("Roles");
		}
	}
}
