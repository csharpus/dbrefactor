namespace DbRefactor.Runner
{
	public interface IVersionedMigration
	{
		void Up();
		void Down();
		int Version { get; }
		string HumanName { get; }
	}
}