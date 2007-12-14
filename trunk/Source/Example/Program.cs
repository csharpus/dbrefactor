using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Reflection;
using DbRefactor.Loggers;
using Migrator.Loggers;

namespace Example
{
	class Program
	{
		static void Main()
		{
			DbRefactor.Migrator migrator 
				= new DbRefactor.Migrator(
					ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
					Assembly.GetExecutingAssembly());
			migrator.Logger = new Logger(true, new ConsoleWriter());
			migrator.MigrateToLastVersion();
		}
	}
}
