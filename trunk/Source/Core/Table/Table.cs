using DbRefactor.Providers;

namespace DbRefactor
{
	public abstract class Table
	{
		private readonly TransformationProvider provider;
		protected IDatabaseEnvironment databaseEnvironment;
		public string TableName { get; set; }

		protected TransformationProvider Provider
		{
			get { return provider; }
		}

		protected Table(TransformationProvider provider, string tableName)
		{
			this.provider = provider;
			TableName = tableName;
		}

	}
}
