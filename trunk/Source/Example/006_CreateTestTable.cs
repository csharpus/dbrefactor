using DbRefactor;

namespace Example
{
	[Migration(6)]
	public class CreateTestTable : Migration
	{
		public override void Up()
		{
			CreateTable("Test").Int("ID", ColumnProperties.PrimaryKeyWithIdentity).Execute();
		}

		public override void Down()
		{
			
		}
	}
}
