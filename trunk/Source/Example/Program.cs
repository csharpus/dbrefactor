using System.Configuration;
using System.Reflection;

namespace Example
{
	class Program
	{
		static void Main()
		{
			var migrator = new DbRefactor.Migrator(
				"SqlServer",
				ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
				Assembly.GetExecutingAssembly());
			migrator.MigrateToLastVersion();
		}
	}
}
