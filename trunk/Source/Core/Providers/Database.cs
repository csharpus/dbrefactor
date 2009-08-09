using DbRefactor.Api;

namespace DbRefactor.Providers
{
	public interface IDatabase
	{
		NewTable CreateTable(string name);
		ActionTable Table(string name);
	}

	public class Database : IDatabase
	{
		private readonly TransformationProvider transformationProvider;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;

		public Database(TransformationProvider transformationProvider, ColumnProviderFactory columnProviderFactory,
		                ColumnPropertyProviderFactory columnPropertyProviderFactory)
		{
			this.transformationProvider = transformationProvider;
			this.columnProviderFactory = columnProviderFactory;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
		}

		public NewTable CreateTable(string name)
		{
			return new NewTable(transformationProvider, columnProviderFactory, columnPropertyProviderFactory, name);
		}

		public ActionTable Table(string name)
		{
			return new ActionTable(transformationProvider, name, columnProviderFactory, columnPropertyProviderFactory);
		}
	}
}