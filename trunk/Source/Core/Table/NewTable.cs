using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;

namespace DbRefactor
{
	public class NewTable : Table
	{
		private ColumnsCollection columns;

		public NewTable(IDatabaseEnvironment environment) : base(environment)
		{
			columns = ColumnsCollection.Create();
		}

		#region Column types

		public NewTable String(string columnName, int size)
		{
			columns.String(columnName, size);
			return this;
		}

		public NewTable String(string columnName, int size, string defaultValue)
		{
			columns.String(columnName, size, defaultValue);
			return this;
		}

		public NewTable Text(string columnName)
		{
			columns.Text(columnName);
			return this;
		}

		public NewTable Text(string columnName, string defaultValue)
		{
			columns.Text(columnName, defaultValue);
			return this;
		}

		public NewTable Int(string columnName)
		{
			columns.Int(columnName);
			return this;
		}

		public NewTable Int(string columnName, int defaultValue)
		{
			columns.Int(columnName, defaultValue);
			return this;
		}

		public NewTable Long(string columnName)
		{
			columns.Long(columnName);
			return this;
		}

		public NewTable Long(string columnName, long defaultValue)
		{
			columns.Long(columnName, defaultValue);
			return this;
		}

		public NewTable DateTime(string columnName)
		{
			columns.DateTime(columnName);
			return this;
		}

		public NewTable DateTime(string columnName, DateTime defaultValue)
		{
			columns.DateTime(columnName, defaultValue);
			return this;
		}


		public NewTable Decimal(string columnName)
		{
			columns.Decimal(columnName);
			return this;
		}

		public NewTable Decimal(string columnName, int whole, int remainder)
		{
			columns.Decimal(columnName, whole, remainder);
			return this;
		}

		public NewTable Decimal(string columnName, decimal defaultValue)
		{
			columns.Decimal(columnName, defaultValue);
			return this;
		}

		public NewTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			columns.Decimal(columnName, whole, remainder, defaultValue);
			return this;
		}

		public NewTable Boolean(string columnName)
		{
			columns.Boolean(columnName);
			return this;
		}

		public NewTable Boolean(string columnName, bool defaultValue)
		{
			columns.Boolean(columnName, defaultValue);
			return this;
		}

		#endregion Column types

		#region Column properties

		public NewTable Identity()
		{
			columns.AddProperty(ColumnProperties.Identity);
			return this;
		}

		public NewTable Indexed()
		{
			columns.AddProperty(ColumnProperties.Indexed);
			return this;
		}

		public NewTable NotNull()
		{
			columns.AddProperty(ColumnProperties.NotNull);
			return this;
		}

		public NewTable Null()
		{
			columns.AddProperty(ColumnProperties.Null);
			return this;
		}

		public NewTable PrimaryKey()
		{
			columns.AddProperty(ColumnProperties.PrimaryKey);
			return this;
		}

		public NewTable PrimaryKeyWithIdentity()
		{
			columns.AddProperty(ColumnProperties.PrimaryKeyWithIdentity);
			return this;
		}

		public NewTable Unique()
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
