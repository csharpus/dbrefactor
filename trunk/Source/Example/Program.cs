using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Reflection;
using DbRefactor.Tools.Loggers;
using DbRefactor;

namespace Example
{
	class Program
	{
		static void Main()
		{
			DbRefactor.Migrator migrator = new DbRefactor.Migrator(
				"SqlServer",
				ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
				Assembly.GetExecutingAssembly());
			migrator.MigrateToLastVersion();
		}
	}
}
