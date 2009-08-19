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

using System.Collections.Generic;
using DbRefactor.Factories;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class ActionTable : Table
	{
		private readonly TransformationProvider provider;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;
		private readonly ConstraintNameService constraintNameService;
		private readonly ApiFactory apiFactory;

		public ActionTable(TransformationProvider provider, string tableName, ColumnProviderFactory columnProviderFactory,
		                   ColumnPropertyProviderFactory columnPropertyProviderFactory,
		                   ConstraintNameService constraintNameService, ApiFactory apiFactory) : base(provider, tableName)
		{
			this.provider = provider;
			this.columnProviderFactory = columnProviderFactory;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
			this.constraintNameService = constraintNameService;
			this.apiFactory = apiFactory;
		}

		public ActionColumn Column(string name)
		{
			return apiFactory.CreateActionColumn(TableName, name);
		}

		/// <summary>
		/// Insert new record to database table
		/// </summary>
		/// <param name="parameters">This is parameters for operation Insert.<br />
		/// To add parameters you could use next syntaxes
		/// Table(TableName).Insert(new {ColumnName1=Parameter1, ColumName2="StringParameter2", ...})
		/// </param>
		/// <returns></returns>
		public void Insert(object parameters)
		{
			List<string> operationParamList = ParametersHelper.GetParameters(parameters);
			provider.Insert(TableName, operationParamList.ToArray());
		}

		/// <summary>
		/// Update record(s) in database table
		/// </summary>
		/// <param name="parameters">This is parameters for operation Update.<br />
		/// To add parameters you could use follow syntax
		/// Table(TableName).Update(new {ColumnName1=Parameter1, ColumName2="StringParameter2", ...})
		/// </param>
		/// <returns></returns>
		public UpdateTable Update(object parameters)
		{
			return new UpdateTable(provider, TableName, parameters);
		}

		/// <summary>
		/// Delete record(s) in datbase table.
		/// To filter deleted rows use method Where
		/// </summary>
		/// <returns></returns>
		public DeleteTable Delete()
		{
			return new DeleteTable(provider, TableName);
		}

		/// <summary>
		/// Select single value from database table
		/// </summary>
		/// <typeparam name="T">Type of return value</typeparam>
		/// <param name="column">Data table field</param>
		/// <returns></returns>
		public SelectScalarTable<T> SelectScalar<T>(string column)
		{
			return new SelectScalarTable<T>(provider, TableName, column);
		}

		/// <summary>
		/// Select multiple values from database table
		/// </summary>
		/// <param name="what">Data table columns</param>
		/// <returns></returns>
		public SelectTable Select(params string[] what)
		{
			return new SelectTable(provider, TableName, what);
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

		/// <summary>
		/// Delete table from database
		/// </summary>
		public void DropTable()
		{
			provider.DropTable(TableName);
		}

		#endregion Table operations

		#region Column operations

		public AddColumnTable AddColumn()
		{
			return new AddColumnTable(provider, columnProviderFactory, TableName,
			                          constraintNameService);
		}

		public void RenameColumn(string oldName, string newName)
		{
			provider.RenameColumn(TableName, oldName, newName);
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