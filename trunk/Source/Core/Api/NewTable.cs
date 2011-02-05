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
using System.Collections.Generic;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class NewTable : Table
	{
		private readonly ObjectNameService objectNameService;
		private readonly List<ColumnProvider> columns;
		private ColumnProvider currentColumn;

		internal NewTable(TransformationProvider provider,
		                  string tableName,
		                  ObjectNameService objectNameService) : base(provider, tableName)
		{
			this.objectNameService = objectNameService;
			columns = new List<ColumnProvider>();
		}

		internal void AddColumn(ColumnProvider provider)
		{
			columns.Add(provider);
			currentColumn = provider;
		}

		#region Column types

		/// <param name="columnName"></param>
		/// <param name="size">You can specify Max.Value constant for maximum string length supported by database</param>
		public NewTable String(string columnName, int size)
		{
			AddColumn(new StringProvider(columnName, null, size));
			return this;
		}

		/// <param name="columnName"></param>
		/// <param name="size">You can specify Max.Value constant for maximum string length supported by database</param>
		/// <param name="defaultValue"></param>
		public NewTable String(string columnName, int size, string defaultValue)
		{
			AddColumn(new StringProvider(columnName, defaultValue, size));
			return this;
		}

		public NewTable Text(string columnName)
		{
			AddColumn(new TextProvider(columnName, null));
			return this;
		}

		public NewTable Text(string columnName, string defaultValue)
		{
			AddColumn(new TextProvider(columnName, defaultValue));
			return this;
		}

		public NewTable Int(string columnName)
		{
			AddColumn(new IntProvider(columnName, null));
			return this;
		}

		public NewTable Int(string columnName, int defaultValue)
		{
			AddColumn(new IntProvider(columnName, defaultValue));
			return this;
		}

		public NewTable Long(string columnName)
		{
			AddColumn(new LongProvider(columnName, null));
			return this;
		}

		public NewTable Long(string columnName, long defaultValue)
		{
			AddColumn(new LongProvider(columnName, defaultValue));
			return this;
		}

		public NewTable DateTime(string columnName)
		{
			AddColumn(new DateTimeProvider(columnName, null));
			return this;
		}

		public NewTable DateTime(string columnName, DateTime defaultValue)
		{
			AddColumn(new DateTimeProvider(columnName, defaultValue));
			return this;
		}


		public NewTable Decimal(string columnName)
		{
			AddColumn(new DecimalProvider(columnName, null, 18, 9)); // change this value
			return this;
		}

		public NewTable Decimal(string columnName, int precision, int scale)
		{
			AddColumn(new DecimalProvider(columnName, null, precision, scale));
			return this;
		}

		public NewTable Decimal(string columnName, decimal defaultValue)
		{
			AddColumn(new DecimalProvider(columnName, defaultValue, 18, 9)); // change this value
			return this;
		}

		public NewTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			AddColumn(new DecimalProvider(columnName, null, whole, remainder));
			return this;
		}

		public NewTable Boolean(string columnName)
		{
			AddColumn(new BooleanProvider(columnName, null));
			return this;
		}

		public NewTable Boolean(string columnName, bool defaultValue)
		{
			AddColumn(new BooleanProvider(columnName, defaultValue));
			return this;
		}

		#endregion Column types

		#region Column properties

		public NewTable Identity()
		{
			currentColumn.AddNotNull();
			currentColumn.AddIdentity();
			return this;
		}

		public NewTable NotNull()
		{
			currentColumn.AddNotNull();
			return this;
		}

		public NewTable Null()
		{
			currentColumn.RemoveNotNull();
			return this;
		}

		public NewTable PrimaryKey()
		{
			currentColumn.AddPrimaryKey(objectNameService.PrimaryKeyName(TableName, currentColumn.Name));
			currentColumn.AddNotNull();
			return this;
		}

		public NewTable Unique()
		{
			currentColumn.AddNotNull();
			currentColumn.AddUnique(objectNameService.UniqueName(TableName, currentColumn.Name));
			return this;
		}

		//public NewTable Index()
		//{
		//    currentColumn.AddProperty(propertyFactory.CreateIndex(IndexName()));
		//    return this;
		//}

		#endregion Column properties

		public void Execute()
		{
			Provider.CreateTable(TableName, columns.ToArray());
		}
	}
}