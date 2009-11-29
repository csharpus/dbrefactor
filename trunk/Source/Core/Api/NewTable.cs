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
using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class NewTable : Table
	{
		private readonly ColumnProviderFactory factory;
		private readonly ObjectNameService objectNameService;
		private readonly List<ColumnProvider> columns;
		private ColumnProvider currentColumn;

		internal NewTable(TransformationProvider provider, ColumnProviderFactory columnProviderFactory,
		                  string tableName,
		                  ObjectNameService objectNameService) : base(provider, tableName)
		{
			factory = columnProviderFactory;
			this.objectNameService = objectNameService;
			columns = new List<ColumnProvider>();
		}

		internal ColumnProviderFactory ColumnFactory
		{
			get { return factory; }
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
			AddColumn(factory.CreateString(columnName, null, size));
			return this;
		}

		/// <param name="columnName"></param>
		/// <param name="size">You can specify Max.Value constant for maximum string length supported by database</param>
		/// <param name="defaultValue"></param>
		public NewTable String(string columnName, int size, string defaultValue)
		{
			AddColumn(factory.CreateString(columnName, defaultValue, size));
			return this;
		}

		public NewTable Text(string columnName)
		{
			AddColumn(factory.CreateText(columnName, null));
			return this;
		}

		public NewTable Text(string columnName, string defaultValue)
		{
			AddColumn(factory.CreateText(columnName, defaultValue));
			return this;
		}

		public NewTable Int(string columnName)
		{
			AddColumn(factory.CreateInt(columnName, null));
			return this;
		}

		public NewTable Int(string columnName, int defaultValue)
		{
			AddColumn(factory.CreateInt(columnName, defaultValue));
			return this;
		}

		public NewTable Long(string columnName)
		{
			AddColumn(factory.CreateLong(columnName, null));
			return this;
		}

		public NewTable Long(string columnName, long defaultValue)
		{
			AddColumn(factory.CreateLong(columnName, defaultValue));
			return this;
		}

		public NewTable DateTime(string columnName)
		{
			AddColumn(factory.CreateDateTime(columnName, null));
			return this;
		}

		public NewTable DateTime(string columnName, DateTime defaultValue)
		{
			AddColumn(factory.CreateDateTime(columnName, defaultValue));
			return this;
		}


		public NewTable Decimal(string columnName)
		{
			AddColumn(factory.CreateDecimal(columnName, null, 18, 9)); // change this value
			return this;
		}

		public NewTable Decimal(string columnName, int precision, int scale)
		{
			AddColumn(factory.CreateDecimal(columnName, null, precision, scale));
			return this;
		}

		public NewTable Decimal(string columnName, decimal defaultValue)
		{
			AddColumn(factory.CreateDecimal(columnName, defaultValue, 18, 9)); // change this value
			return this;
		}

		public NewTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			AddColumn(factory.CreateDecimal(columnName, null, whole, remainder));
			return this;
		}

		public NewTable Boolean(string columnName)
		{
			AddColumn(factory.CreateBoolean(columnName, null));
			return this;
		}

		public NewTable Boolean(string columnName, bool defaultValue)
		{
			AddColumn(factory.CreateBoolean(columnName, defaultValue));
			return this;
		}

		#endregion Column types

		#region Column properties

		public NewTable Identity()
		{
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
			return this;
		}

		public NewTable Unique()
		{
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