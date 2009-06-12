using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;

namespace DbRefactor
{
	public class Table
	{
		private IDatabaseEnvironment databaseEnvironment;
		private ColumnsCollection columns;
		public string TableName { get; set; }

		public Table(IDatabaseEnvironment environment)
		{
			databaseEnvironment = environment;
			columns = ColumnsCollection.Create();
		}

		public Table Int(string columnName)
		{
			columns.Int(columnName);
			return this;
		}

		public Table Int(string columnName, ColumnProperties properties)
		{
			columns.Int(columnName, properties);
			return this;
		}


		public Table Int(string columnName, int defaultValue)
		{
			columns.Int(columnName, defaultValue);
			return this;
		}

		public Table Int(string columnName, ColumnProperties properties, int defaultValue)
		{
			columns.Int(columnName, properties, defaultValue);
			return this;
		}

		public void Execute()
		{
			TransformationProvider provider = new TransformationProvider(databaseEnvironment);
			provider.AddTable(TableName, columns.ToArray());
		}
	}
}
