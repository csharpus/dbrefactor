using System.Configuration;
using System.Reflection;
using DbRefactor.Factories;

namespace Example
{
	class Program
	{
		static void Main()
		{
			var migrator = new ProviderFactory().CreateMigrator(
				"SqlServer",
				ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
				null,
				Assembly.GetExecutingAssembly(),
				false);
			migrator.MigrateToLastVersion();
		}
	}
}
