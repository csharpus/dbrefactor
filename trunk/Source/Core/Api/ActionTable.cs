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
using DbRefactor.Tools;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Api
{
	public class ActionTable: Table
	{
		private readonly TransformationProvider provider;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;

		private enum Operation
		{
			None,
			RemoveForeignKey,
			AddForeignKey,
			DropForeignKey
		}

		private string foreignKeyColumn;
		private string primaryKeyTable;
		private string primaryKeyColumn;
		private OnDelete foreignKeyConstraint = OnDelete.NoAction;
		private string keyName;
		private Operation operation = Operation.None;
		
		public ActionTable(TransformationProvider provider, string tableName, ColumnProviderFactory columnProviderFactory, ColumnPropertyProviderFactory columnPropertyProviderFactory) : base(provider, tableName)
		{
			this.provider = provider;
			this.columnProviderFactory = columnProviderFactory;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
		}

		public ActionColumn Column(string name)
		{
			return new ActionColumn(provider, TableName, name, columnProviderFactory);
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
		/// <param name="what">Data table field</param>
		/// <param name="where">Filter</param>
		/// <returns></returns>
		public T SelectScalar<T>(string what, object where)
		{
			List<string> crieriaParamList = ParametersHelper.GetParameters(where);
			return (T)provider.SelectScalar(what, TableName, String.Join(" AND ", crieriaParamList.ToArray()));
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
			return new AddColumnTable(provider, columnProviderFactory, columnPropertyProviderFactory, TableName);
		}

		public void RemoveColumnConstraints(string column)
		{
			provider.DeleteColumnConstraints(TableName, column);
		}

		public void RenameColumn(string oldName, string newName)
		{
			provider.RenameColumn(TableName, oldName, newName);
		}

		public void DropColumn(string column)
		{
			provider.DropColumn(TableName, column);
		}

		public void AddForeignKey(string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn)
		{
			AddForeignKey(foreignKeyColumn, primaryKeyTable, primaryKeyColumn, OnDelete.NoAction);
		}

		public void AddForeignKey(string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn, OnDelete ondelete)
		{
			operation = Operation.AddForeignKey;
			this.foreignKeyColumn = foreignKeyColumn;
			this.primaryKeyTable = primaryKeyTable;
			this.primaryKeyColumn = primaryKeyColumn;
			foreignKeyConstraint = ondelete;
			Execute();
		}

		public void DropForeignKey(string key)
		{
			operation = Operation.DropForeignKey;
			keyName = key;
			Execute();
		}

		public void DropPrimaryKey()
		{
			provider.DropPrimaryKey(TableName);
		}

		#endregion Column operations

		// does it make sense to use kind of Operation Strategy
		private void Execute()
		{
			Check.Ensure(operation != Operation.None, "The operation has not been set.");

			switch (operation)
			{
				case Operation.RemoveForeignKey:
					provider.DropConstraint(TableName, keyName);
					break;

				case Operation.AddForeignKey:
					string key = keyName;
					if (String.IsNullOrEmpty(keyName))
						key = GenerateForeignKey(TableName, primaryKeyTable);	// FK_TableName_prinaryKeyTable
					provider.AddForeignKey(key, TableName, foreignKeyColumn, primaryKeyTable, primaryKeyColumn, foreignKeyConstraint);
					break;

				case Operation.DropForeignKey:
					string _key = keyName;
					if (String.IsNullOrEmpty(_key))
						provider.DropConstraint(TableName, _key);
					break;
			}
		}

		private string GenerateForeignKey(string tableName, string primaryKeyTable)
		{
			return String.Format("FK_{0}_{1}", tableName, primaryKeyTable);
		}
	}
}