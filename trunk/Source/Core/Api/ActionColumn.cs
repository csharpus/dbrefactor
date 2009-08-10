using System;
using System.Collections.Generic;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class ActionColumn
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;
		private readonly ColumnProviderFactory columnProviderFactory;
		private readonly List<string> columnNames = new List<string>();

		public ActionColumn(TransformationProvider provider, string tableName, string columnName,
		                    ColumnProviderFactory columnProviderFactory)
		{
			this.provider = provider;
			this.tableName = tableName;
			this.columnProviderFactory = columnProviderFactory;
			columnNames.Add(columnName);
		}

		public ActionColumn Column(string name)
		{
			columnNames.Add(name);
			return this;
		}

		public void AddPrimaryKey()
		{
			provider.AddPrimaryKey(GeneratePrimaryKeyName(), tableName, columnNames.ToArray());
		}

		private string GeneratePrimaryKeyName()
		{
			return GenerateConstraintName("PK");
		}

		public void DropPrimaryKey()
		{
			provider.DropPrimaryKey(tableName);
		}

		public void AddUnique()
		{
			provider.AddUnique(GenerateUniqueName(), tableName, columnNames.ToArray());
		}

		private string GenerateUniqueName()
		{
			return GenerateConstraintName("UQ");
		}

		public void DropUnique()
		{
			provider.DropUnique(tableName, columnNames.ToArray());
		}

// AddIdentity is nos supported by sql server

		public void AddIndex()
		{
			provider.AddIndex(GenerateIndexName(), tableName, columnNames.ToArray());
		}

		private string GenerateIndexName()
		{
			return GenerateConstraintName("IX");
		}

		public void DropIndex()
		{
			provider.DropIndex(tableName, columnNames.ToArray());
		}

		private string GenerateConstraintName(string prefix)
		{
			string columns = String.Join("_", columnNames.ToArray());
			return String.Format("{0}_{1}_{2}", prefix, tableName, columns);
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
			provider.SetDefault(GenerateDefaultName(), tableName, columnNames[0], value);
		}

		private string GenerateDefaultName()
		{
			return GenerateConstraintName("DF");
		}

//Table("Users").Column("Name").ConvertTo().String(100)
		public void DropDefault()
		{
			provider.DropDefault(tableName, columnNames[0]);
		}

		public OtherTypeColumn ConvertTo()
		{
			return new OtherTypeColumn(tableName, columnNames[0], columnProviderFactory, provider);
		}

		public void AddForeignKeyTo(string primaryKeyTable, params string[] primaryKeyColumns)
		{
			AddForeignKeyTo(primaryKeyTable, OnDelete.NoAction, primaryKeyColumns);
		}

		public void AddForeignKeyTo(string constraintName, string primaryKeyTable, params string[] primaryKeyColumns)
		{
			AddForeignKeyTo(constraintName, primaryKeyTable, OnDelete.NoAction, primaryKeyColumns);
		}

		public void AddForeignKeyTo(string primaryKeyTable, OnDelete onDeleteAction, params string[] primaryKeyColumns)
		{
			AddForeignKeyTo(GenerateForeignKeyName(primaryKeyTable), primaryKeyTable, onDeleteAction, primaryKeyColumns);
		}

		public void AddForeignKeyTo(string constraintName, string primaryKeyTable, OnDelete onDeleteAction,
		                            params string[] primaryKeyColumns)
		{
			provider.AddForeignKey(constraintName, tableName, columnNames.ToArray(), primaryKeyTable,
			                       primaryKeyColumns, onDeleteAction);
		}

		private string GenerateForeignKeyName(string primaryKeyTable)
		{
			return String.Format("FK_{0}_{1}", tableName, primaryKeyTable);
		}

		public void DropForeignKey(string primaryKeyTable, params string[] primaryKeyColumns)
		{
			provider.DropForeignKey(tableName, columnNames.ToArray(), primaryKeyTable, primaryKeyColumns);
		}
	}

	public class OtherTypeColumn
	{
		private readonly string tableName;
		private readonly string columnName;
		private readonly ColumnProviderFactory factory;
		private readonly TransformationProvider provider;

		public OtherTypeColumn(string tableName, string columnName, ColumnProviderFactory factory,
		                       TransformationProvider provider)
		{
			this.tableName = tableName;
			this.columnName = columnName;
			this.factory = factory;
			this.provider = provider;
		}

		#region Column types

		public void String(int size)
		{
			AlterColumn(factory.CreateString(columnName, null, size));
		}

		public void Text()
		{
			AlterColumn(factory.CreateText(columnName, null));
		}

		public void Int()
		{
			AlterColumn(factory.CreateInt(columnName, null));
		}

		public void Long()
		{
			AlterColumn(factory.CreateLong(columnName, null));
		}

		public void DateTime()
		{
			AlterColumn(factory.CreateDateTime(columnName, null));
		}

		public void Decimal()
		{
			AlterColumn(factory.CreateDecimal(columnName, null, 18, 9)); // change this value
		}

		public void Decimal(int whole, int remainder)
		{
			AlterColumn(factory.CreateDecimal(columnName, null, whole, remainder));
		}

		public void Boolean()
		{
			AlterColumn(factory.CreateBoolean(columnName, null));
		}

		private void AlterColumn(ColumnProvider columnProvider)
		{
			provider.AlterColumn(tableName, columnProvider);
		}

		#endregion Column types
	}
}