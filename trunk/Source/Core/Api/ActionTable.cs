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
using System.Data;
using DbRefactor.Factories;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class ActionTable : Table
	{
		private readonly TransformationProvider provider;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly ObjectNameService objectNameService;
		private readonly ApiFactory apiFactory;

		internal ActionTable(TransformationProvider provider, string tableName, ColumnProviderFactory columnProviderFactory,
		                     ObjectNameService objectNameService, ApiFactory apiFactory) : base(provider, tableName)
		{
			this.provider = provider;
			this.columnProviderFactory = columnProviderFactory;
			this.objectNameService = objectNameService;
			this.apiFactory = apiFactory;
		}

		/// <summary>
		/// Provides an access for column level operations
		/// </summary>
		public ActionColumn Column(string name)
		{
			return apiFactory.CreateActionColumn(TableName, name);
		}

		public IDataReader Select(params string[] columns)
		{
			return provider.Select(TableName, columns, new {});
		}

		/// <summary>
		/// Insert new record to database table
		/// </summary>
		/// <param name="values">
		/// <example>
		/// The syntax is:
		/// <code>
		/// new {ColumnName1 = 100, ColumName2 = "a string", ...}
		/// </code>
		/// </example>
		/// </param>
		/// <returns></returns>
		public void Insert(object values)
		{
			provider.Insert(TableName, values);
		}

		/// <summary>
		/// Condition for Select, SelectScalar, Update and Delete operations
		/// </summary>
		/// <param name="whereParameters">
		/// <example>
		/// The syntax is:
		/// <code>
		/// new {ColumnName1 = 100, ColumName2 = "a string", ...}
		/// </code>.
		/// For several conditions 'and' operation is applied
		/// </example>
		/// </param>
		public WhereTable Where(object whereParameters)
		{
			return new WhereTable(provider, TableName, whereParameters);
		}

		#region Table operations

		/// <summary>
		/// Rename table
		/// </summary>
		/// <param name="newName">New table name</param>
		public void RenameTo(string newName)
		{
			provider.RenameTable(TableName, newName);
		}

		#endregion Table operations

		#region Column operations

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column">Lambda expression for column definition. The syntax is:
		/// <example>
		/// c => c.Int("ColumnName").NotNull().Identity()
		/// </example>
		/// </param>
		public void AddColumn(Func<AddColumnTable, AddColumnTable> column)
		{
			var columnTable = new AddColumnTable(columnProviderFactory, TableName, objectNameService);
			provider.AddColumn(TableName, column(columnTable).CurrentColumn);
		}

		public void DropColumn(string column)
		{
			provider.DropColumn(TableName, column);
		}

		public void DropPrimaryKey()
		{
			provider.DropPrimaryKey(TableName);
		}

		#endregion Column operations
	}
}