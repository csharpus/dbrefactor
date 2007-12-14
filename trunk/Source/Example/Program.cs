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
		static void Main(string[] args)
		{
			DbRefactor.Migrator migrator 
				= new DbRefactor.Migrator(
					"SqlServer", 
					ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
					Assembly.GetExecutingAssembly()
				);
			migrator.Logger = new Logger(true, new ConsoleWriter());
			migrator.MigrateToLastVersion();
		}
	}
}
