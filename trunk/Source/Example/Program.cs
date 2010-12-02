using System.Configuration;
using System.Reflection;
using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;
using System.Threading;

namespace Example
{
	class Program
	{
		static void Main()
		{
			var connectionString =
				ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString;
			var migrator = DbRefactorFactory.BuildSqlServerFactory(connectionString, Logger.NullLogger,
				null).CreateSqlServerMigrator(
				);
			migrator.MigrateToLastVersion(Assembly.GetExecutingAssembly());
		}
	}
}
