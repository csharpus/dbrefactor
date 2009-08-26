using DbRefactor.Engines;
using DbRefactor.Providers.Properties;

namespace DbRefactor.Factories
{
	internal class ColumnPropertyProviderFactory
	{
		private readonly IColumnProperties columnProperties;

		public ColumnPropertyProviderFactory(IColumnProperties columnProperties)
		{
			this.columnProperties = columnProperties;
		}

		public NotNullProvider CreateNotNull()
		{
			return new NotNullProvider(columnProperties);
		}

		public PrimaryKeyProvider CreatePrimaryKey(string name)
		{
			return new PrimaryKeyProvider(name, columnProperties);
		}

		public UniqueProvider CreateUnique(string name)
		{
			return new UniqueProvider(name, columnProperties);
		}

		public IdentityProvider CreateIdentity()
		{
			return new IdentityProvider(columnProperties);
		}

		public EmptyProvider CreateEmpty()
		{
			return new EmptyProvider(columnProperties);
		}
	}
}