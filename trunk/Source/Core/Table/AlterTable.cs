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
			DropTable
		} ;

		private Operation operation = Operation.None;
		private ColumnsCollection columns;				// For column operations
		private string newTableName;
        
		public AlterTable(IDatabaseEnvironment environment, string tableName): base(environment, tableName)
		{
			columns = ColumnsCollection.Create();
		}

		#region Table operations

		public AlterTable RenameTable(string newName)
		{
			newTableName = newName;
			operation = Operation.RenameTable;
			return this;
		}

		public AlterTable DropTable()
		{
			operation = Operation.DropTable;
			return this;
		}

		
		#endregion Table operations

		#region Column operations
		
		public AlterTable AlterColumn(Column column)
		{
			Check.Ensure(operation == Operation.None, "The operation already exists.");
			operation = Operation.AlterColumn;
			columns.Add(column);
			return this;
		}

		public AlterTable RemoveColumnConstraints(string column)
		{
			Check.Ensure(operation == Operation.None, "The operation already exists.");
			operation = Operation.RemoveColumnConstraints;
			columns.Int(column);
			return this;
		}

		public AlterTable RenameColumn(string oldName, string newName)
		{
			Check.Ensure(operation == Operation.None, "The operation already exists.");
			operation = Operation.RenameColumn;
			columns.Int(oldName);
			columns.Int(newName);
			return this;
		}

		public AlterTable DropColumn(string column)
		{
			Check.Ensure(operation == Operation.None, "The operation already exists.");
			operation = Operation.DropColumn;
			columns.Int(column);
			return this;
		}

/*
		protected void AddForeignKey(string name, string foreignKeyTable,
			string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn)
		{
			Database.AddForeignKey(name, foreignKeyTable, foreignKeyColumn,
				primaryKeyTable, primaryKeyColumn);
		}

		protected void AddForeignKey(string name, string foreignKeyTable,
			string foreignKeyColumn, string primaryKeyTable, string primaryKeyColumn, OnDelete ondelete)
		{
			Database.AddForeignKey(name, foreignKeyTable, foreignKeyColumn, primaryKeyTable, primaryKeyColumn, ondelete);
		}

		protected void DropForeignKey(string foreignKeyTable, string name)
		{
			Database.RemoveForeignKey(foreignKeyTable, name);
		}
*/
		#endregion Column operations

		// does it make sense to use kind of Operation Strategy
		public void Execute()
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


				case Operation.RenameTable:
					Check.Ensure(!String.IsNullOrEmpty(newTableName), "New table name has not set");
					provider.RenameTable(TableName, newTableName);
					break;

				case Operation.DropTable:
					provider.DropTable(TableName);
					break;
			}
		}
	}
}
