#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using DbRefactor.Providers;

namespace DbRefactor
{
	public class NewTable : Table
	{
		private readonly ColumnsCollection columns;

		public NewTable(TransformationProvider provider, string tableName): base(provider, tableName)
		{
			columns = ColumnsCollection.Create();
		}

		internal ColumnsCollection Columns
		{
			get { return columns; }
		}

		#region Column types

		public NewTable String(string columnName, int size)
		{
			Columns.String(columnName, size);
			return this;
		}

		public NewTable String(string columnName, int size, string defaultValue)
		{
			Columns.String(columnName, size, defaultValue);
			return this;
		}

		public NewTable Text(string columnName)
		{
			Columns.Text(columnName);
			return this;
		}

		public NewTable Text(string columnName, string defaultValue)
		{
			Columns.Text(columnName, defaultValue);
			return this;
		}

		public NewTable Int(string columnName)
		{
			Columns.Int(columnName);
			return this;
		}

		public NewTable Int(string columnName, int defaultValue)
		{
			Columns.Int(columnName, defaultValue);
			return this;
		}

		public NewTable Long(string columnName)
		{
			Columns.Long(columnName);
			return this;
		}

		public NewTable Long(string columnName, long defaultValue)
		{
			Columns.Long(columnName, defaultValue);
			return this;
		}

		public NewTable DateTime(string columnName)
		{
			Columns.DateTime(columnName);
			return this;
		}

		public NewTable DateTime(string columnName, DateTime defaultValue)
		{
			Columns.DateTime(columnName, defaultValue);
			return this;
		}


		public NewTable Decimal(string columnName)
		{
			Columns.Decimal(columnName);
			return this;
		}

		public NewTable Decimal(string columnName, int whole, int remainder)
		{
			Columns.Decimal(columnName, whole, remainder);
			return this;
		}

		public NewTable Decimal(string columnName, decimal defaultValue)
		{
			Columns.Decimal(columnName, defaultValue);
			return this;
		}

		public NewTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			Columns.Decimal(columnName, whole, remainder, defaultValue);
			return this;
		}

		public NewTable Boolean(string columnName)
		{
			Columns.Boolean(columnName);
			return this;
		}

		public NewTable Boolean(string columnName, bool defaultValue)
		{
			Columns.Boolean(columnName, defaultValue);
			return this;
		}

		#endregion Column types

		#region Column properties

		public NewTable Identity()
		{
			Columns.AddProperty(ColumnProperties.Identity);
			return this;
		}

		public NewTable NotNull()
		{
			Columns.AddProperty(ColumnProperties.NotNull);
			return this;
		}

		public NewTable Null()
		{
			Columns.AddProperty(ColumnProperties.Null);
			return this;
		}

		public NewTable PrimaryKey()
		{
			Columns.AddProperty(ColumnProperties.PrimaryKey);
			return this;
		}

		public NewTable PrimaryKeyWithIdentity()
		{
			Columns.AddProperty(ColumnProperties.PrimaryKeyWithIdentity);
			return this;
		}

		public NewTable Unique()
		{
			Columns.AddProperty(ColumnProperties.Unique);
			return this;
		}

		#endregion Column properties

		public void Execute()
		{
			Provider.AddTable(TableName, Columns.ToArray());
		}
	}
}
