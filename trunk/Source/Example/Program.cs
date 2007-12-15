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
			Migrator migrator = new Migrator(
				ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
				Assembly.GetExecutingAssembly());
			migrator.Logger = new Logger(true, new ConsoleWriter());
			migrator.MigrateToLastVersion();
		}
	}
}
