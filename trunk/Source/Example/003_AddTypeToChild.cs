using DbRefactor;
using DbRefactor.Columns;

namespace Example
{
	[Migration(3)]
	public class AddTypeToChild : Migration
	{
		public override void Up()
		{
			AddInt("Child", "Type", 0);
		}

		public override void Down()
		{
			DropColumn("Child", "Type");
		}
	}
}
