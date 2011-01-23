using DbRefactor.Core;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	public interface IDatabaseProvider
	{
		void CreateTable(string name, ColumnProvider[] columns);
		void DropTable(string name);
		void AddColumn(string table, ColumnProvider columnProvider);
		void DropColumn(string table, string column);
		void AlterColumn(string tableName, ColumnProvider columnProvider);
		void SetNull(string tableName, string columnName);
		void SetNotNull(string tableName, string columnName);
		void AddUnique(string name, string table, string[] columns);
		void SetDefault(string constraintName, string tableName, string columnName, object value);
		void DropIndex(string table, string[] columns);
		void AddIndex(string name, string table, string[] columns);
		void AddPrimaryKey(string name, string table, string[] columns);
		void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, OnDelete constraint);
		void DropUnique(string table, string[] columnNames);
		void DropDefault(string tableName, string columnName);
		void DropForeignKey(string foreignKeyTable, string[] foreignKeyColumns, string primaryKeyTable, string[] primaryKeyColumns);
		void DropPrimaryKey(string table);
		void RenameTable(string oldName, string newName);
		void RenameColumn(string table, string oldColumnName, string newColumnName);
	}
}