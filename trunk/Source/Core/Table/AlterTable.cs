using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor
{
	public class AlterTable : Table
	{
		private enum Operation
		{
			None,
			AlterColumn,
			RemoveColumnConstraints,
			RenameColumn,
			RenameTable,
			DropColumn,
			DropTable,
			RemoveForeignKey,
			AddForeignKey,
			DropForeignKey
		} ;

		private Operation operation = Operation.None;
		private ColumnsCollection columns;				// For column operations
		private string newTableName;

		private string _foreignKeyColumn;
		private string _primaryKeyTable;
		private string _primaryKeyColumn;
		private OnDelete foreignKeyConstraint = OnDelete.NoAction;
		private string keyName;
        
		public AlterTable(IDatabaseEnvironment environment, string tableName): base(environment, tableName)
		{
			columns = ColumnsCollection.Create();
		}

		#region Table operations

		public void RenameTable(string newName)
		{
			newTableName = newName;
			operation = Operation.RenameTable;
			Execute();
		}

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

		protected void AddForeignKey(string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn, OnDelete ondelete)
		{
			operation = Operation.AddForeignKey;
			_foreignKeyColumn = foreignKeyColumn;
			_primaryKeyTable = primaryKeyTable;
			_primaryKeyColumn = primaryKeyColumn;
			foreignKeyConstraint = ondelete;
			Execute();
		}

		protected void DropForeignKey(string key)
		{
			operation = Operation.DropForeignKey;
			keyName = key;
			Execute();
		}

		#endregion Column operations

		// does it make sense to use kind of Operation Strategy
		private void Execute()
		{
			Check.Ensure(operation != Operation.None, "The operation has not been set.");

			TransformationProvider provider = new TransformationProvider(databaseEnvironment);
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
					if(String.IsNullOrEmpty(key))
						key = GenerateForeignKey(TableName, _primaryKeyTable);	// FK_TableName_prinaryKeyTable
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
	}
}
