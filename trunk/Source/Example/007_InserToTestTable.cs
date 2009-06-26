using DbRefactor;

namespace Example
{
	[Migration(7)]
	public class InsertToTestTable : Migration
	{
		public override void Up()
		{
			Table("Test").Insert(new {Name = "Test1"});
		}

		public override void Down()
		{
		}
	}
}
