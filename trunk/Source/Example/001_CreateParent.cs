using DbRefactor;
using DbRefactor.Columns;

namespace Example
{
	[Migration(1)]
	public class CreateParent : Migration
	{
		public override void Up()
		{
			CreateTable(
				"Parent",
				Int("ID", ColumnProperties.PrimaryKeyWithIdentity),
				String("Name", 50),
				Decimal("Price", 6, 2),
				Boolean("IsSold", false),
				DateTime("DateAdded", ColumnProperties.NotNull),
				Text("Description"));
		}

		public override void Down()
		{
			DropTable("Parent");
		}
	}
}
