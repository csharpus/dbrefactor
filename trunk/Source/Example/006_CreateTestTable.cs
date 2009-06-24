using DbRefactor;

namespace Example
{
	[Migration(6)]
	public class CreateTestTable : Migration
	{
		public override void Up()
		{
			CreateTable("Test")
				.Int("ID").PrimaryKeyWithIdentity().NotNull()
				.String("Name", 255)
				.Execute();
		}

		public override void Down()
		{
		}
	}
}
