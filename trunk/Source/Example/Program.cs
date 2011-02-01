using System.Configuration;
using System.Reflection;
using DbRefactor.Factories;
using DbRefactor.Infrastructure.Loggers;

namespace Example
{
	class Program
	{
		static void Main()
		{
			var connectionString =
				ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString;
			var migrator = NewDbRefactorFactory.SqlServer().CreateMigrator(connectionString, Logger.NullLogger);
			migrator.MigrateToLastVersion(Assembly.GetExecutingAssembly());
		}
	}
}
