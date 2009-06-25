using System;
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

		public AlterColumnTable AlterColumn()
		{
			return new AlterColumnTable(TableName, databaseEnvironment);
		}

		public AddColumnTable AddColumn()
		{
			return new AddColumnTable(TableName, databaseEnvironment);
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
	}

	public class AlterColumnTable
	{
		private readonly string name;
		private readonly IDatabaseEnvironment environment;
		private Column column;

		public AlterColumnTable(string name, IDatabaseEnvironment environment)
		{
			this.name = name;
			this.environment = environment;
		}

		#region Column types

		public AlterColumnTable String(string columnName, int size)
		{
			column = Column.String(columnName, size);
			return this;
		}

		public AlterColumnTable String(string columnName, int size, string defaultValue)
		{
			column = Column.String(columnName, size, defaultValue);
			return this;
		}

		public AlterColumnTable Text(string columnName)
		{
			column = Column.Text(columnName);
			return this;
		}

		public AlterColumnTable Text(string columnName, string defaultValue)
		{
			column = Column.Text(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Int(string columnName)
		{
			column = Column.Int(columnName);
			return this;
		}

		public AlterColumnTable Int(string columnName, int defaultValue)
		{
			column = Column.Int(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Long(string columnName)
		{
			column = Column.Long(columnName);
			return this;
		}

		public AlterColumnTable Long(string columnName, long defaultValue)
		{
			column = Column.Long(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable DateTime(string columnName)
		{
			column = Column.DateTime(columnName);
			return this;
		}

		public AlterColumnTable DateTime(string columnName, DateTime defaultValue)
		{
			column = Column.DateTime(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Decimal(string columnName)
		{
			column = Column.Decimal(columnName);
			return this;
		}

		public AlterColumnTable Decimal(string columnName, int whole, int remainder)
		{
			column = Column.Decimal(columnName, whole, remainder);
			return this;
		}

		public AlterColumnTable Decimal(string columnName, decimal defaultValue)
		{
			column = Column.Decimal(columnName, defaultValue);
			return this;
		}

		public AlterColumnTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			column = Column.Decimal(columnName, whole, remainder, defaultValue);
			return this;
		}

		public AlterColumnTable Boolean(string columnName)
		{
			column = Column.Boolean(columnName);
			return this;
		}

		public AlterColumnTable Boolean(string columnName, bool defaultValue)
		{
			column = Column.Boolean(columnName, defaultValue);
			return this;
		}

		#endregion Column types

		#region Column properties

		public AlterColumnTable Identity()
		{
			column.ColumnProperty |= ColumnProperties.Identity;
			return this;
		}

		public AlterColumnTable Indexed()
		{
			column.ColumnProperty |= ColumnProperties.Indexed;
			return this;
		}

		public AlterColumnTable NotNull()
		{
			column.ColumnProperty |= ColumnProperties.NotNull;
			return this;
		}

		public AlterColumnTable Null()
		{
			column.ColumnProperty |= ColumnProperties.Null;
			return this;
		}

		public AlterColumnTable PrimaryKey()
		{
			column.ColumnProperty |= ColumnProperties.PrimaryKey;
			return this;
		}

		public AlterColumnTable PrimaryKeyWithIdentity()
		{
			column.ColumnProperty |= ColumnProperties.PrimaryKeyWithIdentity;
			return this;
		}

		public AlterColumnTable Unique()
		{
			column.ColumnProperty |= ColumnProperties.Unique;
			return this;
		}

		#endregion Column properties

		public void Execute()
		{
			TransformationProvider provider = new TransformationProvider(environment);
			provider.AlterColumn(name, column);
		}
	}

	public class AddColumnTable
	{
		private readonly string name;
		private readonly IDatabaseEnvironment environment;
		private Column column;

		public AddColumnTable(string name, IDatabaseEnvironment environment)
		{
			this.name = name;
			this.environment = environment;
		}

		#region Column types

		public AddColumnTable String(string columnName, int size)
		{
			column = Column.String(columnName, size);
			return this;
		}

		public AddColumnTable String(string columnName, int size, string defaultValue)
		{
			column = Column.String(columnName, size, defaultValue);
			return this;
		}

		public AddColumnTable Text(string columnName)
		{
			column = Column.Text(columnName);
			return this;
		}

		public AddColumnTable Text(string columnName, string defaultValue)
		{
			column = Column.Text(columnName, defaultValue);
			return this;
		}

		public AddColumnTable Int(string columnName)
		{
			column = Column.Int(columnName);
			return this;
		}

		public AddColumnTable Int(string columnName, int defaultValue)
		{
			column = Column.Int(columnName, defaultValue);
			return this;
		}

		public AddColumnTable Long(string columnName)
		{
			column = Column.Long(columnName);
			return this;
		}

		public AddColumnTable Long(string columnName, long defaultValue)
		{
			column = Column.Long(columnName, defaultValue);
			return this;
		}

		public AddColumnTable DateTime(string columnName)
		{
			column = Column.DateTime(columnName);
			return this;
		}

		public AddColumnTable DateTime(string columnName, DateTime defaultValue)
		{
			column = Column.DateTime(columnName, defaultValue);
			return this;
		}

		public AddColumnTable Decimal(string columnName)
		{
			column = Column.Decimal(columnName);
			return this;
		}

		public AddColumnTable Decimal(string columnName, int whole, int remainder)
		{
			column = Column.Decimal(columnName, whole, remainder);
			return this;
		}

		public AddColumnTable Decimal(string columnName, decimal defaultValue)
		{
			column = Column.Decimal(columnName, defaultValue);
			return this;
		}

		public AddColumnTable Decimal(string columnName, int whole, int remainder, decimal defaultValue)
		{
			column = Column.Decimal(columnName, whole, remainder, defaultValue);
			return this;
		}

		public AddColumnTable Boolean(string columnName)
		{
			column = Column.Boolean(columnName);
			return this;
		}

		public AddColumnTable Boolean(string columnName, bool defaultValue)
		{
			column = Column.Boolean(columnName, defaultValue);
			return this;
		}

		#endregion Column types

		#region Column properties

		public AddColumnTable Identity()
		{
			column.ColumnProperty |= ColumnProperties.Identity;
			return this;
		}

		public AddColumnTable Indexed()
		{
			column.ColumnProperty |= ColumnProperties.Indexed;
			return this;
		}

		public AddColumnTable NotNull()
		{
			column.ColumnProperty |= ColumnProperties.NotNull;
			return this;
		}

		public AddColumnTable Null()
		{
			column.ColumnProperty |= ColumnProperties.Null;
			return this;
		}

		public AddColumnTable PrimaryKey()
		{
			column.ColumnProperty |= ColumnProperties.PrimaryKey;
			return this;
		}

		public AddColumnTable PrimaryKeyWithIdentity()
		{
			column.ColumnProperty |= ColumnProperties.PrimaryKeyWithIdentity;
			return this;
		}

		public AddColumnTable Unique()
		{
			column.ColumnProperty |= ColumnProperties.Unique;
			return this;
		}

		#endregion Column properties

		public void Execute()
		{
			TransformationProvider provider = new TransformationProvider(environment);
			provider.AddColumn(name, column);
		}
	}
}
