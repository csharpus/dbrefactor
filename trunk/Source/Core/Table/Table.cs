using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;

namespace DbRefactor
{
	public class Table
	{
		private IDatabaseEnvironment databaseEnvironment;
		private ColumnsCollection Columns;
		public string TableName { get; set; }

		public Table(IDatabaseEnvironment environment)
		{
			databaseEnvironment = environment;
			Columns = ColumnsCollection.Create();
		}

		public Table Int(string columnName)
		{
			Columns.Int(columnName);
			return this;
		}

		public Table Int(string columnName, ColumnProperties properties)
		{
			Columns.Int(columnName, properties);
			return this;
		}


		public Table Int(string columnName, int defaultValue)
		{
			Columns.Int(columnName, defaultValue);
			return this;
		}

		public Table Int(string columnName, ColumnProperties properties, int defaultValue)
		{
			Columns.Int(columnName, properties, defaultValue);
			return this;
		}

		public void Execute()
		{
			TransformationProvider provider = new TransformationProvider(databaseEnvironment);
			provider.AddTable(TableName, Columns.ToArray());
		}
	}
}
