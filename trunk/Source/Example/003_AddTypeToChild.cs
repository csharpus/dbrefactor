using DbRefactor;

namespace Example
{
	[Migration(3)]
	public class AddTypeToChild : Migration
	{
		public override void Up()
		{
			AddTo("Child", Column.Int("Type", 0));
		}

		public override void Down()
		{
			DropColumn("Child", "Type");
		}
	}
}
