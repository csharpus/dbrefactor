using System.Collections.Generic;
using DbRefactor.Factories;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class ActionColumn
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;
		private readonly ApiFactory apiFactory;
		private readonly ConstraintNameService constraintNameService;
		private readonly List<string> columnNames = new List<string>();

		internal ActionColumn(TransformationProvider provider, string tableName, string columnName, ApiFactory apiFactory,
		                    ConstraintNameService constraintNameService)
		{
			this.provider = provider;
			this.tableName = tableName;
			this.apiFactory = apiFactory;
			this.constraintNameService = constraintNameService;
			columnNames.Add(columnName);
		}

		public ActionColumn Column(string name)
		{
			columnNames.Add(name);
			return this;
		}

		public void AddPrimaryKey()
		{
			provider.AddPrimaryKey(
				constraintNameService.PrimaryKeyName(tableName, columnNames.ToArray()), tableName, columnNames.ToArray());
		}

		public void DropPrimaryKey()
		{
			provider.DropPrimaryKey(tableName);
		}

		public void AddUnique()
		{
			provider.AddUnique(constraintNameService.UniqueName(tableName, columnNames.ToArray()), tableName,
			                   columnNames.ToArray());
		}

		public void DropUnique()
		{
			provider.DropUnique(tableName, columnNames.ToArray());
		}

// AddIdentity is nos supported by sql server

		public void AddIndex()
		{
			provider.AddIndex(constraintNameService.IndexName(tableName, columnNames.ToArray()), tableName, columnNames.ToArray());
		}

		public void DropIndex()
		{
			provider.DropIndex(tableName, columnNames.ToArray());
		}

		public void SetNull()
		{
			//TODO: verify column collection
			provider.SetNull(tableName, columnNames[0]);
		}

		public void SetNotNull()
		{
			//TODO: it doesn't work because of get sql without properties
			provider.SetNotNull(tableName, columnNames[0]);
		}

		public void SetDefault(object value)
		{
			provider.SetDefault(constraintNameService.DefaultName(tableName, columnNames.ToArray()), tableName, columnNames[0],
			                    value);
		}

//Table("Users").Column("Name").ConvertTo().String(100)
		public void DropDefault()
		{
			provider.DropDefault(tableName, columnNames[0]);
		}

		public OtherTypeColumn ConvertTo()
		{
			return apiFactory.CreateOtherTypeColumn(tableName, columnNames[0]);
		}

		/// <param name="primaryKeyTable"></param>
		/// <param name="primaryKeyColumn"></param>
		/// <param name="primaryKeyColumns">Used for composite foreign keys</param>
		public void AddForeignKeyTo(string primaryKeyTable, string primaryKeyColumn, params string[] primaryKeyColumns)
		{
			var primaryKeyList = new List<string> {primaryKeyColumn};
			primaryKeyList.AddRange(primaryKeyColumns);
			AddForeignKeyTo(primaryKeyTable, OnDelete.NoAction, primaryKeyList.ToArray());
		}

		//public void AddForeignKeyTo(string constraintName, string primaryKeyTable, params string[] primaryKeyColumns)
		//{
		//    AddForeignKeyTo(constraintName, primaryKeyTable, OnDelete.NoAction, primaryKeyColumns);
		//}

		public void AddForeignKeyTo(string primaryKeyTable, OnDelete onDeleteAction, params string[] primaryKeyColumns)
		{
			AddForeignKeyTo(constraintNameService.ForeignKeyName(tableName, primaryKeyTable), primaryKeyTable, onDeleteAction,
			                primaryKeyColumns);
		}

		private void AddForeignKeyTo(string constraintName, string primaryKeyTable, OnDelete onDeleteAction,
		                             params string[] primaryKeyColumns)
		{
			provider.AddForeignKey(constraintName, tableName, columnNames.ToArray(), primaryKeyTable,
			                       primaryKeyColumns, onDeleteAction);
		}

		public void DropForeignKey(string primaryKeyTable, params string[] primaryKeyColumns)
		{
			provider.DropForeignKey(tableName, columnNames.ToArray(), primaryKeyTable, primaryKeyColumns);
		}

		public void RenameTo(string newName)
		{
			provider.RenameColumn(tableName, columnNames[0], newName);
		}
	}
}