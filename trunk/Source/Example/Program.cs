using System.Configuration;
using System.Reflection;
using DbRefactor.Factories;

namespace Example
{
	class Program
	{
		static void Main()
		{
			var migrator = DbRefactorFactory.BuildSqlServerFactory(ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
				null,
				false).CreateSqlServerMigrator(
				);
			migrator.MigrateToLastVersion(Assembly.GetExecutingAssembly());
		}
	}
}
