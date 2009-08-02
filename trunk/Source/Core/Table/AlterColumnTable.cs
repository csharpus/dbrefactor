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
	public class AlterColumnTable
	{
		private readonly string name;
		private readonly TransformationProvider provider;
		private readonly IDatabaseEnvironment environment;
		private Column column;

		public AlterColumnTable(string name, TransformationProvider provider)
		{
			this.name = name;
			this.provider = provider;
			this.environment = environment;
		}

		#region Column types

		public AlterColumnTable String(string columnName, int size)
		{
			column = Column.String(columnName, size);
			return this;
		}

		public AlterColumnTable String(string columnName, int size, string defaultValue)
		{
			column = Column.String(columnName, size, defaultValue);
			return this;
		}

		public AlterColumnTable Text(string columnName)
		{
			column = Column.Text(columnName);
			return this;
		}

		public AlterColumnTable Text(string columnName, string defaultValue)
		{
			column = Column.Text(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Int(string columnName)
		{
			column = Column.Int(columnName);
			return this;
		}

		public AlterColumnTable Int(string columnName, int defaultValue)
		{
			column = Column.Int(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Long(string columnName)
		{
			column = Column.Long(columnName);
			return this;
		}

		public AlterColumnTable Long(string columnName, long defaultValue)
		{
			column = Column.Long(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable DateTime(string columnName)
		{
			column = Column.DateTime(columnName);
			return this;
		}

		public AlterColumnTable DateTime(string columnName, DateTime defaultValue)
		{
			column = Column.DateTime(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Decimal(string columnName)
		{
			column = Column.Decimal(columnName);
			return this;
		}

		public AlterColumnTable Decimal(string columnName, int whole, int remainder)
		{
			column = Column.Decimal(columnName, whole, remainder);
			return this;
		}

		public AlterColumnTable Decimal(string columnName, decimal defaultValue)
		{
			column = Column.Decimal(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			column = Column.Decimal(columnName, whole, remainder, defaultValue);
			return this;
		}

		public AlterColumnTable Boolean(string columnName)
		{
			column = Column.Boolean(columnName);
			return this;
		}

		public AlterColumnTable Boolean(string columnName, bool defaultValue)
		{
			column = Column.Boolean(columnName, defaultValue);
			return this;
		}

		#endregion Column types

		#region Column properties

		public AlterColumnTable Identity()
		{
			column.ColumnProperty |= ColumnProperties.Identity;
			return this;
		}

		public AlterColumnTable Indexed()
		{
			column.ColumnProperty |= ColumnProperties.Indexed;
			return this;
		}

		public AlterColumnTable NotNull()
		{
			column.ColumnProperty |= ColumnProperties.NotNull;
			return this;
		}

		public AlterColumnTable Null()
		{
			column.ColumnProperty |= ColumnProperties.Null;
			return this;
		}

		public AlterColumnTable PrimaryKey()
		{
			column.ColumnProperty |= ColumnProperties.PrimaryKey;
			return this;
		}

		public AlterColumnTable PrimaryKeyWithIdentity()
		{
			column.ColumnProperty |= ColumnProperties.PrimaryKeyWithIdentity;
			return this;
		}

		public AlterColumnTable Unique()
		{
			column.ColumnProperty |= ColumnProperties.Unique;
			return this;
		}

		#endregion Column properties

		public void Execute()
		{
			provider.AlterColumn(name, column);
		}
	}
}