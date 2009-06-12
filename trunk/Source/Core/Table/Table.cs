using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;

namespace DbRefactor
{
	public abstract class Table
	{
		protected IDatabaseEnvironment databaseEnvironment;
		public string TableName { get; set; }

		protected Table(IDatabaseEnvironment environment)
		{
			databaseEnvironment = environment;
		}

	}
}
