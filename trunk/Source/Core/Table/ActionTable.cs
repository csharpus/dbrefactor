using System;
using System.Collections.Generic;
using System.Data;
using DbRefactor.Providers;
using DbRefactor.Tools;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor
{
	public partial class ActionTable: Table
	{
		private readonly TransformationProvider provider;

		private enum Operation
		{
			None,
			Insert,
			Update,
			Delete,
			AlterColumn,
			RemoveColumnConstraints,
			RenameColumn,
			RenameTable,
			DropColumn,
			DropTable,
			RemoveForeignKey,
			AddForeignKey,
			DropForeignKey,
			SelectScalar
		} ;

		private List<string> columnValues; 
		private ColumnsCollection columns;				// For column operations
        
		private const string StringParameterPattern = "{0}='{1}'";
		private const string NotStringParameterPattern = "{0}={1}";
        
		private string newTableName;
        private string _foreignKeyColumn;
		private string _primaryKeyTable;
		private string _primaryKeyColumn;
		private OnDelete foreignKeyConstraint = OnDelete.NoAction;
		private string keyName;
		private object operationParameters = null;
        private Operation operation = Operation.None;
		
		public ActionTable(TransformationProvider provider, string tableName) : base(provider, tableName)
		{
			this.provider = provider;
			columnValues = new List<string>();
			columns = ColumnsCollection.Create();
		}

		/// <summary>
		/// Insert new record to database table
		/// </summary>
		/// <param name="parameters">This is parameters for operation Insert.<br />
		/// To add parameters you could use next syntaxes
		/// Table(TableName).Insert(new {ColumnName1=Parameter1, ColumName2="StringParameter2", ...})
		/// </param>
		/// <returns></returns>
		public ActionTable Insert(object parameters)
		{
			Check.Ensure(operation == Operation.None || operation == Operation.None, "Only One type of operation allowed.");
			operation = Operation.Insert;
			operationParameters = parameters;
			Execute(operationParameters, null);
			return this;
		}

		/// <summary>
		/// Update record(s) in database table
		/// </summary>
		/// <param name="parameters">This is parameters for operation Update.<br />
		/// To add parameters you could use follow syntax
		/// Table(TableName).Update(new {ColumnName1=Parameter1, ColumName2="StringParameter2", ...})
		/// </param>
		/// <returns></returns>
		public ActionTable Update(object parameters)
		{
			Check.Ensure(operation == Operation.None, "Please specify criteria for previous update operation.");
			operationParameters = parameters;
			operation = Operation.Update;
			return this;
		}

		/// <summary>
		/// Delete record(s) in datbase table.
		/// To filter deleted rows use method Where
		/// </summary>
		/// <returns></returns>
		public ActionTable Delete()
		{
			Check.Ensure(operation == Operation.None, "Please specify criteria for previous operation.");
			operation = Operation.Update;
			return this;
		}
		 
		/// <summary>
		/// This method is a filter for group operations on table records
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public ActionTable Where(object parameters)
		{
			Execute(operationParameters, parameters);
			return this;
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
			return (T)provider.SelectScalar(what, TableName, String.Join(" AND ", crieriaParamList.ToArray())); ;
		}

		/// <summary>
		/// Select multiple values from database table
		/// </summary>
		/// <param name="what">Data table field</param>
		/// <param name="where">Filter</param>
		/// <returns></returns>
		public IDataReader Select(string what, object where)
		{
			List<string> crieriaParamList = ParametersHelper.GetParameters(where);
			return provider.Select(what, TableName, String.Join(" AND ", crieriaParamList.ToArray())); ;
		}

		#region Table operations

		/// <summary>
		/// Rename table
		/// </summary>
		/// <param name="newName">New table name</param>
		public void RenameTo(string newName)
		{
			newTableName = newName;
			operation = Operation.RenameTable;
			Execute();
		}

		/// <summary>
		/// Delete table from database
		/// </summary>
		public void DropTable()
		{
			operation = Operation.DropTable;
			Execute();
		}

		#endregion Table operations

		#region Column operations

		public void AlterColumn(Column column)
		{
			operation = Operation.AlterColumn;
			columns.Add(column);
			Execute();
		}

		public AlterColumnTable AlterColumn()
		{
			return new AlterColumnTable(TableName, provider);
		}

		public AddColumnTable AddColumn()
		{
			return new AddColumnTable(TableName, provider);
		}

		public void RemoveColumnConstraints(string column)
		{
			operation = Operation.RemoveColumnConstraints;
			columns.Int(column);
			Execute();
		}

		public void RenameColumn(string oldName, string newName)
		{
			operation = Operation.RenameColumn;
			columns.Int(oldName);
			columns.Int(newName);
			Execute();
		}

		public void DropColumn(string column)
		{
			operation = Operation.DropColumn;
			columns.Int(column);
			Execute();
		}

		public void AddForeignKey(string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn)
		{
			AddForeignKey(foreignKeyColumn, primaryKeyTable, primaryKeyColumn, OnDelete.NoAction);
		}

		public void AddForeignKey(string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn, OnDelete ondelete)
		{
			operation = Operation.AddForeignKey;
			_foreignKeyColumn = foreignKeyColumn;
			_primaryKeyTable = primaryKeyTable;
			_primaryKeyColumn = primaryKeyColumn;
			foreignKeyConstraint = ondelete;
			Execute();
		}

		public void DropForeignKey(string key)
		{
			operation = Operation.DropForeignKey;
			keyName = key;
			Execute();
		}

		#endregion Column operations
        
		private void Execute(object operationParams, object criteriaParameters)
		{
			List<string> operationParamList = ParametersHelper.GetParameters(operationParams);
			AddParameters(operationParamList);

			Check.Ensure(operation != Operation.None, "The operation has not been set.");
			Check.Ensure(columnValues.Count != 0, "Values have not been set.");

			if (operation == Operation.Insert)
			{
				provider.Insert(TableName, columnValues.ToArray());
			}
			else if (operation == Operation.Update)
			{
				List<string> crieriaParamList = ParametersHelper.GetParameters(criteriaParameters);
				provider.Update(TableName, columnValues.ToArray(), crieriaParamList.ToArray());
			}
			else
			{
				List<string> crieriaParamList = ParametersHelper.GetParameters(criteriaParameters);
				provider.Delete(TableName, crieriaParamList.ToArray());

			}
			operation = Operation.None;
			columnValues = new List<string>();
		}

		// does it make sense to use kind of Operation Strategy
		private void Execute()
		{
			Check.Ensure(operation != Operation.None, "The operation has not been set.");

			switch (operation)
			{
				case Operation.AlterColumn:
					Check.Ensure(columns.ToArray().Length != 0, "Colmn for the operation has not set.");
					Check.Ensure(columns.ToArray().Length < 2, "Currently, we do not support cascade alter colmn operations.");

					provider.AlterColumn(TableName, columns.LastColumnItem);	
					break;

				case Operation.RemoveColumnConstraints:
					Check.Ensure(columns.ToArray().Length != 0, "Colmn for the operation has not set.");
					Check.Ensure(columns.ToArray().Length < 2, "Currently, we do not support cascade remove constraints colmn operations.");

					provider.DeleteColumnConstraints(TableName, columns.LastColumnItem.Name);
					break;

				case Operation.RenameColumn:
					Check.Ensure(columns.ToArray().Length != 0, "Colmns for the operation have not set.");
					Check.Ensure(columns.ToArray().Length < 3, "Rename operation required 2 columns");
					var columnsArray = columns.ToArray();
					provider.RenameColumn(TableName, columnsArray[0].Name, columnsArray[1].Name);
					break;

				case Operation.DropColumn:
					Check.Ensure(columns.ToArray().Length != 0, "Colmn for the operation has not set.");
					Check.Ensure(columns.ToArray().Length < 2, "Currently, we do not support cascade drop colmn operations.");

					provider.DropColumn(TableName, columns.LastColumnItem.Name);
					break;

				case Operation.RemoveForeignKey:
					provider.RemoveForeignKey(TableName, keyName);
					break;

				case Operation.AddForeignKey:
					string key = keyName;
					if (String.IsNullOrEmpty(keyName))
						key = GenerateForeignKey(TableName, _primaryKeyTable);	// FK_TableName_prinaryKeyTable
					provider.AddForeignKey(key, TableName, _foreignKeyColumn, _primaryKeyTable, _primaryKeyColumn, foreignKeyConstraint);
					break;

				case Operation.DropForeignKey:
					string _key = keyName;
					if (String.IsNullOrEmpty(_key))
						provider.RemoveForeignKey(TableName, _key);
					break;

				case Operation.RenameTable:
					Check.Ensure(!String.IsNullOrEmpty(newTableName), "New table name has not set");
					provider.RenameTable(TableName, newTableName);
					break;

				case Operation.DropTable:
					provider.DropTable(TableName);
					break;
			}
		}

		private string GenerateForeignKey(string tableName, string primaryKeyTable)
		{
			return String.Format("FK_{0}_{1}", tableName, primaryKeyTable);
		}

		private void AddParameters(IEnumerable<string> parameters)
		{
			foreach (var parameter in parameters)
				columnValues.Add(parameter);
		}
	}
}
