using DbRefactor;

namespace Database
{
	public class M003_AddAgeToUsers : Migration
	{
		public override void Up()
		{
			Table("Users").AddColumn().Int("Age").Execute();
		}

		public override void Down()
		{
			Table("Users").DropColumn("Age");
		}
	}
}
