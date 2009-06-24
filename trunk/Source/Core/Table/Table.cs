using DbRefactor.Providers;

namespace DbRefactor
{
	public abstract class Table
	{
		protected IDatabaseEnvironment databaseEnvironment;
		public string TableName { get; set; }

		protected Table(IDatabaseEnvironment environment, string tableName)
		{
			databaseEnvironment = environment;
			TableName = tableName;
		}

	}
}
