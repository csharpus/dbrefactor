using DbRefactor;

namespace Example
{
	[Migration(4)]
	public class AddStringFieldToChild : Migration
	{
		public override void Up()
		{
			AddString("Child", "StringField", 300, ColumnProperties.NotNull, string.Empty);
		}

		public override void Down()
		{
			DropColumn("Child", "StringField");
		}
	}
}
