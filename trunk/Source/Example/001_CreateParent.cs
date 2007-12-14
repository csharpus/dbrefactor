using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor;
using DbRefactor.Columns;

namespace Example
{
	[Migration(1)]
	public class CreateParent : Migration
	{
		public override void Up()
		{
			Database.AddTable(
				"Parent",
				Int("ID", ColumnProperties.PrimaryKey),
				String("Name", 50),
				Decimal("Price", 6, 2),
				Boolean("IsSold", false),
				DateTime("DateAdded", ColumnProperties.NotNull),
				Text("Description")
			);
		}

		public override void Down()
		{
			Database.DropTable("Parent");
		}
	}
}
