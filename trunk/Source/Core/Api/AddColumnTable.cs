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
using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class AddColumnTable
	{
		private readonly string tableName;
		private readonly ConstraintNameService constraintNameService;
		private readonly TransformationProvider provider;
		private readonly ColumnProviderFactory factory;
		private ColumnProvider currentColumn;

		internal AddColumnTable(TransformationProvider provider, ColumnProviderFactory columnProviderFactory, string tableName,
		                        ConstraintNameService constraintNameService)
		{
			this.tableName = tableName;
			this.constraintNameService = constraintNameService;
			this.provider = provider;
			factory = columnProviderFactory;
		}

		#region Column types

		public AddColumnTable String(string columnName, int size)
		{
			AddColumn(factory.CreateString(columnName, null, size));
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
			AddColumn(factory.CreateString(columnName, defaultValue, size));
			return this;
		}

		public AddColumnTable Text(string columnName)
		{
			AddColumn(factory.CreateText(columnName, null));
			return this;
		}

		public AddColumnTable Text(string columnName, string defaultValue)
		{
			AddColumn(factory.CreateText(columnName, defaultValue));
			return this;
		}

		public AddColumnTable Int(string columnName)
		{
			AddColumn(factory.CreateInt(columnName, null));
			return this;
		}

		public AddColumnTable Int(string columnName, int defaultValue)
		{
			AddColumn(factory.CreateInt(columnName, defaultValue));
			return this;
		}

		public AddColumnTable Long(string columnName)
		{
			AddColumn(factory.CreateLong(columnName, null));
			return this;
		}

		public AddColumnTable Long(string columnName, long defaultValue)
		{
			AddColumn(factory.CreateLong(columnName, defaultValue));
			return this;
		}

		public AddColumnTable DateTime(string columnName)
		{
			AddColumn(factory.CreateDateTime(columnName, null));
			return this;
		}

		public AddColumnTable DateTime(string columnName, DateTime defaultValue)
		{
			AddColumn(factory.CreateDateTime(columnName, defaultValue));
			return this;
		}


		public AddColumnTable Decimal(string columnName)
		{
			AddColumn(factory.CreateDecimal(columnName, null, 18, 9)); // change this value
			return this;
		}

		public AddColumnTable Decimal(string columnName, int whole, int remainder)
		{
			AddColumn(factory.CreateDecimal(columnName, null, whole, remainder));
			return this;
		}

		public AddColumnTable Decimal(string columnName, decimal defaultValue)
		{
			AddColumn(factory.CreateDecimal(columnName, defaultValue, 18, 9)); // change this value
			return this;
		}

		public AddColumnTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			AddColumn(factory.CreateDecimal(columnName, null, whole, remainder));
			return this;
		}

		public AddColumnTable Boolean(string columnName)
		{
			AddColumn(factory.CreateBoolean(columnName, null));
			return this;
		}

		public AddColumnTable Boolean(string columnName, bool defaultValue)
		{
			AddColumn(factory.CreateBoolean(columnName, defaultValue));
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
			currentColumn.AddPrimaryKey(constraintNameService.PrimaryKeyName(tableName, currentColumn.Name));
			return this;
		}

		public AddColumnTable Unique()
		{
			currentColumn.AddUnique(constraintNameService.UniqueName(tableName, currentColumn.Name));
			return this;
		}

		#endregion Column properties

		public void Execute()
		{
			provider.AddColumn(tableName, currentColumn);
		}
	}
}