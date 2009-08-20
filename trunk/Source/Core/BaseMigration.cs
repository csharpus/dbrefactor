namespace DbRefactor
{
	public abstract class BaseMigration
	{
		public abstract void Up();
		public abstract void Down();
	}
}