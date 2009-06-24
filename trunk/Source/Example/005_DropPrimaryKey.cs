using DbRefactor;

namespace Example
{
	[Migration(5)]
	public class DropPrimaryKey : Migration
	{
		public override void Up()
		{
			DropColumn("Child", "ID");
		}

		public override void Down()
		{
		}
	}
}
