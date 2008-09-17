using System;
using DbRefactor;

namespace Example
{
	[Migration(4)]
	public class AddStringFieldToChild : Migration
	{
		public override void Up()
		{
			AddTo("Child", Column.String("StringField", 300, ColumnProperties.NotNull, String.Empty));
		}

		public override void Down()
		{
			DropColumn("Child", "StringField");
		}
	}
}
