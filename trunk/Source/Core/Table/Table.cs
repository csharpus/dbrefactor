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

		#region Column types

		public Table String(string columnName, int size)
		{
			columns.String(columnName, size);
			return this;
		}

		public Table String(string columnName, int size, string defaultValue)
		{
			columns.String(columnName, size, defaultValue);
			return this;
		}

		public Table Text(string columnName)
		{
			columns.Text(columnName);
			return this;
		}

		public Table Text(string columnName, string defaultValue)
		{
			columns.Text(columnName, defaultValue);
			return this;
		}

		public Table Int(string columnName)
		{
			columns.Int(columnName);
			return this;
		}

		public Table Int(string columnName, int defaultValue)
		{
			columns.Int(columnName, defaultValue);
			return this;
		}

		public Table Long(string columnName)
		{
			columns.Long(columnName);
			return this;
		}

		public Table Long(string columnName, long defaultValue)
		{
			columns.Long(columnName, defaultValue);
			return this;
		}

		public Table DateTime(string columnName)
		{
			columns.DateTime(columnName);
			return this;
		}

		public Table DateTime(string columnName, DateTime defaultValue)
		{
			columns.DateTime(columnName, defaultValue);
			return this;
		}


		public Table Decimal(string columnName)
		{
			columns.Decimal(columnName);
			return this;
		}

		public Table Decimal(string columnName, int whole, int remainder)
		{
			columns.Decimal(columnName, whole, remainder);
			return this;
		}

		public Table Decimal(string columnName, decimal defaultValue)
		{
			columns.Decimal(columnName, defaultValue);
			return this;
		}

		public Table Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			columns.Decimal(columnName, whole, remainder, defaultValue);
			return this;
		}

		public Table Boolean(string columnName)
		{
			columns.Boolean(columnName);
			return this;
		}

		public Table Boolean(string columnName, bool defaultValue)
		{
			columns.Boolean(columnName, defaultValue);
			return this;
		}

		#endregion Column types

		#region Column properties

		public Table Identity()
		{
			columns.AddProperty(ColumnProperties.Identity);
			return this;
		}

		public Table Indexed()
		{
			columns.AddProperty(ColumnProperties.Indexed);
			return this;
		}

		public Table NotNull()
		{
			columns.AddProperty(ColumnProperties.NotNull);
			return this;
		}

		public Table Null()
		{
			columns.AddProperty(ColumnProperties.Null);
			return this;
		}

		public Table PrimaryKey()
		{
			columns.AddProperty(ColumnProperties.PrimaryKey);
			return this;
		}

		public Table PrimaryKeyWithIdentity()
		{
			columns.AddProperty(ColumnProperties.PrimaryKeyWithIdentity);
			return this;
		}

		public Table Unique()
		{
			columns.AddProperty(ColumnProperties.Unique);
			return this;
		}

		#endregion Column properties

		//public Table DefaultValue(object value)
		//{
		//    Type type = columns.LastColumnItem.Type;
		//    columns.LastColumnItem.DefaultValue = Convert.ChangeType(value, type);
			
		//    return this;
		//}

		public void Execute()
		{
			TransformationProvider provider = new TransformationProvider(databaseEnvironment);
			provider.AddTable(TableName, columns.ToArray());
		}
	}
}
