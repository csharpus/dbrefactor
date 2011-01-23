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
using DbRefactor.Exceptions;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class AddColumnTable
	{
		private readonly string tableName;
		private readonly ObjectNameService objectNameService;
		private ColumnProvider currentColumn;

		internal AddColumnTable(string tableName,
		                        ObjectNameService objectNameService)
		{
			this.tableName = tableName;
			this.objectNameService = objectNameService;
		}

		#region Column types

		public AddColumnTable String(string columnName, int size)
		{
			AddColumn(new StringProvider(columnName, null, size));
			return this;
		}

		private void AddColumn(ColumnProvider columnProvider)
		{
			if (currentColumn != null)
				throw new DbRefactorException("You can add only one column at one time");
			currentColumn = columnProvider;
		}

		public AddColumnTable String(string columnName, int size, string defaultValue)
		{
			AddColumn(new StringProvider(columnName, defaultValue, size));
			return this;
		}

		public AddColumnTable Text(string columnName)
		{
			AddColumn(new TextProvider(columnName, null));
			return this;
		}

		public AddColumnTable Text(string columnName, string defaultValue)
		{
			AddColumn(new TextProvider(columnName, defaultValue));
			return this;
		}

		public AddColumnTable Int(string columnName)
		{
			AddColumn(new IntProvider(columnName, null));
			return this;
		}

		public AddColumnTable Int(string columnName, int defaultValue)
		{
			AddColumn(new IntProvider(columnName, defaultValue));
			return this;
		}

		public AddColumnTable Long(string columnName)
		{
			AddColumn(new LongProvider(columnName, null));
			return this;
		}

		public AddColumnTable Long(string columnName, long defaultValue)
		{
			AddColumn(new LongProvider(columnName, defaultValue));
			return this;
		}

		public AddColumnTable DateTime(string columnName)
		{
			AddColumn(new DateTimeProvider(columnName, null));
			return this;
		}

		public AddColumnTable DateTime(string columnName, DateTime defaultValue)
		{
			AddColumn(new DateTimeProvider(columnName, defaultValue));
			return this;
		}


		public AddColumnTable Decimal(string columnName)
		{
			AddColumn(new DecimalProvider(columnName, null, 18, 9)); // change this value
			return this;
		}

		public AddColumnTable Decimal(string columnName, int whole, int remainder)
		{
			AddColumn(new DecimalProvider(columnName, null, whole, remainder));
			return this;
		}

		public AddColumnTable Decimal(string columnName, decimal defaultValue)
		{
			AddColumn(new DecimalProvider(columnName, defaultValue, 18, 9)); // change this value
			return this;
		}

		public AddColumnTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			AddColumn(new DecimalProvider(columnName, null, whole, remainder));
			return this;
		}

		public AddColumnTable Boolean(string columnName)
		{
			AddColumn(new BooleanProvider(columnName, null));
			return this;
		}

		public AddColumnTable Boolean(string columnName, bool defaultValue)
		{
			AddColumn(new BooleanProvider(columnName, defaultValue));
			return this;
		}

		#endregion Column types

		#region Column properties

		public AddColumnTable Identity()
		{
			currentColumn.AddIdentity();
			return this;
		}

		public AddColumnTable NotNull()
		{
			currentColumn.AddNotNull();
			return this;
		}

		public AddColumnTable PrimaryKey()
		{
			currentColumn.AddPrimaryKey(objectNameService.PrimaryKeyName(tableName, currentColumn.Name));
			currentColumn.AddNotNull();
			return this;
		}

		public AddColumnTable Unique()
		{
			currentColumn.AddNotNull();
			currentColumn.AddUnique(objectNameService.UniqueName(tableName, currentColumn.Name));
			return this;
		}

		#endregion Column properties

		internal ColumnProvider CurrentColumn
		{
			get { return currentColumn; }
		}
	}
}