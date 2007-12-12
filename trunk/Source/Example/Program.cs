using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Reflection;
using Migrator.Loggers;

namespace Example
{
	class Program
	{
		static void Main(string[] args)
		{
			Migrator.Migrator migrator 
				= new Migrator.Migrator(
					"SqlServer", 
					ConfigurationManager.ConnectionStrings["SqlServerConnectionString"].ConnectionString,
					Assembly.GetExecutingAssembly()
				);
			migrator.Logger = new Logger(true, new ConsoleWriter());
			migrator.MigrateToLastVersion();
		}
	}
}
